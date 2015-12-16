#region

using System;
using System.Media;
using System.Timers;
using Hearthstone_Deck_Tracker.Enums;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class TimerEventArgs : EventArgs
	{
		public TimerEventArgs(int seconds, int playerSeconds, int opponentSeconds, bool running, ActivePlayer activePlayer)
		{
			Seconds = seconds;
			Running = running;
			PlayerSeconds = playerSeconds;
			OpponentSeconds = opponentSeconds;
			CurrentActivePlayer = activePlayer;
		}

		public int Seconds { get; private set; }
		public int PlayerSeconds { get; private set; }
		public int OpponentSeconds { get; private set; }
		public bool Running { get; private set; }
		public ActivePlayer CurrentActivePlayer { get; private set; }
	}

	internal class TurnTimer
	{
		private bool _opponentMulliganed;
		private bool _playerMulliganed;
		private Timer _timer;
		private int _turnTime;
		public ActivePlayer CurrentActivePlayer;

		private TurnTimer()
		{
		}

		public int Seconds { get; private set; }
		public int PlayerSeconds { get; private set; }
		public int OpponentSeconds { get; private set; }
	    private static TurnTimer _instance;

	    public static TurnTimer Instance
	    {
	        get
	        {
	            if (_instance == null) Create(90);
	            return _instance;
	        }
	    }

	    /// <summary>
		/// 
		/// </summary>
		/// <param name="turnTime">Time of a turn in seconds</param>
		private static void Create(int turnTime)
		{
			_instance = new TurnTimer
			{
				Seconds = turnTime,
				PlayerSeconds = 0,
				OpponentSeconds = 0,
				_turnTime = turnTime,
				_timer = new Timer(1000) {AutoReset = true, Enabled = true}
			};
            _instance._timer.Elapsed += Instance.TimerOnElapsed;
            _instance._timer.Stop();
		}

		private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			if(Seconds > 0)
				Seconds--;
			if(_playerMulliganed && _opponentMulliganed)
			{
				if(CurrentActivePlayer == ActivePlayer.Player)
					PlayerSeconds++;
				else
					OpponentSeconds++;
			}
			TimerTick(this, new TimerEventArgs(Seconds, PlayerSeconds, OpponentSeconds, true, CurrentActivePlayer));
		}

		public void Restart()
		{
			Seconds = _turnTime;
			_timer.Stop();
			_timer.Start();
			if((!_playerMulliganed && _opponentMulliganed) || (!_opponentMulliganed && _playerMulliganed)
			   || (!_opponentMulliganed && !_playerMulliganed && Seconds < 85))
				_playerMulliganed = _opponentMulliganed = true;

			TimerTick(this, new TimerEventArgs(Seconds, PlayerSeconds, OpponentSeconds, true, CurrentActivePlayer));
		}

		public void Stop()
		{
			_timer.Stop();
			PlayerSeconds = 0;
			OpponentSeconds = 0;
			_playerMulliganed = false;
			_opponentMulliganed = false;
			TimerTick(this, new TimerEventArgs(Seconds, PlayerSeconds, OpponentSeconds, false, CurrentActivePlayer));
		}

		public void SetCurrentPlayer(ActivePlayer activePlayer)
		{
			CurrentActivePlayer = activePlayer;
		}

		public void MulliganDone(ActivePlayer activePlayer)
		{
			if(activePlayer.Equals(ActivePlayer.Player))
				_playerMulliganed = true;
			else if(activePlayer.Equals(ActivePlayer.Opponent))
				_opponentMulliganed = true;
		}

		private void TimerTick(TurnTimer sender, TimerEventArgs timerEventArgs)
		{
			Core.Overlay.Dispatcher.BeginInvoke(new Action(() => Core.Overlay.UpdateTurnTimer(timerEventArgs)));
			Core.Windows.TimerWindow.Dispatcher.BeginInvoke(new Action(() => Core.Windows.TimerWindow.Update(timerEventArgs)));

			if(CurrentActivePlayer == ActivePlayer.Player)
				CheckForTimerAlarm();
		}

		private void CheckForTimerAlarm()
		{
			if(Config.Instance.TimerAlert)
			{
				if(Seconds == Config.Instance.TimerAlertSeconds)
				{
					SystemSounds.Asterisk.Play();
					User32.FlashHs();
				}
			}
		}
	}
}