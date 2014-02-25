using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ift585_tp1.HammingCode;

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
                }

                // Receiver --> Emitter (ACK & NAK)*/
                if (!rdyToSendACK && !rdyToReceiveACK)
                {
                    destinationACK = sourceACK;
                    rdyToReceiveACK = true;
                    rdyToSendACK = true;
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
