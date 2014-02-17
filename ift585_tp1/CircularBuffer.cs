using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ift585_tp1
{
    class CircularBuffer<T>
    {
        protected int length;
        protected T[] buffer;
        protected int head;
        protected int tail;
        protected int count;

        public CircularBuffer(int length)
        {
            head = 0;
            tail = 0;
            this.length = length;
            buffer = new T[this.length];
        }

        public virtual void Push(T value)
        {
            if (IsFull())
                throw new Exception("Circular buffer is full");

            buffer[head] = value;
            head = (head + 1) % length;
            count++;
        }

        public virtual T Pop()
        {
            if (IsEmpty())
                throw new Exception("Circular buffer is empty");

            T value = buffer[tail];
            buffer[tail] = default(T);
            tail = (tail + 1) % length;
            count--;
            return value;
        }

        public bool IsFull()
        {
            return count >= length;
        }

        public bool IsEmpty()
        {
            return count <= 0;
        }

        public override string ToString()
        {
            string s = "## CircularBuffer(" + length + ") ##\n";
            for (int i = 0; i < length; i++)
            {
                s += buffer[i];
                if (head == i)
                    s += " <-- H (" + head + ")";
                if (tail == i)
                    s += " <-- T (" + tail + ")";
                s += "\n";
            }
            s += "#########\n";
            return s;
        }
    }
}
