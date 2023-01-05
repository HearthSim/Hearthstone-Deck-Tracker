using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public class EndSpectateMatchBattlegroundsAction : VMBattlegroundsAction
	{
		public const string Name = ValueMomentsConstants.EndMatchSpectateName;

		public EndSpectateMatchBattlegroundsAction(
			int heroDbfId, string heroName, int finalPlacement, GameType gameType, int rating, GameMetrics gameMetrics
		) : base(
			Name,
			ActionSource.App,
			ValueMomentsConstants.EndMatchSpectateType,
			Franchise.Battlegrounds,
			ValueMomentsConstants.EndMatchActionMaxDailyOccurrences,
			heroDbfId, heroName, finalPlacement, gameType, rating, gameMetrics
		)
		{
		}

		[JsonProperty(ValueMomentsConstants.ActionNameProperty)]
		public string ActionName { get => ValueMomentsConstants.EndMatchActionNameValue; }
	}
}
