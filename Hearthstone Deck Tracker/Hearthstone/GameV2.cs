#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using HearthDb.Deckstrings;
using HearthDb.Enums;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPicking;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.QuestPicking;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Session;
using Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem;
using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;
using Hearthstone_Deck_Tracker.Hearthstone.Secrets;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Live;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using HSReplay;
using HSReplay.OAuth.Data;
using HSReplay.Requests;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class GameV2 : IGame
	{
		public readonly List<long> IgnoredArenaDecks = new();
		private GameMode _currentGameMode = GameMode.None;
		private bool? _spectator;
		private MatchInfo? _matchInfo;
		private Mode _currentMode;
		private BrawlInfo? _brawlInfo;
		private BattlegroundRatingInfo? _battlegroundsRatingInfo;
		private MercenariesRatingInfo? _mercenariesRatingInfo;
		private BattlegroundsBoardState? _battlegroundsBoardState;
		public BattlegroundsDuosBoardState? BattlegroundsDuosBoardState { get; set; }
		private Dictionary<int, Dictionary<int, int>> _battlegroundsHeroLatestTavernUpTurn;
		private Dictionary<int, Dictionary<int, int>> _battlegroundsHeroTriplesByTier;
		private MulliganGuideParams? _mulliganGuideParams;
		internal QueueEvents QueueEvents { get; }

		public BattlegroundsSessionViewModel BattlegroundsSessionViewModel { get; } = new();
		public GameMetrics Metrics { get; private set; } = new();
		public ActiveEffects ActiveEffects { get; }
		public CounterManager CounterManager { get; }
		public RelatedCardsManager RelatedCardsManager { get; }
		public GameV2()
		{
			Player = new Player(this, true);
			Opponent = new Player(this, false);
			IsInMenu = true;
			SecretsManager = new SecretsManager(this, new RemoteArenaSettings());
			ActiveEffects = new ActiveEffects();
			CounterManager = new CounterManager(this);
			RelatedCardsManager = new RelatedCardsManager();
			_battlegroundsBoardState = new BattlegroundsBoardState(this);
			_battlegroundsHeroLatestTavernUpTurn = new Dictionary<int, Dictionary<int, int>>();
			_battlegroundsHeroTriplesByTier = new Dictionary<int, Dictionary<int, int>>();
			QueueEvents = new(this);
			Reset();
			LiveDataManager.OnStreamingChecked += async streaming =>
			{
				MetaData.TwitchVodData = await UpdateTwitchVodData(streaming);
			};
		}

		private async Task<UploadMetaData.TwitchVodData?> UpdateTwitchVodData(bool streaming)
		{
			if(!streaming)
				return null;
			bool Selected(TwitchAccount x) => x.Id == Config.Instance.SelectedTwitchUser;
			var user = HSReplayNetOAuth.TwitchUsers?.FirstOrDefault(Selected);
			if(user == null)
				return null;
			var currentVideo = await HSReplayNetOAuth.GetCurrentVideo(user.Id);
			if(currentVideo == null)
				return null;
			return new UploadMetaData.TwitchVodData
			{
				ChannelName = user.Username,
				Url = currentVideo.Url,
				Language = currentVideo.Language
			};
		}

		public List<string> PowerLog { get; } = new List<string>();
		public Deck? IgnoreIncorrectDeck { get; set; }
		public GameTime GameTime { get; } = new GameTime();
		public bool IsMinionInPlay => Entities.Values.FirstOrDefault(x => x.IsInPlay && x.IsMinion) != null;
		public bool IsOpponentMinionInPlay => Entities.Values.FirstOrDefault(x => x.IsInPlay && x.IsMinion && x.IsControlledBy(Opponent.Id)) != null;
		public int OpponentMinionCount => Entities.Values.Count(x => x.IsInPlay && x.IsMinion && !x.HasTag(GameTag.UNTOUCHABLE) && x.IsControlledBy(Opponent.Id));
		public int OpponentBoardCount => Entities.Values.Count(x => x.IsInPlay && x.TakesBoardSlot && x.IsControlledBy(Opponent.Id));
		public int PlayerMinionCount => Entities.Values.Count(x => x.IsInPlay && x.IsMinion && x.IsControlledBy(Player.Id));
		public int PlayerBoardCount => Entities.Values.Count(x => x.IsInPlay && x.TakesBoardSlot && x.IsControlledBy(Player.Id));
		public int OpponentHandCount => Entities.Values.Count(x => x.IsInHand && x.IsControlledBy(Opponent.Id));
		public int OpponentSecretCount => Entities.Values.Count(x => x.IsInSecret && x.IsSecret && x.IsControlledBy(Opponent.Id));
		public int PlayerHandCount => Entities.Values.Count(x => x.IsInHand && x.IsControlledBy(Player.Id));

		public int PrimaryPlayerId { get; set; }
		public Player Player { get; set; }
		public Player Opponent { get; set; }
		public bool IsInMenu { get; set; }
		public bool IsUsingPremade { get; set; }
		public bool IsRunning { get; set; }
		public Region CurrentRegion { get; set; }
		public GameStats? CurrentGameStats { get; set; }
		public HearthMirror.Objects.Deck? CurrentSelectedDeck { get; set; }
		public SecretsManager SecretsManager { get; }
		public List<Card>? DrawnLastGame { get; set; }
		public Dictionary<int, Entity> Entities { get; } = new Dictionary<int, Entity>();
		public GameMetaData MetaData { get; } = new GameMetaData();
		internal List<Tuple<uint, List<string>>> StoredPowerLogs { get; } = new List<Tuple<uint, List<string>>>();
		internal Dictionary<int, string> StoredPlayerNames { get; } = new Dictionary<int, string>();
		internal GameStats? StoredGameStats { get; set; }
		public int ProposedAttacker { get; set; }
		public int ProposedDefender { get; set; }
		public bool SetupDone { get; set; }

		public bool PlayerChallengeable => CurrentMode == Mode.HUB || CurrentMode == Mode.TOURNAMENT || CurrentMode == Mode.ADVENTURE
					|| CurrentMode == Mode.TAVERN_BRAWL || CurrentMode == Mode.DRAFT || CurrentMode == Mode.PACKOPENING
					|| CurrentMode == Mode.COLLECTIONMANAGER || CurrentMode == Mode.BACON;

		public bool? IsDungeonMatch => string.IsNullOrEmpty(CurrentGameStats?.OpponentHeroCardId) || CurrentGameType == GameType.GT_UNKNOWN ? (bool?)null
			: CurrentGameType == GameType.GT_VS_AI && DungeonRun.IsDungeonBoss(CurrentGameStats?.OpponentHeroCardId);

		public bool IsBattlegroundsMatch => IsBattlegroundsSoloMatch || IsBattlegroundsDuosMatch;
		public bool IsBattlegroundsSoloMatch =>
			CurrentGameType is
				GameType.GT_BATTLEGROUNDS or
				GameType.GT_BATTLEGROUNDS_FRIENDLY or
				GameType.GT_BATTLEGROUNDS_AI_VS_AI or
				GameType.GT_BATTLEGROUNDS_PLAYER_VS_AI;

		public bool IsBattlegroundsDuosMatch =>
			CurrentGameType is
				GameType.GT_BATTLEGROUNDS_DUO or
				GameType.GT_BATTLEGROUNDS_DUO_VS_AI or
				GameType.GT_BATTLEGROUNDS_DUO_FRIENDLY or
				GameType.GT_BATTLEGROUNDS_DUO_AI_VS_AI;

		public bool IsMercenariesMatch => CurrentGameType == GameType.GT_MERCENARIES_AI_VS_AI || CurrentGameType == GameType.GT_MERCENARIES_FRIENDLY
					|| CurrentGameType == GameType.GT_MERCENARIES_PVE || CurrentGameType == GameType.GT_MERCENARIES_PVP
					|| CurrentGameType == GameType.GT_MERCENARIES_PVE_COOP;
		public bool IsMercenariesPvpMatch => CurrentGameType == GameType.GT_MERCENARIES_PVP;
		public bool IsMercenariesPveMatch => CurrentGameType == GameType.GT_MERCENARIES_PVE || CurrentGameType == GameType.GT_MERCENARIES_PVE_COOP;
		public bool IsConstructedMatch => CurrentGameType == GameType.GT_RANKED
										|| CurrentGameType == GameType.GT_CASUAL
										|| CurrentGameType == GameType.GT_VS_FRIEND;
		public bool IsArenaMatch => CurrentGameType == GameType.GT_ARENA;
		public bool IsFriendlyMatch => CurrentGameType == GameType.GT_VS_FRIEND;

		public bool IsTraditionalHearthstoneMatch => !IsBattlegroundsMatch && !IsMercenariesMatch;

		public Mode CurrentMode
		{
			get { return _currentMode; }
			set
			{
				_currentMode = value;
				Log.Info(value.ToString());
			}
		}

		private FormatType _currentFormatType = FormatType.FT_UNKNOWN;
		public FormatType CurrentFormatType
		{
			get
			{
				if(_currentFormatType == FormatType.FT_UNKNOWN)
					_currentFormatType = (FormatType)HearthMirror.Reflection.Client.GetFormat();
				return _currentFormatType;
			}
		}

		public Format? CurrentFormat => HearthDbConverter.GetFormat(CurrentFormatType);

		public Mode PreviousMode { get; set; }

		public bool SavedReplay { get; set; }

		public Entity? PlayerEntity => Entities.Values.FirstOrDefault(x => x.IsPlayer);

		public Entity? OpponentEntity => Entities.Values.FirstOrDefault(x => x.HasTag(GameTag.PLAYER_ID) && !x.IsPlayer);

		public Entity? GameEntity => Entities.Values.FirstOrDefault(x => x.Name == "GameEntity");

		public bool IsMulliganDone
		{
			get
			{
				if(IsBattlegroundsMatch)
					return true;
				var player = Entities.FirstOrDefault(x => x.Value.IsPlayer);
				var opponent = Entities.FirstOrDefault(x => x.Value.HasTag(GameTag.PLAYER_ID) && !x.Value.IsPlayer);
				if(player.Value == null || opponent.Value == null)
					return false;
				return player.Value.GetTag(GameTag.MULLIGAN_STATE) == (int)Mulligan.DONE
					   && opponent.Value.GetTag(GameTag.MULLIGAN_STATE) == (int)Mulligan.DONE;
			}
		}

		public bool Spectator => _spectator ?? (bool)(_spectator = HearthMirror.Reflection.Client.IsSpectating());

		public GameMode CurrentGameMode
		{
			get
			{
				if(Spectator)
					return GameMode.Spectator;
				if(_currentGameMode == GameMode.None)
					_currentGameMode = HearthDbConverter.GetGameMode(CurrentGameType);
				return _currentGameMode;
			}
		}

		private GameType _currentGameType;
		public GameType CurrentGameType
		{
			get
			{
				if(_currentGameType != GameType.GT_UNKNOWN)
					return _currentGameType;
				if(_currentMode == Mode.GAMEPLAY)
				{
					_currentGameType = (GameType)HearthMirror.Reflection.Client.GetGameType();
					return _currentGameType;
				}
				return GameType.GT_UNKNOWN;
			}
		}

		public MatchInfo MatchInfo => _matchInfo ??= HearthMirror.Reflection.Client.GetMatchInfo();
		private bool _matchInfoCacheInvalid = true;

		public MatchInfo.MedalInfo? PlayerMedalInfo
		{
			get
			{
				var localPlayer = MatchInfo?.LocalPlayer;
				if(localPlayer is null || CurrentGameType != GameType.GT_RANKED)
					return null;

				return CurrentFormat switch
				{
					Format.Wild => localPlayer.Wild,
					Format.Classic => localPlayer.Classic,
					Format.Twist => localPlayer.Twist,
					Format.Standard => localPlayer.Standard,
					_ => null,
				};
			}
		}

		public BrawlInfo? BrawlInfo => _brawlInfo ?? (_brawlInfo = HearthMirror.Reflection.Client.GetBrawlInfo());

		public BattlegroundRatingInfo? BattlegroundsRatingInfo => _battlegroundsRatingInfo ?? (_battlegroundsRatingInfo = HearthMirror.Reflection.Client.GetBattlegroundRatingInfo());

		public int? CurrentBattlegroundsRating => IsBattlegroundsMatch
			? (IsBattlegroundsDuosMatch ? BattlegroundsRatingInfo?.DuosRating : BattlegroundsRatingInfo?.Rating)
			: null;

		public MercenariesRatingInfo? MercenariesRatingInfo => _mercenariesRatingInfo ?? (_mercenariesRatingInfo = HearthMirror.Reflection.Client.GetMercenariesRatingInfo());

		public MercenariesMapInfo MercenariesMapInfo => HearthMirror.Reflection.Client.GetMercenariesMapInfo();

		private bool IsValidPlayerInfo(MatchInfo.Player? playerInfo, bool allowMissing = true)
		{
			var name = playerInfo?.Name ?? playerInfo?.BattleTag?.Name;
			var valid = allowMissing || name != null;
			Log.Debug($"valid={valid}, gameMode={CurrentGameMode}, player={name}, starLevel={playerInfo?.Standard?.StarLevel}");
			return valid;
		}

		private bool IsMedalInfoPresent(MatchInfo.Player? playerInfo)
		{
			return playerInfo?.Standard != null || playerInfo?.Wild != null || playerInfo?.Classic != null || playerInfo?.Twist != null;
		}

		internal async void CacheMatchInfo()
		{
			if(!_matchInfoCacheInvalid)
				return;

			var missingMedals = 0;
			MatchInfo? matchInfo = null;
			for(var i = 0; i <= 30; i++)
			{
				if(i > 0)
				{
					Log.Info($"Waiting for matchInfo... (matchInfo={matchInfo}, localPlayer={matchInfo?.LocalPlayer?.Name}, opposingPlayer={matchInfo?.OpposingPlayer?.Name})");
					await Task.Delay(1000);
				}

				matchInfo = HearthMirror.Reflection.Client.GetMatchInfo();
				if(matchInfo == null)
					continue;

				// the player info will probably arrive shortly
				if(!IsValidPlayerInfo(matchInfo.LocalPlayer) || !IsValidPlayerInfo(matchInfo.OpposingPlayer, IsMercenariesMatch))
					continue;

				// sometimes the opponent player info is incomplete for a moment, give it a chance
				if(missingMedals++ < 2 && IsMedalInfoPresent(matchInfo.LocalPlayer) != IsMedalInfoPresent(matchInfo.OpposingPlayer))
					continue;

				// looking good
				break;
			}

			if(matchInfo == null)
			{
				Log.Info("Giving up waiting for matchInfo");
				return;
			}

			_matchInfo = matchInfo;
			UpdatePlayers(matchInfo);
			_matchInfoCacheInvalid = false;
		}

		private void UpdatePlayers(MatchInfo matchInfo)
		{
			string GetName(MatchInfo.Player player)
			{
				if(player.BattleTag != null)
					return $"{player.BattleTag.Name}#{player.BattleTag.Number}";
				return player.Name;
			}
			Player.Name = GetName(matchInfo.LocalPlayer);
			Opponent.Name = GetName(matchInfo.OpposingPlayer);
			Player.Id = matchInfo.LocalPlayer.Id;
			Opponent.Id = matchInfo.OpposingPlayer.Id;
			Log.Info($"{Player.Name} [PlayerId={Player.Id}] vs {Opponent.Name} [PlayerId={Opponent.Id}]");
		}

		internal async void CacheGameType()
		{
			while((_currentGameType = (GameType)HearthMirror.Reflection.Client.GetGameType()) == GameType.GT_UNKNOWN)
				await Task.Delay(1000);
		}

		internal void CacheSpectator() => _spectator = HearthMirror.Reflection.Client.IsSpectating();

		internal void CacheBrawlInfo() => _brawlInfo = HearthMirror.Reflection.Client.GetBrawlInfo();

		internal void CacheBattlegroundRatingInfo() => _battlegroundsRatingInfo = HearthMirror.Reflection.Client.GetBattlegroundRatingInfo();

		internal void CacheMercenariesRatingInfo() => _mercenariesRatingInfo = HearthMirror.Reflection.Client.GetMercenariesRatingInfo();

		internal void InvalidateMatchInfoCache() => _matchInfoCacheInvalid = true;

		public void Reset(bool resetStats = true)
		{
			Log.Info("-------- Reset ---------");

			Player.Reset();
			Opponent.Reset();
			ActiveEffects.Reset();
			RelatedCardsManager.Reset();
			if(!_matchInfoCacheInvalid && MatchInfo?.LocalPlayer != null && MatchInfo.OpposingPlayer != null)
				UpdatePlayers(MatchInfo);
			ProposedAttacker = 0;
			ProposedDefender = 0;
			Entities.Clear();
			SavedReplay = false;
			SecretsManager.Reset();
			SetupDone = false;
			_spectator = null;
			_currentGameMode = GameMode.None;
			_currentGameType = GameType.GT_UNKNOWN;
			_currentFormatType = FormatType.FT_UNKNOWN;
			if(!IsInMenu && resetStats)
				CurrentGameStats = new GameStats(GameResult.None, "", "") {PlayerName = "", OpponentName = "", Region = CurrentRegion};
			PowerLog.Clear();
			_battlegroundsBoardState?.Reset();
			BattlegroundsDuosBoardState = null;
			_battlegroundsHeroLatestTavernUpTurn = new Dictionary<int, Dictionary<int, int>>();
			_battlegroundsHeroTriplesByTier = new Dictionary<int, Dictionary<int, int>>();
			_mulliganGuideParams = null;
			_mulliganState = null;
			_battlegroundsHeroPickStatsParams = null;
			_battlegroundsHeroPickState = null;
			BattlegroundsTrinketPickStates.Clear();
			Metrics = new GameMetrics();

			if(Core._game != null && Core.Overlay != null)
			{
				Core.UpdatePlayerCards(true);
				Core.UpdateOpponentCards(true);
			}
		}

		public void StoreGameState()
		{
			if(MetaData.ServerInfo == null || MetaData.ServerInfo.GameHandle == 0)
				return;
			Log.Info($"Storing PowerLog for gameId={MetaData.ServerInfo.GameHandle}");
			StoredPowerLogs.Add(new Tuple<uint, List<string>>(MetaData.ServerInfo.GameHandle, new List<string>(PowerLog)));
			if(Player.Id != -1 && !StoredPlayerNames.ContainsKey(Player.Id) && Player.Name != null)
				StoredPlayerNames.Add(Player.Id, Player.Name);
			if(Opponent.Id != -1 && !StoredPlayerNames.ContainsKey(Opponent.Id) && Opponent.Name != null)
				StoredPlayerNames.Add(Opponent.Id, Opponent.Name);
			if(StoredGameStats == null)
				StoredGameStats = CurrentGameStats;
		}

		public string GetStoredPlayerName(int id) => StoredPlayerNames.TryGetValue(id, out var name) ? name : string.Empty;

		internal void ResetStoredGameState()
		{
			StoredPowerLogs.Clear();
			StoredPlayerNames.Clear();
			StoredGameStats = null;
		}

		public int GetTurnNumber()
		{
			if (!IsMulliganDone)
				return 0;
			return (GameEntity?.GetTag(GameTag.TURN) + 1) / 2 ?? 0;
		}

		//We do count+1 because the friendly hero is not in setaside
		public int BattlegroundsHeroCount() => Entities.Values.Where(x =>
			x.IsHero &&
			x.IsInSetAside &&
			// BACON_HERO_CAN_BE_DRAFTED and BACON_SKIN cover most cases including early disconnects,
			// and PLAYER_TECH_LEVEL helps to account for the Aranna transformation.
			(x.HasTag(GameTag.BACON_HERO_CAN_BE_DRAFTED) || x.HasTag(GameTag.BACON_SKIN) || x.HasTag(GameTag.PLAYER_TECH_LEVEL))
		).Count() + 1;

		public void SnapshotBattlegroundsBoardState() => _battlegroundsBoardState?.SnapshotCurrentBoard();

		public BoardSnapshot? GetBattlegroundsBoardStateFor(int id) => _battlegroundsBoardState?.GetSnapshot(id);

		public void UpdateBattlegroundsPlayerTechLevel(int id, int value)
		{
			if(!_battlegroundsHeroLatestTavernUpTurn.ContainsKey(id))
				_battlegroundsHeroLatestTavernUpTurn[id] = new Dictionary<int, int>();
			if (value > 1)
				_battlegroundsHeroLatestTavernUpTurn[id][value] = GetTurnNumber();
		}

		public Dictionary<int, int>? GetBattlegroundsHeroLatestTavernUpTurn(int id)
		{
			return _battlegroundsHeroLatestTavernUpTurn.TryGetValue(id, out var data) ? data : null;
		}

		public void UpdateBattlegroundsPlayerTriples(int id, int value)
		{
			if(!Entities.TryGetValue(id, out var entity))
				return;

			var heroCurrentTier = entity.GetTag(GameTag.PLAYER_TECH_LEVEL);
			if(!_battlegroundsHeroTriplesByTier.ContainsKey(id))
				_battlegroundsHeroTriplesByTier[id] = new Dictionary<int, int>();

			var previousTiersTriples = _battlegroundsHeroTriplesByTier[id].Where(s => s.Key < heroCurrentTier)
				.Select(s => s.Value)
				.Sum();
			_battlegroundsHeroTriplesByTier[id][heroCurrentTier] = value - previousTiersTriples;
		}

		public Dictionary<int, int>? GetBattlegroundsHeroTriplesByTier(int id)
		{
			return _battlegroundsHeroTriplesByTier.TryGetValue(id, out var data) ? data : null;
		}

		private MulliganState? _mulliganState;
		private MulliganState MulliganState
		{
			get
			{
				if(_mulliganState == null)
				{
					_mulliganState = new MulliganState(this);
				}

				return _mulliganState;
			}
		}

		public List<Entity> SnapshotMulligan() => MulliganState.SnapshotMulligan();
		public List<Entity> SnapshotMulliganChoices(IHsCompletedChoice choice) => MulliganState.SnapshotMulliganChoices(choice);
		public List<Entity> SnapshotOpeningHand() => MulliganState.SnapshotOpeningHand();

		public void CacheMulliganGuideParams()
		{
			if(_mulliganGuideParams != null)
				return;

			var activeDeck = DeckList.Instance.ActiveDeck;
			if(activeDeck == null)
				return;

			try
			{
				var opponentClass = Opponent.PlayerEntities.FirstOrDefault(x => x.IsHero && x.IsInPlay)?.Card.CardClass ?? CardClass.INVALID;
				var starLevel = PlayerMedalInfo?.StarLevel ?? 0;
				var starsPerWin = PlayerMedalInfo?.StarsPerWin ?? 0;

				_mulliganGuideParams = new MulliganGuideParams
				{
					Deckstring = DeckSerializer.Serialize(HearthDbConverter.ToHearthDbDeck(activeDeck), false),
					OpponentClass = opponentClass.ToString(),
					PlayerInitiative = PlayerEntity?.GetTag(GameTag.FIRST_PLAYER) == 1 ? "FIRST" : "COIN",
					PlayerRegion = ((BnetRegion)CurrentRegion).ToString(),
					PlayerStarLevel = starLevel > 0 ? starLevel : null,
					PlayerStarMultiplier = starsPerWin > 0 ? starsPerWin : null,
					GameType = (int)HearthDbConverter.GetBnetGameType(CurrentGameType, CurrentFormat),
					FormatType = (int)CurrentFormatType,
				};
			}
			catch(Exception e)
			{
				Log.Error(e);
				Influx.OnMulliganGuideDeckSerializationError(e.GetType().Name, e.Message);
			}
		}

		public MulliganGuideParams? GetMulliganGuideParams()
		{
			return _mulliganGuideParams;
		}

		public MulliganGuideFeedbackParams? GetMulliganGuideFeedbackParams()
		{
			// Use a cached version because e.g. the opponentClass may no longer be detectable
			return _mulliganGuideParams?.WithFeedback(
				MulliganState.OfferedCards?.Select(x => x.Card.DbfId).ToArray(),
				MulliganState.KeptCards?.Select(x => x.Card.DbfId).ToArray(),
				MulliganState.FinalCardsInHand?.Select(x => x.Card.DbfId).ToArray(),
				Metrics.ConstructedMulliganGuideOverlayDisplayed,
				PlayerEntity?.GetTag(GameTag.PLAYSTATE) ?? 0
			);
		}

		public List<Entity>? GetMulliganSwappedCards()
		{
			var offered = MulliganState.OfferedCards;
			var kept = MulliganState.KeptCards;

			if(offered is null || kept is null)
				return null;

			// assemble a list of cards that were
			var retval = new List<Entity>();
			foreach(var card in offered)
			{
				if(!kept.Contains(card))
					retval.Add(card);
			}

			return retval;
		}

		public Dictionary<int, SingleCardStats>? MulliganCardStats { get; set; } = null;

		private BattlegroundsHeroPickStatsParams? _battlegroundsHeroPickStatsParams;

		public void CacheBattlegroundsHeroPickParams(bool isReroll)
		{
			if(_battlegroundsHeroPickStatsParams != null)
			{
				// Already set? Probably a reroll - just update the hero dbf ids
				var newHeroDbfIds = BattlegroundsHeroPickState.OfferedHeroDbfIds;
				if(newHeroDbfIds == null)
					return;

				_battlegroundsHeroPickStatsParams = new BattlegroundsHeroPickStatsParams
				{
					HeroDbfIds = newHeroDbfIds,
					BattlegroundsRaces = _battlegroundsHeroPickStatsParams.BattlegroundsRaces,
					AnomalyDbfId = BattlegroundsUtils.GetBattlegroundsAnomalyDbfId(Core.Game.GameEntity),
					LanguageCode = Helper.GetCardLanguage(),
					BattlegroundsRating = Core.Game.CurrentBattlegroundsRating,
					IsReroll = isReroll,
				};
				return;
			}

			var availableRaces = BattlegroundsUtils.GetAvailableRaces();
			if(availableRaces == null)
				return;

			var heroDbfIds = BattlegroundsHeroPickState.OfferedHeroDbfIds;
			if(heroDbfIds == null)
				return;

			_battlegroundsHeroPickStatsParams = new BattlegroundsHeroPickStatsParams
			{
				HeroDbfIds = heroDbfIds,
				BattlegroundsRaces = availableRaces.Cast<int>().ToArray(),
				AnomalyDbfId = BattlegroundsUtils.GetBattlegroundsAnomalyDbfId(Core.Game.GameEntity),
				LanguageCode = Helper.GetCardLanguage(),
				BattlegroundsRating = Core.Game.CurrentBattlegroundsRating,
				IsReroll = isReroll,
			};
		}

		public BattlegroundsHeroPickStatsParams? GetBattlegroundsHeroPickParams()
		{
			return _battlegroundsHeroPickStatsParams;
		}

		private BattlegroundsHeroPickState? _battlegroundsHeroPickState;
		private BattlegroundsHeroPickState BattlegroundsHeroPickState
		{
			get
			{
				if(_battlegroundsHeroPickState == null)
				{
					_battlegroundsHeroPickState = new BattlegroundsHeroPickState(this);
				}

				return _battlegroundsHeroPickState;
			}
		}

		public void SnapshotBattlegroundsOfferedHeroes(IEnumerable<Entity> heroes) => BattlegroundsHeroPickState.SnapshotOfferedHeroes(heroes);
		public void SnapshotBattlegroundsHeroPick() => BattlegroundsHeroPickState.SnapshotPickedHero();

		public BattlegroundsHeroPickFeedbackParams? GetBattlegroundsHeroPickFeedbackParams(int finalPlacement)
		{
			if(
				BattlegroundsHeroPickState.PickedHeroDbfId is int heroDbfId &&
				finalPlacement > 0
			)
				return _battlegroundsHeroPickStatsParams?.WithFeedback(
					finalPlacement,
					heroDbfId,
					Metrics.Tier7HeroOverlayDisplayed
				);

			return null;
		}

		public bool DuosWasPlayerHeroModified { get; private set; }
		public bool DuosWasOpponentHeroModified { get; private set; }

		public void DuosSetHeroModified(bool isPlayer)
		{
			if(isPlayer)
			{
				DuosWasPlayerHeroModified = true;
			}
			else
			{
				DuosWasOpponentHeroModified = true;
			}
		}

		public void DuosResetHeroTracking()
		{
			DuosWasPlayerHeroModified = false;
			DuosWasOpponentHeroModified = false;
		}

		public bool BattlegroundsBuddiesEnabled => GameEntity?.GetTag(GameTag.BACON_BUDDY_ENABLED) > 0;

		private List<BattlegroundsTrinketPickState> BattlegroundsTrinketPickStates { get; } = new();

		public BattlegroundsTrinketPickParams? SnapshotOfferedTrinkets(IHsChoice choice)
		{
			var availableRaces = BattlegroundsUtils.GetAvailableRaces();
			if(availableRaces == null)
				return null;

			var hero = Entities.Values.FirstOrDefault(x => x.IsPlayer && x.IsHero);
			var heroCardId = hero?.CardId != null ? BattlegroundsUtils.GetOriginalHeroId(hero.CardId) : null;
			var heroCard = heroCardId != null ? Database.GetCardFromId(heroCardId) : null;
			if(heroCard == null)
				return null;

			if(!Entities.TryGetValue(choice.SourceEntityId, out var sourceEntity))
				return null;

			var offeredTrinkets = choice.OfferedEntityIds
				.Select(id => Entities.TryGetValue(id, out var entity) ? entity : null)
				.WhereNotNull()
				.Select(entity => new BattlegroundsTrinketPickParams.OfferedTrinket()
				{
					TrinketDbfId = entity.Card.DbfId,
					ExtraData = entity.Tags.TryGetValue(GameTag.TAG_SCRIPT_DATA_NUM_1, out var value) ? value : 0,
				})
				.ToArray();
			if(offeredTrinkets.Length == 0)
				return null;

			var parameters = new BattlegroundsTrinketPickParams()
			{
				HeroDbfId = heroCard.DbfId,
				HeroPowerDbfIds = Core.Game.Player.PastHeroPowers.Select(x => Database.GetCardFromId(x)?.DbfId).Where(x => x.HasValue).Cast<int>().ToArray(),
				Turn = Core.Game.GetTurnNumber(),
				SourceDbfId = sourceEntity.Card.DbfId,
				MinionTypes = availableRaces.Cast<int>().ToArray(),
				AnomalyDbfId = BattlegroundsUtils.GetBattlegroundsAnomalyDbfId(Core.Game.GameEntity),
				LanguageCode = Helper.GetCardLanguage(),
				BattlegroundsRating = Core.Game.CurrentBattlegroundsRating,
				OfferedTrinkets = offeredTrinkets,
				GameType = (int)HearthDbConverter.GetBnetGameType(Core.Game.CurrentGameType, Core.Game.CurrentFormat)
			};

			BattlegroundsTrinketPickStates.Add(new BattlegroundsTrinketPickState(choice.Id, parameters));

			return parameters;
		}

		public void SnapshotChosenTrinket(IHsCompletedChoice choice)
		{
			if(BattlegroundsTrinketPickStates.Count == 0)
				return;
			var state = BattlegroundsTrinketPickStates.Last();
			if(state.ChoiceId != choice.Id)
				return;

			if(choice.ChosenEntityIds.Count() != 1 || !Entities.TryGetValue(choice.ChosenEntityIds.Single(), out var chosen))
				return;

			state.PickTrinket(chosen);
		}

		public bool IsTrinketChoiceComplete(int choiceId)
		{
			if(BattlegroundsTrinketPickStates.Count == 0)
				return false;
			var state = BattlegroundsTrinketPickStates.Last();
			if(choiceId > state.ChoiceId)
				return false;
			if(choiceId < state.ChoiceId)
				return true;

			return state.ChosenTrinketDbfId != null;
		}

		public List<BattlegroundsTrinketPickFeedbackParams> GetTrinketPickingFeedback(int finalPlacement)
		{
			return BattlegroundsTrinketPickStates
				.Select(x => x.ChosenTrinketDbfId is int dbfId ? x.Params.WithFeedback(
					finalPlacement,
					dbfId,
					Metrics.Tier7TrinketOverlayDisplayed
				) : null)
				.WhereNotNull()
				.ToList();
		}
	}
}
