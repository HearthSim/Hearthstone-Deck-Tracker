#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthDb.Enums;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Hearthstone.Secrets;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Live;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.Twitch;
using HSReplay;
using HSReplay.OAuth.Data;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class GameV2 : IGame
	{
		public readonly List<long> IgnoredArenaDecks = new List<long>();
		private GameMode _currentGameMode = GameMode.None;
		private bool? _spectator;
		private MatchInfo _matchInfo;
		private Mode _currentMode;
		private BrawlInfo _brawlInfo;

		public GameV2()
		{
			Player = new Player(this, true);
			Opponent = new Player(this, false);
			IsInMenu = true;
			SecretsManager = new SecretsManager(this);
			Reset();
			LiveDataManager.OnStreamingChecked += async streaming =>
			{
				MetaData.TwitchVodData = await UpdateTwitchVodData(streaming);
			};
		}

		private async Task<UploadMetaData.TwitchVodData> UpdateTwitchVodData(bool streaming)
		{
			if(!streaming)
				return null;
			bool Selected(TwitchAccount x) => x.Id == Config.Instance.SelectedTwitchUser;
			var user = HSReplayNetOAuth.TwitchUsers?.FirstOrDefault(Selected);
			if(user == null)
				return null;
			var url = await TwitchApi.GetVodUrl(user.Id);
			if(url == null)
				return null;
			return new UploadMetaData.TwitchVodData
			{
				ChannelName = user.Username,
				Url = url
			};
		}

		public List<string> PowerLog { get; } = new List<string>();
		public Deck IgnoreIncorrectDeck { get; set; }
		public GameTime GameTime { get; } = new GameTime();
		public bool IsMinionInPlay => Entities.FirstOrDefault(x => (x.Value.IsInPlay && x.Value.IsMinion)).Value != null;

		public bool IsOpponentMinionInPlay
			=> Entities.FirstOrDefault(x => (x.Value.IsInPlay && x.Value.IsMinion && x.Value.IsControlledBy(Opponent.Id))).Value != null;

		public int OpponentMinionCount => Entities.Count(x => (x.Value.IsInPlay && x.Value.IsMinion && x.Value.IsControlledBy(Opponent.Id)));
		public int PlayerMinionCount => Entities.Count(x => (x.Value.IsInPlay && x.Value.IsMinion && x.Value.IsControlledBy(Player.Id)));
		public int OpponentHandCount => Entities.Count(x => x.Value.IsInHand && x.Value.IsControlledBy(Opponent.Id));

		public Player Player { get; set; }
		public Player Opponent { get; set; }
		public bool IsInMenu { get; set; }
		public bool IsUsingPremade { get; set; }
		public bool IsRunning { get; set; }
		public Region CurrentRegion { get; set; }
		public GameStats CurrentGameStats { get; set; }
		public HearthMirror.Objects.Deck CurrentSelectedDeck { get; set; }
		public SecretsManager SecretsManager { get; }
		public List<Card> DrawnLastGame { get; set; }
		public Dictionary<int, Entity> Entities { get; } = new Dictionary<int, Entity>();
		public GameMetaData MetaData { get; } = new GameMetaData();
		internal List<Tuple<int, List<string>>> StoredPowerLogs { get; } = new List<Tuple<int, List<string>>>();
		internal Dictionary<int, string> StoredPlayerNames { get; } = new Dictionary<int, string>();
		internal GameStats StoredGameStats { get; set; }
		public int ProposedAttacker { get; set; }
		public int ProposedDefender { get; set; }
		public bool SetupDone { get; set; }

		public bool PlayerChallengeable => CurrentMode == Mode.HUB || CurrentMode == Mode.TOURNAMENT || CurrentMode == Mode.ADVENTURE
					|| CurrentMode == Mode.TAVERN_BRAWL || CurrentMode == Mode.DRAFT || CurrentMode == Mode.PACKOPENING
					|| CurrentMode == Mode.COLLECTIONMANAGER;

		public bool? IsDungeonMatch => string.IsNullOrEmpty(CurrentGameStats?.OpponentHeroCardId) || CurrentGameType == GameType.GT_UNKNOWN ? (bool?)null
			: CurrentGameType == GameType.GT_VS_AI && DungeonRun.IsDungeonBoss(CurrentGameStats.OpponentHeroCardId);

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
					_currentFormat = (FormatType)HearthMirror.Reflection.GetFormat();
				return HearthDbConverter.GetFormat(_currentFormat);
			}
		}

		public Mode PreviousMode { get; set; }

		public bool SavedReplay { get; set; }

		public Entity PlayerEntity => Entities.FirstOrDefault(x => x.Value?.IsPlayer ?? false).Value;

		public Entity OpponentEntity => Entities.FirstOrDefault(x => x.Value != null && x.Value.HasTag(GameTag.PLAYER_ID) && !x.Value.IsPlayer).Value;

		public Entity GameEntity => Entities.FirstOrDefault(x => x.Value?.Name == "GameEntity").Value;

		public bool IsMulliganDone
		{
			get
			{
				var player = Entities.FirstOrDefault(x => x.Value.IsPlayer);
				var opponent = Entities.FirstOrDefault(x => x.Value.HasTag(GameTag.PLAYER_ID) && !x.Value.IsPlayer);
				if(player.Value == null || opponent.Value == null)
					return false;
				return player.Value.GetTag(GameTag.MULLIGAN_STATE) == (int)Mulligan.DONE
					   && opponent.Value.GetTag(GameTag.MULLIGAN_STATE) == (int)Mulligan.DONE;
			}
		}

		public bool Spectator => _spectator ?? (bool)(_spectator = HearthMirror.Reflection.IsSpectating());

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
		public GameType CurrentGameType => _currentGameType != GameType.GT_UNKNOWN ? _currentGameType : (_currentGameType = (GameType)HearthMirror.Reflection.GetGameType());

		public MatchInfo MatchInfo => _matchInfo ?? (_matchInfo = HearthMirror.Reflection.GetMatchInfo());
		private bool _matchInfoCacheInvalid = true;

		public BrawlInfo BrawlInfo => _brawlInfo ?? (_brawlInfo = HearthMirror.Reflection.GetBrawlInfo());

		internal async void CacheMatchInfo()
		{
			if(!_matchInfoCacheInvalid)
				return;
			MatchInfo matchInfo;
			while((matchInfo = HearthMirror.Reflection.GetMatchInfo()) == null || matchInfo.LocalPlayer == null || matchInfo.OpposingPlayer == null)
			{
				Log.Info($"Waiting for matchInfo... (matchInfo={matchInfo}, localPlayer={matchInfo?.LocalPlayer}, opposingPlayer={matchInfo?.OpposingPlayer})");
				await Task.Delay(1000);
			}
			_matchInfo = matchInfo;
			UpdatePlayers();
			_matchInfoCacheInvalid = false;
		}

		private void UpdatePlayers()
		{
			string GetName(MatchInfo.Player player)
			{
				if(player.BattleTag != null)
					return $"{player.BattleTag.Name}#{player.BattleTag.Number}";
				return player.Name;
			}
			Player.Name = GetName(_matchInfo.LocalPlayer);
			Opponent.Name = GetName(_matchInfo.OpposingPlayer);
			Player.Id = _matchInfo.LocalPlayer.Id;
			Opponent.Id = _matchInfo.OpposingPlayer.Id;
			Log.Info($"{Player.Name} [PlayerId={Player.Id}] vs {Opponent.Name} [PlayerId={Opponent.Id}]");
		}

		internal async void CacheGameType()
		{
			while((_currentGameType = (GameType)HearthMirror.Reflection.GetGameType()) == GameType.GT_UNKNOWN)
				await Task.Delay(1000);
		}

		internal void CacheSpectator() => _spectator = HearthMirror.Reflection.IsSpectating();

		internal void CacheBrawlInfo() => _brawlInfo = HearthMirror.Reflection.GetBrawlInfo();

		internal void InvalidateMatchInfoCache() => _matchInfoCacheInvalid = true;

		public void Reset(bool resetStats = true)
		{
			Log.Info("-------- Reset ---------");

			Player.Reset();
			Opponent.Reset();
			if(!_matchInfoCacheInvalid && MatchInfo?.LocalPlayer != null && MatchInfo.OpposingPlayer != null)
				UpdatePlayers();
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

			if(Core.Game != null && Core.Overlay != null)
			{
				Core.UpdatePlayerCards(true);
				Core.UpdateOpponentCards(true);
			}
		}

		public void StoreGameState()
		{
			if(MetaData.ServerInfo.GameHandle == 0)
				return;
			Log.Info($"Storing PowerLog for gameId={MetaData.ServerInfo.GameHandle}");
			StoredPowerLogs.Add(new Tuple<int, List<string>>(MetaData.ServerInfo.GameHandle, new List<string>(PowerLog)));
			if(Player.Id != -1 && !StoredPlayerNames.ContainsKey(Player.Id))
				StoredPlayerNames.Add(Player.Id, Player.Name);
			if(Opponent.Id != -1 && !StoredPlayerNames.ContainsKey(Opponent.Id))
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
	}
}
