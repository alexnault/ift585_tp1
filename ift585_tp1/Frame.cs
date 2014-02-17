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
        int ttl;
        int length;
        int checksum;
        // source
        // destination
        
        public const int NB_BYTES = 5; // TODO (Cisco) : is 5 appropriate? no

        public byte[] data;

        public Frame(byte[] data)
        {
            // TODO (build frame from byte array)
            
            this.data = data;
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
