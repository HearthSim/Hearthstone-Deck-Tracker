using System;
using System.Linq;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;

namespace Hearthstone_Deck_Tracker.Utility.Extensions
{
	public static class GroupingExtensions
	{
		public static ConstructedDeckStats ToConstructedDeckStats(this IGrouping<Guid, GameStats> grouping) 
			=> grouping != null ? new ConstructedDeckStats(grouping) : null;
	}
}
