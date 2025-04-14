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
using Hearthstone_Deck_Tracker.BobsBuddy;
using Hearthstone_Deck_Tracker.Utility.Battlegrounds;
using ControlzEx.Standard;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPicking;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Minions;
using Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan;
using Hearthstone_Deck_Tracker.Hearthstone.CardExtraInfo;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility.Exceptions;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Newtonsoft.Json;
using HSReplay.Requests;
using HSReplay.Responses;
using static HSReplay.Responses.MulliganGuideData;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class GameEventHandler : IGameHandler
	{
		private readonly GameV2 _game;
		private ArenaRewardDialog? _arenaRewardDialog;
		private Deck? _assignedDeck;

		private Entity? _attackingEntity;
		private Entity? _defendingEntity;
		private bool _handledGameEnd;
		private GameStats? _lastGame;
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
											 || _game.CurrentGameMode == Spectator && Config.Instance.RecordSpectator
											 || _game.CurrentGameMode == Duels && Config.Instance.RecordDuels;

		public bool UploadCurrentGameMode => _game.CurrentGameMode == Practice && Config.Instance.HsReplayUploadPractice
											 || _game.CurrentGameMode == Arena && Config.Instance.HsReplayUploadArena
											 || _game.CurrentGameMode == Brawl && Config.Instance.HsReplayUploadBrawl
											 || _game.CurrentGameMode == Ranked && Config.Instance.HsReplayUploadRanked
											 || _game.CurrentGameMode == Friendly && Config.Instance.HsReplayUploadFriendly
											 || _game.CurrentGameMode == Casual && Config.Instance.HsReplayUploadCasual
											 || _game.CurrentGameMode == Spectator && Config.Instance.HsReplayUploadSpectator
											 || _game.IsBattlegroundsMatch && Config.Instance.HsReplayUploadBattlegrounds
											 || _game.CurrentGameMode == Duels && Config.Instance.HsReplayUploadDuels
											 || _game.IsMercenariesMatch && Config.Instance.HsReplayUploadMercenaries;

		public void HandleInMenu()
		{
			if(_game.IsInMenu)
				return;

			if(!_handledGameEnd)
			{
				Log.Warn("Game end has not been handled");
				HandleGameEnd(false);
			}

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
				DeckList.Instance.ActiveDeck.Archive(true);

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

			if(_game.CurrentGameStats != null && _game.CurrentGameStats.GameMode == Battlegrounds)
			{
				Core.Game.BattlegroundsSessionViewModel.Update();
				Core.Overlay.BattlegroundsCompsGuidesVM.Reset();
			}

			if(!_game.IsUsingPremade)
				_game.DrawnLastGame =
					new List<Card>(_game.Player.RevealedEntities.Where(x => !x.Info.Created && !x.Info.Stolen && x.Card.Collectible
									&& x.IsPlayableCard).GroupBy(x => x.CardId).Select(x =>
					{
						var card = Database.GetCardFromId(x.Key);
						if(card == null)
							return null;
						card.Count = x.Count();
						return card;
					}).WhereNotNull());

			if(!Config.Instance.KeepDecksVisible)
				Core.Reset().Forget();

			Core.Overlay.HideLinkOpponentDeckDisplay();

			GameEvents.OnInMenu.Execute();
		}

		private bool _savedReplay;
		private async Task SaveReplays(GameStats gs)
		{
			if(gs == null || _savedReplay)
				return;
			_savedReplay = true;

			var complete = await LogIsComplete();

			if(complete)
			{
				switch(gs.GameMode)
				{
					case Ranked:
						await UpdatePostGameRanks(gs);
						break;
					case Battlegrounds:
						await UpdatePostGameBattlegroundsRating(gs);
						break;
					case Mercenaries:
						if(gs.MercenariesRating != 0)
							await UpdatePostGameMercenariesRating(gs);
						await UpdatePostGameMercenariesRewards(gs);
						break;
				}
			}

			var powerLog = new List<string>();
			foreach(var stored in _game.StoredPowerLogs.Where(x => x.Item1 == _game.MetaData.ServerInfo?.GameHandle))
				powerLog.AddRange(stored.Item2);
			powerLog.AddRange(_game.PowerLog);

			var createGameCount = 0;
			powerLog = powerLog.TakeWhile(x => !(x.Contains("CREATE_GAME") && createGameCount++ == 1)).ToList();

			if(Config.Instance.RecordReplays && RecordCurrentGameMode && _game.Entities.Count > 0 && !_game.SavedReplay && gs.ReplayFile == null)
				gs.ReplayFile = ReplayMaker.SaveToDisk(gs, powerLog);

			if(Config.Instance.HsReplayAutoUpload && UploadCurrentGameMode)
			{
				var log = powerLog.ToArray();
				var validationResult = LogValidator.Validate(log);
				if(validationResult.IsValid)
					await LogUploader.Upload(log, (GameMetaData)_game.MetaData.Clone(), gs);
				else
				{
					Log.Error("Invalid log: " + validationResult.Reason);
					Influx.OnEndOfGameUploadError(validationResult.Reason);
				}
			}
		}

		private async Task<bool> LogIsComplete()
		{
			if(LogContainsStateComplete)
				return true;
			Log.Info("Waiting for STATE COMPLETE...");
			await Task.Delay(500);
			if(LogContainsStateComplete || _game.IsInMenu)
				return LogContainsStateComplete;
			Log.Info("STATE COMPLETE not found");
			for(var i = 0; i < 5; i++)
			{
				await Task.Delay(1000);
				if(LogContainsStateComplete || _game.IsInMenu)
					break;
				Log.Info($"Waiting for STATE COMPLETE some more... ({i})");
			}
			return LogContainsStateComplete;
		}

		private async Task UpdatePostGameRanks(GameStats gs)
		{
			var medalInfo = await Helper.RetryWhileNull(Reflection.Client.GetMedalInfo);
			if(medalInfo == null)
			{
				Log.Warn("Could not get MedalInfo");
				return;
			}
			var data = gs.Format switch
			{
				Format.Wild => medalInfo.Wild,
				Format.Classic => medalInfo.Classic,
				Format.Twist => medalInfo.Twist,
				_ => medalInfo.Standard,
			};
			gs.StarsAfter = data.Stars;
			gs.StarLevelAfter = data.StarLevel;
			gs.LegendRankAfter = data.LegendRank;
		}

		private async Task UpdatePostGameBattlegroundsRating(GameStats gs)
		{
			var data = await Helper.RetryWhileNull(Reflection.Client.GetBaconRatingChangeData);
			if(data == null)
			{
				Log.Warn("Could not get battlegrounds rating");
				return;
			}
			gs.BattlegroundsRatingAfter = data.NewRating;
		}

		private async Task UpdatePostGameMercenariesRating(GameStats gs)
		{
			var data = await Helper.RetryWhileNull(Reflection.Client.GetMercenariesRatingChangeData);
			if(data == null)
			{
				Log.Warn("Could not get mercenaries rating");
				return;
			}
			gs.MercenariesRatingAfter = data.NewRating;
		}

		private async Task UpdatePostGameMercenariesRewards(GameStats gs)
		{
			for(var i = 0; i < 5; i++)
			{
				var delta = MercenariesCoins.Update();
				if(delta.Count > 0)
				{
					gs.MercenariesBountyRunRewards = delta;
					break;
				}
				await Task.Delay(250);
			}
		}

		private bool LogContainsStateComplete
			=> _game?.PowerLog?.Any(x => x.Contains("tag=STATE value=COMPLETE")) ?? false;

		public void HandleConcede()
		{
			if(_game.CurrentGameStats != null)
				_game.CurrentGameStats.WasConceded = true;
		}

		public void HandleAttackingEntity(Entity? entity)
		{
			_attackingEntity = entity;
			if(_attackingEntity == null || _defendingEntity == null)
				return;
			if(entity!.IsControlledBy(_game.Player.Id))
				_game.SecretsManager.HandleAttack(_attackingEntity, _defendingEntity);
			OnAttackEvent();
		}

		public void HandleDefendingEntity(Entity? entity)
		{
			_defendingEntity = entity;
			if(_attackingEntity == null || _defendingEntity == null)
				return;
			if(entity!.IsControlledBy(_game.Opponent.Id))
				_game.SecretsManager.HandleAttack(_attackingEntity, _defendingEntity);
			OnAttackEvent();
		}

		private void OnAttackEvent()
		{
			if(_attackingEntity == null || _defendingEntity == null)
				return;

			if(_game.IsBattlegroundsMatch && Config.Instance.RunBobsBuddy && _game.CurrentGameStats != null)
			{
				BobsBuddyInvoker.GetInstance(_game.CurrentGameStats.GameId, _game.GetTurnNumber())?
					.UpdateAttackingEntities(_attackingEntity, _defendingEntity);
			}
			var attackInfo = new AttackInfo((Card)_attackingEntity.Card.Clone(), (Card)_defendingEntity.Card.Clone());
			if(_attackingEntity.IsControlledBy(_game.Player.Id))
				GameEvents.OnPlayerMinionAttack.Execute(attackInfo);
			else
				GameEvents.OnOpponentMinionAttack.Execute(attackInfo);
		}

		public void HandlePlayerMinionPlayed(Entity entity)
		{
			_game.SecretsManager.HandleMinionPlayed(entity);
		}

		public void HandleOpponentMinionDeath(Entity entity, int turn)
		{
			_game.SecretsManager.HandleOpponentMinionDeath(entity);
		}

		public void HandlePlayerMinionDeath(Entity entity)
		{
			_game.SecretsManager.HandlePlayerMinionDeath(entity);
		}

		public void HandleEntityPredamage(Entity entity, int value)
		{
			GameEvents.OnEntityWillTakeDamage.Execute(new PredamageInfo(entity, value));
		}

		public void HandleEntityDamage(Entity dealer, Entity target, int value)
		{
			if(_game.PlayerEntity?.IsCurrentPlayer ?? false)
			{
				_game.SecretsManager.HandleEntityDamageAsync(dealer, target, value);
			}
		}

		public void HandleEntityLostArmor(Entity target, int value)
		{
			if(_game.PlayerEntity?.IsCurrentPlayer ?? false)
			{
				_game.SecretsManager.HandleEntityLostArmor(target, value);
			}
		}

		public void HandleProposedAttackerChange(Entity entity)
		{
			if(_game.IsBattlegroundsMatch && Config.Instance.RunBobsBuddy && _game.CurrentGameStats != null)
			{
				BobsBuddyInvoker.GetInstance(_game.CurrentGameStats.GameId, _game.GetTurnNumber())?.HandleNewAttackingEntity(entity);
			}
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

		public void SetOpponentHero(string? cardId)
		{
			var heroName = Database.GetHeroNameFromId(cardId);
			if(string.IsNullOrEmpty(heroName))
				return;
			_game.Opponent.OriginalClass = heroName;
			_game.Opponent.CurrentClass = heroName;
			if(_game.CurrentGameStats != null)
			{
				_game.CurrentGameStats.OpponentHero = heroName;
				_game.CurrentGameStats.OpponentHeroCardId = cardId;
				var hero = Database.GetCardFromId(cardId);
				if (hero != null)
					_game.CurrentGameStats.OpponentHeroClasses = hero.GetClasses().ToArray();
			}
			Log.Info("Opponent=" + heroName);
		}

		public void SetPlayerHero(string? cardId)
		{
			var heroName = Database.GetHeroNameFromId(cardId);
			if(string.IsNullOrEmpty(heroName))
				return;
			_game.Player.OriginalClass = heroName;
			_game.Player.CurrentClass = heroName;
			if(_game.CurrentGameStats != null)
			{
				_game.CurrentGameStats.PlayerHero = heroName;
				_game.CurrentGameStats.PlayerHeroCardId = cardId;
				var hero = Database.GetCardFromId(cardId);
				if (hero != null)
					_game.CurrentGameStats.PlayerHeroClasses = hero.GetClasses().ToArray();
			}
			Log.Info("Player=" + heroName);
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
			{
				HandleOpponentEndOfTurn(turn.Item2 - 1);
				_game.Opponent.OnTurnEnd();
				_game.SecretsManager.HandlePlayerTurnStart();
			}
			else if (player == ActivePlayer.Opponent)
			{
				HandlePlayerEndOfTurn(turn.Item2 - 1);
				_game.Player.OnTurnEnd();
				_game.SecretsManager.HandleOpponentTurnStart();

			}
			GameEvents.OnTurnStart.Execute(player);
			if(_turnQueue.Count > 0)
				return;
			TurnTimer.Instance.SetPlayer(player);
			if(player == ActivePlayer.Player && !_game.IsInMenu)
			{
				// Clear some state that should never be active at the start of a turn in case another hiding mechanism fails
				Core.Overlay.HideMulliganGuideStats();
				Core.Game.Player.MulliganCardStats = null;
				Core.Overlay.BattlegroundsHeroPickingViewModel.Reset();
				Core.Overlay.ResetAnomalyGuidesMulliganTrigger();
				Core.Overlay.BattlegroundsQuestPickingViewModel.Reset();
				Core.Overlay.BattlegroundsTrinketPickingViewModel.Reset();
				Core.Overlay.HideBattlegroundsHeroPanel();
				Core.Overlay.BattlegroundsSessionViewModelVM.Update();

				if(_game.IsBattlegroundsMatch)
				{
					_game.PrimaryPlayerId = _game.Player.Id;
					OpponentDeadForTracker.ShoppingStarted(_game);
					if(_game.CurrentGameStats != null && turn.Item2 > 1)
						BobsBuddyInvoker.GetInstance(_game.CurrentGameStats.GameId, turn.Item2 - 1)?.StartShoppingAsync();
					Core.Overlay.BattlegroundsMinionsVM.OnHeroPowers(_game.Player.Board.Where(x => x.IsHeroPower).Select(x => x.Card.Id));
					Core.Overlay.BattlegroundsMinionsVM.OnTrinkets(Core.Game.Player.Trinkets.Select(x => x.Card.Id));
					Core.Overlay.BattlegroundsInspirationViewModel.OnShoppingStart();
				}
				switch(Config.Instance.TurnStartAction)
				{
						case HsActionType.Flash:
							User32.FlashHs();
							break;
						case HsActionType.Popup:
							User32.BringHsToForeground();
							break;
				}
			}
			else if(player == ActivePlayer.Opponent && !_game.IsInMenu && _game.IsBattlegroundsSoloMatch)
			{
				Core.Overlay.BattlegroundsInspirationViewModel.OnShoppingEnd();
			}
			Core.Overlay.TurnCounter.UpdateTurn(turn.Item2);
		}

		private void HandlePlayerEndOfTurn(int turn)
		{
			HandleIncindiusEndOfTurn(isOpponent: false, turn);
		}

		private void HandleOpponentEndOfTurn(int turn)
		{
			HandleThaurissanCostReduction();
			HandleIncindiusEndOfTurn(isOpponent: true, turn);
		}

		private void HandleThaurissanCostReduction()
		{
			var thaurissans = _game.Opponent.Board.Where(x =>
				x.CardId is HearthDb.CardIds.Collectible.Neutral.EmperorThaurissanFP2 or HearthDb.CardIds.Collectible.Neutral.EmperorThaurissanWONDERS
				&& !x.HasTag(SILENCED)).ToList();
			if(!thaurissans.Any())
				return;

			HandleOpponentHandCostReduction(thaurissans.Count);
		}

		private void HandleIncindiusEndOfTurn(bool isOpponent, int turn)
		{
			var player = isOpponent ? _game.Opponent : _game.Player;
			var incindiusEntities = player.Board.Where(x =>
				x.CardId is HearthDb.CardIds.Collectible.Neutral.Incindius
				&& !x.HasTag(SILENCED)).ToList();
			if(!incindiusEntities.Any())
				return;

			var eruptions = player.Deck.Where(x =>
				x.CardId is HearthDb.CardIds.NonCollectible.Neutral.Incindius_EruptionToken).ToArray();

			foreach(var _ in incindiusEntities)
			{
				foreach(var entity in eruptions)
				{
					if(entity.Info.ExtraInfo is IncindiusCounter counter)
					{
						counter.Counter += 1;
					}
					else
					{
						entity.Info.ExtraInfo = new IncindiusCounter(turn);
					}
				}
			}

			if(isOpponent)
			{
				Core.UpdateOpponentCards();
			}
			else
			{
				Core.UpdatePlayerCards();
			}
		}

		private DateTime _lastGameStartTimestamp = DateTime.MinValue;
		public void HandleGameStart(DateTime timestamp)
		{
			_game.InvalidateMatchInfoCache();
			if(_game.CurrentGameMode == Practice && !_game.IsInMenu && !_handledGameEnd
				&& _lastGameStartTimestamp > DateTime.MinValue && timestamp > _lastGameStartTimestamp)
				HandleAdventureRestart();
			_lastGameStartTimestamp = timestamp;
			if(DateTime.Now - _lastGameStart < new TimeSpan(0, 0, 0, 5)) //game already started
				return;
			_handledGameEnd = false;
			_lastGameStart = DateTime.Now;
			Log.Info("--- Game start ---");

			switch(Config.Instance.TurnStartAction)
			{
				case HsActionType.Flash:
					User32.FlashHs();
					break;
				case HsActionType.Popup:
					User32.BringHsToForeground();
					break;
			}
			_lastTurnStart[0] = _lastTurnStart[1] = 0;
			_arenaRewardDialog = null;
			_showedNoteDialog = false;
			_game.IsInMenu = false;
			_savedReplay = false;
			_game.Reset();
			_game.CacheMatchInfo();
			_game.CacheGameType();
			_game.CacheSpectator();

			_game.MetaData.ServerInfo = Reflection.Client.GetServerInfo();
			TurnTimer.Instance.Start(_game).Forget();

			var selectedDeck = DeckList.Instance.ActiveDeckVersion;

			if(selectedDeck != null)
				_game.IsUsingPremade = true;
			Core.Windows.CapturableOverlay?.UpdateContentVisibility();
			GameEvents.OnGameStart.Execute();
			LiveDataManager.WatchBoardState();

			_game.CounterManager.Reset();

			Core.Overlay.LinkOpponentDeckDisplay.IsFriendlyMatch = _game.IsFriendlyMatch;

			if(_game.IsBattlegroundsMatch && _game.CurrentGameMode == GameMode.Spectator)
			{
				Core.Overlay.ShowBgsTopBarAndBobsBuddyPanel();
				Core.Overlay.Tier7PreLobbyViewModel.Reset();
			}
			if(_game.IsFriendlyMatch)
				if(!Config.Instance.InteractedWithLinkOpponentDeck)
					Core.Overlay.ShowLinkOpponentDeckDisplay();

			if(_game.IsMercenariesPveMatch)
			{
				// Called here so that UpdatePostGameMercenariesRewards can generate an accurate delta.
				MercenariesCoins.Update();
			}
			if(_game.IsBattlegroundsMatch)
			{
				Core.Overlay.BattlegroundsSessionViewModelVM.Update();
				Core.Overlay.BattlegroundsHeroGuideListViewModel.Update();
				Core.Overlay.BattlegroundsAnomalyGuideListViewModel.Update();
				Core.Overlay.BattlegroundsSessionViewModelVM.UpdateCompositionStatsVisibility();
			}
		}

		private DateTime _lastReconnectStartTimestamp = DateTime.MinValue;
		public async void HandleGameReconnect(DateTime timestamp)
		{
			Log.Info("Joined after mulligan, assuming reconnect.");

			if(DateTime.Now - _lastReconnectStartTimestamp < new TimeSpan(0, 0, 0, 5)) // game already started
				return;
			_lastReconnectStartTimestamp = timestamp;

			for(var i = 0; i < 20 && (_game.GameEntity is null || _game.CurrentMode != Mode.GAMEPLAY); i++)
				await Task.Delay(500);

			if(_game.GameEntity is null || _game.CurrentMode != Mode.GAMEPLAY)
				return;

			if(_game.IsBattlegroundsMatch)
			{
				if(_game.GameEntity?.GetTag(STEP) > (int)Step.BEGIN_MULLIGAN)
				{
					Core.Overlay.ShowBgsTopBarAndBobsBuddyPanel();
					Core.Overlay.BattlegroundsSessionViewModelVM.Update();
					Core.Overlay.BattlegroundsHeroGuideListViewModel.Update();
					Core.Overlay.BattlegroundsAnomalyGuideListViewModel.Update();
					Watchers.BattlegroundsLeaderboardWatcher.Run();
					if(_game.IsBattlegroundsDuosMatch)
						Watchers.BattlegroundsTeammateBoardStateWatcher.Run();
				}
			}
		}

		private void HandleAdventureRestart()
		{
			//The game end is not logged in PowerTaskList
			Log.Info("Adventure was restarted. Simulating game end.");
			HandleConcede();
			HandleLoss();
			HandleGameEnd(false);
			HandleInMenu();
		}

#pragma warning disable 4014
		public async void HandleGameEnd(bool stateComplete)
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
				Core.Overlay.HideMercenariesGameOverlay();
				DeckManager.ResetAutoSelectCount();
				LiveDataManager.Stop();
				if(_game.IsBattlegroundsMatch && stateComplete)
				{
					BobsBuddyInvoker.GetInstance(_game.CurrentGameStats.GameId, _game.GetTurnNumber())?.StartShoppingAsync(true);
					OpponentDeadForTracker.Reset();
				}
				if(_game.IsConstructedMatch)
				{
					Core.Overlay.HideMulliganToast(false);
					Core.Game.Player.MulliganCardStats = null;
					Core.Overlay.HideMulliganGuideStats();
				}
				Log.Info("Game ended...");
				_game.InvalidateMatchInfoCache();
				if(_game.CurrentGameMode == Spectator && _game.CurrentGameStats.Result == GameResult.None)
				{
					Log.Info("Game was spectator mode without a game result. Probably exited spectator mode early.");
					Sentry.ClearBobsBuddyEvents();
					return;
				}
				var player = _game.Entities.FirstOrDefault(e => e.Value?.IsPlayer ?? false).Value;
				var opponent = _game.Entities.FirstOrDefault(e => e.Value != null && e.Value.HasTag(PLAYER_ID) && !e.Value.IsPlayer).Value;
				if(player != null)
				{
					_game.CurrentGameStats.PlayerName = player.Name;
					_game.CurrentGameStats.Coin = !player.HasTag(FIRST_PLAYER);
				}
				var oppHero = _game.CurrentGameStats.OpponentHero;
				if(oppHero != null)
				{
					if(opponent != null && CardIds.HeroIdDict.ContainsValue(oppHero))
						_game.CurrentGameStats.OpponentName = opponent.Name;
					else
						_game.CurrentGameStats.OpponentName = oppHero;
				}

				_game.CurrentGameStats.Turns = _game.GetTurnNumber();
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
					var isWild = _game.CurrentFormat == Format.Wild;
					var isClassic = _game.CurrentFormat == Format.Classic;
					var isTwist = _game.CurrentFormat == Format.Twist;

					var localPlayer = _game.MatchInfo.LocalPlayer;
					var opposingPlayer = _game.MatchInfo.OpposingPlayer;

					var playerInfo =  isClassic ? localPlayer.Classic : isWild ? localPlayer.Wild : isTwist ? localPlayer.Twist : localPlayer.Standard;
					var opponentInfo = isClassic ? opposingPlayer.Classic : isWild ? opposingPlayer.Wild : isTwist ? opposingPlayer.Twist : opposingPlayer.Standard;

					_game.CurrentGameStats.LeagueId = playerInfo?.LeagueId ?? 0;
					if (playerInfo?.LeagueId < 5)
					{
						_game.CurrentGameStats.Rank = isClassic ? localPlayer.ClassicRank : isWild ? localPlayer.WildRank : isTwist ? localPlayer.TwistRank : localPlayer.StandardRank;
						_game.CurrentGameStats.OpponentRank = isClassic ? opposingPlayer.ClassicRank : isWild ? opposingPlayer.WildRank : isTwist ? localPlayer.TwistRank : opposingPlayer.StandardRank;
					}
					_game.CurrentGameStats.StarLevel = playerInfo?.StarLevel ?? 0;
					_game.CurrentGameStats.StarMultiplier = playerInfo?.StarsPerWin ?? 0;
					_game.CurrentGameStats.Stars = playerInfo?.Stars ?? 0;
					_game.CurrentGameStats.OpponentStarLevel = opponentInfo?.StarLevel ?? 0;
					_game.CurrentGameStats.LegendRank = playerInfo?.LegendRank ?? 0;
					_game.CurrentGameStats.OpponentLegendRank = opponentInfo?.LegendRank ?? 0;
				}
				else if(_game.CurrentGameMode == Arena)
				{
					_game.CurrentGameStats.ArenaWins = DeckImporter.ArenaInfoCache?.Wins ?? 0;
					_game.CurrentGameStats.ArenaLosses = DeckImporter.ArenaInfoCache?.Losses ?? 0;
				}
				else if(_game.CurrentGameMode == Brawl)
				{
					var brawlInfo = _game.BrawlInfo;
					if(brawlInfo != null)
					{
						_game.CurrentGameStats.BrawlWins = brawlInfo.Wins;
						_game.CurrentGameStats.BrawlLosses = brawlInfo.Losses;
					}
				}
				else if (_game.IsBattlegroundsMatch)
				{
					var rating = _game.CurrentBattlegroundsRating;
					if(rating.HasValue)
						_game.CurrentGameStats.BattlegroundsRating = rating.Value;
				}
				else if (_game.IsMercenariesMatch)
				{
					var ratingInfo = _game.MercenariesRatingInfo;
					if(_game.IsMercenariesPvpMatch && ratingInfo != null)
						_game.CurrentGameStats.MercenariesRating = ratingInfo.Rating;
					if(_game.IsMercenariesPveMatch)
					{
						_game.CurrentGameStats.MercenariesBountyRunId = _game.MercenariesMapInfo?.Seed.ToString();
						_game.CurrentGameStats.MercenariesBountyRunTurnsTaken = (int)(_game.MercenariesMapInfo?.TurnsTaken ?? 0);
						_game.CurrentGameStats.MercenariesBountyRunCompletedNodes = _game.MercenariesMapInfo?.CompletedNodes ?? 0;
					}
				}
				_game.CurrentGameStats.GameType = _game.CurrentGameType;
				_game.CurrentGameStats.ServerInfo = _game.MetaData.ServerInfo;
				_game.CurrentGameStats.PlayerCardbackId = _game.MatchInfo?.LocalPlayer.CardBackId ?? 0;
				_game.CurrentGameStats.OpponentCardbackId = _game.MatchInfo?.OpposingPlayer.CardBackId ?? 0;
				_game.CurrentGameStats.FriendlyPlayerId = _game.MatchInfo?.LocalPlayer.Id ?? 0;
				_game.CurrentGameStats.OpponentPlayerId = _game.MatchInfo?.OpposingPlayer.Id ?? 0;
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
					_game.CurrentGameStats.SetPlayerSideboardsFromDict(_game.CurrentSelectedDeck.Sideboards);
				}
				else
				{
					_game.CurrentGameStats.HsDeckId = DeckList.Instance.ActiveDeckVersion?.HsId ?? 0;
					_game.CurrentGameStats.SetPlayerCards(DeckList.Instance.ActiveDeckVersion, confirmedCards);
					_game.CurrentGameStats.PlayerSideboards = DeckList.Instance.ActiveDeckVersion?.Sideboards.ToList() ?? new List<Sideboard>();
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
					selectedDeck.AddGameResult(_lastGame);

					if(Config.Instance.ArenaRewardDialog && (selectedDeck.IsArenaRunCompleted ?? false))
					{
						if (selectedDeck.ArenaReward != null && selectedDeck.ArenaReward.Packs[0] == ArenaRewardPacks.None)
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
						_assignedDeck.Archive(false);
					}
					_lastGame = null;
				}
				else
				{
					try
					{
						if(!string.IsNullOrEmpty(_game.Player.OriginalClass))
						{
							DefaultDeckStats.Instance.GetDeckStats(_game.Player.OriginalClass)?.Games.Add(_game.CurrentGameStats);
							Log.Info($"Assigned current deck to default {_game.Player.OriginalClass} deck.");
						}
						else
							Log.Debug("Not assigning a deck, no player class found.");
					}
					catch(Exception ex)
					{
						Log.Error("Error saving to DefaultDeckStats: " + ex);
					}
					_assignedDeck = null;
				}

				if(_game.StoredGameStats != null)
					_game.CurrentGameStats.StartTime = _game.StoredGameStats.StartTime;

				if(Config.Instance.ShowGameResultNotifications && RecordCurrentGameMode)
				{
					var deckName = _assignedDeck == null ? "No deck - " + _game.CurrentGameStats.PlayerHero : _assignedDeck.NameAndVersion;
					ToastManager.ShowGameResultToast(deckName ?? "Unknown deck", _game.CurrentGameStats);
				}

				if(_game.IsConstructedMatch || _game.IsFriendlyMatch || _game.IsArenaMatch)
				{
					CaptureMulliganGuideFeedback();
				}

				await SaveReplays(_game.CurrentGameStats);

				if(_game.IsConstructedMatch || _game.CurrentGameMode is GameMode.Arena or GameMode.Duels)
					HSReplayNetClientAnalytics.OnConstructedMatchEnds(
						_game.CurrentGameStats,
						_game.CurrentGameMode,
						_game.CurrentGameType,
						_game.Spectator,
						_game.Metrics
					);

				if(_game.IsBattlegroundsMatch)
				{
					if(LogContainsStateComplete)
						Sentry.SendQueuedBobsBuddyEvents(_game.CurrentGameStats.HsReplay.UploadId);
					else
						Sentry.ClearBobsBuddyEvents();
					RecordBattlegroundsGame();
					Tier7Trial.Clear();
					Core.Game.BattlegroundsSessionViewModel.OnGameEnd();
					Core.Windows.BattlegroundsSessionWindow.OnGameEnd();
					Core.Overlay.BattlegroundsHeroGuideListViewModel.Reset();
					Core.Overlay.BattlegroundsAnomalyGuideListViewModel.Reset();
					Core.Overlay.BattlegroundsHeroPickingViewModel.Reset();
					Core.Overlay.ResetAnomalyGuidesMulliganTrigger();
					Core.Overlay.BattlegroundsQuestPickingViewModel.Reset();
					Core.Overlay.BattlegroundsTrinketPickingViewModel.Reset();
					Core.Overlay.HideBattlegroundsHeroPanel();
					var hero = _game.Entities.Values.FirstOrDefault(x => x.IsPlayer && x.IsHero);
					var finalPlacement = hero?.GetTag(GameTag.PLAYER_LEADERBOARD_PLACE) ?? 0;
					CaptureBattlegroundsFeedback(finalPlacement);
					HSReplayNetClientAnalytics.OnBattlegroundsMatchEnds(
						hero?.CardId,
						finalPlacement,
						_game.CurrentGameStats,
						_game.Metrics,
						_game.CurrentGameType,
						_game.Spectator
					);
				}

				if(_game.IsMercenariesMatch)
					HSReplayNetClientAnalytics.OnMercenariesMatchEnds(
						_game.CurrentGameStats,
						_game.Metrics,
						_game.CurrentGameType,
						_game.Spectator
					);

				Influx.SendQueuedMetrics();
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

		private void RecordBattlegroundsGame()
		{
			if (Core.Game.Spectator)
				return;

			var hero = _game.Entities.Values.FirstOrDefault(x => x.IsPlayer && x.IsHero);
			var startTime = _game.CurrentGameStats?.StartTime.ToString("o");
			var endTime = _game.CurrentGameStats?.EndTime.ToString("o");
			var heroCardId = hero?.CardId != null ? BattlegroundsUtils.GetOriginalHeroId(hero.CardId) : null;
			var rating = _game.CurrentGameStats?.BattlegroundsRating;
			var ratingAfter = _game.CurrentGameStats?.BattlegroundsRatingAfter;
			var finalBoard = _game.Entities.Values
				.Where(x => x.IsMinion && x.IsInZone(HearthDb.Enums.Zone.PLAY) && x.IsControlledBy(_game.Player.Id))
				.Select(x => x.Clone())
				.ToArray();
			var friendlyGame = _game.CurrentGameType is GameType.GT_BATTLEGROUNDS_FRIENDLY or GameType.GT_BATTLEGROUNDS_DUO_FRIENDLY;
			var duos = _game.IsBattlegroundsDuosMatch;
			var placement = Math.Min(hero?.GetTag(GameTag.PLAYER_LEADERBOARD_PLACE) ?? 0, duos ? 4 : 8);

			if(startTime != null && endTime != null && heroCardId != null && rating != null && ratingAfter != null && placement > 0)
			{
				BattlegroundsLastGames.Instance.AddGame(
					startTime,
					endTime,
					heroCardId,
					(int)rating,
					(int)ratingAfter,
					placement,
					finalBoard,
					friendlyGame,
					duos,
					save: true
				);
			}
			else
			{
				Log.Error("Missing data while trying to record battleground game");
			}
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
				if(Config.Instance.ShowNoteDialogAfterGame && Config.Instance.NoteDialogDelayed && !_showedNoteDialog && _game.CurrentGameStats != null)
				{
					_showedNoteDialog = true;
					new NoteDialog(_game.CurrentGameStats);
				}

				if(_game.CurrentGameStats != null)
				{
					_game.CurrentGameStats.Turns = _game.GetTurnNumber();
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
					DefaultDeckStats.Save();
				else
					DeckStatsList.Save();
				if(_game.CurrentGameStats != null)
				{
					LastGames.Instance.Add(_game.CurrentGameStats);
					LastGames.Save();
				}
			}
			else if(_assignedDeck != null && _game.CurrentGameStats != null && _assignedDeck.DeckStats.Games.Contains(_game.CurrentGameStats))
			{
				//game was not supposed to be recorded, remove from deck again.
				_assignedDeck.RemoveGameResult(_game.CurrentGameStats);
				Log.Info($"Local deck stats are disabled for {_game.CurrentGameMode}. Removed game from {_assignedDeck}.");
			}
			else if(_assignedDeck == null)
			{
				var defaultDeck = DefaultDeckStats.Instance.GetDeckStats(_game.Player.OriginalClass);
				if(defaultDeck != null)
				{
					if(_game.CurrentGameStats != null)
						defaultDeck.Games.Remove(_game.CurrentGameStats);
					Log.Info($"Local deck stats are disabled for {_game.CurrentGameMode}. Removed game from default {_game.Player.OriginalClass}.");
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

		public void HandlePlayerMaxHealthChange(int value)
		{
			_game.Player.MaxHealth = value;
			Core.UpdatePlayerResourcesWidget();
		}

		public void HandleOpponentMaxHealthChange(int value)
		{
			_game.Opponent.MaxHealth = value;
			Core.UpdateOpponentResourcesWidget();
		}

		public void HandlePlayerMaxManaChange(int value)
		{
			_game.Player.MaxMana = value;
			Core.UpdatePlayerResourcesWidget();
		}

		public void HandleOpponentMaxManaChange(int value)
		{
			_game.Opponent.MaxMana = value;
			Core.UpdateOpponentResourcesWidget();
		}

		public void HandlePlayerMaxHandSizeChange(int value)
		{
			_game.Player.MaxHandSize = value;
			Core.UpdatePlayerResourcesWidget();
		}

		public void HandleOpponentMaxHandSizeChange(int value)
		{
			_game.Opponent.MaxHandSize = value;
			Core.UpdateOpponentResourcesWidget();
		}

		public void HandleBeginMulligan()
		{
			var isConstructed = _game.IsConstructedMatch;
			if(_game.IsBattlegroundsMatch)
			{
				HandleBattlegroundsStart();
			}
			else if(_game.IsConstructedMatch || _game.IsFriendlyMatch || _game.IsArenaMatch)
				HandleHearthstoneMulliganPhase();
		}

		public async void HandlePlayerMulliganDone()
		{
			if(_game.IsBattlegroundsMatch)
			{
				_game.SnapshotBattlegroundsHeroPick();
				Core.Overlay.HideBattlegroundsHeroPanel();
				Core.Overlay.BattlegroundsHeroPickingViewModel.Reset();
				Core.Overlay.BattlegroundsSessionViewModelVM.HideCompStatsOnError();
				Core.Overlay.BattlegroundsHeroGuideListViewModel.OnMulliganEnded();
				Core.Overlay.ResetAnomalyGuidesMulliganTrigger();
			}
			else if(_game.IsConstructedMatch || _game.IsFriendlyMatch || _game.IsArenaMatch)
			{
				Core.Overlay.HideMulliganToast(false);

				var openingHand = _game.SnapshotOpeningHand();

				if(_game.MulliganCardStats != null && Config.Instance.EnableMulliganGuide)
				{
					var numSwappedCards = _game.GetMulliganSwappedCards()?.Count ?? 0;
					if(numSwappedCards > 0)
					{
						// Show the updated cards
						var dbfIds = openingHand.Select(x => x.Card.DeckbuildingCard.DbfId).ToList();
						if(dbfIds.Any())
						{
							Core.Overlay.ShowMulliganGuideStats(
								dbfIds.Select(dbfId => _game.MulliganCardStats.TryGetValue(dbfId, out var stats) ? stats : new SingleCardStats(dbfId)),
								_game.MulliganCardStats.Count,
								null
							);
						}
					}

					// Delay until the cards fly away
					await Task.Delay(2_375 + (Math.Max(1, numSwappedCards)) * 475);

					// Wait for the mulligan to be complete (component or animation)
					for(var j = 0; j < 16 * 60 * 120; j++) // 2 minutes
					{
						if(_game.IsInMenu || (_game.GameEntity?.GetTag(STEP) ?? 0) > (int)Step.BEGIN_MULLIGAN)
						{
							Core.Overlay.HideMulliganGuideStats();
							Core.Game.Player.MulliganCardStats = null;
							return;
						}

						if(
							(_game.PlayerEntity?.GetTag(GameTag.MULLIGAN_STATE) ?? 0) >= (int)Mulligan.DONE &&
							(_game.OpponentEntity?.GetTag(GameTag.MULLIGAN_STATE) ?? 0) >= (int)Mulligan.DONE
						)
							break;
						await Task.Delay(16);
					}

					Core.Overlay.HideMulliganGuideStats();
					Core.Game.Player.MulliganCardStats = null;
				}
			}
		}

		public void HandlePlayerEntityChoices(IHsChoice choice)
		{
			if(choice.ChoiceType == ChoiceType.GENERAL && _game.IsBattlegroundsMatch)
			{
				var offeredEntities = choice.OfferedEntityIds?
					.Select(id => _game.Entities.TryGetValue(id, out var e) ? e : null)
					.WhereNotNull().ToList() ?? new List<Entity>();

				// trinket picking
				if(
					_game.Entities.TryGetValue(choice.SourceEntityId, out var source) &&
					source.GetTag(GameTag.BACON_IS_MAGIC_ITEM_DISCOVER) > 0 &&
					offeredEntities.All(x => x.IsBattlegroundsTrinket)
				)
				{
					HandleBattlegroundsTrinketChoice(choice).Forget();
				}
				// hero power choice
				else if(offeredEntities.All(x => x.IsHeroPower))
				{
					var offered = offeredEntities.Where(x => x.IsHeroPower).ToList();
					Core.Overlay.BattlegroundsMinionsVM.OnHeroPowers(
						_game.Player.Board.Where(x => x.IsHeroPower).Concat(offered).Select(x => x.Card.Id)
					);
				}
			}

			_game.Player.OfferedEntityIds = choice.OfferedEntityIds.ToList();
			Core.Overlay.PlayerCounters.UpdateVisibleCounters();
		}

		public async Task HandleBattlegroundsTrinketChoice(IHsChoice choice)
		{

			var offeredEntities = choice.OfferedEntityIds?
				.Select(id => _game.Entities.TryGetValue(id, out var e) ? e : null)
				.WhereNotNull().ToList() ?? new List<Entity>();

			var offered = offeredEntities.Where(x => x.IsBattlegroundsTrinket).ToList();
			Core.Overlay.BattlegroundsMinionsVM.OnTrinkets(_game.Player.Trinkets.Concat(offered).Select(x => x.Card.Id));

			var result = await GetTrinketPickStats(choice);

			if(result != null && !Core.Game.IsTrinketChoiceComplete(choice.Id))
			{
				Core.Overlay.BattlegroundsTrinketPickingViewModel.SetTrinketStats(
					offeredEntities.Select(entity => result.Data.FirstOrDefault(x => x.TrinketDbfId == entity.Card.DbfId))
				);
			}
		}

		private async Task<BattlegroundsTrinketPickStats?> GetTrinketPickStats(IHsChoice choice)
		{
			if(Core.Game.Spectator)
				return null;

			if(!Config.Instance.EnableBattlegroundsTier7Overlay)
				return null;

			if(Remote.Config.Data?.Tier7?.Disabled ?? false)
				return null;

			var requestParams = _game.SnapshotOfferedTrinkets(choice);
			if(requestParams == null)
			{
				return null;
			}

			var userOwnsTier7 = HSReplayNetOAuth.AccountData?.IsTier7 ?? false;
			if(!userOwnsTier7 && Tier7Trial.Token == null)
				return null;

			var result = Tier7Trial.Token != null
				? await ApiWrapper.GetTier7TrinketPickStats(Tier7Trial.Token, requestParams)
				: await HSReplayNetOAuth.MakeRequest(c => c.GetTier7TrinketPickStats(requestParams));

			return result;
		}

		public void HandleOpponentEntitiesChosen(IHsCompletedChoice choice)
		{
			if(choice.ChoiceType == ChoiceType.GENERAL)
			{
				_game.CounterManager.HandleChoicePicked(choice);
			}
		}

		public void HandlePlayerEntitiesChosen(IHsCompletedChoice choice)
		{
			var chosen = choice.ChosenEntityIds
				?.Select(id => _game.Entities.TryGetValue(id, out var e) ? e : null)
				.WhereNotNull()
				.ToList() ?? new List<Entity>();
			var source = _game.Entities.TryGetValue(choice.SourceEntityId, out var e) ? e : null;
			switch(choice.ChoiceType)
			{
				case ChoiceType.MULLIGAN:
					if(_game.IsBattlegroundsMatch)
					{
						if(chosen.Count == 1)
						{
							var hero = chosen.Single();
							var heroPower = Database.GetCardFromDbfId(hero.GetTag(GameTag.HERO_POWER), collectible: false)
								?.Id;
							if(heroPower is string hp)
								Core.Overlay.BattlegroundsMinionsVM.OnHeroPowers(new List<string>() { hp });
						}
						else
						{
							Log.Error($"Could not reliably determine Battlegrounds hero power. {chosen.Count} hero(es) chosen.");
						}

						Core.Overlay.BattlegroundsHeroPickingViewModel.Reset();
					}
					else if(_game.IsConstructedMatch || _game.IsFriendlyMatch || _game.IsArenaMatch)
					{
						_game.SnapshotMulliganChoices(choice);
					}
					break;
				case ChoiceType.GENERAL:
					_game.CounterManager.HandleChoicePicked(choice);
					if(_game.IsBattlegroundsMatch)
					{
						Core.Overlay.BattlegroundsQuestPickingViewModel.Reset();
						Core.Overlay.BattlegroundsTrinketPickingViewModel.Reset();
						if((source?.GetTag(GameTag.BACON_IS_MAGIC_ITEM_DISCOVER) ?? 0) > 0)
						{
							Core.Overlay.BattlegroundsMinionsVM.OnTrinkets(Core.Game.Player.Trinkets.Concat(chosen).Select(x => x.Card.Id));
							_game.SnapshotChosenTrinket(choice);
						}
						Core.Overlay.BattlegroundsMinionsVM.OnHeroPowers(
							_game.Player.Board.Where(x => x.IsHeroPower).Select(x => x.Card.Id)
						);
					}
					break;
			}

			_game.Player.OfferedEntityIds.Clear();
			Core.Overlay.PlayerCounters.UpdateVisibleCounters();
		}

		private async void HandleHearthstoneMulliganPhase()
		{
			for(var i = 0; i < 10; i++)
			{
				await Task.Delay(500);
				var step = _game.GameEntity?.GetTag(STEP) ?? 0;
				if(step == 0)
					continue;
				if(step > (int)Step.BEGIN_MULLIGAN)
					break;

				_game.SnapshotMulligan();
				_game.CacheMulliganGuideParams();

				var showToast = Config.Instance.ShowMulliganToast && !_game.IsArenaMatch;
				if(showToast || Config.Instance.EnableMulliganGuide)
				{
					// Show Mulligan Guide Elements (Overlay and/or Toast)
					var shortId = DeckList.Instance.ActiveDeckVersion?.ShortId;
					if(!string.IsNullOrEmpty(shortId))
					{
						var cards = _game.Player.PlayerEntities.Where(x => x.IsInHand && !x.Info.Created);
						var dbfIds = cards.OrderBy(x => x.ZonePosition).Select(x => x.Card.DeckbuildingCard.DbfId).ToArray();

						MulliganGuideData? mulliganGuideData = null;
						if(Config.Instance.EnableMulliganGuide)
						{
							mulliganGuideData = await GetMulliganGuideData();
						}

						if(mulliganGuideData is MulliganGuideData data)
						{
							// Show mulligan guide with parameters as selected by the API
							if(showToast)
							{
								Core.Overlay.ShowMulliganToast(shortId!, dbfIds, data.Toast?.Parameters, true);
								showToast = false;
							}

							// Wait for the mulligan to be ready
							await WaitForMulliganStart();

							Dictionary<int, SingleCardStats>? cardStats = null;
							// GroupBy before ToDictionary to deal with (unsupported) dbfId duplicates from the server
							var grouped = data.DeckDbfIdList.GroupBy(x => x.DbfId).ToDictionary(x => x.Key, x => x.First());
							try
							{
								cardStats = SingleCardStats.GroupCardStats(grouped, data.BaseWinrate);
							}
							catch(Exception e)
							{
								Log.Error(e);
								cardStats = null;
							}

							if(cardStats != null)
							{
								_game.MulliganCardStats = cardStats;
								Core.Game.Player.MulliganCardStats = cardStats.Values;
								Core.Overlay.ShowMulliganGuideStats(
									dbfIds.Select(dbfId =>
										cardStats.TryGetValue(dbfId, out var stats) ? stats
											: new SingleCardStats(dbfId)),
									cardStats.Count,
									data.SelectedParams
								);
							}
							// Something went wrong generating the card stats, continue to locally generated toast (if enabled)
						}

						if(showToast)
						{
							Dictionary<string, string>? parameters = null;
							if(HsReplayDataManager.Decks.AvailableDecks.Contains(shortId!))
							{
								var opponentClass = _game.Opponent.PlayerEntities.FirstOrDefault(x => x.IsHero && x.IsInPlay)?.Card.CardClass ?? CardClass.INVALID;
								var isFirst = _game.PlayerEntity?.GetTag(GameTag.FIRST_PLAYER) == 1;

								// Show mulligan toast with locally sourced parameters
								parameters = new Dictionary<string, string>
								{
									["opponentClasses"] = opponentClass.ToString(),
									["playerInitiative"] = isFirst ? "FIRST" : "COIN"
								};

								var playerStarLevel = _game.PlayerMedalInfo?.StarLevel ?? 0;
								if(playerStarLevel > 0)
								{
									parameters.Add("mulliganPlayerStarLevel", playerStarLevel.ToString());
								}

								// Let the website do it's thing and fallback for non-Premium users
								parameters.Add("mulliganAutoFilter", "yes");
							}

							Core.Overlay.ShowMulliganToast(shortId!, dbfIds, parameters);
						}
					}
				}

				break;
			}
		}

		private void CaptureMulliganGuideFeedback()
		{
			if(!Config.Instance.GoogleAnalytics || _game.Spectator)
				return;

			var parameters = _game.GetMulliganGuideFeedbackParams();
			if(parameters is null)
				return;

			ApiWrapper.PostMulliganGuideFeedback(parameters).Forget();
		}

		private async Task<MulliganGuideData?> GetMulliganGuideData()
		{
			if(Core.Game.Spectator)
				return null;

			if(Remote.Config.Data?.MulliganGuide?.Disabled ?? false)
			{
				return null;
			}

			var userOwnsPremium = HSReplayNetOAuth.AccountData?.IsPremium ?? false;
			if(!userOwnsPremium)
			{
				// No trial support yet
				return null;
			}

			// Assemble request
			var parameters = _game.GetMulliganGuideParams();
			if(parameters == null)
			{
				return null;
			}

#if(DEBUG)
			var json = JsonConvert.SerializeObject(parameters);
			Log.Debug($"Fetching Mulligan Guide with parameters={json}...");
#endif

			return await HSReplayNetOAuth.MakeRequest(c => c.GetConstructedMulliganGuide(parameters));
		}

		private async Task WaitForMulliganStart(int timeout = 60)
		{
			for(var j = 0; j < 16 * 60 * timeout; j++)
			{
				if(_game.IsInMenu || (_game.GameEntity?.GetTag(STEP) ?? 0) > (int)Step.BEGIN_MULLIGAN)
					return;
				if(Reflection.Client.GetIsMulliganWaitingForUserInput())
					break;
				await Task.Delay(16);
			}
		}

		private async void HandleBattlegroundsStart()
		{
			Watchers.BattlegroundsLeaderboardWatcher.Run();
			OpponentDeadForTracker.Reset();
			Core.Overlay.BattlegroundsInspirationViewModel.Reset();

			IEnumerable<Entity> heroes = new List<Entity>();
			for(var i = 0; i < 10; i++)
			{
				await Task.Delay(500);
				heroes = Core.Game.Player.PlayerEntities.Where(x => x.IsHero && (x.HasTag(BACON_HERO_CAN_BE_DRAFTED) || x.HasTag(BACON_SKIN)) && !x.HasTag(BACON_LOCKED_MULLIGAN_HERO)).ToList();
				if(heroes.Count() >= 2)
					break;
			}

			await Task.Delay(500);

			Core.Overlay.BattlegroundsSessionViewModelVM.Update();
			Core.Overlay.BattlegroundsHeroGuideListViewModel.Update();
			Core.Overlay.BattlegroundsAnomalyGuideListViewModel.Update();

			if(Core.Game.IsBattlegroundsDuosMatch)
				Watchers.BattlegroundsTeammateBoardStateWatcher.Run();

			if(_game.GameEntity?.GetTag(STEP) != (int)Step.BEGIN_MULLIGAN)
			{
				Core.Overlay.ShowBgsTopBarAndBobsBuddyPanel();
				return;
			}

			_game.SnapshotBattlegroundsOfferedHeroes(heroes);
			_game.CacheBattlegroundsHeroPickParams(false);

			var heroIds = heroes.OrderBy(x => x.ZonePosition).Select(x => x.Card.DbfId).ToArray();
			if(Config.Instance.HideOverlay)
			{
				if(Config.Instance.ShowBattlegroundsToast)
				{
					// Wait for the mulligan to be ready
					await WaitForMulliganStart();

					// Wait for screen to fade in
					await Task.Delay(500);

					var anomalyDbfId = BattlegroundsUtils.GetBattlegroundsAnomalyDbfId(Core.Game.GameEntity);
					ToastManager.ShowBattlegroundsToast(
						heroIds,
						_game.IsBattlegroundsDuosMatch,
						anomalyDbfId
					);

					Core.Overlay.ShowBgsTopBarAndBobsBuddyPanel(); // in case the overlay is enabled later
				}
			}
			else
			{
				var statsTask = GetBattlegroundsHeroPickStats();

				// Wait for the mulligan to be ready
				await WaitForMulliganStart();

				async Task WaitAndAppear()
				{
					await Task.Delay(500);
					Core.Overlay.ShowBgsTopBarAndBobsBuddyPanel();
				}

				var appearTask = WaitAndAppear();
				BattlegroundsHeroPickStats? battlegroundsHeroPickStats = null;
				try
				{
					await Task.WhenAll(statsTask, appearTask);
					battlegroundsHeroPickStats = await statsTask;
				}
				catch(Exception)
				{
					// pass
				}

				Core.Overlay.ShowBgsTopBarAndBobsBuddyPanel();

				Dictionary<string, string>? toastParams = null;
				if(battlegroundsHeroPickStats is BattlegroundsHeroPickStats stats)
				{
					toastParams = stats.Toast.Parameters;

					Core.Overlay.ShowBattlegroundsHeroPickingStats(
						heroIds.Select(dbfId => stats.Data.FirstOrDefault(x => x.HeroDbfId == dbfId)),
						stats.Toast.Parameters,
						stats.Toast.MinMmr,
						stats.Toast.AnomalyAdjusted ?? false
					);
				}
				else if(statsTask.Exception != null)
				{
					if(statsTask.Exception.InnerExceptions.Any(x => x is HeroPickingDisabledException))
					{
						Core.Overlay.BattlegroundsHeroPickingViewModel.ShowDisabledMessage();
					}
					else
					{
						Core.Overlay.BattlegroundsHeroPickingViewModel.ShowErrorMessage();
					}
				}

				if(Config.Instance.ShowBattlegroundsToast)
				{
					Core.Overlay.ShowBattlegroundsHeroPanel(heroIds, _game.IsBattlegroundsDuosMatch, toastParams);
				}

				Core.Overlay.SetAnomalyGuidesMulliganTrigger();
			}
		}

		private async Task<BattlegroundsHeroPickStats?> GetBattlegroundsHeroPickStats()
		{
			if(Core.Game.Spectator)
				return null;

			if(!Config.Instance.EnableBattlegroundsTier7Overlay)
				return null;

			if(Remote.Config.Data?.Tier7?.Disabled ?? false)
				throw new HeroPickingDisabledException("Hero picking remotely disabled");

			var userOwnsTier7 = HSReplayNetOAuth.AccountData?.IsTier7 ?? false;
			if(!userOwnsTier7 && (Tier7Trial.RemainingTrials ?? 0) == 0)
				return null;

			var parameters = _game.GetBattlegroundsHeroPickParams();

			// Avoid using a trial when we can't get the api params anyway.
			if(parameters == null)
				throw new HeroPickingException("Unable to get API parameters");

			// Use a trial if we can
			string? token = null;
			if(!userOwnsTier7)
			{
				var acc = Reflection.Client.GetAccountId();
				token = acc != null ? await Tier7Trial.ActivateOrContinue(acc.Hi, acc.Lo, _game.MetaData.ServerInfo?.GameHandle) : null;
				if(token == null)
					throw new HeroPickingException("Unable to get trial token");

				Core.Game.Metrics.Tier7TrialsRemaining = Math.Max(0, (Tier7Trial.RemainingTrials ?? 0) - 1);
			}

#if(DEBUG)
			var json = JsonConvert.SerializeObject(parameters);
			Log.Debug($"Fetching Battlegrounds Hero Pick stats with parameters={json}...");
#endif

			// At this point the user either owns tier7 or has an active trial!

			var isDuos = Core.Game.IsBattlegroundsDuosMatch;
			var stats = token != null && !userOwnsTier7
				? isDuos
					? await ApiWrapper.GetTier7DuosHeroPickStats(token, parameters)
					: await ApiWrapper.GetTier7HeroPickStats(token, parameters)
				: await HSReplayNetOAuth.MakeRequest(c =>
					isDuos ? c.GetTier7DuosHeroPickStats(parameters) : c.GetTier7HeroPickStats(parameters)
				);

			if(stats == null)
				throw new HeroPickingException("Invalid server response");

			return stats;
		}

		private int battlegroundsHeroPickingLatch = 0;
		private async Task RefreshBattlegroundsHeroPickStats()
		{
			var heroes = Core.Game.Player.PlayerEntities.Where(x => x.IsHero && (x.HasTag(BACON_HERO_CAN_BE_DRAFTED) || x.HasTag(BACON_SKIN)) && !x.HasTag(BACON_LOCKED_MULLIGAN_HERO)).ToList();

			// refresh the offered heroes
			_game.SnapshotBattlegroundsOfferedHeroes(heroes);
			_game.CacheBattlegroundsHeroPickParams(true);

			try
			{
				var latchOut = ++battlegroundsHeroPickingLatch;
				BattlegroundsHeroPickStats? battlegroundsHeroPickStats = null;
				try
				{
					battlegroundsHeroPickStats = await GetBattlegroundsHeroPickStats();
				}
				catch(Exception)
				{
					// pass
				}

				// another task has updated them since (fast reroll)
				if(latchOut != battlegroundsHeroPickingLatch)
					return;

				// the stats are no longer relevant
				if(_game.GameEntity?.GetTag(STEP) > (int)Step.BEGIN_MULLIGAN || _game.IsInMenu || Core.Overlay.BattlegroundsHeroPickingViewModel.HeroStats == null)
					return;

				Dictionary<string, string>? toastParams = null;
				if(battlegroundsHeroPickStats is BattlegroundsHeroPickStats stats)
				{
					var heroIds = heroes.OrderBy(x => x.ZonePosition).Select(x => x.Card.DbfId).ToArray();
					Core.Overlay.ShowBattlegroundsHeroPickingStats(
						heroIds.Select(dbfId => stats.Data.FirstOrDefault(x => x.HeroDbfId == dbfId)),
						stats.Toast.Parameters,
						stats.Toast.MinMmr,
						stats.Toast.AnomalyAdjusted ?? false
					);
				}
			}
			finally
			{
				battlegroundsHeroPickingLatch--;
			}
		}

		private void CaptureBattlegroundsFeedback(int finalPlacement)
		{
			if(!Config.Instance.GoogleAnalytics || _game.Spectator)
				return;

			var heroPickFeedback = _game.GetBattlegroundsHeroPickFeedbackParams(finalPlacement);
			if(heroPickFeedback is not null)
				ApiWrapper.PostBattlegroundsHeroPickFeedback(heroPickFeedback, isDuos: _game.IsBattlegroundsDuosMatch).Forget();

			var trinketFeedback = _game.GetTrinketPickingFeedback(finalPlacement);
			foreach(var feedback in trinketFeedback)
				ApiWrapper.PostBattlegroundsTrinketFeedback(feedback).Forget();
		}

		#region Player

		public void HandlePlayerGetToDeck(Entity entity, string cardId, int turn)
		{
			_game.Player.CreateInDeck(entity, turn);
			Core.UpdatePlayerCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnPlayerCreateInDeck.Execute(card);
		}

		public void HandlePlayerGet(Entity entity, string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			_game.Player.CreateInHand(entity, turn);
			Core.UpdatePlayerCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnPlayerGet.Execute(card);
		}

		public void HandlePlayerBackToHand(Entity entity, string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			Core.UpdatePlayerCards();
			_game.Player.BoardToHand(entity, turn);
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnPlayerPlayToHand.Execute(card);
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
			_game.SecretsManager.HandleCardDrawn(entity);
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnPlayerDraw.Execute(card);
		}


		public void HandlePlayerMulligan(Entity entity, string cardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			_game.Player.Mulligan(entity);
			Core.UpdatePlayerCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnPlayerMulligan.Execute(card);
		}

		public void HandlePlayerSecretPlayed(Entity entity, string cardId, int turn, Zone fromZone, string parentBlockCardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			var card = Database.GetCardFromId(cardId);
			if(!entity.IsSecret)
			{
				if(entity.IsQuest && !entity.IsQuestlinePart || entity.IsSideQuest)
				{
					_game.Player.QuestPlayedFromHand(entity, turn);
					if(card != null)
						GameEvents.OnPlayerPlay.Execute(card);
				}
				else if(entity.IsSigil)
				{
					_game.Player.SigilPlayedFromHand(entity, turn);
					if(card != null)
						GameEvents.OnPlayerPlay.Execute(card);
				}
				else if(entity.IsObjective)
				{
					_game.Player.ObjectivePlayedFromHand(entity, turn);
					if(card != null)
						GameEvents.OnPlayerPlay.Execute(card);
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
					_game.SecretsManager.HandleCardPlayed(entity, parentBlockCardId);
					break;
				default:
					_game.Player.CreateInSecret(entity, turn);
					return;
			}
			Core.UpdatePlayerCards();
			if(card != null)
			{
				switch(fromZone)
				{
					case Zone.DECK:
						GameEvents.OnPlayerDeckToPlay.Execute(card);
						break;
					case Zone.HAND:
						GameEvents.OnPlayerPlay.Execute(card);
						break;
					default:
						break;

				}
			}
		}

		public void HandlePlayerSecretTrigger(Entity entity, string? cardId, int turn, int otherId)
		{
			if (!entity.IsSecret)
				return;
			_game.Player.SecretTriggered(entity, turn);
		}

		public void HandlePlayerHandDiscard(Entity entity, string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			_game.Player.HandDiscard(entity, turn);
			Core.UpdatePlayerCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnPlayerHandDiscard.Execute(card);
		}

		public void HandlePlayerPlay(Entity entity, string cardId, int turn, string parentBlockCardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			_game.Player.Play(entity, turn);
			Core.UpdatePlayerCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnPlayerPlay.Execute(card);
			_game.SecretsManager.HandleCardPlayed(entity, parentBlockCardId);
		}

		public void HandlePlayerDeckDiscard(Entity entity, string cardId, int turn)
		{
			_game.Player.DeckDiscard(entity, turn);
			DeckManager.DetectCurrentDeck().Forget();
			Core.UpdatePlayerCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnPlayerDeckDiscard.Execute(card);
		}

		public void HandlePlayerPlayToDeck(Entity entity, string cardId, int turn)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			_game.Player.BoardToDeck(entity, turn);
			Core.UpdatePlayerCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnPlayerPlayToDeck.Execute(card);
		}

		public void HandlePlayerDredge()
		{
			Core.UpdatePlayerCards();
		}

		public void HandlePlayerUnknownCardAddedToDeck()
		{
			foreach(var card in _game.Player.Deck)
				card.Info.DeckIndex = 0;
		}

		public void HandleMercenariesStateChange()
		{
			if(_game.IsMercenariesMatch)
			{
				Core.Overlay.UpdateMercenariesOverlay();
			}
		}

		public void HandleBattlegroundsPlayerTechLevel(int id, int value)
		{
			if(_game.IsBattlegroundsMatch)
			{
				_game.UpdateBattlegroundsPlayerTechLevel(id, value);
			}
		}

		public void HandleBattlegroundsPlayerTriples(int id, int value)
		{
			if(_game.IsBattlegroundsMatch)
			{
				_game.UpdateBattlegroundsPlayerTriples(id, value);
			}
		}

		#endregion

		#region Opponent

		public void HandleOpponentGetToDeck(Entity entity, int turn)
		{
			_game.Opponent.CreateInDeck(entity, turn);
			Core.UpdateOpponentCards();
		}

		public void HandlePlayerRemoveFromPlay(Entity entity, int turn)
		{
			if(entity.IsEnchantment)
			{
				_game.ActiveEffects.TryRemoveEffect(entity, true);
			}

			_game.Player.RemoveFromPlay(entity, turn);
		}

		public void HandleOpponentRemoveFromPlay(Entity entity, int turn)
		{
			if(entity.IsEnchantment)
			{
				_game.ActiveEffects.TryRemoveEffect(entity, false);
			}

			_game.Player.RemoveFromPlay(entity, turn);
		}

		public void HandlePlayerCreateInSetAside(Entity entity, int turn) => _game.Player.CreateInSetAside(entity, turn);

		public void HandleOpponentCreateInSetAside(Entity entity, int turn) => _game.Opponent.CreateInSetAside(entity, turn);

		public void HandlePlayerPlayToGraveyard(Entity entity, string cardId, int turn, bool playersTurn)
		{
			if(entity.IsEnchantment)
			{
				_game.ActiveEffects.TryRemoveEffect(entity, true);
			}

			_game.Player.PlayToGraveyard(entity, turn);
			var card = Database.GetCardFromId(entity.Info.LatestCardId);
			if(card != null)
				GameEvents.OnPlayerPlayToGraveyard.Execute(card);

			if(playersTurn && entity.IsMinion)
				HandlePlayerMinionDeath(entity);
		}

		public async void HandleOpponentPlayToGraveyard(Entity entity, string? cardId, int turn, bool playersTurn)
		{
			if(entity.IsEnchantment)
			{
				_game.ActiveEffects.TryRemoveEffect(entity, false);
			}

			if(cardId != null)
				_game.Opponent.PlayToGraveyard(entity, turn);
			var card = Database.GetCardFromId(entity.Info.LatestCardId);
			if(card != null)
				GameEvents.OnOpponentPlayToGraveyard.Execute(card);
			if(playersTurn && entity.IsMinion)
				HandleOpponentMinionDeath(entity, turn);

			if(!playersTurn && entity.Info.WasTransformed)
			{
				await Task.Delay(3000);
				var transformedSecert = _game.SecretsManager.Secrets.Where(x => x.Entity.Id == entity.Id).FirstOrDefault();
				if(transformedSecert != null)
				{
					_game.SecretsManager.Secrets.Remove(transformedSecert);
					_game.SecretsManager.Refresh();
				}
			}
		}

		private int GetMaestraDbfid() => HearthDb.Cards.All.TryGetValue(HearthDb.CardIds.NonCollectible.Neutral.MaestraoftheMasquerade_DisguiseEnchantment, out var maestra) ? maestra.DbfId : -1;

		private bool IsMaestraHero(Entity entity) => entity.IsHero && entity.GetTag(GameTag.CREATOR_DBID) == GetMaestraDbfid();

		private void OpponentIsDisguisedRogue()
		{
			SetOpponentHero(HearthDb.CardIds.Collectible.Rogue.ValeeraSanguinarHeroHeroSkins);
			Core.Overlay.SetWinRates();
			_game.Opponent.PredictUniqueCardInDeck(HearthDb.CardIds.Collectible.Rogue.MaestraOfTheMasquerade, false);
			Core.UpdateOpponentCards();
		}

		public void HandlePlayerCreateInPlay(Entity entity, string cardId, int turn)
		{
			if(entity.IsEnchantment)
			{
				_game.ActiveEffects.TryAddEffect(entity, true);
			}

			_game.Player.CreateInPlay(entity, turn);
			var card = Database.GetCardFromId(cardId);

			if(card != null)
				GameEvents.OnPlayerCreateInPlay.Execute(card);
		}

		public void HandleOpponentCreateInPlay(Entity entity, string? cardId, int turn)
		{
			if(entity.IsEnchantment)
			{
				_game.ActiveEffects.TryAddEffect(entity, false);
			}

			if(IsMaestraHero(entity))
				OpponentIsDisguisedRogue();
			_game.Opponent.CreateInPlay(entity, turn);
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnOpponentCreateInPlay.Execute(card);
		}

		public void HandleChameleosReveal(string cardId)
		{
			_game.Opponent.PredictUniqueCardInDeck(cardId, false);
			Core.UpdateOpponentCards();
		}

		public void HandleCardCopy()
		{
			Core.UpdateOpponentCards();
		}

		public void HandlePlayerJoust(Entity entity, string cardId, int turn)
		{
			_game.Player.JoustReveal(entity, turn);
			Core.UpdatePlayerCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnPlayerJoustReveal.Execute(card);
		}

		public void HandlePlayerDeckToPlay(Entity entity, string? cardId, int turn)
		{
			_game.Player.DeckToPlay(entity, turn);
			Core.UpdatePlayerCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnPlayerDeckToPlay.Execute(card);
		}

		public void HandleOpponentDeckToPlay(Entity entity, string? cardId, int turn)
		{
			_game.Opponent.DeckToPlay(entity, turn);
			Core.UpdateOpponentCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnOpponentDeckToPlay.Execute(card);
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

		public void HandlePlayerStolen(Entity entity, string? cardId, int turn)
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
						if(!Enum.TryParse(_game.Opponent.OriginalClass, out heroClass))
							return;
					}
				}
				else
				{
					if(!Enum.TryParse(_game.Opponent.OriginalClass, out heroClass))
						return;
				}
				_game.SecretsManager.NewSecret(entity);
			}
		}

		public void HandleOpponentStolen(Entity entity, string? cardId, int turn)
		{
			_game.Opponent.StolenByOpponent(entity, turn);
			_game.Player.StolenFromOpponent(entity, turn);
			if(entity.IsSecret)
			{
				_game.SecretsManager.RemoveSecret(entity);
				Core.UpdateOpponentCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
					GameEvents.OnOpponentSecretTriggered.Execute(card);
			}
		}

		public void HandleOpponentPlay(Entity entity, string? cardId, int from, int turn)
		{
			_game.Opponent.Play(entity, turn);
			Core.UpdateOpponentCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnOpponentPlay.Execute(card);
		}


		public void HandleOpponentJoust(Entity entity, string? cardId, int turn)
		{
			_game.Opponent.JoustReveal(entity, turn);
			Core.UpdateOpponentCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnOpponentJoustReveal.Execute(card);
		}

		public void HandleOpponentHandDiscard(Entity entity, string? cardId, int from, int turn)
		{
			_game.Opponent.HandDiscard(entity, turn);
			Core.UpdateOpponentCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnOpponentHandDiscard.Execute(card);
		}

		public void HandleOpponentDraw(Entity entity, int turn, string? cardId, int? drawerId)
		{
			var blacklist = Remote.Config.Data?.DrawCardBlacklist?.WhereNotNull().Select(obj => obj.DbfId).ToList() ?? new List<int>();
			if(drawerId > 0 && _game.Entities.TryGetValue(drawerId ?? 0, out var drawer))
			{
				if(!blacklist.Contains(drawer.Card.DbfId))
				{
					entity.Info.DrawerId = drawerId;
				}
			}
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
				entity.CardId = HearthDb.CardIds.NonCollectible.Neutral.TheCoinCore;
			_game.Opponent.CreateInHand(entity, turn);
			Core.UpdateOpponentCards();
			GameEvents.OnOpponentGet.Execute();
		}

		public void HandleOpponentSecretPlayed(Entity entity, string? cardId, int from, int turn, Zone fromZone, int otherId)
		{
			var card = Database.GetCardFromId(cardId);
			if(!entity.IsSecret)
			{
				if(entity.IsQuest && !entity.IsQuestlinePart || entity.IsSideQuest)
				{
					_game.Opponent.QuestPlayedFromHand(entity, turn);
					if(card != null)
						GameEvents.OnOpponentPlay.Execute(card);
				}
				else if(entity.IsSigil)
				{
					_game.Opponent.SigilPlayedFromHand(entity, turn);
					if(card != null)
						GameEvents.OnOpponentPlay.Execute(card);
				}
				else if(entity.IsObjective)
				{
					_game.Opponent.ObjectivePlayedFromHand(entity, turn);
					if(card != null)
						GameEvents.OnOpponentPlay.Execute(card);
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
				if(!Enum.TryParse(className, out heroClass) && !Enum.TryParse(_game.Opponent.OriginalClass, out heroClass))
					return;
			}
			else if(!Enum.TryParse(_game.Opponent.OriginalClass, out heroClass))
				return;
			_game.SecretsManager.NewSecret(entity);
			if(card != null)
			{
				switch(fromZone)
				{
					case Zone.DECK:
						GameEvents.OnOpponentDeckToPlay.Execute(card);
						break;
					case Zone.HAND:
						GameEvents.OnOpponentPlay.Execute(card);
						break;
				}
			}
		}

		public void HandleOpponentPlayToHand(Entity entity, string? cardId, int turn, int id)
		{
			_game.Opponent.BoardToHand(entity, turn);
			Core.UpdateOpponentCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnOpponentPlayToHand.Execute(card);
		}

		public void HandleOpponentHandToDeck(Entity entity, string? cardId, IHsGameState gameState)
		{
			if(!string.IsNullOrEmpty(cardId) && entity.HasTag(GameTag.IS_USING_TRADE_OPTION))
			{
				_game.Opponent.PredictUniqueCardInDeck(cardId!, entity.Info.Created);
				entity.Info.GuessedCardState = GuessedCardState.None;
				entity.Info.Hidden = true;
				if(gameState.CurrentBlock != null)
					gameState.CurrentBlock.IsTradeableAction = true;
			}
			_game.Opponent.HandToDeck(entity, gameState.GetTurnNumber());
			Core.UpdateOpponentCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnOpponentHandToDeck.Execute(card);
		}

		public void HandleOpponentPlayToDeck(Entity entity, string? cardId, int turn)
		{
			_game.Opponent.BoardToDeck(entity, turn);
			Core.UpdateOpponentCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnOpponentPlayToDeck.Execute(card);
		}

		public void HandleOpponentSecretRemove(Entity entity, string? cardId, int turn)
		{
			if(!entity.IsSecret)
				return;
			_game.Opponent.RemoveFromPlay(entity, turn);
			_game.SecretsManager.RemoveSecret(entity);
			Core.UpdateOpponentCards();
		}

		public void HandleOpponentSecretTrigger(Entity entity, string? cardId, int turn, int otherId)
		{
			if (!entity.IsSecret)
				return;
			_game.Opponent.SecretTriggered(entity, turn);
			_game.Opponent.OpponentSecretTriggered(entity, turn);
			_game.SecretsManager.RemoveSecret(entity);
			Core.UpdateOpponentCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnOpponentSecretTriggered.Execute(card);
			if(_game.IsBattlegroundsMatch && Config.Instance.RunBobsBuddy && _game.CurrentGameStats != null)
				BobsBuddyInvoker.GetInstance(_game.CurrentGameStats.GameId, _game.GetTurnNumber())
					?.UpdateOpponentSecret(entity);
		}

		public void HandleOpponentDeckDiscard(Entity entity, string? cardId, int turn)
		{
			_game.Opponent.DeckDiscard(entity, turn);

			//there seems to be an issue with the overlay not updating here.
			//possibly a problem with order of logs?
			Core.UpdateOpponentCards();
			var card = Database.GetCardFromId(cardId);
			if(card != null)
				GameEvents.OnOpponentDeckDiscard.Execute(card);
		}

		void HandlePlayerLibramReduction(int value) => _game.Player.UpdateLibramReduction(value);

		void HandleOpponentLibramReduction(int value) => _game.Opponent.UpdateLibramReduction(value);

		void HandlePlayerHandCostReduction(int value)
		{
			foreach(var card in _game.Player.Hand)
				card.Info.CostReduction += value;
		}

		void HandleOpponentHandCostReduction(int value)
		{
			foreach(var card in _game.Opponent.Hand)
				card.Info.CostReduction += value;
		}

		void HandlePlayerAbyssalCurse(int value) => _game.Player.UpdateAbyssalCurse(value);
		void HandleOpponentAbyssalCurse(int value) => _game.Opponent.UpdateAbyssalCurse(value);

		#endregion

		public void HandleQuestRewardDatabaseId(int id, int value)
		{
			if(_game.IsBattlegroundsMatch && _game.Entities.TryGetValue(id, out var entity))
			{
				Core.Overlay.BattlegroundsQuestPickingViewModel.OnBattlegroundsQuest(entity).Forget();
			}
		}

		public void HandleBattlegroundsHeroReroll(Entity entity, string? oldCardId)
		{
			if(_game.IsBattlegroundsMatch)
			{
				var dbfId = oldCardId is string cardId ? Database.GetCardFromId(cardId)?.DbfId : null;
				if(dbfId is int theDbfId)
					Core.Overlay.InvalidateBattlegroundsHeroPickingStats(theDbfId);

				RefreshBattlegroundsHeroPickStats().Forget();
			}
		}

		#region IGameHandlerImplementation

		void IGameHandler.HandlePlayerBackToHand(Entity entity, string cardId, int turn) => HandlePlayerBackToHand(entity, cardId, turn);
		void IGameHandler.HandlePlayerDraw(Entity entity, string cardId, int turn) => HandlePlayerDraw(entity, cardId, turn);
		void IGameHandler.HandlePlayerMulligan(Entity entity, string cardId) => HandlePlayerMulligan(entity, cardId);
		void IGameHandler.HandlePlayerSecretPlayed(Entity entity, string cardId, int turn, Zone fromZone, string parentBlockCardId) => HandlePlayerSecretPlayed(entity, cardId, turn, fromZone, parentBlockCardId);
		void IGameHandler.HandlePlayerSecretTrigger(Entity entity, string? cardId, int turn, int otherId) => HandlePlayerSecretTrigger(entity, cardId, turn, otherId);
		void IGameHandler.HandlePlayerHandDiscard(Entity entity, string cardId, int turn) => HandlePlayerHandDiscard(entity, cardId, turn);
		void IGameHandler.HandlePlayerPlay(Entity entity, string cardId, int turn, string parentBlockCardId) => HandlePlayerPlay(entity, cardId, turn, parentBlockCardId);
		void IGameHandler.HandlePlayerDeckDiscard(Entity entity, string cardId, int turn) => HandlePlayerDeckDiscard(entity, cardId, turn);
		void IGameHandler.HandlePlayerHeroPower(string cardId, int turn) => HandlePlayerHeroPower(cardId, turn);
		void IGameHandler.HandleOpponentPlay(Entity entity, string? cardId, int @from, int turn) => HandleOpponentPlay(entity, cardId, @from, turn);
		void IGameHandler.HandleOpponentHandDiscard(Entity entity, string? cardId, int @from, int turn) => HandleOpponentHandDiscard(entity, cardId, @from, turn);
		void IGameHandler.HandleOpponentDraw(Entity entity, int turn, string? cardId, int? drawerId) => HandleOpponentDraw(entity, turn, cardId, drawerId);
		void IGameHandler.HandleOpponentMulligan(Entity entity, int @from) => HandleOpponentMulligan(entity, @from);
		void IGameHandler.HandleOpponentGet(Entity entity, int turn, int id) => HandleOpponentGet(entity, turn, id);
		void IGameHandler.HandleOpponentSecretPlayed(Entity entity, string? cardId, int @from, int turn, Zone fromZone, int otherId) => HandleOpponentSecretPlayed(entity, cardId, @from, turn, fromZone, otherId);
		void IGameHandler.HandleOpponentHandToDeck(Entity entity, string? cardId, IHsGameState gamestate) => HandleOpponentHandToDeck(entity, cardId, gamestate);
		void IGameHandler.HandleOpponentPlayToHand(Entity entity, string? cardId, int turn, int id) => HandleOpponentPlayToHand(entity, cardId, turn, id);
		void IGameHandler.HandleOpponentSecretTrigger(Entity entity, string? cardId, int turn, int otherId) => HandleOpponentSecretTrigger(entity, cardId, turn, otherId);
		void IGameHandler.HandleOpponentDeckDiscard(Entity entity, string? cardId, int turn) => HandleOpponentDeckDiscard(entity, cardId, turn);
		void IGameHandler.SetOpponentHero(string? cardId) => SetOpponentHero(cardId);
		void IGameHandler.SetPlayerHero(string? cardId) => SetPlayerHero(cardId);
		void IGameHandler.HandleOpponentHeroPower(string cardId, int turn) => HandleOpponentHeroPower(cardId, turn);
		void IGameHandler.HandlePlayerEntityChoices(IHsChoice choice) => HandlePlayerEntityChoices(choice);
		void IGameHandler.HandlePlayerEntitiesChosen(IHsCompletedChoice choice) => HandlePlayerEntitiesChosen(choice);
		void IGameHandler.HandleOpponentEntitiesChosen(IHsCompletedChoice choice) => HandleOpponentEntitiesChosen(choice);
		void IGameHandler.TurnStart(ActivePlayer player, int turnNumber) => TurnStart(player, turnNumber);
		void IGameHandler.HandleGameStart(DateTime timestamp) => HandleGameStart(timestamp);
		void IGameHandler.HandleGameEnd(bool stateComplete) => HandleGameEnd(stateComplete);
		void IGameHandler.HandleLoss() => HandleLoss();
		void IGameHandler.HandleWin() => HandleWin();
		void IGameHandler.HandleTied() => HandleTied();
		void IGameHandler.HandlePlayerGet(Entity entity, string cardId, int turn) => HandlePlayerGet(entity, cardId, turn);
		void IGameHandler.HandlePlayerPlayToDeck(Entity entity, string cardId, int turn) => HandlePlayerPlayToDeck(entity, cardId, turn);
		void IGameHandler.HandleOpponentPlayToDeck(Entity entity, string? cardId, int turn) => HandleOpponentPlayToDeck(entity, cardId, turn);
		void IGameHandler.HandlePlayerFatigue(int currentDamage) => HandlePlayerFatigue(currentDamage);
		void IGameHandler.HandleOpponentFatigue(int currentDamage) => HandleOpponentFatigue(currentDamage);
		void IGameHandler.HandlePlayerMaxHealthChange(int value) => HandlePlayerMaxHealthChange(value);
		void IGameHandler.HandleOpponentMaxHealthChange(int value) => HandleOpponentMaxHealthChange(value);
		void IGameHandler.HandlePlayerMaxManaChange(int value) => HandlePlayerMaxManaChange(value);
		void IGameHandler.HandleOpponentMaxManaChange(int value) => HandleOpponentMaxManaChange(value);
		void IGameHandler.HandlePlayerMaxHandSizeChange(int value) => HandlePlayerMaxHandSizeChange(value);
		void IGameHandler.HandleOpponentMaxHandSizeChange(int value) => HandleOpponentMaxHandSizeChange(value);
		void IGameHandler.HandlePlayerLibramReduction(int value) => HandlePlayerLibramReduction(value);
		void IGameHandler.HandleOpponentLibramReduction(int value) => HandleOpponentLibramReduction(value);
		void IGameHandler.HandlePlayerHandCostReduction(int value) => HandlePlayerHandCostReduction(value);
		void IGameHandler.HandleOpponentHandCostReduction(int value) => HandleOpponentHandCostReduction(value);
		void IGameHandler.HandleMercenariesStateChange() => HandleMercenariesStateChange();
		void IGameHandler.HandlePlayerDredge() => HandlePlayerDredge();
		void IGameHandler.HandlePlayerUnknownCardAddedToDeck() => HandlePlayerUnknownCardAddedToDeck();
		void IGameHandler.HandlePlayerAbyssalCurse(int value) => HandlePlayerAbyssalCurse(value);
		void IGameHandler.HandleOpponentAbyssalCurse(int value) => HandleOpponentAbyssalCurse(value);
		void IGameHandler.HandleBattlegroundsHeroReroll(Entity entity, string? oldCardId) => HandleBattlegroundsHeroReroll(entity, oldCardId);

		#endregion IGameHandlerImplementation
	}
}
