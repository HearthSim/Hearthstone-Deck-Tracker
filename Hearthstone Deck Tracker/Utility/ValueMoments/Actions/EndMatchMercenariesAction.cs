using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public class EndMatchMercenariesAction : VMMercenariesAction
	{
		public const string Name = ValueMomentsConstants.EndMatchName;

		public EndMatchMercenariesAction(GameResult matchResult, GameType gameType, GameMetrics gameMetrics) : base(
			Name,
			ActionSource.App,
			ValueMomentsConstants.EndMatchType,
			Franchise.Mercenaries,
			ValueMomentsConstants.EndMatchActionMaxDailyOccurrences,
			matchResult, gameType, gameMetrics
		)
		{
		}

		[JsonProperty(ValueMomentsConstants.ActionNameProperty)]
		public string ActionName { get => ValueMomentsConstants.EndMatchActionNameValue; }
	}
}
