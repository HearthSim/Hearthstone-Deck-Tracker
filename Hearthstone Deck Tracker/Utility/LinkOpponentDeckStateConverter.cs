using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker
{
	internal static class LinkOpponentDeckStateConverter
	{
		public static string GetLinkMessage(LinkOpponentDeckState state)
		{
			if(!Config.Instance.InteractedWithLinkOpponentDeck)
				return LocUtil.Get("LinkOpponentDeck_Dismiss");
			switch(state)
			{
				case LinkOpponentDeckState.InKnownDeckMode:
					return LocUtil.Get("LinkOpponentDeck_Clear");
			}
			return "";
		}
	}
}
