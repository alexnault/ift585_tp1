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

        public FrameBuffer inBuffer;
        public FrameBuffer outBuffer;

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

            //Timer section
            this.timeout = timeout;


            inBuffer = new FrameBuffer(bufferLength * Frame.NB_BYTES);
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
                        //Console.WriteLine("try send");
                        if (network.rdyToSend )
                        {
                            Frame frame = outBuffer.FrameToSend(); 
                            Console.WriteLine("Sending " + frame.ToString());
                            network.Send(frame.toBytes());
                            
                        }
                        if (network.rdyToReceiveACK)
                        {
                            Frame frameACKReceive = new Frame(network.ReceiveACK());
                            Console.WriteLine("Receiving ACK" + outBuffer.Pop());
                        }
                    }
                }
                else if (outputPath != null)
                {
                    //TODO (Cisco) : Receive frames

                    if (network.rdyToReceive)
                    {
                        Frame frame = new Frame(network.Receive());
                        Console.WriteLine("Receiving " + frame.ToString());
                       
                        //if no error { Create ACK to send
                        Frame frameACKSend = new Frame(null,2);
                        Console.WriteLine("Sending ACK" + frame.ToString());
                        network.SendACK(frameACKSend.toBytes());
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

            return new Frame(bytes);
        }
    }
}
