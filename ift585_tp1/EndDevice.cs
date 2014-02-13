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
        protected Network network;
        
        protected string inputPath;
        protected string outputPath;

        protected int timeout;

        CircularBuffer<byte> buffer;

        public EndDevice(Network network, int bufferLength, string inputPath, string outputPath, int timeout)
        {
            this.network = network;
            this.inputPath = inputPath;
            this.outputPath = outputPath;
            this.timeout = timeout;

            buffer = new CircularBuffer<byte>(bufferLength * Frame.NB_BYTES);
        }

        public void Start()
        {
            Run();
        }

        private void Run()
        {
            bool once = true; // TODO (Cisco) remove

            while (true)
            {
                
                if (inputPath != null)
                {
                    Frame frame = ReadNext();
                    PushFrame(frame);

                    if (once)
                    {
                        Console.WriteLine(frame.ToString());
                        Console.WriteLine(buffer.ToString());
                        once = false;
                    }
                    //TODO (Cisco) : Send message when ready
                    //Send(frame);
                }
                else if (outputPath != null)
                {
                    //TODO (Cisco) : Receive frames
                }
            }
        }

        private Frame ReadNext()
        {
            if (inputPath == null)
                return null;
            if (!File.Exists(inputPath))
                return null;

            //TODO (Cisco) : Read N chars of the input file (where N = Frame.NB_BYTES chars)
            FileStream fs = null;
            try
            {
                fs = File.OpenRead(inputPath);
                byte[] bytes = new byte[Frame.NB_BYTES]; // maybe less?
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                return new Frame(bytes);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }

        private void Send(Frame frame)
        {
            
        }

        private void PushFrame(Frame frame)
        {
            for (int i = 0; i < Frame.NB_BYTES; i++)
            {
                buffer.Push(frame.data[i]);
            }
        }

        private Frame PopFrame()
        {
            byte[] data = new byte[Frame.NB_BYTES];
            for (int i = 0; i < Frame.NB_BYTES; i++)
            {
                data[i] = buffer.Pop();
            }
            return new Frame(data);
        }
    }
}
