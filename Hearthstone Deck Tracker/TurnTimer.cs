using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Hearthstone_Deck_Tracker
{
    public class TimerEventArgs : EventArgs
    {
        public TimerEventArgs(int seconds, bool running)
        {
            Seconds = seconds;
            Running = running;
        }
        public int Seconds { get; private set; }
        public bool Running { get; private set; }
    }

    class TurnTimer
    {
        private readonly Timer _timer;
        private readonly int _turnTime;
        public int Seconds { get; private set; }
        public event TimerTickHandler TimerTick;
        public delegate void TimerTickHandler(TurnTimer sender, TimerEventArgs e);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="turnTime">Time of a turn in seconds</param>
        public TurnTimer(int turnTime)
        {
            Seconds = turnTime;
            _turnTime = turnTime;
            _timer = new Timer(1000);
            _timer.AutoReset = true;
            _timer.Enabled = true;
            _timer.Elapsed += TimerOnElapsed;
            _timer.Stop();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (Seconds > 0)
                Seconds--;
            TimerTick(this, new TimerEventArgs(Seconds, true));
        }

        public void Restart()
        {
            Seconds = _turnTime;
            _timer.Stop();
            _timer.Start();
        }
        public void Stop()
        {
            _timer.Stop();
            TimerTick(this, new TimerEventArgs(Seconds, false));
        }

    }
}
