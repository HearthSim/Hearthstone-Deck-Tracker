using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.RemoteData;

namespace Hearthstone_Deck_Tracker.BobsBuddy
{
	internal static class StatusMessageConverter
	{
		public static string GetStatusMessage(BobsBuddyState state, BobsBuddyErrorState errorState, bool statsShown, string? errorMessage)
		{
			if(errorState != BobsBuddyErrorState.None)
			{
				if(errorMessage != null)
					return errorMessage;

				switch(errorState)
				{
					case BobsBuddyErrorState.UpdateRequired:
						var version = Remote.Config.Data?.BobsBuddy?.MinRequiredVersion ?? "";
						return string.Format(LocUtil.Get("BobsBuddyStatusMessage_UpdateRequired"), version);
					case BobsBuddyErrorState.NotEnoughData:
						return LocUtil.Get("BobsBuddyStatusMessage_NotEnoughData");
					case BobsBuddyErrorState.UnkownCards:
						return LocUtil.Get("BobsBuddyStatusMessage_UnknownCards");
					case BobsBuddyErrorState.UnsupportedCards:
						return LocUtil.Get("BobsBuddyStatusMessage_UnsupportedCards");
					case BobsBuddyErrorState.UnsupportedInteraction:
						return LocUtil.Get("BobsBuddyStatusMessage_UnsupportedInteraction");
				}
			}
			switch(state)
			{
				case BobsBuddyState.Initial:
					return LocUtil.Get("BobsBuddyStatusMessage_WaitingForCombat");
				case BobsBuddyState.WaitingForTeammates:
					return LocUtil.Get("BobsBuddyStatusMessage_WaitingForTeammates");
				case BobsBuddyState.Combat:
					return LocUtil.Get(statsShown ? "BobsBuddyStatusMessage_CurrentCombat" : "BobsBuddyStatusMessage_ShowCurrentCombat");
				case BobsBuddyState.Shopping:
					return LocUtil.Get(statsShown ? "BobsBuddyStatusMessage_PreviousCombat" : "BobsBuddyStatusMessage_ShowPreviousCombat");
				case BobsBuddyState.GameOver:
					return LocUtil.Get(statsShown ? "BobsBuddyStatusMessage_FinalCombat" : "BobsBuddyStatusMessage_ShowFinalCombat");
				case BobsBuddyState.CombatPartial:
					return LocUtil.Get(statsShown ? "BobsBuddyStatusMessage_CurrentCombatPartial" : "BobsBuddyStatusMessage_ShowCurrentCombatPartial");
				case BobsBuddyState.ShoppingAfterPartial:
					return LocUtil.Get(statsShown ? "BobsBuddyStatusMessage_PreviousCombatPartial" : "BobsBuddyStatusMessage_ShowPreviousCombatPartial");
				case BobsBuddyState.GameOverAfterPartial:
					return LocUtil.Get(statsShown ? "BobsBuddyStatusMessage_FinalCombatPartial" : "BobsBuddyStatusMessage_ShowFinalCombatPartial");
				case BobsBuddyState.CombatWithoutSimulation:
					return LocUtil.Get("BobsBuddyStatusMessage_AwaitingShopping");
			}
			return "";
		}
	}
}
