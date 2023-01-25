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
		public EndMatchMercenariesAction(GameResult matchResult, GameType gameType, GameMetrics gameMetrics) : base(
			Franchise.Mercenaries,
			EndMatchActionMaxDailyOccurrences,
			matchResult, gameType, gameMetrics
		)
		{
		}

		public override string Name => EndMatchName;
		public override string Type => EndMatchType;
	}
}
