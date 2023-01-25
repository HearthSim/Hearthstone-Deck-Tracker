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
		public EndSpectateMatchMercenariesAction(GameResult matchResult, GameType gameType, GameMetrics gameMetrics) : base(
			Franchise.Mercenaries,
			EndMatchActionMaxDailyOccurrences,
			matchResult, gameType, gameMetrics
		)
		{
		}

		public override string Name => EndMatchSpectateName;
		public override string Type => EndMatchSpectateType;
	}
}
