using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ift585_tp1
{
    // TODO (Cisco) : Currently, the circular buffer overwrites previous values when full; is it the desired behavior? 

    class CircularBuffer<T>
    {
        protected int length;
        protected T[] buffer;
        protected int index;

        public CircularBuffer(int length)
        {
            index = -1;
            this.length = length;
            buffer = new T[this.length];
        }

        public void Push(T value)
        {
            index = (index + 1) % length;
            buffer[index] = value;
        }

        public T Pop()
        {
            index = (index - 1) % length;
            return buffer[index];
        }

        public override string ToString()
        {
            string s = "## CircularBuffer(" + length + ") ##\n";
            for (int i = 0; i < length; i++)
            {
                s += buffer[i];
                if (index == i)
                    s += " <-- " + index;
                s += "\n";
            }
            s += "#########\n";
            return s;
        }
    }
}
