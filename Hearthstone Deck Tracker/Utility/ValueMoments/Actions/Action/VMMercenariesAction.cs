using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public abstract class VMMercenariesAction : VMEndMatchAction
	{
		protected VMMercenariesAction(
			Franchise franchise, int? maxDailyOccurrences,
			GameResult matchResult, GameType gameType, GameMetrics gameMetrics
		) : base(franchise, null, maxDailyOccurrences)
		{
			MatchResult = matchResult;
			GameType = gameType;
			NumHoverOpponentMercAbility = gameMetrics.MercenariesHoversOpponentMercToShowAbility;
			NumHoverMercTaskOverlay = gameMetrics.MercenariesHoverTasksDuringMatch;
			MercenariesSettings = new MercenariesSettings();
		}

		[JsonProperty("match_result")]
		public GameResult MatchResult { get; }

		[JsonProperty("game_type")]
		public GameType GameType { get; }

		[JsonProperty("num_hover_opponent_merc_ability")]
		public int NumHoverOpponentMercAbility { get; }

		[JsonProperty("num_hover_merc_task_overlay")]
		public int NumHoverMercTaskOverlay { get; }

		[JsonIgnore]
		public MercenariesSettings MercenariesSettings { get; }

		[JsonProperty("hdt_mercenaries_settings_enabled")]
		[JsonConverter(typeof(VMEnabledSettingsJsonConverter))]
		protected MercenariesSettings MercenariesSettingsEnabled { get => MercenariesSettings; }

		[JsonProperty("hdt_mercenaries_settings_disabled")]
		[JsonConverter(typeof(VMDisabledSettingsJsonConverter))]
		protected MercenariesSettings MercenariesSettingsDisabled { get => MercenariesSettings; }
	}
}
