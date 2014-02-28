using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ift585_tp1
{
    class FrameBuffer : CircularBuffer<Frame>
    {
        protected int current;

        List<FrameTimer> frameTimerList = new List<FrameTimer>();
        private readonly object _mutex = new object();

        public FrameBuffer(int length) : base(length)
        {
            this.current = tail;
        }

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
            Frame toSend = null;
            if ((tail < head && current >= tail && current < head) // [...T...C...H...] or
                || (tail > head && !(current >= head && current < tail))) // [...H...T...C...] or [...C...H...T...]
            {
                toSend = buffer[current];
                current = (current + 1) % length;
            }
            return toSend;
        }

        public Frame GetFrameFromId(int id)
        {
            return buffer.FirstOrDefault(x => x!=null && x.id == id);
        }

        public void RemoveFromId(int id)
        {
            for (int i = 0; i < count; i++)
            {
                if (buffer[i].id == id)
                {
                    lock (_mutex)
                    {
                        this.RemoveAt(i);
                    }
                    break;
                }
            }
        }

        public void RemoveLessOrEqualId(int id)
        {
            int i = 0;
            while (i < count)
            {
                if (buffer[i].id <= id)
                {
                    lock (_mutex)
                    {
                        this.RemoveAt(i);
                    }
                }
                else
                    i++;
            }
        }

        public void RemoveAt(int index)
        {
            // validate the index
            if (index < 0 || index >= count)
                throw new IndexOutOfRangeException();
            // move all items above the specified position one step
            // closer to zeri
            for (int i = index; i < count - 1; i++)
            {
                // get the next relative target position of the item
                int to = (head - count + i) % length;
                // get the next relative source position of the item
                int from = (head - count + i + 1) % length;
                // move the item
                buffer[to] = buffer[from];
            }
            // get the relative position of the last item, which becomes empty
            // after deletion and set the item as empty
            int last = (head - 1) % length;
            buffer[last] = default(Frame);
            // adjust storage information
            head--;
            count--;
        }

        public int GetFreeCount()
        {
            return length - count;
        }

        /// <summary>
        /// Create Timer and add to the list
        /// </summary>
        /// <param name="id"></param>
        /// <param name="timeout"></param>
        public void StartTimer(int id, int timeout)
        {
            Timer timer = new Timer(Callback, id, timeout, Timeout.Infinite);
            FrameTimer frameTimer = new FrameTimer(id, timer);
            frameTimerList.Add(frameTimer);
        }

        /// <summary>
        /// Remove FrameTimers which id are less or equal to the specified id and stop them
        /// </summary>
        /// <param name="id"></param>
        public void RemoveFrameTimerLessOrEqualId(int id)
        {
            int i = 0;
            while (i < frameTimerList.Count)
            {
                if (frameTimerList[i].Id <= id)
                {
                    frameTimerList[i].Timer.Dispose();
                    frameTimerList.RemoveAt(i);
                }
                else
                    i++;
            }
        }

        /// <summary>
        /// Lorsque l'intervalle est arrivé on devra réenvoyé la trame
        /// </summary>
        /// <param name="id"></param>
        private void Callback(Object id)
        {
            Console.WriteLine("Timeout " + (int)id);
            //on va chercher le frame dans le buffer selon le id
            Frame frame = GetFrameFromId((int)id);
            if (frame != null)
                SetMustResendFrame(frame.id);
        }

        public Frame GetMustResendFrame()
        {
            for (int i = 0; i < count; i++)
            {
                if (buffer[i].mustResend == 1)
                {
                    buffer[i].mustResend = 0;
                    return buffer[i];
                }
            }
            return null;
        }

        public void SetMustResendFrame(int id)
        {
            for (int i = 0; i < count; i++)
            {
                if (buffer[i].id == id)
                {
                    buffer[i].mustResend = 1;
                }
            }
        }
    }
}
