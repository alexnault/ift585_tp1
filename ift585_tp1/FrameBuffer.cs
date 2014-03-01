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
            Frame toSend = buffer[current];
            current = (current + 1) % length;
            return toSend;
        }

        public Frame GetFrameFromId(int id)
        {
            return buffer.FirstOrDefault(x => x != null && x.id == id);
        }

        public void RemoveFromId(int id)
        {
            if (!IsEmpty())
            {
                int i = tail;
                do
                {
                    if (buffer[i].id == id)
                    {
                        lock (_mutex)
                        {
                            this.RemoveAt(i);
                        }
                        break;
                    }
                    i = (i + 1) % length;
                } while (i != head);
            }
        }

        public void RemoveLessOrEqualId(int id)
        {
            if (!IsEmpty())
            {
                int i = tail;
                do
                {
                    if (buffer[i].id <= id)
                    {
                        lock (_mutex)
                        {
                            this.RemoveAt(i);
                        }
                    }
                    i = (i + 1) % length;
                } while (i != head);
            }
        }

        public void RemoveAt(int index)
        {
            // Validate the index
            if (index < 0 || index >= length)
                throw new IndexOutOfRangeException();

            // Shift right items between the tail and the index
            int i = index;
            while (i != tail)
            {
                buffer[i] = buffer[(i - 1 + length) % length];
                i = (i - 1 + length) % length;
            }
            base.Pop(); // Remove at tail
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
            if (!IsEmpty())
            {
                int i = tail;
                do
                {
                    if (buffer[i].mustResend == 1)
                    {
                        buffer[i].mustResend = 0;
                        return buffer[i];
                    }
                    i = (i + 1) % length;
                } while (i != head);
            }
            return null;
        }

        public void SetMustResendFrame(int id)
        {
            if (!IsEmpty())
            {
                int i = tail;
                do
                {
                    if (buffer[i].id == id)
                    {
                        buffer[i].mustResend = 1;
                        break;
                    }
                    i = (i + 1) % length;
                } while (i != head);
            }
        }

    }
}
