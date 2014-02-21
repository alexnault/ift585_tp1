using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

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
                        if (network.rdyToSend)
                        {
                            Frame frame = outBuffer.Pop(); 
                            Console.WriteLine("Sending " + frame.ToString());
                            network.Send(frame.toBytes()); // TODO must not actually POP the value (because of ACK). need more FrameBuffer logic
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
