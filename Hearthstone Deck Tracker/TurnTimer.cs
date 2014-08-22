using System;
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

	internal class TurnTimer
	{
		public Turn CurrentTurn;
		private bool _opponentMulliganed;
		private bool _playerMulliganed;
		private Timer _timer;
		private int _turnTime;

		private TurnTimer()
		{
		}

		public int Seconds { get; private set; }
		public int PlayerSeconds { get; private set; }
		public int OpponentSeconds { get; private set; }


		public static TurnTimer Instance { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="turnTime">Time of a turn in seconds</param>
		public static void Create(int turnTime)
		{
			Instance = new TurnTimer {Seconds = turnTime, PlayerSeconds = 0, OpponentSeconds = 0, _turnTime = turnTime, _timer = new Timer(1000) {AutoReset = true, Enabled = true}};
			Instance._timer.Elapsed += Instance.TimerOnElapsed;
			Instance._timer.Stop();
		}

		private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			if(Seconds > 0)
				Seconds--;
			if(_playerMulliganed && _opponentMulliganed)
			{
				if(CurrentTurn == Turn.Player)
					PlayerSeconds++;
				else
					OpponentSeconds++;
			}
			TimerTick(this, new TimerEventArgs(Seconds, PlayerSeconds, OpponentSeconds, true, CurrentTurn));
		}

		public void Restart()
		{
			Seconds = _turnTime;
			_timer.Stop();
			_timer.Start();
			if((!_playerMulliganed && _opponentMulliganed)
			   || (!_opponentMulliganed && _playerMulliganed)
			   || (!_opponentMulliganed && !_playerMulliganed && Seconds < 85))
				_playerMulliganed = _opponentMulliganed = true;

			TimerTick(this, new TimerEventArgs(Seconds, PlayerSeconds, OpponentSeconds, true, CurrentTurn));
		}

		public void Stop()
		{
			_timer.Stop();
			PlayerSeconds = 0;
			OpponentSeconds = 0;
			_playerMulliganed = false;
			_opponentMulliganed = false;
			TimerTick(this, new TimerEventArgs(Seconds, PlayerSeconds, OpponentSeconds, false, CurrentTurn));
		}

		public void SetCurrentPlayer(Turn turn)
		{
			CurrentTurn = turn;
		}

		public void MulliganDone(Turn turn)
		{
			if(turn.Equals(Turn.Player))
				_playerMulliganed = true;
			else if(turn.Equals(Turn.Opponent))
				_opponentMulliganed = true;
		}


		private void TimerTick(TurnTimer sender, TimerEventArgs timerEventArgs)
		{
			Helper.MainWindow.Overlay.Dispatcher.BeginInvoke(
				new Action(() => Helper.MainWindow.Overlay.UpdateTurnTimer(timerEventArgs)));
			Helper.MainWindow.TimerWindow.Dispatcher.BeginInvoke(
				new Action(() => Helper.MainWindow.TimerWindow.Update(timerEventArgs)));
		}
	}
}