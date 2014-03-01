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
        public enum protocol
        {
            global = 0,
            selectif = 1
        };
        protected string inputPath;
        protected FileStream infs;
        protected string outputPath;
        protected FileStream outfs;

        protected int timeout;

        public FrameBuffer outBuffer;
        public FrameBuffer inBuffer;

        protected int frameId;

        protected readonly Network network;

        protected int protocolType = 0;
        protected int awaitedFrameId = 0;

        public EndDevice(Network network, int bufferLength, string inputPath, string outputPath, int timeout, int protocolType)
        {
            this.network = network;
            this.inputPath = inputPath;
            if (inputPath != null)
            {
                infs = File.OpenRead(inputPath);
            }
            this.outputPath = outputPath;
            if (outputPath != null)
            {
                File.WriteAllText(outputPath, string.Empty);
                outfs = File.OpenWrite(outputPath);
            }

            this.timeout = timeout;
            this.protocolType = protocolType;
            outBuffer = new FrameBuffer(bufferLength);
            inBuffer = new FrameBuffer(bufferLength);
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
                            Console.WriteLine("Sending   --> " + frameToSend.ToString());
                            network.Send(Hamming.AddHamming(frameToSend));
                            outBuffer.StartTimer(frameToSend.id, timeout);
                        }
                    }
                    // Receive ACK/NAK
                    if (network.rdyToReceiveACK)
                    {
                        //On copie l'objet car il peut être modififé ailleurs dans un autre Thread
                        Binary binary = DeepClone<Binary>(network.ReceiveACK());
                        Tuple<bool, Frame> t = Hamming.RemoveHamming(binary);
                        bool hammingIsFine = t.Item1;
                        Frame ackOrNak = t.Item2;

                        if (hammingIsFine)
                        {
                            int ackOrNakForId = BitConverter.ToInt32(ackOrNak.data, 0);

                            if (ackOrNak.type == Frame.Type.ACK)
                            {
                                Console.WriteLine("Receiving <-- " + ackOrNak.ToString() + " for frame " + ackOrNakForId + ".");
                                outBuffer.RemoveLessOrEqualId(ackOrNakForId);
                                outBuffer.RemoveFrameTimerLessOrEqualId(ackOrNakForId);
                            }
                            else
                            {
                                Console.WriteLine("Receiving <-- " + ackOrNak.ToString() + " for frame " + ackOrNakForId + ".");
                                outBuffer.GetFrameFromId(ackOrNakForId).mustResend = 1;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Receiving <-- " + ackOrNak.ToString() + ", rejecting due to error.");
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

                        #region "Algo de rejet global"
                        if (protocolType == (int)protocol.global)
                        {
                            if (!hammingIsFine || !frame.checksumIsFine())
                            {
                                Console.WriteLine("Receiving --> " + frame.ToString() + ", reject due to error.");
                                // We can't be sure of the ID due to error, so we send NAK for the awaited frame instead
                                outBuffer.Push(new Frame(frameId++, Frame.Type.NAK, BitConverter.GetBytes(awaitedFrameId)));
                            }
                            else if (frame.id > awaitedFrameId)
                            {
                                Console.WriteLine("Receiving --> " + frame.ToString() + ", global reject.");
                                outBuffer.Push(new Frame(frameId++, Frame.Type.NAK, BitConverter.GetBytes(awaitedFrameId)));
                            }
                            else if (frame.id == awaitedFrameId)
                            {
                                Console.WriteLine("Receiving --> " + frame.ToString() + ", OK.");
                                awaitedFrameId++;
                                outBuffer.Push(new Frame(frameId++, Frame.Type.ACK, BitConverter.GetBytes(frame.id)));
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Writing frame " + frame.id + " to file (content in yellow) :");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine(frame.DataToString());
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                WriteNext(frame.data);
                            }
                            else
                            {
                                Console.WriteLine("Receiving --> " + frame.ToString() + ", ignore since we already have it.");
                            }
                        }
                        #endregion

                        #region "Algo selectif"
                        else if (protocolType == (int)protocol.selectif)
                        {
                            if (!hammingIsFine || !frame.checksumIsFine())
                            {
                                Console.WriteLine("Receiving --> " + frame.ToString() + ", reject due to error.");
                                // We can't be sure of the ID due to error, so we send NAK for the awaited frame instead
                                outBuffer.Push(new Frame(frameId++, Frame.Type.NAK, BitConverter.GetBytes(awaitedFrameId)));
                            }
                            else if (frame.id < awaitedFrameId || inBuffer.GetFrameFromId(frameId) != null)
                            {
                                Console.WriteLine("Receiving --> " + frame.ToString() + ", ignore since we already have it.");
                            }
                            else if (frame.id > awaitedFrameId)
                            {
                                if (inBuffer.GetFreeCount() > 1) // We have more than one free entry
                                {
                                    Console.WriteLine("Receiving --> " + frame.ToString() + ", stored, waiting for " + awaitedFrameId + ".");
                                    inBuffer.Push(frame);
                                }
                                else
                                {
                                    Console.WriteLine("Receiving --> " + frame.ToString() + ", rejected, waiting for " + awaitedFrameId + ".");
                                }
                                outBuffer.Push(new Frame(frameId++, Frame.Type.NAK, BitConverter.GetBytes(awaitedFrameId)));
                            }
                            else // frame.id == awaitedFrameId
                            {
                                Console.WriteLine("Receiving --> " + frame.ToString() + ", OK.");
                                inBuffer.Push(frame);

                                Frame next;
                                do
                                {
                                    next = inBuffer.GetFrameFromId(awaitedFrameId);
                                    if (next != null)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine("Writing frame " + next.id + " to file (content in yellow) :");
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Console.WriteLine(frame.DataToString());
                                        Console.ForegroundColor = ConsoleColor.DarkGray;
                                        WriteNext(next.data);
                                        inBuffer.RemoveFromId(next.id);
                                        awaitedFrameId++;
                                    }
                                    else
                                    {
                                        outBuffer.Push(new Frame(frameId++, Frame.Type.ACK, BitConverter.GetBytes(awaitedFrameId - 1)));
                                    }
                                } while (next != null);
                            }
                        }
                        #endregion

                    }
                    // Send ACK/NAK
                    if (network.rdyToSendACK && !outBuffer.IsEmpty())
                    {
                        Frame ackOrNak = outBuffer.Pop(); // Receiver's buffer
                        if (ackOrNak.type == Frame.Type.ACK)
                            Console.WriteLine("Sending   <-- " + ackOrNak.ToString());
                        else
                            Console.WriteLine("Sending   <-- " + ackOrNak.ToString());
                        network.SendACK(Hamming.AddHamming(ackOrNak));
                    }
                }
            }
        }

        private byte[] ReadNext()
        {
            byte[] bytes = null;
            if (infs.CanRead)
            {
                if (infs.Position >= infs.Length)
                {
                    infs.Dispose();
                }
                else
                {
                    int dataLenght = Convert.ToInt32(Math.Min(Frame.NB_MAX_DATA_BYTES, (infs.Length - infs.Position)));
                    bytes = new byte[dataLenght];
                    infs.Read(bytes, 0, dataLenght);
                }
            }
            return bytes;
        }

        private void WriteNext(byte[] data)
        {
            outfs.Write(data, 0, data.Length);
            outfs.Flush(); // Ensure the data is written
        }
    }
}