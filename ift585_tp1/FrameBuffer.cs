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
        List<FrameTimer> frameTimerList = new List<FrameTimer>();
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
            if(current < head)
                return buffer[current++];
            return null;
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
                    this.RemoveAt(i);
                    break;
                }
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



        // TODO Logic for getting data without popping it.

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
        /// Remove FrameTimer and stop it
        /// </summary>
        /// <param name="id"></param>
        public void RemoveFrameTimer(int id)
        {
            FrameTimer timerFrame = frameTimerList.FirstOrDefault(x => x.Id == id);
            timerFrame.Timer.Dispose();
            frameTimerList.Remove(timerFrame);
        }

        /// <summary>
        /// Lorsque l'intervalle est arrivé on devra réenvoyé la trame
        /// </summary>
        /// <param name="id"></param>
        private void Callback(Object id)
        {
            Console.WriteLine("Timeout");
            //on va chercher le frame dans le buffer selon le id
            Frame frame = GetFrameFromId((int)id);
            if(frame!=null)
             frame.mustResend = 1;
        }

        public Frame GetMusTResendFrame()
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
    }
}
