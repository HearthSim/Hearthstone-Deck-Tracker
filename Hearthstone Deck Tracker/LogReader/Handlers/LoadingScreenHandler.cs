#region

using System;
using System.Collections.Generic;
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
		public event Action? OnHearthMirrorCheckFailed;
		private List<Mode> ShowExperienceDuringMode = new List<Mode>() { Mode.HUB, Mode.GAME_MODE, Mode.TOURNAMENT, Mode.BACON, Mode.DRAFT, Mode.PVP_DUNGEON_RUN };

		private List<Mode> LettuceModes = new List<Mode>
		{
			Mode.LETTUCE_VILLAGE,
			Mode.LETTUCE_BOUNTY_BOARD,
			Mode.LETTUCE_MAP,
			Mode.LETTUCE_PLAY,
			Mode.LETTUCE_COLLECTION,
			Mode.LETTUCE_COOP,
			Mode.LETTUCE_FRIENDLY,
			Mode.LETTUCE_BOUNTY_TEAM_SELECT,
			Mode.LETTUCE_PACK_OPENING
		};

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
						CollectionHelpers.Hearthstone.UpdateCollection().Forget();

					if(game.PreviousMode == Mode.LETTUCE_COLLECTION || game.CurrentMode == Mode.LETTUCE_COLLECTION
						|| game.PreviousMode == Mode.LETTUCE_PACK_OPENING)
						CollectionHelpers.Mercenaries.UpdateCollection().Forget();

					if(game.CurrentMode == Mode.HUB)
						if(!_checkedMirrorStatus)
							CheckMirrorStatus();
				}

				if(ShowExperienceDuringMode.Contains(game.CurrentMode))
					Core.Overlay.ShowExperienceCounter();
				else
				{
					if(ShowExperienceDuringMode.Contains(game.PreviousMode))
						Core.Overlay.HideExperienceCounter();
				}

				if(game.PreviousMode == Mode.GAMEPLAY && game.CurrentMode != Mode.GAMEPLAY)
					gameState.GameHandler?.HandleInMenu();

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

				if(game.CurrentMode == Mode.BACON)
				{
					Core.Game.CacheBattlegroundRatingInfo();
					Core.Game.BattlegroundsSessionViewModel.Update();
					if (Config.Instance.ShowSessionRecapBetweenGames)
					{
						Core.Overlay.BattlegroundsSession.Show();
					}
				}
				else if (game.CurrentMode != Mode.GAMEPLAY)
					Core.Overlay.BattlegroundsSession.Hide();

				if(game.CurrentMode == Mode.LETTUCE_PLAY)
					Core.Game.CacheMercenariesRatingInfo();

				if(Config.Instance.ShowMercsTasks)
				{
					if(LettuceModes.Contains(game.CurrentMode) || (LettuceModes.Contains(game.PreviousMode) && game.CurrentMode == Mode.GAMEPLAY))
					{
							Core.Overlay.ShowMercenariesTasksButton();
							Core.Overlay.MercenariesTaskListVM.Update();

							if(game.CurrentMode == Mode.GAMEPLAY)
								Core.Overlay.MercenariesTaskListVM.GameNoticeVisibility = Visibility.Visible;
							else 
								Core.Overlay.MercenariesTaskListVM.GameNoticeVisibility = Visibility.Collapsed;
					}
					else
						Core.Overlay.HideMercenariesTasksButton();
				}

				if(game.CurrentMode == Mode.ADVENTURE || game.PreviousMode == Mode.ADVENTURE && game.CurrentMode == Mode.GAMEPLAY)
					Watchers.DungeonRunWatcher.Run();
				else
					Watchers.DungeonRunWatcher.Stop();

				if(game.CurrentMode == Mode.PVP_DUNGEON_RUN || game.PreviousMode == Mode.PVP_DUNGEON_RUN && game.CurrentMode == Mode.GAMEPLAY)
					Watchers.PVPDungeonRunWatcher.Run();
				else
					Watchers.PVPDungeonRunWatcher.Stop();

				if(game.PlayerChallengeable && Config.Instance.ChallengeAction != Enums.HsActionType.None)
					Watchers.FriendlyChallengeWatcher.Run();
				else
					Watchers.FriendlyChallengeWatcher.Stop(); 

				API.GameEvents.OnModeChanged.Execute(game.CurrentMode);
			}
			else if(logLine.Line.Contains("Gameplay.Start"))
			{
				gameState.Reset();
				gameState.GameHandler?.HandleGameStart(logLine.Time);
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
