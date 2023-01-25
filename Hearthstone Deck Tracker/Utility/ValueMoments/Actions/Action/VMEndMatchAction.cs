using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public abstract class VMEndMatchAction : VMAction
	{
		internal const string EndMatchName = "End Match Action HDT";
		internal const string EndMatchType = "End Match Action";
		internal const string EndMatchSpectateName = "End Spectate Match Action HDT";
		internal const string EndMatchSpectateType = "End Spectate Match Action";
		internal const int EndMatchActionMaxDailyOccurrences = 1;

		protected VMEndMatchAction(
			Franchise franchise, SubFranchise[]? subFranchise,
			int? maxDailyOccurrences, bool withPersonalStatsSettings = false
		) : base(franchise, subFranchise, maxDailyOccurrences, withPersonalStatsSettings)
		{
		}

		public override ActionSource Source { get => ActionSource.App; }

		[JsonProperty("action_name")]
		public string ActionName => "end_match";
	}
}
