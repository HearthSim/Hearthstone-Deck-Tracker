using Hearthstone_Deck_Tracker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Documents;

namespace Hearthstone_Deck_Tracker.BobsBuddy
{
	internal static class AverageDamageTooltipMessageConverter
	{
		public static string GetAverageDamagetTooltipMessage(BobsBuddyState state, int lastCombatResult, List<int> possibilities)
		{
			if(possibilities == null || possibilities.Count == 0)
				return "Waiting for combat";

			switch(state)
			{
				case BobsBuddyState.Initial:
					return "Waiting for combat";
				case BobsBuddyState.Combat:
					return "80% DMG";
				case BobsBuddyState.Shopping:
					return "Middle 80% percentile of damage";
			}
			return "";

		}
	}
}
