using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using SubFranchiseEnum = Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums.SubFranchise;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public abstract class VMHearthstoneAction : VMEndMatchAction
	{
		protected VMHearthstoneAction(
			Franchise franchise, int? maxDailyOccurrences,
			int heroDbfId, string heroName, GameResult matchResult, GameMode gameMode, GameType gameType, int starLevel, GameMetrics gameMetrics
		) : base(
			franchise, gameMode switch
			{
				GameMode.Arena => new[] { SubFranchiseEnum.Arena },
				GameMode.Brawl => new[] { SubFranchiseEnum.Brawl },
				GameMode.Duels => new[] { SubFranchiseEnum.Duels },
				_ => null
			}, maxDailyOccurrences
		)
		{
			HeroDbfId = heroDbfId;
			HeroName = heroName;
			MatchResult = matchResult;
			GameType = gameType;
			StarLevel = starLevel;
			MulliganGuideOverlayDisplayed = gameMetrics.ConstructedMulliganGuideOverlayDisplayed;
			ShowedOpponentArenaPackage = gameMetrics.ArenaShowedOpponentPackage;
			HearthstoneSettings = new HearthstoneSettings();
		}

		[JsonProperty("hero_dbf_id")]
		public int HeroDbfId { get; }

		[JsonProperty("hero_name")]
		public string HeroName { get; }

		[JsonProperty("match_result")]
		public GameResult MatchResult { get; }

		[JsonProperty("game_type")]
		public GameType GameType { get; }

		[JsonProperty("star_level")]
		public int StarLevel { get; }

		[JsonIgnore]
		public bool MulliganGuideOverlayDisplayed { get; }

		[JsonIgnore]
		public HearthstoneSettings HearthstoneSettings { get; }

		[JsonProperty("showed_opponent_arena_package")]
		public bool ShowedOpponentArenaPackage { get; }

		[JsonProperty("hdt_hsconstructed_settings_enabled")]
		[JsonConverter(typeof(VMEnabledSettingsJsonConverter))]
		protected HearthstoneSettings HearthstoneSettingsEnabled { get => HearthstoneSettings; }

		[JsonProperty("hdt_hsconstructed_settings_disabled")]
		[JsonConverter(typeof(VMDisabledSettingsJsonConverter))]
		protected HearthstoneSettings HearthstoneSettingsDisabled { get => HearthstoneSettings; }
	}
}
