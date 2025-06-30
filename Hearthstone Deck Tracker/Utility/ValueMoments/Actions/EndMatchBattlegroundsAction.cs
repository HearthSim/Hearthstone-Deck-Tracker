using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public class EndMatchBattlegroundsAction : VMBattlegroundsAction
	{
		public EndMatchBattlegroundsAction(
			int heroDbfId, string heroName, int finalPlacement, int finalTurn, GameType gameType, int rating, GameMetrics gameMetrics
		) : base(
			Franchise.Battlegrounds,
			EndMatchActionMaxDailyOccurrences,
			heroDbfId, heroName, finalPlacement, finalTurn, gameType, rating, gameMetrics
		)
		{
		}

		public override string Name => EndMatchName;
		public override string Type => EndMatchType;
	}
}
