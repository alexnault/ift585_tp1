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
        public int errorType;
        public int timeout;
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
                // Emitter --> Receiver
                if (!rdyToSend && !rdyToReceive)
                {
                    destination = source; // might need to make copy instead
                    rdyToReceive = true;
                    rdyToSend = true;
                    switch (errorType)
                    {
                        case 0:
                            Random random = new Random();
                            int randomNumber = random.Next(128, destination.Length);
                            destination[134] = !destination[134];
                            break;
                        case 1:
                            Thread.Sleep(timeout);
                            break;
                        default:
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