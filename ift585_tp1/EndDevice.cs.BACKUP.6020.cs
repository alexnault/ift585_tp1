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

<<<<<<< HEAD
            //Timer section
            this.timeout = timeout;


            inBuffer = new FrameBuffer(bufferLength * Frame.NB_BYTES);
=======
            this.timeout = timeout;

>>>>>>> Implentation du ACK et du timer
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
<<<<<<< HEAD
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
=======

                        Frame frameToSend = null;

                        frameToSend = outBuffer.GetTimeoutFrame();
                        if (frameToSend == null)
                        {
                            frameToSend = outBuffer.FrameToSend();
                        }
                        if (frameToSend != null)
                        {
                            while (!network.rdyToSend)
                            { }

                            Console.WriteLine("Sending " + frameToSend.ToString());
                            network.Send(frameToSend.toBytes());
                            outBuffer.StartTimer(frameToSend.Id, timeout);
                        }


                        if (network.rdyToReceiveACK)
                        {
                            Frame frameACKReceive = new Frame(network.ReceiveACK());
                            int idFrame = BitConverter.ToInt32(frameACKReceive.data, 0);
                            Console.WriteLine("Receiving ACK" + idFrame);
                            outBuffer.RemoveFromId(idFrame);

                            //enlever le frametimer de la liste selon le id de la frame
                            outBuffer.RemoveFrameTimer(idFrame);
>>>>>>> Implentation du ACK et du timer
                        }
                    }
                }
                else if (outputPath != null)
                {
                    //TODO (Cisco) : Receive frames

                    if (network.rdyToReceive)
                    {
                        Frame frame = new Frame(14, 0, network.Receive());
                        Console.WriteLine("Receiving " + frame.ToString());
<<<<<<< HEAD
                       
                        //if no error { Create ACK to send
                        Frame frameACKSend = new Frame(null,2);
                        Console.WriteLine("Sending ACK" + frame.ToString());
                        network.SendACK(frameACKSend.toBytes());
=======

                        if (network.rdyToSendACK)
                        {
                            Frame frameACKSend = new Frame(BitConverter.GetBytes(frame.Id), 2);
                            Console.WriteLine("Sending ACK" + frame.ToString());
                            network.SendACK(frameACKSend.toBytes());
                        }
>>>>>>> Implentation du ACK et du timer
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

            return new Frame(40, 0, bytes);
        }
    }
}
