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

        protected int awaitedFrameId = 0;

        public EndDevice(Network network, int bufferLength, string inputPath, string outputPath, int timeout)
        {
            this.network = network;
            this.inputPath = inputPath;
            if (this.inputPath != null)
                infs = File.OpenRead(inputPath);
            this.outputPath = outputPath;
            if (this.outputPath != null)
                outfs = File.OpenWrite(outputPath);

            this.timeout = timeout;

            outBuffer = new FrameBuffer(bufferLength * Frame.NB_MAX_DATA_BYTES);
        }

        public void Start()
        {
            Run();
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
                    if (network.rdyToReceiveACK)
                    {
                        Tuple<bool, Frame> t = Hamming.RemoveHamming(network.ReceiveACK());
                        bool hammingIsFine = t.Item1;
                        Frame ackOrNak = t.Item2;
                        
                        int ackOrNakForId = BitConverter.ToInt32(ackOrNak.data, 0);

                        if (ackOrNak.type == Frame.Type.ACK)
                        {
                            Console.WriteLine("Receiving ACK " + ackOrNak.ToString());
                            outBuffer.RemoveFromId(ackOrNakForId);
                            outBuffer.RemoveFrameTimer(ackOrNakForId);
                        }
                        else
                        {
                            Console.WriteLine("Receiving NAK " + ackOrNak.ToString());
                            outBuffer.GetFrameFromId(ackOrNakForId).mustResend = 1;
                        }
                    }
                }
                // RECEIVER
                else if (outputPath != null)
                {
                    // Receive frame and add response to buffer
                    if (network.rdyToReceive && !outBuffer.IsFull())
                    {
                        Binary binary = network.Receive();
                        Tuple<bool, Frame> t = Hamming.RemoveHamming(binary);
                        bool hammingIsFine = t.Item1;
                        Frame frame = t.Item2;

                        if (frame.id > awaitedFrameId)
                        {
                            Console.WriteLine("Receiving " + frame.ToString() + ", global reject.");
                            outBuffer.Push(new Frame(frameId++, Frame.Type.NAK, BitConverter.GetBytes(awaitedFrameId)));
                        }
                        else if (frame.id == awaitedFrameId)
                        {
                            if (!hammingIsFine || !frame.checksumIsFine())
                            {
                                Console.WriteLine("Receiving " + frame.ToString() + ", reject due to error.");
                                outBuffer.Push(new Frame(frameId++, Frame.Type.NAK, BitConverter.GetBytes(frame.id)));
                            }
                            else
                            {
                                Console.WriteLine("Receiving " + frame.ToString() + ", OK.");
                                awaitedFrameId++;
                                outBuffer.Push(new Frame(frameId++, Frame.Type.ACK, BitConverter.GetBytes(frame.id)));
                                WriteNext(frame.data);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Receiving " + frame.ToString() + ", ignore since we already have it.");
                        }
                    }
                    // Send ACK/NAK
                    if (network.rdyToSendACK && !outBuffer.IsEmpty())
                    {
                        Frame acknak = outBuffer.Pop(); // Receiver's buffer
                        Console.WriteLine("Sending ACK : " + acknak.ToString());
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

        private void WriteNext(byte[] data)
        {
            outfs.Write(data, 0, data.Length);
            outfs.Flush(); // Ensure the data is written
        }
    }
}
