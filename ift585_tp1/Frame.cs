using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ift585_tp1
{
    class Frame
    {
        // TODO ()
        int id;
        int length;
        int checksum;
        int type; //1 = normal, 2 = ack, 3 = nack
        bool timeout;

        
        // source
        // destination
        
        public const int NB_BYTES = 5; // TODO (Cisco) : is 5 appropriate? no

        public byte[] data;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public bool Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        public Frame(byte[] data, int type = 1)
        {
            // TODO (build frame from byte array)
            this.data = data;
            this.type = type;
        }

        public byte[] toBytes()
        {
            // TODO return byte array representing frame
            return data;
        }

        public override string ToString()
        {
            string s = "Frame : ";
            for (int i = 0; i < NB_BYTES; i++)
            {
                s += data[i] + " ";
            }
            return s;
        }

    }
}
