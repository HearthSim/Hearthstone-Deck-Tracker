using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Stats;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public abstract class VMBattlegroundsAction : VMEndMatchAction
	{
		protected VMBattlegroundsAction(
			Franchise franchise, int? maxDailyOccurrences,
			int heroDbfId, string heroName, int finalPlacement, GameType gameType, int rating, GameMetrics gameMetrics
		) : base(franchise, null, maxDailyOccurrences)
		{
			HeroDbfId = heroDbfId;
			HeroName = heroName;
			FinalPlacement = finalPlacement;
			GameType = gameType;
			Rating = rating;
			Tier7HeroOverlayDisplayed = gameMetrics.Tier7HeroOverlayDisplayed;
			Tier7QuestOverlayDisplayed = gameMetrics.Tier7QuestOverlayDisplayed;
			NumClickBattlegroundsMinionTab = gameMetrics.BattlegroundsMinionsTabClicks;
			if (gameMetrics.Tier7TrialActivated)
				TrialsActivated = new[] { ValueMomentsConstants.Tier7OverlayTrial };
			if (gameMetrics.Tier7TrialsRemaining.HasValue)
				TrialsRemaining = new[] { $"{ValueMomentsConstants.Tier7OverlayTrial}:{gameMetrics.Tier7TrialsRemaining}" };
			BattlegroundsSettings = new BattlegroundsSettings();
		}

		[JsonProperty("hero_dbf_id")]
		public int HeroDbfId { get; }

		[JsonProperty("hero_name")]
		public string HeroName { get; }

		[JsonProperty("final_placement")]
		public int FinalPlacement { get; }

		[JsonProperty("game_type")]
		public GameType GameType { get; }

		[JsonProperty("battlegrounds_rating")]
		public int Rating { get; }

		[JsonIgnore]
		public bool Tier7HeroOverlayDisplayed { get; }

		[JsonIgnore]
		public bool Tier7QuestOverlayDisplayed { get;  }

		[JsonProperty("num_click_battlegrounds_minion_tab")]
		public int NumClickBattlegroundsMinionTab { get;  }

		[JsonProperty("trials_activated", NullValueHandling = NullValueHandling.Ignore)]
		public string[]? TrialsActivated { get; }

		[JsonProperty("trials_remaining", NullValueHandling = NullValueHandling.Ignore)]
		public string[]? TrialsRemaining { get; }

		[JsonIgnore]
		public BattlegroundsSettings BattlegroundsSettings { get; }

		[JsonProperty("hdt_battlegrounds_settings_enabled")]
		[JsonConverter(typeof(VMEnabledSettingsJsonConverter))]
		protected BattlegroundsSettings BattlegroundsSettingsEnabled { get => BattlegroundsSettings; }

		[JsonProperty("hdt_battlegrounds_settings_disabled")]
		[JsonConverter(typeof(VMDisabledSettingsJsonConverter))]
		protected BattlegroundsSettings BattlegroundsSettingsDisabled { get => BattlegroundsSettings; }
	}
}
