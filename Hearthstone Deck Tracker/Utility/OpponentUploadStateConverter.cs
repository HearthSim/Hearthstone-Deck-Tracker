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
					return "Init";
				case OpponentUploadState.UploadSucceeded:
					return "UploadSucceed";
				case OpponentUploadState.Error:
					return "Err";
			}
			return "";
		}
	}
}
