#region

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using HearthMirror;
using HearthMirror.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using HearthWatcher.LogReader;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	public class LoadingScreenHandler
	{
		private DateTime _lastAutoImport;
		private bool _checkedMirrorStatus;
		public event Action OnHearthMirrorCheckFailed;

		public void Handle(LogLine logLine, IHsGameState gameState, IGame game)
		{
			var match = LogConstants.GameModeRegex.Match(logLine.Line);
			if(match.Success)
			{
				game.CurrentMode = GetMode(match.Groups["curr"].Value);
				game.PreviousMode = GetMode(match.Groups["prev"].Value);

				if((DateTime.Now - logLine.Time).TotalSeconds < 5)
				{
					if(_lastAutoImport < logLine.Time && game.CurrentMode == Mode.TOURNAMENT)
					{
						_lastAutoImport = logLine.Time;
						var decks = DeckImporter.FromConstructed();
						if(decks.Any() && (Config.Instance.ConstructedAutoImportNew || Config.Instance.ConstructedAutoUpdate))
						{
							DeckManager.ImportDecks(decks, false, Config.Instance.ConstructedAutoImportNew,
								Config.Instance.ConstructedAutoUpdate);
						}
					}

					if(game.PreviousMode == Mode.COLLECTIONMANAGER || game.CurrentMode == Mode.COLLECTIONMANAGER
						|| game.PreviousMode == Mode.PACKOPENING)
						CollectionHelper.UpdateCollection().Forget();

					if(game.CurrentMode == Mode.HUB && !_checkedMirrorStatus)
						CheckMirrorStatus();
				}

				if(game.PreviousMode == Mode.GAMEPLAY && game.CurrentMode != Mode.GAMEPLAY)
					gameState.GameHandler.HandleInMenu();

				if(game.CurrentMode == Mode.DRAFT)
					Watchers.ArenaWatcher.Run();
				else
					Watchers.ArenaWatcher.Stop();

				if(game.CurrentMode == Mode.PACKOPENING)
					Watchers.PackWatcher.Run();
				else
					Watchers.PackWatcher.Stop();

				if(game.CurrentMode == Mode.TAVERN_BRAWL)
					Core.Game.CacheBrawlInfo();

				if(game.CurrentMode == Mode.ADVENTURE || game.PreviousMode == Mode.ADVENTURE && game.CurrentMode == Mode.GAMEPLAY)
					Watchers.DungeonRunWatcher.Run();
				else
					Watchers.DungeonRunWatcher.Stop();

				if(game.PlayerChallengeable && Config.Instance.ChallengeAction != Enums.HsActionType.None)
					Watchers.FriendlyChallengeWatcher.Run();
				else
					Watchers.FriendlyChallengeWatcher.Stop(); 

				API.GameEvents.OnModeChanged.Execute(game.CurrentMode);
			}
			else if(logLine.Line.Contains("Gameplay.Start"))
			{
				gameState.Reset();
				gameState.GameHandler.HandleGameStart(logLine.Time);
			}
		}

		private async void CheckMirrorStatus()
		{
			_checkedMirrorStatus = true;
			Status status;
			while((status = Status.GetStatus()).MirrorStatus == MirrorStatus.ProcNotFound)
				await Task.Delay(1000);
			Log.Info($"Mirror status: {status.MirrorStatus}");
			if(status.MirrorStatus != MirrorStatus.Error)
				return;
			Log.Error(status.Exception);
			if(!(status.Exception is Win32Exception))
			{
				Log.Info("Not a Win32Exception - Process probably exited. Checking again later.");
				_checkedMirrorStatus = false;
				return;
			}
			Influx.OnUnevenPermissions();
			OnHearthMirrorCheckFailed?.Invoke();
		}

		private Mode GetMode(string modeString) => Enum.TryParse(modeString, out Mode mode) ? mode : Mode.INVALID;
	}
}
