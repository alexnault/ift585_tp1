using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ift585_tp1
{
    class FrameTimer
    {
        int id;
        Timer timer;

        public FrameTimer(int id, Timer timer)
        {
            this.Id = id;
            this.Timer = timer;
        }

        /// <summary>
        /// Id de la trame
        /// </summary>
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Le timer 
        /// </summary>
        public Timer Timer
        {
            get { return timer; }
            set { timer = value; }
        }


    }
}
