using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public abstract class VMArenaAction : VMAction
	{
		protected VMArenaAction(int? maxDailyOccurrences) : base(Franchise.HSConstructed,
			new [] { Enums.SubFranchise.Arena }, maxDailyOccurrences)
		{
			ArenaSettings = new ArenaSettings();
		}

		[JsonIgnore]
		public ArenaSettings ArenaSettings { get; }

		[JsonProperty("hdt_arena_settings_enabled")]
		[JsonConverter(typeof(VMEnabledSettingsJsonConverter))]
		protected ArenaSettings ArenaSettingsEnabled { get => ArenaSettings; }

		[JsonProperty("hdt_arena_settings_disabled")]
		[JsonConverter(typeof(VMDisabledSettingsJsonConverter))]
		protected ArenaSettings ArenaSettingsDisabled { get => ArenaSettings; }
	}
}
