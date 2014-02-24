using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Timers;
using System.Diagnostics;
using ift585_tp1.HammingCode;
using System.Runtime.Serialization.Formatters.Binary;

namespace ift585_tp1
{
    class EndDevice
    {
        protected string inputPath;
        protected FileStream infs;
        protected string outputPath;
        protected FileStream outfs;

        protected int timeout;

        public FrameBuffer outBuffer;

        protected int frameId;

        protected readonly Network network;

        public EndDevice(Network network, int bufferLength, string inputPath, string outputPath, int timeout)
        {
            this.network = network;
            this.inputPath = inputPath;
            if (this.inputPath != null)
            {
                infs = File.OpenRead(inputPath);
            }
            this.outputPath = outputPath;
            //if (this.outputPath != null)
            //    outfs = File.OpenRead(outputPath);

            this.timeout = timeout;

            outBuffer = new FrameBuffer(bufferLength * Frame.NB_MAX_DATA_BYTES);
        }

        public void Start()
        {
            Run();
        }

        /// <summary>
        /// Copie de l'objet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        private void Run()
        {
            while (true)
            {
                // EMITTER
                if (inputPath != null)
                {
                    // Read and insert in outbuffer
                    if (!outBuffer.IsFull())
                    {
                        byte[] bytes = ReadNext();
                        if (bytes != null)
                        {
                            Frame frame = new Frame(frameId++, Frame.Type.Normal, bytes);
                            outBuffer.Push(frame);
                        }
                    }
                    // Send frame
                    if (!outBuffer.IsEmpty() && network.rdyToSend)
                    {
                        Frame frameToSend = outBuffer.GetMustResendFrame();
                        if (frameToSend == null)
                        {
                            frameToSend = outBuffer.FrameToSend();
                        }
                        if (frameToSend != null)
                        {
                            Console.WriteLine("Sending " + frameToSend.ToString());
                            network.Send(Hamming.AddHamming(frameToSend));
                            outBuffer.StartTimer(frameToSend.id, timeout);
                        }
                    }
                    // Receive ACK/NAK
                    //NACK : Hamming wrong -  3 secondes 

                    if (network.rdyToReceiveACK)
                    {
                        Tuple<bool, Frame> t = Hamming.RemoveHamming(network.ReceiveACK());
                        Frame acknak = t.Item2;
                        bool hammingIsFine = t.Item1;

                        if (!hammingIsFine)
                        {
                            outBuffer.SetMustResendFrame(acknak.id);
                        }
                        else
                        {
                            if (t.Item2.type == Frame.Type.NAK)
                            {
                                outBuffer.SetMustResendFrame(acknak.id);
                                Console.WriteLine("Receiving NACK " + acknak.ToString());
                            }
                            else if (t.Item2.type == Frame.Type.ACK)
                            {
                                Console.WriteLine("Receiving ACK " + acknak.ToString());
                                outBuffer.RemoveFromId(acknak.id);
                            }
                            //enlever le frametimer de la liste selon le id de la frame
                            outBuffer.RemoveFrameTimer(acknak.id);
                        }
                    }
                }
                // RECEIVER
                else if (outputPath != null)
                {
                    // Receive frame and add response to buffer
                    if (network.rdyToReceive && !outBuffer.IsFull())
                    {
                        //On copie l'objet car il peut être modififé ailleurs dans un autre Thread
                        Binary binary = DeepClone<Binary>(network.Receive());
                        Tuple<bool, Frame> t = Hamming.RemoveHamming(binary);
                        bool hammingIsFine = t.Item1;
                        Frame frame = t.Item2;
                        Console.WriteLine("Receiving " + frame.ToString());

                        if (!hammingIsFine || !frame.checksumIsFine())
                        {
                            outBuffer.Push(new Frame(frameId++, Frame.Type.NAK, BitConverter.GetBytes(frame.id)));
                        }
                        else
                        {
                            outBuffer.Push(new Frame(frameId++, Frame.Type.ACK, BitConverter.GetBytes(frame.id)));
                            // TODO write in output file
                        }
                    }
                    // Send ACK/NAK
                    if (network.rdyToSendACK && !outBuffer.IsEmpty())
                    {
                        Frame acknak = outBuffer.Pop(); // Receiver's buffer
                        if (acknak.type == Frame.Type.NAK)
                        {
                            Console.WriteLine("Sending NACK : " + acknak.ToString());
                        }
                        else if (acknak.type == Frame.Type.ACK)
                        {
                            Console.WriteLine("Sending ACK : " + acknak.ToString());
                        }
                        network.SendACK(Hamming.AddHamming(acknak));
                    }
                }
            }
        }

        private byte[] ReadNext()
        {
            if (infs.Position >= infs.Length)
                return null; // TODO close stream

            byte[] bytes = new byte[Frame.NB_MAX_DATA_BYTES];
            infs.Read(bytes, 0, Frame.NB_MAX_DATA_BYTES);

            return bytes;
        }
    }
}
