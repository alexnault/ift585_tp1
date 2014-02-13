using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ift585_tp1
{
    class Frame
    {
        public const int NB_BYTES = 5; // TODO (Cisco) : is 5 appropriate? no

        public byte[] data;

        public Frame(byte[] data)
        {
            this.data = data;
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
