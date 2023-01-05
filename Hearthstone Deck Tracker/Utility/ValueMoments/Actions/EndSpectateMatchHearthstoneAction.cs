using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public class EndSpectateMatchHearthstoneAction : VMHearthstoneAction
	{
		public const string Name = ValueMomentsConstants.EndMatchSpectateName;

		public EndSpectateMatchHearthstoneAction(
			int heroDbfId, string heroName, GameResult matchResult, GameMode gameMode, GameType gameType, int starLevel
		) : base(
			Name,
			ActionSource.App,
			ValueMomentsConstants.EndMatchSpectateType,
			Franchise.HSConstructed,
			ValueMomentsConstants.EndMatchActionMaxDailyOccurrences,
			heroDbfId, heroName, matchResult, gameMode, gameType, starLevel)
		{
		}

		[JsonProperty(ValueMomentsConstants.ActionNameProperty)]
		public string ActionName { get => ValueMomentsConstants.EndMatchActionNameValue; }
	}
}
