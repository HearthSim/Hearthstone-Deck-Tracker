using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public class EndSpectateMatchBattlegroundsAction : VMBattlegroundsAction
	{
		public EndSpectateMatchBattlegroundsAction(
			int heroDbfId, string heroName, int finalPlacement, GameType gameType, int rating, GameMetrics gameMetrics
		) : base(
			Franchise.Battlegrounds,
			EndMatchActionMaxDailyOccurrences,
			heroDbfId, heroName, finalPlacement, gameType, rating, gameMetrics
		)
		{
		}

		public override string Name => EndMatchSpectateName;
		public override string Type => EndMatchSpectateType;
	}
}
