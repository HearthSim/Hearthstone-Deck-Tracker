using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public class EndSpectateMatchHearthstoneAction : VMHearthstoneAction
	{
		public EndSpectateMatchHearthstoneAction(
			int heroDbfId, string heroName, GameResult matchResult, GameMode gameMode, GameType gameType, int starLevel
		) : base(
			Franchise.HSConstructed,
			ValueMomentsConstants.EndMatchActionMaxDailyOccurrences,
			heroDbfId, heroName, matchResult, gameMode, gameType, starLevel)
		{
		}

		public override string Name => ValueMomentsConstants.EndMatchSpectateName;
		public override string Type => ValueMomentsConstants.EndMatchSpectateType;
	}
}
