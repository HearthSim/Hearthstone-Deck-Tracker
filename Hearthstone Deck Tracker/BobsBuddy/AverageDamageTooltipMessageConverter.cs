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
					return "Middle 80% of outcomes.";
				case BobsBuddyState.Shopping:
					var betterThanPortion = possibilities.Where(x => x < lastCombatResult).Count() / possibilities.Count;
					var positiveLastResult = Math.Abs(lastCombatResult);
					var description = lastCombatResult > 0 ? string.Format("Dealing {0} damage", positiveLastResult) : lastCombatResult < 0 ? string.Format("Taking {0} damage", positiveLastResult) : "Tying";

					if(!possibilities.Where(x => x != lastCombatResult).Any())
						return description + " was the only possible outcome";
					//else if(betterThanPortion == 0)
					//	return description + " was the worst possible outcome";
					//else if(!possibilities.Where(x => x > lastCombatResult).Any())
					//	return description + " was the best possible outcome";
					if(betterThanPortion > .5)
						return string.Format(description + " was better than {0:0.#%} of outcomes.", betterThanPortion);
					else
						return string.Format(description + " was worse than {0:0.#%} of outcomes.", 1-betterThanPortion);
			}
			return "";
		}
	}
}
