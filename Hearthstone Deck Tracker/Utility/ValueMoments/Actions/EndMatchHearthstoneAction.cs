using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public class EndMatchHearthstoneAction : VMHearthstoneAction
	{
		public EndMatchHearthstoneAction(
			int heroDbfId, string heroName, GameResult matchResult, GameMode gameMode, GameType gameType, int starLevel
		) : base(
			Franchise.HSConstructed,
			ValueMomentsConstants.EndMatchActionMaxDailyOccurrences,
			heroDbfId, heroName, matchResult, gameMode, gameType, starLevel
		)
		{
		}

		public override string Name => ValueMomentsConstants.EndMatchName;
		public override string Type => ValueMomentsConstants.EndMatchType;
	}
}
