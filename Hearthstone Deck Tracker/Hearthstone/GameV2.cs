﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthDb.Enums;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPicking;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.QuestPicking;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Session;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Hearthstone.Secrets;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Live;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using HSReplay;
using HSReplay.OAuth.Data;

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
		private Dictionary<int, Dictionary<int, int>> _battlegroundsHeroLatestTavernUpTurn;
		private Dictionary<int, Dictionary<int, int>> _battlegroundsHeroTriplesByTier;
		internal QueueEvents QueueEvents { get; }

		public BattlegroundsSessionViewModel BattlegroundsSessionViewModel { get; } = new();
		public GameMetrics Metrics { get; private set; } = new();
		public GameV2()
		{
			Player = new Player(this, true);
			Opponent = new Player(this, false);
			IsInMenu = true;
			SecretsManager = new SecretsManager(this, new RemoteArenaSettings());
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
		public int OpponentMinionCount => Entities.Values.Count(x => x.IsInPlay && x.IsMinion && x.IsControlledBy(Opponent.Id));
		public int OpponentBoardCount => Entities.Values.Count(x => x.IsInPlay && x.IsMinionOrLocation && x.IsControlledBy(Opponent.Id));
		public int PlayerMinionCount => Entities.Values.Count(x => x.IsInPlay && x.IsMinion && x.IsControlledBy(Player.Id));
		public int PlayerBoardCount => Entities.Values.Count(x => x.IsInPlay && x.IsMinionOrLocation && x.IsControlledBy(Player.Id));
		public int OpponentHandCount => Entities.Values.Count(x => x.IsInHand && x.IsControlledBy(Opponent.Id));
		public int OpponentSecretCount => Entities.Values.Count(x => x.IsInSecret && x.IsSecret && x.IsControlledBy(Opponent.Id));
		public int PlayerHandCount => Entities.Values.Count(x => x.IsInHand && x.IsControlledBy(Player.Id));

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

		public bool IsBattlegroundsMatch => CurrentGameType == GameType.GT_BATTLEGROUNDS || CurrentGameType == GameType.GT_BATTLEGROUNDS_FRIENDLY;
		public bool IsMercenariesMatch => CurrentGameType == GameType.GT_MERCENARIES_AI_VS_AI || CurrentGameType == GameType.GT_MERCENARIES_FRIENDLY
					|| CurrentGameType == GameType.GT_MERCENARIES_PVE || CurrentGameType == GameType.GT_MERCENARIES_PVP
					|| CurrentGameType == GameType.GT_MERCENARIES_PVE_COOP;
		public bool IsMercenariesPvpMatch => CurrentGameType == GameType.GT_MERCENARIES_PVP;
		public bool IsMercenariesPveMatch => CurrentGameType == GameType.GT_MERCENARIES_PVE || CurrentGameType == GameType.GT_MERCENARIES_PVE_COOP;
		public bool IsConstructedMatch => CurrentGameType == GameType.GT_RANKED
										|| CurrentGameType == GameType.GT_CASUAL
										|| CurrentGameType == GameType.GT_VS_FRIEND;
		public bool IsFriendlyMatch => CurrentGameType == GameType.GT_VS_FRIEND;

		public Mode CurrentMode
		{
			get { return _currentMode; }
			set
			{
				_currentMode = value;
				Log.Info(value.ToString());
			}
		}

		private FormatType _currentFormat = FormatType.FT_UNKNOWN;
		public Format? CurrentFormat
		{
			get
			{
				if(_currentFormat == FormatType.FT_UNKNOWN)
					_currentFormat = (FormatType)HearthMirror.Reflection.Client.GetFormat();
				return HearthDbConverter.GetFormat(_currentFormat);
			}
		}

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

		public MatchInfo MatchInfo => _matchInfo ?? (_matchInfo = HearthMirror.Reflection.Client.GetMatchInfo());
		private bool _matchInfoCacheInvalid = true;

		public BrawlInfo BrawlInfo => _brawlInfo ?? (_brawlInfo = HearthMirror.Reflection.Client.GetBrawlInfo());

		public BattlegroundRatingInfo BattlegroundsRatingInfo => _battlegroundsRatingInfo ?? (_battlegroundsRatingInfo = HearthMirror.Reflection.Client.GetBattlegroundRatingInfo());

		public MercenariesRatingInfo MercenariesRatingInfo => _mercenariesRatingInfo ?? (_mercenariesRatingInfo = HearthMirror.Reflection.Client.GetMercenariesRatingInfo());

		public MercenariesMapInfo MercenariesMapInfo => HearthMirror.Reflection.Client.GetMercenariesMapInfo();

		private bool IsValidPlayerInfo(MatchInfo.Player playerInfo, bool allowMissing = true)
		{
			var name = playerInfo?.Name ?? playerInfo?.BattleTag?.Name;
			var valid = allowMissing || name != null;
			Log.Debug($"valid={valid}, gameMode={CurrentGameMode}, player={name}, starLevel={playerInfo?.Standard?.StarLevel}");
			return valid;
		}

		internal async void CacheMatchInfo()
		{
			if(!_matchInfoCacheInvalid)
				return;
			MatchInfo matchInfo;
			while((matchInfo = HearthMirror.Reflection.Client.GetMatchInfo()) == null || !IsValidPlayerInfo(matchInfo.LocalPlayer) || !IsValidPlayerInfo(matchInfo.OpposingPlayer, IsMercenariesMatch))
			{
				Log.Info($"Waiting for matchInfo... (matchInfo={matchInfo}, localPlayer={matchInfo?.LocalPlayer?.Name}, opposingPlayer={matchInfo?.OpposingPlayer?.Name})");
				await Task.Delay(1000);
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
			_currentFormat = FormatType.FT_UNKNOWN;
			if(!IsInMenu && resetStats)
				CurrentGameStats = new GameStats(GameResult.None, "", "") {PlayerName = "", OpponentName = "", Region = CurrentRegion};
			PowerLog.Clear();
			_battlegroundsBoardState?.Reset();
			_battlegroundsHeroLatestTavernUpTurn = new Dictionary<int, Dictionary<int, int>>();
			_battlegroundsHeroTriplesByTier = new Dictionary<int, Dictionary<int, int>>();
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

		public BoardSnapshot? GetBattlegroundsBoardStateFor(string? cardId) => _battlegroundsBoardState?.GetSnapshot(cardId);

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
			var heroCurrentTier = Entities[id].GetTag(GameTag.PLAYER_TECH_LEVEL);
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
	}
}
