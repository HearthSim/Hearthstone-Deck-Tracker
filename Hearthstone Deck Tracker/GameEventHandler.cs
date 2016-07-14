#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.LogReader;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using static Hearthstone_Deck_Tracker.Enums.GameMode;
using static HearthDb.Enums.GameTag;
using static Hearthstone_Deck_Tracker.Hearthstone.CardIds.Secrets;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class GameEventHandler : IGameHandler
	{
		private const int AvengeDelay = 50;
		private readonly GameV2 _game;
		private ArenaRewardDialog _arenaRewardDialog;
		private Deck _assignedDeck;

		private Entity _attackingEntity;
		private int _avengeDeathRattleCount;

		private bool _awaitingAvenge;
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

			if(_game.CurrentGameStats != null)
			{
				if(Config.Instance.RecordReplays && _game.Entities.Count > 0 && !_game.SavedReplay
				   && _game.CurrentGameStats.ReplayFile == null && RecordCurrentGameMode)
					_game.CurrentGameStats.ReplayFile = ReplayMaker.SaveToDisk(_game.PowerLog);

				if(_game.StoredGameStats != null)
					_game.CurrentGameStats.StartTime = _game.StoredGameStats.StartTime;

			}

			SaveAndUpdateStats();

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
									&& (x.IsMinion || x.IsSpell || x.IsWeapon)).GroupBy(x => x.CardId).Select(x =>
					{
						var card = Database.GetCardFromId(x.Key);
						card.Count = x.Count();
						return card;
					}));

			if(!Config.Instance.KeepDecksVisible)
				Core.Reset().Forget();
			GameEvents.OnInMenu.Execute();
		}

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
				_game.OpponentSecrets.ZeroFromAttack(_attackingEntity, _defendingEntity);
			OnAttackEvent();
		}

		public void HandleDefendingEntity(Entity entity)
		{
			_defendingEntity = entity;
			if(_attackingEntity == null || _defendingEntity == null)
				return;
			if(entity.IsControlledBy(_game.Opponent.Id))
				_game.OpponentSecrets.ZeroFromAttack(_attackingEntity, _defendingEntity);
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
			if(!Config.Instance.AutoGrayoutSecrets)
				return;

			_game.OpponentSecrets.SetZero(Hunter.Snipe);
			_game.OpponentSecrets.SetZero(Mage.MirrorEntity);
			_game.OpponentSecrets.SetZero(Paladin.Repentance);

			if(Core.MainWindow != null)
				Core.Overlay.ShowSecrets();
		}

		public void HandleOpponentMinionDeath(Entity entity, int turn)
		{
			if(!Config.Instance.AutoGrayoutSecrets)
				return;

			if(_game.Opponent.HandCount < 10)
				_game.OpponentSecrets.SetZero(Mage.Duplicate);

			var numDeathrattleMinions = 0;

			if(entity.IsActiveDeathrattle)
			{
				if(!CardIds.DeathrattleSummonCardIds.TryGetValue(entity.CardId ?? "", out numDeathrattleMinions))
				{
					if(entity.CardId == HearthDb.CardIds.Collectible.Neutral.Stalagg
					   && _game.Opponent.Graveyard.Any(x => x.CardId == HearthDb.CardIds.Collectible.Neutral.Feugen)
					   || entity.CardId == HearthDb.CardIds.Collectible.Neutral.Feugen
					   && _game.Opponent.Graveyard.Any(x => x.CardId == HearthDb.CardIds.Collectible.Neutral.Stalagg))
						numDeathrattleMinions = 1;
				}
				if(
					_game.Entities.Any(
					                   x =>
					                   x.Value.CardId == HearthDb.CardIds.NonCollectible.Druid.SouloftheForest_SoulOfTheForestEnchantment
					                   && x.Value.GetTag(ATTACHED) == entity.Id))
					numDeathrattleMinions++;
				if(
					_game.Entities.Any(
					                   x =>
					                   x.Value.CardId == HearthDb.CardIds.NonCollectible.Shaman.AncestralSpirit_AncestralSpiritEnchantment
					                   && x.Value.GetTag(ATTACHED) == entity.Id))
					numDeathrattleMinions++;
			}

			if(_game.OpponentEntity != null && _game.OpponentEntity.HasTag(EXTRA_DEATHRATTLES))
				numDeathrattleMinions *= (_game.OpponentEntity.GetTag(EXTRA_DEATHRATTLES) + 1);

			HandleAvengeAsync(numDeathrattleMinions);

			// redemption never triggers if a deathrattle effect fills up the board
			// effigy can trigger ahead of the deathrattle effect, but only if effigy was played before the deathrattle minion
			if(_game.OpponentMinionCount < 7 - numDeathrattleMinions)
			{
				_game.OpponentSecrets.SetZero(Paladin.Redemption);
				_game.OpponentSecrets.SetZero(Mage.Effigy);
			}
			else
			{
				// todo: need to properly break ties when effigy + deathrattle played in same turn
				var minionTurnPlayed = turn - entity.GetTag(NUM_TURNS_IN_PLAY);
				var secret = _game.OpponentSecrets.Secrets.FirstOrDefault(x => x.TurnPlayed >= minionTurnPlayed);
				var secretOffset = secret != null ? _game.OpponentSecrets.Secrets.IndexOf(secret) : 0;
				_game.OpponentSecrets.SetZeroOlder(Mage.Effigy, secretOffset);
			}

			if(Core.MainWindow != null)
				Core.Overlay.ShowSecrets();
		}

		public void HandleOpponentDamage(Entity entity)
		{
			if(!Config.Instance.AutoGrayoutSecrets)
				return;
			if(!entity.IsHero || !entity.IsControlledBy(_game.Opponent.Id))
				return;
			_game.OpponentSecrets.SetZero(Paladin.EyeForAnEye);
			if(Core.MainWindow != null)
				Core.Overlay.ShowSecrets();
		}

		public void HandleOpponentTurnStart(Entity entity)
		{
			if(!Config.Instance.AutoGrayoutSecrets)
				return;
			if(!entity.IsMinion)
				return;
			_game.OpponentSecrets.SetZero(Paladin.CompetitiveSpirit);
			if(Core.MainWindow != null)
				Core.Overlay.ShowSecrets();
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

			foreach(var impFavor in _game.Opponent.Board.Where(x => x.CardId == HearthDb.CardIds.NonCollectible.Neutral.EmperorThaurissan_ImperialFavorEnchantment))
			{
				Entity entity;
				if(_game.Entities.TryGetValue(impFavor.GetTag(ATTACHED), out entity))
					entity.Info.CostReduction += thaurissans.Count;
			}
		}

		public async void HandleAvengeAsync(int deathRattleCount)
		{
			_avengeDeathRattleCount += deathRattleCount;
			if(_awaitingAvenge)
				return;
			_awaitingAvenge = true;
			if(_game.OpponentMinionCount != 0)
			{
				await _game.GameTime.WaitForDuration(AvengeDelay);
				if(_game.OpponentMinionCount - _avengeDeathRattleCount > 0)
				{
					_game.OpponentSecrets.SetZero(Paladin.Avenge);
					if (Core.MainWindow != null)
						Core.Overlay.ShowSecrets();
				}
			}
			_awaitingAvenge = false;
			_avengeDeathRattleCount = 0;
		}

		public void HandleGameStart()
		{
			if(DateTime.Now - _lastGameStart < new TimeSpan(0, 0, 0, 5)) //game already started
				return;
			_handledGameEnd = false;
			_lastGameStart = DateTime.Now;
			Log.Info("--- Game start ---");

			if(Config.Instance.FlashHsOnTurnStart)
				User32.FlashHs();
			if(Config.Instance.BringHsToForeground)
				User32.BringHsToForeground();

			_arenaRewardDialog = null;
			_showedNoteDialog = false;
			_game.IsInMenu = false;
			_game.Reset();
			_game.CacheMatchInfo();
			TurnTimer.Instance.Start(_game).Forget();

			var selectedDeck = DeckList.Instance.ActiveDeckVersion;

			if(Config.Instance.SpectatorUseNoDeck && _game.CurrentGameMode == Spectator)
			{
				Log.Info("SpectatorUseNoDeck is enabled");
				if(selectedDeck != null)
				{
					Config.Instance.ReselectLastDeckUsed = true;
					Log.Info("ReselectLastUsedDeck set to true");
					Config.Save();
				}
				Core.MainWindow.SelectDeck(null, true);
			}
			else if(selectedDeck != null)
				_game.IsUsingPremade = true;
			Core.Windows.CapturableOverlay?.UpdateContentVisibility();
			GameEvents.OnGameStart.Execute();
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
				Log.Info("Game ended...");
				_game.InvalidateMatchInfoCache();
				if(_game.CurrentGameMode == Spectator && !Config.Instance.RecordSpectator)
				{
					if(Config.Instance.ReselectLastDeckUsed && DeckList.Instance.ActiveDeck == null)
					{
						Core.MainWindow.SelectLastUsedDeck();
						Config.Instance.ReselectLastDeckUsed = false;
						Log.Info("ReselectLastUsedDeck set to false");
						Config.Save();
					}
					Log.Info("Game is in Spectator mode, discarded. (Record Spectator disabled)");
					_assignedDeck = null;
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

				_game.CurrentGameStats.Turns = LogReaderManager.GetTurnNumber();
				if(Config.Instance.DiscardZeroTurnGame && _game.CurrentGameStats.Turns < 1)
				{
					Log.Info("Game has 0 turns, discarded. (DiscardZeroTurnGame)");
					_assignedDeck = null;
					GameEvents.OnGameEnd.Execute();
					return;
				}
				_game.CurrentGameStats.GameMode = _game.CurrentGameMode;
				if(_game.CurrentGameMode == Ranked || _game.CurrentGameMode == Casual)
				{
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
				}
				_game.CurrentGameStats.PlayerCardbackId = _game.MatchInfo?.LocalPlayer.CardBackId ?? 0;
				_game.CurrentGameStats.OpponentCardbackId = _game.MatchInfo?.OpposingPlayer.CardBackId ?? 0;
				_game.CurrentGameStats.FriendlyPlayerId = _game.MatchInfo?.LocalPlayer.Id ?? 0;
				_game.CurrentGameStats.ScenarioId = _game.MatchInfo?.MissionId ?? 0;
				_game.CurrentGameStats.SetPlayerCards(DeckList.Instance.ActiveDeckVersion, _game.Player.RevealedCards.ToList());
				_game.CurrentGameStats.SetOpponentCards(_game.Opponent.OpponentCardList.Where(x => !x.IsCreated).ToList());
				_game.CurrentGameStats.GameEnd();
				GameEvents.OnGameEnd.Execute();
				Influx.OnGameEnd(HearthDbConverter.GetGameType(_game.CurrentGameStats.GameMode, _game.CurrentGameStats.Format));
				var selectedDeck = DeckList.Instance.ActiveDeck;
				if(selectedDeck != null)
				{
					var revealed = _game.Player.RevealedEntities.Where(x => x != null).ToList();
					if(Config.Instance.DiscardGameIfIncorrectDeck
					   && !revealed.Where(x => (x.IsMinion || x.IsSpell || x.IsWeapon) && !x.Info.Created && !x.Info.Stolen)
					   .GroupBy(x => x.CardId).All(x => selectedDeck.GetSelectedDeckVersion().Cards.Any(c2 => x.Key == c2.Id && x.Count() <= c2.Count)))
					{
						if(Config.Instance.AskBeforeDiscardingGame)
						{
							var discardDialog = new DiscardGameDialog(_game.CurrentGameStats) {Topmost = true};
							discardDialog.ShowDialog();
							if(discardDialog.Result == DiscardGameDialogResult.Discard)
							{
								Log.Info("Assigned current game to NO deck - selected deck does not match cards played (dialogresult: discard)");
								_game.CurrentGameStats.DeleteGameFile();
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
									_game.CurrentGameStats.HearthStatsDeckVersionId = targetDeck.GetVersion(moveDialog.SelectedVersion).HearthStatsDeckVersionId;
									//...continue as normal
								}
								else
								{
									Log.Info("No deck selected in move game dialog after discard dialog, discarding game");
									_game.CurrentGameStats.DeleteGameFile();
									_assignedDeck = null;
									return;
								}
							}
						}
						else
						{
							Log.Info("Assigned current game to NO deck - selected deck does not match cards played (no dialog)");
							_game.CurrentGameStats.DeleteGameFile();
							_assignedDeck = null;
							return;
						}
					}
					else
					{
						_game.CurrentGameStats.PlayerDeckVersion = selectedDeck.GetSelectedDeckVersion().Version;
						_game.CurrentGameStats.HearthStatsDeckVersionId = selectedDeck.GetSelectedDeckVersion().HearthStatsDeckVersionId;
					}

					_lastGame = _game.CurrentGameStats;
					selectedDeck.DeckStats.AddGameResult(_lastGame);

					var isArenaRunCompleted = selectedDeck.IsArenaRunCompleted.HasValue && selectedDeck.IsArenaRunCompleted.Value;
					if(Config.Instance.ArenaRewardDialog && isArenaRunCompleted)
						_arenaRewardDialog = new ArenaRewardDialog(selectedDeck);

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

					if (Config.Instance.AutoArchiveArenaDecks && isArenaRunCompleted)
						Core.MainWindow.ArchiveDeck(selectedDeck, true);

					if(HearthStatsAPI.IsLoggedIn && Config.Instance.HearthStatsAutoUploadNewGames)
					{
						Log.Info("Waiting for game mode to be saved to game...");
						await GameModeSaved(15);
						Log.Info("Game mode was saved, continuing.");
						if(_game.CurrentGameMode == Arena)
							HearthStatsManager.UploadArenaMatchAsync(_lastGame, selectedDeck, background: true);
						else if(_game.CurrentGameMode != Brawl)
							HearthStatsManager.UploadMatchAsync(_lastGame, selectedDeck, background: true);
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

				if(Config.Instance.ReselectLastDeckUsed && selectedDeck == null)
				{
					Core.MainWindow.SelectLastUsedDeck();
					Config.Instance.ReselectLastDeckUsed = false;
					Log.Info("ReselectLastUsedDeck set to false");
					Config.Save();
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
				if(Config.Instance.ShowGameResultNotifications && !Config.Instance.GameResultNotificationsUnexpectedOnly)
				{
					var deckName = _assignedDeck == null ? "No deck - " + _game.CurrentGameStats.PlayerHero : _assignedDeck.NameAndVersion;
					new GameResultNotificationWindow(deckName, _game.CurrentGameStats).Show();
				}
				if(Config.Instance.ShowNoteDialogAfterGame && Config.Instance.NoteDialogDelayed && !_showedNoteDialog)
				{
					_showedNoteDialog = true;
					new NoteDialog(_game.CurrentGameStats);
				}

				if(_game.CurrentGameStats != null)
				{
					_game.CurrentGameStats.Turns = LogReaderManager.GetTurnNumber();
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
			GameEvents.OnPlayerHeroPower.Execute();

			if(!Config.Instance.AutoGrayoutSecrets)
				return;
			_game.OpponentSecrets.SetZero(Hunter.DartTrap);

			if(Core.MainWindow != null)
				Core.Overlay.ShowSecrets();
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
			switch(fromZone)
			{
				case Zone.DECK:
					_game.Player.SecretPlayedFromDeck(entity, turn);
					DeckManager.DetectCurrentDeck().Forget();
					break;
				case Zone.HAND:
					_game.Player.SecretPlayedFromHand(entity, turn);
					HandleSecretsOnPlay(entity);
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
			HandleSecretsOnPlay(entity);
		}

		public async void HandleSecretsOnPlay(Entity entity)
		{
			if(!Config.Instance.AutoGrayoutSecrets)
				return;
			if(entity.IsSpell)
			{
				_game.OpponentSecrets.SetZero(Mage.Counterspell);

				if(_game.OpponentMinionCount < 7)
				{
					//CARD_TARGET is set after ZONE, wait for 50ms gametime before checking
					await _game.GameTime.WaitForDuration(50);
					Entity target;
					if(entity.HasTag(CARD_TARGET) && _game.Entities.TryGetValue(entity.GetTag(CARD_TARGET), out target) && target.IsMinion)
						_game.OpponentSecrets.SetZero(Mage.Spellbender);
				}

				if(Core.MainWindow != null)
					Core.Overlay.ShowSecrets();
			}
			else if(entity.IsMinion && _game.PlayerMinionCount > 3)
			{
				_game.OpponentSecrets.SetZero(Paladin.SacredTrial);

				if(Core.MainWindow != null)
					Core.Overlay.ShowSecrets();
			}
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
				_game.OpponentSecretCount++;
				_game.OpponentSecrets.NewSecretPlayed(heroClass, entity.Id, turn, cardId);
				Core.Overlay.ShowSecrets();
			}
		}

		public void HandleOpponentStolen(Entity entity, string cardId, int turn)
		{
			_game.Opponent.StolenByOpponent(entity, turn);
			_game.Player.StolenFromOpponent(entity, turn);
			if(entity.IsSecret)
			{
				_game.OpponentSecretCount--;
				_game.OpponentSecrets.SecretRemoved(entity.Id, cardId);
				if(_game.OpponentSecretCount <= 0)
					Core.Overlay.HideSecrets();
				else
				{
					if(Config.Instance.AutoGrayoutSecrets)
						_game.OpponentSecrets.SetZero(cardId);
					Core.Overlay.ShowSecrets();
				}
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
			_game.OpponentSecretCount++;
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
			
			_game.OpponentSecrets.NewSecretPlayed(heroClass, otherId, turn);

			if(Core.MainWindow != null)
				Core.Overlay.ShowSecrets();
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
			_game.Opponent.SecretTriggered(entity, turn);
			_game.OpponentSecretCount--;
			_game.OpponentSecrets.SecretRemoved(otherId, cardId);

			if(_game.OpponentSecretCount <= 0)
				Core.Overlay.HideSecrets();
			else
			{
				if(Config.Instance.AutoGrayoutSecrets)
					_game.OpponentSecrets.SetZero(cardId);
				Core.Overlay.ShowSecrets();
			}
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
		void IGameHandler.HandleGameStart() => HandleGameStart();
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