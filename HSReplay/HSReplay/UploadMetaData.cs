using Newtonsoft.Json;

namespace HSReplay
{
	public class UploadMetaData
	{
		[JsonProperty("server_ip", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string ServerIp { get; set; }
		
		[JsonProperty("server_port", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string ServerPort { get; set; }
		
		[JsonProperty("game_handle", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string GameHandle { get; set; }

		[JsonProperty("client_handle", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string ClientHandle { get; set; }

		[JsonProperty("reconnecting", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool Reconnected { get; set; }

		[JsonProperty("resumable", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool Resumable { get; set; }

		[JsonProperty("spectator_password", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string SpectatePassword { get; set; }

		[JsonProperty("aurora_password", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string AuroraPassword { get; set; }

		[JsonProperty("server_version", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string ServerVersion { get; set; }

		/// <summary>
		/// In ISO 8601 format
		/// </summary>
		[JsonProperty("match_start", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string MatchStart { get; set; }

		[JsonProperty("build", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? HearthstoneBuild { get; set; }

		/// <summary>
		/// Using the Hearthstone BnetGameType enum.
		/// </summary>
		[JsonProperty("game_type", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? GameType { get; set; }

		[JsonProperty("spectator_mode", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool SpectatorMode { get; set; }

		[JsonProperty("friendly_player", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? FriendlyPlayerId { get; set; }

		[JsonProperty("scenario_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? ScenarioId { get; set; }

		[JsonProperty("brawl_season", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? BrawlSeason { get; set; }

		[JsonProperty("ladder_season", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? LadderSeason { get; set; }

		/// <summary>
		/// Using the Hearthstone FormatType enum.
		/// </summary>
		[JsonProperty("format", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? Format { get; set; }

		[JsonProperty("player1", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Player Player1 { get; set; } = new Player();

		[JsonProperty("player2", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Player Player2 { get; set; } = new Player();

		/// <summary>
		/// Set to true when not sending actual user data.
		/// </summary>
		[JsonProperty("test_data", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool TestData { get; set; }


		public class Player
		{
			/// <summary>
			/// Rank before the game, in the current format.
			/// </summary>
			[JsonProperty("rank", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public int? Rank { get; set; }

			/// <summary>
			/// Legend rank before the game, in the current format.
			/// </summary>
			[JsonProperty("legend_rank", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public int? LegendRank { get; set; }

			/// <summary>
			/// Ranked stars before the game, in the current format.
			/// </summary>
			[JsonProperty("stars", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public int? Stars { get; set; }

			/// <summary>
			/// Arena wins before the game.
			/// </summary>
			[JsonProperty("wins", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public int? Wins { get; set; }

			/// <summary>
			/// Arena losses before the game.
			/// </summary>
			[JsonProperty("losses", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public int? Losses { get; set; }

			/// <summary>
			/// Only fill this with the complete deck and if certain it's correct.
			/// </summary>
			[JsonProperty("deck", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string[] DeckList { get; set; }

			/// <summary>
			/// Hearthstone internal deck ID.
			/// </summary>
			[JsonProperty("deck_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public long? DeckId { get; set; }

			[JsonProperty("cardback", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public int? Cardback { get; set; }
		}
	}
}