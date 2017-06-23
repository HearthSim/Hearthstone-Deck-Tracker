#region

using System;
using System.Media;
using System.Threading.Tasks;
using System.Timers;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker
{
	internal class TimerState
	{
		public TimerState(double seconds, int playerSeconds, int opponentSeconds)
		{
			Seconds = seconds;
			PlayerSeconds = playerSeconds;
			OpponentSeconds = opponentSeconds;
		}

		public double Seconds { get; private set; }
		public int PlayerSeconds { get; private set; }
		public int OpponentSeconds { get; private set; }
	}

	internal class TurnTimer
	{
		private readonly Timer _timer = new Timer(1000) {AutoReset = true};
		private GameV2 _game;

		private TurnTimer()
		{
			_timer.Elapsed += TimerOnElapsed;
		}

		static TurnTimer()
		{
		}

		public double Seconds { get; private set; }
		public int PlayerSeconds { get; private set; }
		public int OpponentSeconds { get; private set; }

		private bool IsPlayersTurn => _game.PlayerEntity?.HasTag(GameTag.CURRENT_PLAYER) ?? false;

		public static TurnTimer Instance { get; } = new TurnTimer();

		private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			Seconds--;
			if(_game.IsMulliganDone)
			{
				if(IsPlayersTurn)
					PlayerSeconds++;
				else
					OpponentSeconds++;
			}
			TimerTick(new TimerState(Seconds, PlayerSeconds, OpponentSeconds));
		}

		public async Task Start(GameV2 game)
		{
			if(game == null)
			{
				Log.Warn("Could not start timer, game is null");
				return;
			}
			Log.Info("Starting turn timer");
			if(_game != null)
			{
				Log.Warn("Turn timer is already running");
				return;
			}
			_game = game;
			PlayerSeconds = 0;
			OpponentSeconds = 0;
			Seconds = 75;
			if(game.PlayerEntity == null)
				Log.Warn("Waiting for player entity");
			while(game.PlayerEntity == null)
				await Task.Delay(100);
			if(game.OpponentEntity == null)
				Log.Warn("Waiting for player entity");
			while(game.OpponentEntity == null)
				await Task.Delay(100);
			TimerTick(new TimerState(Seconds, PlayerSeconds, OpponentSeconds));
			_timer.Start();
		}

		public void Stop()
		{
			if(_game == null)
				return;
			Log.Info("Stopping turn timer");
			_timer.Stop();
			_game = null;
		}

		private void TimerTick(TimerState timerState)
		{
			Core.Overlay.Dispatcher.BeginInvoke(new Action(() => Core.Overlay.UpdateTurnTimer(timerState)));
			Core.Windows.TimerWindow.Dispatcher.BeginInvoke(new Action(() => Core.Windows.TimerWindow.Update(timerState)));
			if(IsPlayersTurn)
				CheckForTimerAlarm();
		}

		private void CheckForTimerAlarm()
		{
			if(!Config.Instance.TimerAlert || Seconds != Config.Instance.TimerAlertSeconds)
				return;
			SystemSounds.Asterisk.Play();
			User32.FlashHs();
		}

		public void SetPlayer(ActivePlayer player)
		{
			if(_game == null)
			{
				Seconds = 75;
				Log.Warn("Set timer to 75, game is null");
				return;
			}
			if(player == ActivePlayer.Player && _game.PlayerEntity != null)
				Seconds = _game.PlayerEntity.HasTag(GameTag.TIMEOUT) ? _game.PlayerEntity.GetTag(GameTag.TIMEOUT) : double.PositiveInfinity;
			else if(player == ActivePlayer.Opponent && _game.OpponentEntity != null)
				Seconds = _game.OpponentEntity.HasTag(GameTag.TIMEOUT) ? _game.OpponentEntity.GetTag(GameTag.TIMEOUT) : double.PositiveInfinity;
			else
			{
				Seconds = 75;
				Log.Warn("Set timer to 75, both player entities are null");
			}
		}
	}
}
