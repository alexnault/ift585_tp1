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
            return buffer[tail];
        }

        public Frame GetFrameFromId(int id)
        {
            return buffer.FirstOrDefault(x => x.Id == id);        
        }

        public void RemoveFromId(int id)
        {
            for (int i = 0; i < count; i++)
            {
                if (buffer[i].Id == id)
                {
                    this.RemoveAt(i);
                    break;
                }
            }
        }

        

        // TODO Logic for getting data without popping it.

        /// <summary>
        /// Create Timer and add to the list
        /// </summary>
        /// <param name="id"></param>
        /// <param name="timeout"></param>
        public void StartTimer(int id, int timeout)
        {
            Timer timer = new Timer( Callback, id, timeout, Timeout.Infinite );
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
            GetFrameFromId((int)id).Timeout = true;
        }

        public Frame GetTimeoutFrame()
        {
            for(int i=0;i<count;i++)
            {
                if (buffer[i].Timeout)
                {
                    buffer[i].Timeout = false;
                    return buffer[i];
                }
            }

            return null;

        }
    }
}
