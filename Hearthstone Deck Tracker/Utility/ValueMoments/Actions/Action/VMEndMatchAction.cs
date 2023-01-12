using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public abstract class VMEndMatchAction : VMAction
	{
		protected VMEndMatchAction(
			Franchise franchise, SubFranchise[]? subFranchise,
			int? maxDailyOccurrences, bool withPersonalStatsSettings = false
		) : base(franchise, subFranchise, maxDailyOccurrences, withPersonalStatsSettings)
		{
		}

		public override ActionSource Source { get => ActionSource.App; }

		[JsonProperty(ValueMomentsConstants.ActionNameProperty)]
		public string ActionName => ValueMomentsConstants.EndMatchActionNameValue;
	}
}
