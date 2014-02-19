using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ift585_tp1
{
    class FrameBuffer : CircularBuffer<Frame>
    {
        private readonly object _mutex = new object();
        
        public FrameBuffer(int length) : base(length) { }

        public override void Push(Frame frame)
        {
            lock (_mutex)
            {
                base.Push(frame);
            }
        }

        public override Frame Pop()
        {
            Frame frame;
            lock (_mutex)
            {
                frame = base.Pop();
            }
            return frame;
        }

        public Frame FrameToSend()
        {
            return buffer[tail];
        }

        // TODO Logic for getting data without popping it.
    }
}
