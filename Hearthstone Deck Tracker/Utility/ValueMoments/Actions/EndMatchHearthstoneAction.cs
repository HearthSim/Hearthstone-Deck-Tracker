using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public class EndMatchHearthstoneAction : VMHearthstoneAction
	{
		public EndMatchHearthstoneAction(
			int heroDbfId, string heroName, GameResult matchResult, GameMode gameMode, GameType gameType, int starLevel, GameMetrics gameMetrics
		) : base(
			Franchise.HSConstructed,
			EndMatchActionMaxDailyOccurrences,
			heroDbfId, heroName, matchResult, gameMode, gameType, starLevel, gameMetrics
		)
		{
		}

		public override string Name => EndMatchName;
		public override string Type => EndMatchType;
	}
}
