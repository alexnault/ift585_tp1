using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Timers;
using System.Diagnostics;

namespace ift585_tp1
{
    class EndDevice
    {
        //protected Network network;

        protected string inputPath;
        protected FileStream fs;

        protected string outputPath;

        protected int timeout;

        public FrameBuffer outBuffer;

        protected int frameId;

        //public readonly AutoResetEvent send = new AutoResetEvent(false);
        //public readonly AutoResetEvent receive = new AutoResetEvent(false);

        protected Network network;

        public EndDevice(Network network, int bufferLength, string inputPath, string outputPath, int timeout)
        {
            this.network = network;
            this.inputPath = inputPath;
            if (this.inputPath != null)
                fs = File.OpenRead(inputPath);

            this.outputPath = outputPath;

            this.timeout = timeout;

            outBuffer = new FrameBuffer(bufferLength * Frame.NB_BYTES);
        }


        public void Start()
        {
            Run();
        }

        private void Run()
        {
            //bool once = true; // TODO (Cisco) remove

            while (true)
            {
                if (inputPath != null)
                {
                    // Read and insert in buffer
                    if (!outBuffer.IsFull())
                    {
                        Frame frame = ReadNext();
                        if (frame != null)
                            outBuffer.Push(frame);
                    }

                    // Try sending
                    if (!outBuffer.IsEmpty())
                    {

                        Frame frameToSend = null;

                        frameToSend = outBuffer.GetMusTResendFrame();
                        if (frameToSend == null)
                        {
                            frameToSend = outBuffer.FrameToSend();
                        }
                        if (frameToSend != null)
                        {
                            if (network.rdyToSend)
                            {
                                Console.WriteLine("Sending " + frameToSend.ToString() + "\n");
                                network.Send(Hamming.Hamming.AddHamming(frameToSend));
                                outBuffer.StartTimer(frameToSend.id, timeout);
                            }
                        }


                        if (network.rdyToReceiveACK)
                        {
                            Frame frameACKReceive = Hamming.Hamming.RemoveHamming(network.ReceiveACK());
                            int idFrame = frameACKReceive.id;
                            Console.WriteLine("Receiving ACK " + frameACKReceive.ToString() + "\n");
                            outBuffer.RemoveFromId(idFrame);

                            //enlever le frametimer de la liste selon le id de la frame
                            outBuffer.RemoveFrameTimer(idFrame);
                        }
                    }
                }
                else if (outputPath != null)
                {
                    //TODO (Cisco) : Receive frames
                    if (network.rdyToReceive)
                    {
                        Frame frame = Hamming.Hamming.RemoveHamming(network.Receive());
                        Console.WriteLine("Receiving " + frame.ToString() + "\n");

                        if (network.rdyToSendACK)
                        {
                            Frame frameACKSend = new Frame(frameId++, 2, BitConverter.GetBytes(frame.id));
                            Console.WriteLine("Sending ACK : " + frameACKSend.ToString() + "\n");
                            network.SendACK(Hamming.Hamming.AddHamming(frameACKSend));
                        }
                    }



                    /*if (!inBuffer.IsEmpty())
                    {
                        Console.WriteLine("receiver received");
                        Frame frame = inBuffer.Pop();

                        Console.WriteLine(frame.ToString());
                        Console.WriteLine(inBuffer.ToString());
                    }*/
                }
            }
        }

        private Frame ReadNext()
        {
            if (fs.Position >= fs.Length)
                return null; // TODO and close stream

            byte[] bytes = new byte[Frame.NB_BYTES]; // TODO read according to data size in frame format
            fs.Read(bytes, 0, Frame.NB_BYTES);

            return new Frame(frameId++, 0, bytes);
        }
    }
}
