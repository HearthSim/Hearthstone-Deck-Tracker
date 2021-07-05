using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker
{
	internal static class LinkOpponentDeckStateConverter
	{
		public static string GetLinkMessage(LinkOpponentDeckState state)
		{
			if(!Config.Instance.SeenLinkOpponentDeck)
				return "Dismiss";
			switch(state)
			{
				case LinkOpponentDeckState.InKnownDeckMode:
					return "Clear Linked Deck";
			}
			return "";
		}
	}
}
