using System;
using System.Linq;
using System.Text.RegularExpressions;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay.Converter;
using Hearthstone_Deck_Tracker.Stats;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.HsReplay.API
{
	public class UploadMetaData
	{
		private readonly GameStats _game;
		private readonly GameMetaData _gameMetaData;
		private int? _friendlyPlayerId;

		[JsonIgnore]
		public readonly string[] Log;

		private UploadMetaData(string[] log, GameMetaData gameMetaData, GameStats game)
		{
			Log = log;
			_gameMetaData = gameMetaData;
			_game = game;
			FillPlayerData();
		}

		[JsonProperty("server_ip", NullValueHandling = NullValueHandling.Ignore)]
		public string ServerIp => _gameMetaData?.ServerInfo?.Address;

		[JsonProperty("server_port", NullValueHandling = NullValueHandling.Ignore)]
		public string ServerPort => _gameMetaData?.ServerInfo?.Port.ToString();

		[JsonProperty("game_handle", NullValueHandling = NullValueHandling.Ignore)]
		public string GameHandle => _gameMetaData?.ServerInfo?.GameHandle.ToString();

		[JsonProperty("client_handle", NullValueHandling = NullValueHandling.Ignore)]
		public string ClientHandle => _gameMetaData?.ServerInfo?.ClientHandle.ToString();

		[JsonProperty("reconnecting", NullValueHandling = NullValueHandling.Ignore)]
		public string Reconnected => _gameMetaData?.Reconnected ?? false ? "true" : null;

		[JsonProperty("resumable", NullValueHandling = NullValueHandling.Ignore)]
		public string Resumable => _gameMetaData?.ServerInfo?.Resumable.ToString().ToLower();

		[JsonProperty("spectator_password", NullValueHandling = NullValueHandling.Ignore)]
		public string SpectatePassword => _gameMetaData?.ServerInfo?.SpectatorPassword;

		[JsonProperty("aurora_password", NullValueHandling = NullValueHandling.Ignore)]
		public string AuroraPassword => _gameMetaData?.ServerInfo?.AuroraPassword;

		[JsonProperty("server_version", NullValueHandling = NullValueHandling.Ignore)]
		public string ServerVersion => _gameMetaData?.ServerInfo?.Version;

		[JsonProperty("match_start", NullValueHandling = NullValueHandling.Ignore)]
		public string MatchStart => _game?.StartTime != DateTime.MinValue ? _game?.StartTime.ToString("o") : null;

		[JsonProperty("build", NullValueHandling = NullValueHandling.Ignore)]
		public int? HearthstoneBuild => _gameMetaData?.HearthstoneBuild ?? _game?.HearthstoneBuild ?? (_game != null ? BuildDates.GetByDate(_game.StartTime) : null);

		[JsonProperty("game_type", NullValueHandling = NullValueHandling.Ignore)]
		public int? GameType => _game != null ? (int)HearthDbConverter.GetGameType(_game.GameMode, _game.Format) : (int?)null;

		[JsonProperty("spectator_mode", NullValueHandling = NullValueHandling.Ignore)]
		public string SpectatorMode => _game?.GameMode == GameMode.Spectator ? "true" : null;

		[JsonProperty("friendly_player", NullValueHandling = NullValueHandling.Ignore)]
		public int? FriendlyPlayerId => _game?.FriendlyPlayerId > 0 ? _game.FriendlyPlayerId : (_friendlyPlayerId > 0 ? _friendlyPlayerId : null);

		[JsonProperty("scenario_id", NullValueHandling = NullValueHandling.Ignore)]
		public int? ScenarioId => _game?.ScenarioId ?? _gameMetaData?.ServerInfo?.Mission;

		[JsonProperty("format", NullValueHandling = NullValueHandling.Ignore)]
		public int? Format => _game?.Format != null ? (int)HearthDbConverter.GetFormatType(_game.Format) : (int?)null;

		[JsonProperty("player1", NullValueHandling = NullValueHandling.Ignore)]
		public Player Player1 { get; set; } = new Player();

		[JsonProperty("player2", NullValueHandling = NullValueHandling.Ignore)]
		public Player Player2 { get; set; } = new Player();


		public class Player
		{
			[JsonProperty("rank", NullValueHandling = NullValueHandling.Ignore)]
			public int? Rank{ get; set; }

			[JsonProperty("legendrank", NullValueHandling = NullValueHandling.Ignore)]
			public int? LegendRank { get; set; }

			[JsonProperty("stars", NullValueHandling = NullValueHandling.Ignore)]
			public int? Stars { get; set; }

			[JsonProperty("wins", NullValueHandling = NullValueHandling.Ignore)]
			public int? Wins { get; set; }

			[JsonProperty("losses", NullValueHandling = NullValueHandling.Ignore)]
			public int? Losses { get; set; }

			[JsonProperty("deck", NullValueHandling = NullValueHandling.Ignore)]
			public string[] DeckList { get; set; }

			[JsonProperty("deck_id", NullValueHandling = NullValueHandling.Ignore)]
			public long? DeckId { get; set; }

			[JsonProperty("cardback", NullValueHandling = NullValueHandling.Ignore)]
			public int? Cardback { get; set; }
		}

		private void FillPlayerData()
		{
			var friendly = new Player();
			var opposing = new Player();

			if(_game?.Rank > 0)
				friendly.Rank = _game.Rank;
			if(_game?.LegendRank > 0)
				friendly.LegendRank = _game.LegendRank;
			if(_game?.PlayerCardbackId > 0)
				friendly.Cardback = _game.PlayerCardbackId;
			if(_game?.Stars > 0)
				friendly.Stars = _game.Stars;
			if(_game?.PlayerCards.Sum(x => x.Count) == 30 && _game?.PlayerCards.Sum(x => x.Unconfirmed) <= 24)
			{
				friendly.DeckList = _game?.PlayerCards.SelectMany(x => Enumerable.Repeat(x.Id, x.Count)).ToArray();
				friendly.DeckId = _game?.HsDeckId;
			}

			if(_game?.OpponentRank > 0)
				opposing.Rank = _game.OpponentRank;
			if(_game?.OpponentLegendRank > 0)
				opposing.LegendRank = _game.OpponentLegendRank;
			if(_game?.OpponentCardbackId > 0)
				opposing.Cardback = _game.OpponentCardbackId;

			if(_game?.FriendlyPlayerId > 0)
			{
				Player1 = _game.FriendlyPlayerId == 1 ? friendly : opposing;
				Player2 = _game.FriendlyPlayerId == 2 ? friendly : opposing;
			}
			else
			{
				var player1Name = GetPlayer1Name();
				if(player1Name == _game?.PlayerName)
				{
					_friendlyPlayerId = 1;
					Player1 = friendly;
					Player2 = opposing;
				}
				else if(player1Name == _game?.OpponentName)
				{
					_friendlyPlayerId = 2;
					Player2 = friendly;
					Player1 = opposing;
				}
			}
		}

		private string GetPlayer1Name()
		{
			foreach(var line in Log)
			{
				var match = Regex.Match(line, @"TAG_CHANGE Entity=(?<name>(.+)) tag=CONTROLLER value=1");
				if(!match.Success)
					continue;
				return match.Groups["name"].Value;
			}
			return null;
		}

		public static UploadMetaData Generate(string[] logLines, GameMetaData gameMetaData, GameStats game) 
			=> new UploadMetaData(logLines, gameMetaData, game);

	}
}
