using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker
{
	internal static class OpponentUploadStateConverter
	{
		public static string GetLinkMessage(OpponentUploadState state)
		{
			if(!Config.Instance.SeenLinkOpponentDeck)
				return "Dismiss";
			switch(state)
			{
				case OpponentUploadState.InKnownDeckMode:
					return "Clear Linked Deck";
			}
			return "";
		}
	}
}
