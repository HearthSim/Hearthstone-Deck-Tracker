using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker
{
	internal static class OpponentUploadStateConverter
	{
		public static string GetStatusMessage(OpponentUploadState state)
		{
			switch(state)
			{
				case OpponentUploadState.Initial:
					return "Upload your Opponent's Deck Id";
				case OpponentUploadState.UploadSucceeded:
					return "Upload Successful";
				case OpponentUploadState.InKnownDeckMode:
					return "Return to No Opponent Deck Mode";
				case OpponentUploadState.Error:
					return "Deck Id parsing unsucessful.";
			}
			return "";
		}
	}
}
