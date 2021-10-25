using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.RemoteData;

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
						var version = Remote.Config.Data?.BobsBuddy?.MinRequiredVersion ?? "";
						return string.Format(LocUtil.Get("BobsBuddyStatusMessage_UpdateRequired"), version);
					case BobsBuddyErrorState.NotEnoughData:
						return LocUtil.Get("BobsBuddyStatusMessage_NotEnoughData");
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
				case BobsBuddyState.CombatWithoutSimulation:
					return LocUtil.Get("BobsBuddyStatusMessage_AwaitingShopping");
			}
			return "";
		}
	}
}
