using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;

namespace Mercenaries_Deck_Tracker.Utility.ValueMoments.Actions
{
	public class EndSpectateMatchMercenariesAction : VMMercenariesAction
	{
		public const string Name = ValueMomentsConstants.EndMatchSpectateName;

		public EndSpectateMatchMercenariesAction(GameResult matchResult, GameType gameType, GameMetrics gameMetrics) : base(
			Name,
			ActionSource.App,
			ValueMomentsConstants.EndMatchSpectateType,
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
