using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ift585_tp1
{
    class Network
    {
        protected byte[] source = new byte[Frame.NB_BYTES];
        protected byte[] destination = new byte[Frame.NB_BYTES];

        public bool rdyToSend = true;
        public bool rdyToReceive = false;

        protected byte[] sourceACK = new byte[Frame.NB_BYTES];
        protected byte[] destinationACK = new byte[Frame.NB_BYTES];

        public bool rdyToSendACK = true;
        public bool rdyToReceiveACK = false;

        // TODO ACK & NAK logic (duplicate properties)

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
                    Array.Copy(source, destination, source.Length); // might need to make copy instead
                    rdyToReceive = true;
                    rdyToSend = true;
                }

                // Receiver --> Emitter (ACK & NAK)*/
                if (!rdyToSendACK && !rdyToReceiveACK)
                {
                    Array.Copy(sourceACK, destinationACK, sourceACK.Length);
                    rdyToReceiveACK = true;
                    rdyToSendACK = true;
                }

            }
        }

        public void Send(byte[] data)
        {
            rdyToSend = false;
            source = data;
        }

        public byte[] Receive()
        {
            rdyToReceive = false;
            return destination;
        }

        public void SendACK(byte[] data)
        {
            rdyToSendACK = false;
            if (data != null)
            {
                sourceACK = data;
            }
        }

        public byte[] ReceiveACK()
        {
            rdyToReceiveACK = false;
            return destinationACK;
        }
    }
}
