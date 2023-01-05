using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using SubFranchiseEnum = Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums.SubFranchise;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public abstract class VMHearthstoneAction : VMAction
	{
		protected VMHearthstoneAction(
			string name, ActionSource source, string type, Franchise franchise, int? maxDailyOccurrences,
			int heroDbfId, string heroName, GameResult matchResult, GameMode gameMode, GameType gameType, int starLevel
		) : base(
			name, source, type, franchise, gameMode switch
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
		public HearthstoneSettings HearthstoneSettings { get; }

		[JsonProperty("hdt_hsconstructed_settings_enabled")]
		[JsonConverter(typeof(VMEnabledSettingsJsonConverter))]
		protected HearthstoneSettings HearthstoneSettingsEnabled { get => HearthstoneSettings; }

		[JsonProperty("hdt_hsconstructed_settings_disabled")]
		[JsonConverter(typeof(VMDisabledSettingsJsonConverter))]
		protected HearthstoneSettings HearthstoneSettingsDisabled { get => HearthstoneSettings; }
	}
}
