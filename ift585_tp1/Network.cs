using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ift585_tp1.HammingCode;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
namespace ift585_tp1
{
    class Network
    {
        protected Binary source;
        protected Binary destination;
        public bool rdyToSend = true;
        public bool rdyToReceive = false;
        protected Binary sourceACK;
        protected Binary destinationACK;
        public bool rdyToSendACK = true;
        public bool rdyToReceiveACK = false;
        protected int errorType;
        protected int timeout;
        public Network(int errorType, int timeout)
        {
            this.errorType = errorType;
            this.timeout = timeout;
        }
        public void Start()
        {
            Run();
        }

        private void Run()
        {
            while (true)
            {
                Thread.Sleep(1000);
                // Emitter --> Receiver
                if (!rdyToSend && !rdyToReceive)
                {
                    // Generate error == 0 else debug purpose
                    switch (errorType)
                    {
                        case 0:
                            Console.WriteLine(">Insert error: No error(0), Flipping bit(1), Destroy frame(2).");
                            string sErrorType = Console.ReadLine();
                            if (sErrorType == "1") 
                            {
                                Console.WriteLine(">Bit to flip: (0 to "+ source.Length + ")?");
                                string sbitFlipper = Console.ReadLine();
                                if (Convert.ToInt32(sbitFlipper) >= 0 && Convert.ToInt32(sbitFlipper) < source.Length) 
                                {
                                    source[Convert.ToInt32(sbitFlipper)] = !source[Convert.ToInt32(sbitFlipper)];
                                }
                                else
                                {
                                    Console.WriteLine("The number set in the parameter is out of bound");
                                }

                                destination = source; // might need to make copy instead
                                rdyToReceive = true;
                                rdyToSend = true;
                            }
                            else if (sErrorType == "2")
                            {
                                // frame is lost
                                rdyToSend = true;
                            }
                            else
                            {
                                destination = source; // might need to make copy instead
                                rdyToReceive = true;
                                rdyToSend = true;
                            }
                            break;
                        default:
                            destination = source; // might need to make copy instead
                            rdyToReceive = true;
                            rdyToSend = true;
                            break;
                    }
                }

                // Receiver --> Emitter (ACK & NAK)*/
                if (!rdyToSendACK && !rdyToReceiveACK)
                {
                    destinationACK = sourceACK;
                    rdyToReceiveACK = true;
                    rdyToSendACK = true;
                    //switch (errorType)
                    //{
                    //    case 0:
                    //        Random random = new Random();
                    //        int randomNumber = random.Next(0, destination.Length);
                    //        destinationACK[randomNumber] = !destinationACK[randomNumber];
                    //        break;
                    //    case 1:
                    //        break;
                    //    case 2:
                    //        break;
                    //    default:
                    //        break;
                    //}
                }
            }
        }

        public void Send(Binary data)
        {
            rdyToSend = false;
            source = data;
        }

        public Binary Receive()
        {
            rdyToReceive = false;
            return destination;
        }

        public void SendACK(Binary data)
        {
            if (data != null)
            {
                sourceACK = data;
            }
            rdyToSendACK = false;
        }

        public Binary ReceiveACK()
        {
            rdyToReceiveACK = false;
            return destinationACK;
        }
    }
}