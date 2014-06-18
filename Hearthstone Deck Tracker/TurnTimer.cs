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
        public TimerEventArgs(int seconds, int playerSeconds, int opponentSeconds, bool running, Turn turn)
        {
            Seconds = seconds;
            Running = running;
            PlayerSeconds = playerSeconds;
            OpponentSeconds = opponentSeconds;
            CurrentTurn = turn;
        }
        public int Seconds { get; private set; }
        public int PlayerSeconds { get; private set; }
        public int OpponentSeconds { get; private set; }
        public bool Running { get; private set; }
        public Turn CurrentTurn { get; private set; }
    }

    class TurnTimer
    {
        private readonly Timer _timer;
        private readonly int _turnTime;
        public int Seconds { get; private set; }
        public int PlayerSeconds { get; private set; }
        public int OpponentSeconds { get; private set; }
        public event TimerTickHandler TimerTick;
        public delegate void TimerTickHandler(TurnTimer sender, TimerEventArgs e);

        public Turn _currentTurn;
        private bool _playerMulliganed = false;
        private bool _opponentMulliganed = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="turnTime">Time of a turn in seconds</param>
        public TurnTimer(int turnTime)
        {
            Seconds = turnTime;
            PlayerSeconds = 0;
            OpponentSeconds = 0;
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
            if (_playerMulliganed && _opponentMulliganed)
            {
                if (_currentTurn == Turn.Player)
                {
                    PlayerSeconds++;
                }
                else
                {
                    OpponentSeconds++;
                }
            }
            TimerTick(this, new TimerEventArgs(Seconds, PlayerSeconds, OpponentSeconds, true, _currentTurn));
        }

        public void Restart()
        {
            Seconds = _turnTime;
            _timer.Stop();
            _timer.Start();
            if ((!_playerMulliganed && _opponentMulliganed) 
                || (!_opponentMulliganed && _playerMulliganed)
                || (!_opponentMulliganed && !_playerMulliganed && Seconds < 85))
            {
                _playerMulliganed = _opponentMulliganed = true;
            }

            TimerTick(this, new TimerEventArgs(Seconds, PlayerSeconds, OpponentSeconds, true, _currentTurn));
        }
        public void Stop()
        {
            _timer.Stop();
            PlayerSeconds = 0;
            OpponentSeconds = 0;
            _playerMulliganed = false;
            _opponentMulliganed = false;
            TimerTick(this, new TimerEventArgs(Seconds, PlayerSeconds, OpponentSeconds, false, _currentTurn));
        }

        public void SetCurrentPlayer(Turn turn)
        {
            _currentTurn = turn;
        }

        public void MulliganDone(Turn turn)
        {
            if (turn.Equals(Turn.Player))
            {
                _playerMulliganed = true;
            }
            else if (turn.Equals(Turn.Opponent))
            {
                _opponentMulliganed = true;
            }
        }
    }
}
