using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.BobsBuddy
{
	internal static class StatusMessageConverter
	{
		public static string GetStatusMessage(BobsBuddyState state, BobsBuddyErrorState errorState, bool statsShown)
		{
			if(errorState != BobsBuddyErrorState.None)
			{
				switch(errorState)
				{
					case BobsBuddyErrorState.UpdateRequired:
						var version = RemoteConfig.Instance.Data?.BobsBuddy?.MinRequiredVersion ?? "";
						return string.Format(LocUtil.Get("BobsBuddyStatusMessage_UpdateRequired"), version);
					case BobsBuddyErrorState.NotEnoughData:
						return LocUtil.Get("BobsBuddyStatusMessage_NotEnoughData");
					case BobsBuddyErrorState.SecretsNotSupported:
						return LocUtil.Get("BobsBuddyStatusMessage_SecretsNotSupported");
					case BobsBuddyErrorState.UnkownCards:
						return LocUtil.Get("BobsBuddyStatusMessage_UnknownCards");
				}
			}
			switch(state)
			{
				case BobsBuddyState.Initial:
						return LocUtil.Get("BobsBuddyStatusMessage_WaitingForCombat");
				case BobsBuddyState.Combat:
					return LocUtil.Get(statsShown ? "BobsBuddyStatusMessage_CurrentCombat" : "BobsBuddyStatusMessage_ShowCurrentCombat");
				case BobsBuddyState.Shopping:
					return LocUtil.Get(statsShown ? "BobsBuddyStatusMessage_PreviousCombat" : "BobsBuddyStatusMessage_PreviousCurrentCombat");
			}
			return "";
		}
	}
}
