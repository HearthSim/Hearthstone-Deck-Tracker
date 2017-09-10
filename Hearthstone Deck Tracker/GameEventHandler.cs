#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HearthDb.Enums;
using HearthMirror;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.Live;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.Toasts;
using Hearthstone_Deck_Tracker.Windows;
using HSReplay.LogValidation;
using static Hearthstone_Deck_Tracker.Enums.GameMode;
using static HearthDb.Enums.GameTag;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class GameEventHandler : IGameHandler
	{
		private readonly GameV2 _game;
		private ArenaRewardDialog _arenaRewardDialog;
		private Deck _assignedDeck;

		private Entity _attackingEntity;
		private Entity _defendingEntity;
		private bool _handledGameEnd;
		private GameStats _lastGame;
		private DateTime _lastGameStart;


		private bool _showedNoteDialog;

		public GameEventHandler(GameV2 game)
		{
			_game = game;
		}

		public bool RecordCurrentGameMode => _game.CurrentGameMode == None && Config.Instance.RecordOther
											 || _game.CurrentGameMode == Practice && Config.Instance.RecordPractice
											 || _game.CurrentGameMode == Arena && Config.Instance.RecordArena
											 || _game.CurrentGameMode == Brawl && Config.Instance.RecordBrawl
											 || _game.CurrentGameMode == Ranked && Config.Instance.RecordRanked
											 || _game.CurrentGameMode == Friendly && Config.Instance.RecordFriendly
											 || _game.CurrentGameMode == Casual && Config.Instance.RecordCasual
											 || _game.CurrentGameMode == Spectator && Config.Instance.RecordSpectator;

		public bool UploadCurrentGameMode => _game.CurrentGameMode == Practice && Config.Instance.HsReplayUploadPractice
											 || _game.CurrentGameMode == Arena && Config.Instance.HsReplayUploadArena
											 || _game.CurrentGameMode == Brawl && Config.Instance.HsReplayUploadBrawl
											 || _game.CurrentGameMode == Ranked && Config.Instance.HsReplayUploadRanked
											 || _game.CurrentGameMode == Friendly && Config.Instance.HsReplayUploadFriendly
											 || _game.CurrentGameMode == Casual && Config.Instance.HsReplayUploadCasual
											 || _game.CurrentGameMode == Spectator && Config.Instance.HsReplayUploadSpectator;

		public void HandleInMenu()
		{
			if(_game.IsInMenu)
				return;

			Log.Info("Game is now in menu.");
			_game.IsInMenu = true;

			TurnTimer.Instance.Stop();
			Core.Overlay.HideTimers();
			Core.Overlay.HideSecrets();
			Core.Overlay.Update(true);
			DeckManager.ResetIgnoredDeckId();
			Core.Windows.CapturableOverlay?.UpdateContentVisibility();

			SaveAndUpdateStats();

			if(Config.Instance.AutoArchiveArenaDecks && (DeckList.Instance.ActiveDeck?.IsArenaRunCompleted ?? false))
				Core.MainWindow.ArchiveDeck(DeckList.Instance.ActiveDeck, true);

			_game.ResetStoredGameState();

			if(_arenaRewardDialog != null)
			{
				_arenaRewardDialog.Show();
				_arenaRewardDialog.Activate();
			}

			if(_game.CurrentGameStats != null && _game.CurrentGameStats.GameMode == Arena)
			{
				ArenaStats.Instance.UpdateArenaRuns();
				ArenaStats.Instance.UpdateArenaStats();
				ArenaStats.Instance.UpdateArenaStatsHighlights();
			}

			if(!_game.IsUsingPremade)
				_game.DrawnLastGame =
					new List<Card>(_game.Player.RevealedEntities.Where(x => !x.Info.Created && !x.Info.Stolen && x.Card.Collectible 
									&& x.IsPlayableCard).GroupBy(x => x.CardId).Select(x =>
					{
						var card = Database.GetCardFromId(x.Key);
						card.Count = x.Count();
						return card;
					}));

			if(!Config.Instance.KeepDecksVisible)
				Core.Reset().Forget();
			GameEvents.OnInMenu.Execute();
		}

		private bool _savedReplay;
		private async Task SaveReplays()
		{
			if(!_savedReplay && _game.CurrentGameStats != null)
			{
				_savedReplay = true;
				await LogIsComplete();
				var powerLog = new List<string>();
				foreach(var stored in _game.StoredPowerLogs.Where(x => x.Item1 == _game.MetaData.ServerInfo.GameHandle))
					powerLog.AddRange(stored.Item2);
				powerLog.AddRange(_game.PowerLog);

				var createGameCount = 0;
				powerLog = powerLog.TakeWhile(x => !(x.Contains("CREATE_GAME") && createGameCount++ == 1)).ToList();

				if(Config.Instance.RecordReplays && RecordCurrentGameMode && _game.Entities.Count > 0 && !_game.SavedReplay
					&& _game.CurrentGameStats.ReplayFile == null)
					_game.CurrentGameStats.ReplayFile = ReplayMaker.SaveToDisk(_game.CurrentGameStats, powerLog);

				if(Config.Instance.HsReplayAutoUpload && UploadCurrentGameMode)
				{
					var log = powerLog.ToArray();
					var validationResult = LogValidator.Validate(log);
					if(validationResult.IsValid)
						LogUploader.Upload(log, (GameMetaData)_game.MetaData.Clone(), _game.CurrentGameStats).Forget();
					else 
					{
						Log.Error("Invalid log: " + validationResult.Reason);
						Influx.OnEndOfGameUploadError(validationResult.Reason);
					}
				}
			}
		}

		private async Task LogIsComplete()
		{
			if(LogContainsGoldRewardState || _game.CurrentGameMode == Practice && LogContainsStateComplete)
				return;
			Log.Info("GOLD_REWARD_STATE not found");
			await Task.Delay(500);
			if(LogContainsStateComplete || _game.IsInMenu)
				return;
			Log.Info("STATE COMPLETE not found");
			for(var i = 0; i < 5; i++)
			{
				await Task.Delay(1000);
				if(LogContainsStateComplete || _game.IsInMenu)
					break;
				Log.Info($"Waiting for STATE COMPLETE... ({i})");
			}
		}

		private bool LogContainsGoldRewardState
			=> _game?.PowerLog?.Count(x => x.Contains("tag=GOLD_REWARD_STATE value=1")) == 2;

		private bool LogContainsStateComplete
			=> _game?.PowerLog?.Any(x => x.Contains("tag=STATE value=COMPLETE")) ?? false;

		public void HandleConcede()
		{
			if(_game.CurrentGameStats != null)
				_game.CurrentGameStats.WasConceded = true;
		}

		public void HandleAttackingEntity(Entity entity)
		{
			_attackingEntity = entity;
			if(_attackingEntity == null || _defendingEntity == null)
				return;
			if(entity.IsControlledBy(_game.Player.Id))
				_game.SecretsManager.HandleAttack(_attackingEntity, _defendingEntity);
			OnAttackEvent();
		}

		public void HandleDefendingEntity(Entity entity)
		{
			_defendingEntity = entity;
			if(_attackingEntity == null || _defendingEntity == null)
				return;
			if(entity.IsControlledBy(_game.Opponent.Id))
				_game.SecretsManager.HandleAttack(_attackingEntity, _defendingEntity);
			OnAttackEvent();
		}

		private void OnAttackEvent()
		{
			var attackInfo = new AttackInfo((Card)_attackingEntity.Card.Clone(), (Card)_defendingEntity.Card.Clone());
			if(_attackingEntity.IsControlledBy(_game.Player.Id))
				GameEvents.OnPlayerMinionAttack.Execute(attackInfo);
			else
				GameEvents.OnOpponentMinionAttack.Execute(attackInfo);
		}

		public void HandlePlayerMinionPlayed()
		{
			_game.SecretsManager.HandleMinionPlayed();
		}

		public void HandleOpponentMinionDeath(Entity entity, int turn)
		{
			_game.SecretsManager.HandleMinionDeath(entity);
		}


		public void HandleEntityPredamage(Entity entity, int value)
		{
			if(_game.PlayerEntity?.IsCurrentPlayer ?? false)
				HandleOpponentDamage(entity);
			GameEvents.OnEntityWillTakeDamage.Execute(new PredamageInfo(entity, value));
		}

		public void HandleOpponentDamage(Entity entity)
		{
			_game.SecretsManager.HandleOpponentDamage(entity);
		}

		private readonly int[] _lastTurnStart = new int[2];
		public void HandleTurnsInPlayChange(Entity entity, int turn)
		{
			if(_game.OpponentEntity == null)
				return;
			if(entity.IsHero)
			{
				var player = _game.OpponentEntity.IsCurrentPlayer ? ActivePlayer.Opponent : ActivePlayer.Player;
				if(_lastTurnStart[(int)player] >= turn)
					return;
				_lastTurnStart[(int)player] = turn;
				TurnStart(player, turn);
				return;
			}
			_game.SecretsManager.HandleTurnsInPlayChange(entity, turn);
		}

		public void SetOpponentHero(string hero)
		{
			if(string.IsNullOrEmpty(hero))
				return;
			_game.Opponent.Class = hero;

			if(_game.CurrentGameStats != null)
				_game.CurrentGameStats.OpponentHero = hero;
			Log.Info("Opponent=" + hero);
		}

		public void SetPlayerHero(string hero)
		{
			if(string.IsNullOrEmpty(hero))
				return;
			_game.Player.Class = hero;
			if(_game.CurrentGameStats != null)
				_game.CurrentGameStats.PlayerHero = hero;
			Log.Info("Player=" + hero);
		}

		private readonly Queue<Tuple<ActivePlayer, int>> _turnQueue = new Queue<Tuple<ActivePlayer, int>>();
		public async void TurnStart(ActivePlayer player, int turnNumber)
		{
			if(!_game.IsMulliganDone)
				Log.Info("--- Mulligan ---");
			if(turnNumber == 0)
				turnNumber++;
			_turnQueue.Enqueue(new Tuple<ActivePlayer, int>(player, turnNumber));
			while(!_game.IsMulliganDone)
				await Task.Delay(100);
			while(_turnQueue.Any())
				HandleTurnStart(_turnQueue.Dequeue());
		}

		private void HandleTurnStart(Tuple<ActivePlayer, int> turn)
		{
			var player = turn.Item1;
			Log.Info($"--- {player} turn {turn.Item2} ---");
			if(player == ActivePlayer.Player)
				HandleThaurissanCostReduction();
			GameEvents.OnTurnStart.Execute(player);
			if(_turnQueue.Count > 0)
				return;
			TurnTimer.Instance.SetPlayer(player);
			if(player == ActivePlayer.Player && !_game.IsInMenu)
			{
				if(Config.Instance.FlashHsOnTurnStart)
					User32.FlashHs();

				if(Config.Instance.BringHsToForeground)
					User32.BringHsToForeground();
			}
		}

		private void HandleThaurissanCostReduction()
		{
			var thaurissans = _game.Opponent.Board.Where(x => x.CardId == HearthDb.CardIds.Collectible.Neutral.EmperorThaurissan && !x.HasTag(SILENCED)).ToList();
			if(!thaurissans.Any())
				return;

			foreach(var card in _game.Opponent.Hand)
				card.Info.CostReduction += thaurissans.Count;
		}

		private DateTime _lastGameStartTimestamp = DateTime.MinValue;
		public void HandleGameStart(DateTime timestamp)
		{
			if(_game.CurrentGameMode == Practice && !_game.IsInMenu && !_handledGameEnd
				&& _lastGameStartTimestamp  > DateTime.MinValue && timestamp > _lastGameStartTimestamp)
				HandleAdventureRestart();
			_lastGameStartTimestamp = timestamp;
			if(DateTime.Now - _lastGameStart < new TimeSpan(0, 0, 0, 5)) //game already started
				return;
			_handledGameEnd = false;
			_lastGameStart = DateTime.Now;
			Log.Info("--- Game start ---");

			if(Config.Instance.FlashHsOnTurnStart)
				User32.FlashHs();
			if(Config.Instance.BringHsToForeground)
				User32.BringHsToForeground();
			_lastTurnStart[0] = _lastTurnStart[1] = 0;
			_arenaRewardDialog = null;
			_showedNoteDialog = false;
			_game.IsInMenu = false;
			_savedReplay = false;
			_game.Reset();
			_game.CacheMatchInfo();
			_game.CacheGameType();
			_game.CacheSpectator();
			_game.MetaData.ServerInfo = Reflection.GetServerInfo();
			if(!string.IsNullOrEmpty(_game.MetaData.ServerInfo?.Address))
			{
				var region = Helper.GetRegionByServerIp(_game.MetaData.ServerInfo.Address);
				if(_game.CurrentRegion == Region.UNKNOWN || region == Region.CHINA)
				{
					_game.CurrentRegion = region;
					Log.Info("Set current region to" + region);
				}
			}
			TurnTimer.Instance.Start(_game).Forget();

			var selectedDeck = DeckList.Instance.ActiveDeckVersion;

			if(selectedDeck != null)
				_game.IsUsingPremade = true;
			Core.Windows.CapturableOverlay?.UpdateContentVisibility();
			GameEvents.OnGameStart.Execute();
			LiveDataManager.WatchBoardState();
		}

		private void HandleAdventureRestart()
		{
			//The game end is not logged in PowerTaskList
			Log.Info("Adventure was restarted. Simulating game end.");
			HandleConcede();
			HandleLoss();
			HandleGameEnd();
			HandleInMenu();
		}

#pragma warning disable 4014
		public async void HandleGameEnd()
		{
			try
			{
				if(_game.CurrentGameStats == null || _handledGameEnd)
				{
					Log.Warn("HandleGameEnd was already called.");
					return;
				}
				_handledGameEnd = true;
				TurnTimer.Instance.Stop();
				Core.Overlay.HideTimers();
				DeckManager.ResetAutoSelectCount();
				LiveDataManager.Stop();
				Log.Info("Game ended...");
				_game.InvalidateMatchInfoCache();
				if(_game.CurrentGameMode == Spectator && _game.CurrentGameStats.Result == GameResult.None)
				{
					Log.Info("Game was spectator mode without a game result. Probably exited spectator mode early.");
					return;
				}
				var player = _game.Entities.FirstOrDefault(e => e.Value?.IsPlayer ?? false).Value;
				var opponent = _game.Entities.FirstOrDefault(e => e.Value != null && e.Value.HasTag(PLAYER_ID) && !e.Value.IsPlayer).Value;
				if(player != null)
				{
					_game.CurrentGameStats.PlayerName = player.Name;
					_game.CurrentGameStats.Coin = !player.HasTag(FIRST_PLAYER);
				}
				if(opponent != null && CardIds.HeroIdDict.ContainsValue(_game.CurrentGameStats.OpponentHero))
					_game.CurrentGameStats.OpponentName = opponent.Name;
				else
					_game.CurrentGameStats.OpponentName = _game.CurrentGameStats.OpponentHero;

				_game.CurrentGameStats.Turns = Core.GetTurnNumber();
				if(Config.Instance.DiscardZeroTurnGame && _game.CurrentGameStats.Turns < 1)
				{
					Log.Info("Game has 0 turns, discarded. (DiscardZeroTurnGame)");
					_assignedDeck = null;
					GameEvents.OnGameEnd.Execute();
					return;
				}
				_game.CurrentGameStats.GameMode = _game.CurrentGameMode;
				_game.CurrentGameStats.Format = _game.CurrentFormat;
				Log.Info("Format: " + _game.CurrentGameStats.Format);
				if(_game.CurrentGameMode == Ranked && _game.MatchInfo != null)
				{
					var wild = _game.CurrentFormat == Format.Wild;
					_game.CurrentGameStats.Rank = wild ? _game.MatchInfo.LocalPlayer.WildRank : _game.MatchInfo.LocalPlayer.StandardRank;
					_game.CurrentGameStats.OpponentRank = wild ? _game.MatchInfo.OpposingPlayer.WildRank : _game.MatchInfo.OpposingPlayer.StandardRank;
					_game.CurrentGameStats.LegendRank = wild ? _game.MatchInfo.LocalPlayer.WildLegendRank : _game.MatchInfo.LocalPlayer.StandardLegendRank;
					_game.CurrentGameStats.OpponentLegendRank = wild ? _game.MatchInfo.OpposingPlayer.WildLegendRank : _game.MatchInfo.OpposingPlayer.StandardLegendRank;
					_game.CurrentGameStats.Stars = wild ? _game.MatchInfo.LocalPlayer.WildStars : _game.MatchInfo.LocalPlayer.StandardStars;
				}
				else if(_game.CurrentGameMode == Arena)
				{
					_game.CurrentGameStats.ArenaWins = DeckImporter.ArenaInfoCache?.Wins ?? 0;
					_game.CurrentGameStats.ArenaLosses = DeckImporter.ArenaInfoCache?.Losses ?? 0;
				}
				else if(_game.CurrentGameMode == Brawl && _game.BrawlInfo != null)
				{
					_game.CurrentGameStats.BrawlWins = _game.BrawlInfo.Wins;
					_game.CurrentGameStats.BrawlLosses = _game.BrawlInfo.Losses;
				}
				_game.CurrentGameStats.GameType = _game.CurrentGameType;
				_game.CurrentGameStats.ServerInfo = _game.MetaData.ServerInfo;
				_game.CurrentGameStats.PlayerCardbackId = _game.MatchInfo?.LocalPlayer.CardBackId ?? 0;
				_game.CurrentGameStats.OpponentCardbackId = _game.MatchInfo?.OpposingPlayer.CardBackId ?? 0;
				_game.CurrentGameStats.FriendlyPlayerId = _game.MatchInfo?.LocalPlayer.Id ?? 0;
				_game.CurrentGameStats.ScenarioId = _game.MatchInfo?.MissionId ?? 0;
				_game.CurrentGameStats.BrawlSeasonId = _game.MatchInfo?.BrawlSeasonId ?? 0;
				_game.CurrentGameStats.RankedSeasonId = _game.MatchInfo?.RankedSeasonId ?? 0;
				var confirmedCards = _game.Player.RevealedCards.Where(x => x.Collectible)
					.Concat(_game.Player.KnownCardsInDeck.Where(x => x.Collectible && !x.IsCreated))
					.ToList();
				if(_game.CurrentSelectedDeck != null && _game.CurrentSelectedDeck.Id > 0)
				{
					_game.CurrentGameStats.HsDeckId = _game.CurrentSelectedDeck.Id;
					_game.CurrentGameStats.SetPlayerCards(_game.CurrentSelectedDeck, confirmedCards);
				}
				else
				{
					_game.CurrentGameStats.HsDeckId = DeckList.Instance.ActiveDeckVersion?.HsId ?? 0;
					_game.CurrentGameStats.SetPlayerCards(DeckList.Instance.ActiveDeckVersion, confirmedCards);
				}
				_game.CurrentGameStats.SetOpponentCards(_game.Opponent.OpponentCardList.Where(x => !x.IsCreated).ToList());
				_game.CurrentGameStats.GameEnd();
				GameEvents.OnGameEnd.Execute();
				_game.CurrentSelectedDeck = null;
				var selectedDeck = DeckList.Instance.ActiveDeck;
				if(selectedDeck != null)
				{
					var revealed = _game.Player.RevealedEntities.Where(x => x != null).ToList();
					if(Config.Instance.DiscardGameIfIncorrectDeck
					   && !revealed.Where(x => x.IsPlayableCard && !x.Info.Created && !x.Info.Stolen && x.Card.Collectible)
					   .GroupBy(x => x.CardId).All(x => selectedDeck.GetSelectedDeckVersion().Cards.Any(c2 => x.Key == c2.Id && x.Count() <= c2.Count)))
					{
						if(Config.Instance.AskBeforeDiscardingGame)
						{
							var discardDialog = new DiscardGameDialog(_game.CurrentGameStats) {Topmost = true};
							discardDialog.ShowDialog();
							if(discardDialog.Result == DiscardGameDialogResult.Discard)
							{
								Log.Info("Assigned current game to NO deck - selected deck does not match cards played (dialogresult: discard)");
								_assignedDeck = null;
								return;
							}
							if(discardDialog.Result == DiscardGameDialogResult.MoveToOther)
							{
								var moveDialog = new MoveGameDialog(DeckList.Instance.Decks.Where(d => d.Class == _game.CurrentGameStats.PlayerHero))
								{
									Topmost = true
								};
								moveDialog.ShowDialog();
								var targetDeck = moveDialog.SelectedDeck;
								if(targetDeck != null)
								{
									selectedDeck = targetDeck;
									_game.CurrentGameStats.PlayerDeckVersion = moveDialog.SelectedVersion;
									//...continue as normal
								}
								else
								{
									Log.Info("No deck selected in move game dialog after discard dialog, discarding game");
									_assignedDeck = null;
									return;
								}
							}
						}
						else
						{
							Log.Info("Assigned current game to NO deck - selected deck does not match cards played (no dialog)");
							_assignedDeck = null;
							return;
						}
					}
					else
						_game.CurrentGameStats.PlayerDeckVersion = selectedDeck.GetSelectedDeckVersion().Version;

					_lastGame = _game.CurrentGameStats;
					selectedDeck.DeckStats.AddGameResult(_lastGame);

					if(Config.Instance.ArenaRewardDialog && (selectedDeck.IsArenaRunCompleted ?? false))
					{
						if (selectedDeck.ArenaReward.Packs[0] == ArenaRewardPacks.None)
							selectedDeck.ArenaReward.Packs[0] = Enum.GetValues(typeof(ArenaRewardPacks)).OfType<ArenaRewardPacks>().Max();

						_arenaRewardDialog = new ArenaRewardDialog(selectedDeck);
					}
					if(Config.Instance.ShowNoteDialogAfterGame && !Config.Instance.NoteDialogDelayed && !_showedNoteDialog)
					{
						_showedNoteDialog = true;
						new NoteDialog(_game.CurrentGameStats);
					}
					Log.Info("Assigned current game to deck: " + selectedDeck.Name);
					_assignedDeck = selectedDeck;

					// Unarchive the active deck after we have played a game with it
					if(_assignedDeck.Archived)
					{
						Log.Info("Automatically unarchiving deck " + selectedDeck.Name + " after assigning current game");
						Core.MainWindow.ArchiveDeck(_assignedDeck, false);
					}
					_lastGame = null;
				}
				else
				{
					try
					{
						DefaultDeckStats.Instance.GetDeckStats(_game.Player.Class).AddGameResult(_game.CurrentGameStats);
						Log.Info($"Assigned current deck to default {_game.Player.Class} deck.");
					}
					catch(Exception ex)
					{
						Log.Error("Error saving to DefaultDeckStats: " + ex);
					}
					_assignedDeck = null;
				}

				if(_game.StoredGameStats != null)
					_game.CurrentGameStats.StartTime = _game.StoredGameStats.StartTime;

				await SaveReplays();

				if(Config.Instance.ShowGameResultNotifications && RecordCurrentGameMode)
				{
					var deckName = _assignedDeck == null ? "No deck - " + _game.CurrentGameStats.PlayerHero : _assignedDeck.NameAndVersion;
					ToastManager.ShowGameResultToast(deckName, _game.CurrentGameStats);
				}
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

#pragma warning restore 4014
		private async Task GameModeSaved(int timeoutInSeconds)
		{
			var startTime = DateTime.Now;
			var timeout = TimeSpan.FromSeconds(timeoutInSeconds);
			while(_lastGame != null && _lastGame.GameMode == None && (DateTime.Now - startTime) < timeout)
				await Task.Delay(100);
		}

		private void LogEvent(string type, string id = "", int turn = 0, int from = -1, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
			=> Log.Info($"{type} (id:{id} turn:{turn} from:{@from})", memberName, sourceFilePath);

		public void HandleWin()
		{
			if(_game.CurrentGameStats == null)
				return;
			Log.Info("--- Game was won! ---");
			_game.CurrentGameStats.Result = GameResult.Win;
			GameEvents.OnGameWon.Execute();
		}

		public void HandleLoss()
		{
			if(_game.CurrentGameStats == null)
				return;
			Log.Info("--- Game was lost! ---");
			_game.CurrentGameStats.Result = GameResult.Loss;
			GameEvents.OnGameLost.Execute();
		}

		public void HandleTied()
		{
			if(_game.CurrentGameStats == null)
				return;
			Log.Info("--- Game was a tie! ---");
			_game.CurrentGameStats.Result = GameResult.Draw;
			GameEvents.OnGameTied.Execute();
		}

		private void SaveAndUpdateStats()
		{
			if(RecordCurrentGameMode)
			{
				if(Config.Instance.ShowNoteDialogAfterGame && Config.Instance.NoteDialogDelayed && !_showedNoteDialog)
				{
					_showedNoteDialog = true;
					new NoteDialog(_game.CurrentGameStats);
				}

				if(_game.CurrentGameStats != null)
				{
					_game.CurrentGameStats.Turns = Core.GetTurnNumber();
					if(Config.Instance.DiscardZeroTurnGame && _game.CurrentGameStats.Turns < 1)
					{
						Log.Info("Game has 0 turns, discarded. (DiscardZeroTurnGame)");
						return;
					}
					if(_game.CurrentGameStats.GameMode != _game.CurrentGameMode)
					{
						_game.CurrentGameStats.GameMode = _game.CurrentGameMode;
						Log.Info("Set CurrentGameStats.GameMode to " + _game.CurrentGameMode);
					}
					if(_game.CurrentGameStats.GameMode == Arena)
					{
						ArenaStats.Instance.UpdateArenaStats();
						ArenaStats.Instance.UpdateArenaRuns();
						ArenaStats.Instance.UpdateArenaStatsHighlights();
					}
					else
						ConstructedStats.Instance.UpdateConstructedStats();
				}

				if(_assignedDeck == null)
				{
					Log.Info("Saving DefaultDeckStats");
					DefaultDeckStats.Save();
				}
				else
				{
					_assignedDeck.StatsUpdated();
					Log.Info("Saving DeckStats");
					DeckStatsList.Save();
				}
				LastGames.Instance.Add(_game.CurrentGameStats);
				LastGames.Save();
			}
			else if(_assignedDeck != null && _assignedDeck.DeckStats.Games.Contains(_game.CurrentGameStats))
			{
				//game was not supposed to be recorded, remove from deck again.
				_assignedDeck.DeckStats.Games.Remove(_game.CurrentGameStats);
				Log.Info($"Gamemode {_game.CurrentGameMode} is not supposed to be saved. Removed game from {_assignedDeck}.");
			}
			else if(_assignedDeck == null)
			{
				var defaultDeck = DefaultDeckStats.Instance.GetDeckStats(_game.Player.Class);
				if(defaultDeck != null)
				{
					defaultDeck.Games.Remove(_game.CurrentGameStats);
					Log.Info($"Gamemode {_game.CurrentGameMode} is not supposed to be saved. Removed game from default {_game.Player.Class}.");
				}
			}
		}

		public void HandlePlayerHeroPower(string cardId, int turn)
		{
			LogEvent("PlayerHeroPower", cardId, turn);
			_game.SecretsManager.HandleHeroPower();
			GameEvents.OnPlayerHeroPower.Execute();
		}

		public void HandleOpponentHeroPower(string cardId, int turn)
		{
			LogEvent("OpponentHeroPower", cardId, turn);
			GameEvents.OnOpponentHeroPower.Execute();
		}

		public void HandlePlayerFatigue(int currentDamage)
		{
			LogEvent("PlayerFatigue", "", currentDamage);
			_game.Player.Fatigue = currentDamage;
			GameEvents.OnPlayerFatigue.Execute(currentDamage);
		}

		public void HandleOpponentFatigue(int currentDamage)
		{
			LogEvent("OpponentFatigue", "", currentDamage);
			_game.Opponent.Fatigue = currentDamage;
			GameEvents.OnOpponentFatigue.Execute(currentDamage);
		}

		#region Player

		public void HandlePlayerGetToDeck(Entity entity, string cardId, int turn)
		{
			_game.Player.CreateInDeck(entity, turn);
			Core.UpdatePlayerCards();
			GameEvents.OnPlayerCreateInDeck.Execute(Database.GetCardFromId(cardId));
		}

		public void HandlePlayerGet(Entity entity, string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			_game.Player.CreateInHand(entity, turn);
			Core.UpdatePlayerCards();
			GameEvents.OnPlayerGet.Execute(Database.GetCardFromId(cardId));
		}

		public void HandlePlayerBackToHand(Entity entity, string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			Core.UpdatePlayerCards();
			_game.Player.BoardToHand(entity, turn);
			GameEvents.OnPlayerPlayToHand.Execute(Database.GetCardFromId(cardId));
		}

		public void HandlePlayerDraw(Entity entity, string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			if(cardId == "GAME_005")
				HandlePlayerGet(entity, cardId, turn);
			else
			{
				_game.Player.Draw(entity, turn);
				Core.UpdatePlayerCards();
				DeckManager.DetectCurrentDeck().Forget();
			}
			GameEvents.OnPlayerDraw.Execute(Database.GetCardFromId(cardId));
		}


		public void HandlePlayerMulligan(Entity entity, string cardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			_game.Player.Mulligan(entity);
			Core.UpdatePlayerCards();
			GameEvents.OnPlayerMulligan.Execute(Database.GetCardFromId(cardId));
		}

		public void HandlePlayerSecretPlayed(Entity entity, string cardId, int turn, Zone fromZone)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			if(!entity.IsSecret)
			{
				if(entity.IsQuest)
				{
					_game.Player.QuestPlayedFromHand(entity, turn);
					GameEvents.OnPlayerPlay.Execute(Database.GetCardFromId(cardId));
				}
				return;
			}

			switch (fromZone)
			{
				case Zone.DECK:
					_game.Player.SecretPlayedFromDeck(entity, turn);
					DeckManager.DetectCurrentDeck().Forget();
					break;
				case Zone.HAND:
					_game.Player.SecretPlayedFromHand(entity, turn);
					_game.SecretsManager.HandleCardPlayed(entity);
					break;
				default:
					_game.Player.CreateInSecret(entity, turn);
					return;
			}
			Core.UpdatePlayerCards();
			GameEvents.OnPlayerPlay.Execute(Database.GetCardFromId(cardId));
		}

		public void HandlePlayerHandDiscard(Entity entity, string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			_game.Player.HandDiscard(entity, turn);
			Core.UpdatePlayerCards();
			GameEvents.OnPlayerHandDiscard.Execute(Database.GetCardFromId(cardId));
		}

		public void HandlePlayerPlay(Entity entity, string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			_game.Player.Play(entity, turn);
			Core.UpdatePlayerCards();
			GameEvents.OnPlayerPlay.Execute(Database.GetCardFromId(cardId));
			_game.SecretsManager.HandleCardPlayed(entity);
		}

		public void HandlePlayerDeckDiscard(Entity entity, string cardId, int turn)
		{
			_game.Player.DeckDiscard(entity, turn);
			DeckManager.DetectCurrentDeck().Forget();
			Core.UpdatePlayerCards();
			GameEvents.OnPlayerDeckDiscard.Execute(Database.GetCardFromId(cardId));
		}

		public void HandlePlayerPlayToDeck(Entity entity, string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			_game.Player.BoardToDeck(entity, turn);
			Core.UpdatePlayerCards();
			GameEvents.OnPlayerPlayToDeck.Execute(Database.GetCardFromId(cardId));
		}

		#endregion

		#region Opponent

		public void HandleOpponentGetToDeck(Entity entity, int turn)
		{
			_game.Opponent.CreateInDeck(entity, turn);
			Core.UpdateOpponentCards();
		}

		public void HandlePlayerRemoveFromPlay(Entity entity, int turn) => _game.Player.RemoveFromPlay(entity, turn);

		public void HandleOpponentRemoveFromPlay(Entity entity, int turn) => _game.Player.RemoveFromPlay(entity, turn);

		public void HandlePlayerCreateInSetAside(Entity entity, int turn) => _game.Player.CreateInSetAside(entity, turn);

		public void HandleOpponentCreateInSetAside(Entity entity, int turn) => _game.Opponent.CreateInSetAside(entity, turn);

		public void HandlePlayerPlayToGraveyard(Entity entity, string cardId, int turn)
		{
			_game.Player.PlayToGraveyard(entity, cardId, turn);
			GameEvents.OnPlayerPlayToGraveyard.Execute((Card)entity.Card.Clone());
		}

		public void HandleOpponentPlayToGraveyard(Entity entity, string cardId, int turn, bool playersTurn)
		{
			_game.Opponent.PlayToGraveyard(entity, cardId, turn);
			GameEvents.OnOpponentPlayToGraveyard.Execute((Card)entity.Card.Clone());
			if(playersTurn && entity.IsMinion)
				HandleOpponentMinionDeath(entity, turn);
		}

		public void HandlePlayerCreateInPlay(Entity entity, string cardId, int turn)
		{
			_game.Player.CreateInPlay(entity, turn);
			GameEvents.OnPlayerCreateInPlay.Execute(Database.GetCardFromId(cardId));
		}

		public void HandleOpponentCreateInPlay(Entity entity, string cardId, int turn)
		{
			_game.Opponent.CreateInPlay(entity, turn);
			GameEvents.OnOpponentCreateInPlay.Execute(Database.GetCardFromId(cardId));
		}

		public void HandlePlayerJoust(Entity entity, string cardId, int turn)
		{
			_game.Player.JoustReveal(entity, turn);
			Core.UpdatePlayerCards();
			GameEvents.OnPlayerJoustReveal.Execute(Database.GetCardFromId(cardId));
		}

		public void HandlePlayerDeckToPlay(Entity entity, string cardId, int turn)
		{
			_game.Player.DeckToPlay(entity, turn);
			Core.UpdatePlayerCards();
			GameEvents.OnPlayerDeckToPlay.Execute(Database.GetCardFromId(cardId));
		}

		public void HandleOpponentDeckToPlay(Entity entity, string cardId, int turn)
		{
			_game.Opponent.DeckToPlay(entity, turn);
			Core.UpdateOpponentCards();
			GameEvents.OnOpponentDeckToPlay.Execute(Database.GetCardFromId(cardId));
		}

		public void HandlePlayerRemoveFromDeck(Entity entity, int turn)
		{
			_game.Player.RemoveFromDeck(entity, turn);
			DeckManager.DetectCurrentDeck().Forget();
			Core.UpdatePlayerCards();
		}

		public void HandleOpponentRemoveFromDeck(Entity entity, int turn)
		{
			_game.Opponent.RemoveFromDeck(entity, turn);
			Core.UpdateOpponentCards();
		}

		public void HandlePlayerStolen(Entity entity, string cardId, int turn)
		{
			_game.Player.StolenByOpponent(entity, turn);
			_game.Opponent.StolenFromOpponent(entity, turn);
			if(entity.IsSecret)
			{
				HeroClass heroClass;
				var className = ((CardClass)entity.GetTag(CLASS)).ToString();
				if(!string.IsNullOrEmpty(className))
				{
					className = className.Substring(0, 1).ToUpper() + className.Substring(1, className.Length - 1).ToLower();
					if(!Enum.TryParse(className, out heroClass))
					{
						if(!Enum.TryParse(_game.Opponent.Class, out heroClass))
							return;
					}
				}
				else
				{
					if(!Enum.TryParse(_game.Opponent.Class, out heroClass))
						return;
				}
				_game.SecretsManager.NewSecret(entity);
			}
		}

		public void HandleOpponentStolen(Entity entity, string cardId, int turn)
		{
			_game.Opponent.StolenByOpponent(entity, turn);
			_game.Player.StolenFromOpponent(entity, turn);
			if(entity.IsSecret)
			{
				_game.SecretsManager.RemoveSecret(entity);
				Core.UpdateOpponentCards();
				GameEvents.OnOpponentSecretTriggered.Execute(Database.GetCardFromId(cardId));
			}
		}

		public void HandleOpponentPlay(Entity entity, string cardId, int from, int turn)
		{
			_game.Opponent.Play(entity, turn);
			Core.UpdateOpponentCards();
			GameEvents.OnOpponentPlay.Execute(Database.GetCardFromId(cardId));
		}


		public void HandleOpponentJoust(Entity entity, string cardId, int turn)
		{
			_game.Opponent.JoustReveal(entity, turn);
			Core.UpdateOpponentCards();
			GameEvents.OnOpponentJoustReveal.Execute(Database.GetCardFromId(cardId));
		}

		public void HandleOpponentHandDiscard(Entity entity, string cardId, int from, int turn)
		{
			_game.Opponent.HandDiscard(entity, turn);
			Core.UpdateOpponentCards();
			GameEvents.OnOpponentHandDiscard.Execute(Database.GetCardFromId(cardId));
		}

		public void HandlOpponentDraw(Entity entity, int turn)
		{
			_game.Opponent.Draw(entity, turn);
			GameEvents.OnOpponentDraw.Execute();
		}

		public void HandleOpponentMulligan(Entity entity, int from)
		{
			_game.Opponent.Mulligan(entity);
			GameEvents.OnOpponentMulligan.Execute();
		}

		public void HandleOpponentGet(Entity entity, int turn, int id)
		{
			if(!_game.IsMulliganDone && entity.GetTag(ZONE_POSITION) == 5)
				entity.CardId = HearthDb.CardIds.NonCollectible.Neutral.TheCoin;
			_game.Opponent.CreateInHand(entity, turn);
			Core.UpdateOpponentCards();
			GameEvents.OnOpponentGet.Execute();
		}

		public void HandleOpponentSecretPlayed(Entity entity, string cardId, int from, int turn, Zone fromZone, int otherId)
		{
			if(!entity.IsSecret)
			{
				if(entity.IsQuest)
				{
					_game.Opponent.QuestPlayedFromHand(entity, turn);
					GameEvents.OnOpponentPlay.Execute(Database.GetCardFromId(cardId));
				}
				return;
			}
			switch(fromZone)
			{
				case Zone.DECK:
					_game.Opponent.SecretPlayedFromDeck(entity, turn);
					break;
				case Zone.HAND:
					_game.Opponent.SecretPlayedFromHand(entity, turn);
					break;
				default:
					_game.Opponent.CreateInSecret(entity, turn);
					break;
			}

			HeroClass heroClass;
			var className = ((CardClass)entity.GetTag(CLASS)).ToString();
			if(!string.IsNullOrEmpty(className))
			{
				className = className.Substring(0, 1).ToUpper() + className.Substring(1, className.Length - 1).ToLower();
				if(!Enum.TryParse(className, out heroClass) && !Enum.TryParse(_game.Opponent.Class, out heroClass))
					return;
			}
			else if(!Enum.TryParse(_game.Opponent.Class, out heroClass))
				return;
			_game.SecretsManager.NewSecret(entity);
			GameEvents.OnOpponentPlay.Execute(Database.GetCardFromId(cardId));
		}

		public void HandleOpponentPlayToHand(Entity entity, string cardId, int turn, int id)
		{
			_game.Opponent.BoardToHand(entity, turn);
			Core.UpdateOpponentCards();
			GameEvents.OnOpponentPlayToHand.Execute(Database.GetCardFromId(cardId));
		}


		public void HandleOpponentPlayToDeck(Entity entity, string cardId, int turn)
		{
			_game.Opponent.BoardToDeck(entity, turn);
			Core.UpdateOpponentCards();
			GameEvents.OnOpponentPlayToDeck.Execute(Database.GetCardFromId(cardId));
		}

		public void HandleOpponentSecretTrigger(Entity entity, string cardId, int turn, int otherId)
		{
			if (!entity.IsSecret)
				return;
			_game.Opponent.SecretTriggered(entity, turn);
			_game.SecretsManager.RemoveSecret(entity);
			Core.UpdateOpponentCards();
			GameEvents.OnOpponentSecretTriggered.Execute(Database.GetCardFromId(cardId));
		}

		public void HandleOpponentDeckDiscard(Entity entity, string cardId, int turn)
		{
			_game.Opponent.DeckDiscard(entity, turn);

			//there seems to be an issue with the overlay not updating here.
			//possibly a problem with order of logs?
			Core.UpdateOpponentCards();
			GameEvents.OnOpponentDeckDiscard.Execute(Database.GetCardFromId(cardId));
		}

		#endregion

		#region IGameHandlerImplementation

		void IGameHandler.HandlePlayerBackToHand(Entity entity, string cardId, int turn) => HandlePlayerBackToHand(entity, cardId, turn);
		void IGameHandler.HandlePlayerDraw(Entity entity, string cardId, int turn) => HandlePlayerDraw(entity, cardId, turn);
		void IGameHandler.HandlePlayerMulligan(Entity entity, string cardId) => HandlePlayerMulligan(entity, cardId);
		void IGameHandler.HandlePlayerSecretPlayed(Entity entity, string cardId, int turn, Zone fromZone) => HandlePlayerSecretPlayed(entity, cardId, turn, fromZone);
		void IGameHandler.HandlePlayerHandDiscard(Entity entity, string cardId, int turn) => HandlePlayerHandDiscard(entity, cardId, turn);
		void IGameHandler.HandlePlayerPlay(Entity entity, string cardId, int turn) => HandlePlayerPlay(entity, cardId, turn);
		void IGameHandler.HandlePlayerDeckDiscard(Entity entity, string cardId, int turn) => HandlePlayerDeckDiscard(entity, cardId, turn);
		void IGameHandler.HandlePlayerHeroPower(string cardId, int turn) => HandlePlayerHeroPower(cardId, turn);
		void IGameHandler.HandleOpponentPlay(Entity entity, string cardId, int @from, int turn) => HandleOpponentPlay(entity, cardId, @from, turn);
		void IGameHandler.HandleOpponentHandDiscard(Entity entity, string cardId, int @from, int turn) => HandleOpponentHandDiscard(entity, cardId, @from, turn);
		void IGameHandler.HandleOpponentDraw(Entity entity, int turn) => HandlOpponentDraw(entity, turn);
		void IGameHandler.HandleOpponentMulligan(Entity entity, int @from) => HandleOpponentMulligan(entity, @from);
		void IGameHandler.HandleOpponentGet(Entity entity, int turn, int id) => HandleOpponentGet(entity, turn, id);
		void IGameHandler.HandleOpponentSecretPlayed(Entity entity, string cardId, int @from, int turn, Zone fromZone, int otherId) => HandleOpponentSecretPlayed(entity, cardId, @from, turn, fromZone, otherId);
		void IGameHandler.HandleOpponentPlayToHand(Entity entity, string cardId, int turn, int id) => HandleOpponentPlayToHand(entity, cardId, turn, id);
		void IGameHandler.HandleOpponentSecretTrigger(Entity entity, string cardId, int turn, int otherId) => HandleOpponentSecretTrigger(entity, cardId, turn, otherId);
		void IGameHandler.HandleOpponentDeckDiscard(Entity entity, string cardId, int turn) => HandleOpponentDeckDiscard(entity, cardId, turn);
		void IGameHandler.SetOpponentHero(string hero) => SetOpponentHero(hero);
		void IGameHandler.SetPlayerHero(string hero) => SetPlayerHero(hero);
		void IGameHandler.HandleOpponentHeroPower(string cardId, int turn) => HandleOpponentHeroPower(cardId, turn);
		void IGameHandler.TurnStart(ActivePlayer player, int turnNumber) => TurnStart(player, turnNumber);
		void IGameHandler.HandleGameStart(DateTime timestamp) => HandleGameStart(timestamp);
		void IGameHandler.HandleGameEnd() => HandleGameEnd();
		void IGameHandler.HandleLoss() => HandleLoss();
		void IGameHandler.HandleWin() => HandleWin();
		void IGameHandler.HandleTied() => HandleTied();
		void IGameHandler.HandlePlayerGet(Entity entity, string cardId, int turn) => HandlePlayerGet(entity, cardId, turn);
		void IGameHandler.HandlePlayerPlayToDeck(Entity entity, string cardId, int turn) => HandlePlayerPlayToDeck(entity, cardId, turn);
		void IGameHandler.HandleOpponentPlayToDeck(Entity entity, string cardId, int turn) => HandleOpponentPlayToDeck(entity, cardId, turn);
		void IGameHandler.HandlePlayerFatigue(int currentDamage) => HandlePlayerFatigue(currentDamage);
		void IGameHandler.HandleOpponentFatigue(int currentDamage) => HandleOpponentFatigue(currentDamage);

		#endregion IGameHandlerImplementation
	}
}
