using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Settings;

/// <summary>
/// Readouts that are configured like counters but drawn elsewhere in the overlay, so the reflection
/// sweep in <see cref="CounterTypeProvider"/> cannot find them.
/// </summary>
public static class WidgetCounters
{
	public static IReadOnlyList<CounterDescriptor> Descriptors { get; } = new CounterDescriptor[]
	{
		CorpsesCounterDescriptor.Instance,
	};

	public static IEnumerable<string> Ids => Descriptors.Select(d => d.CounterId);
}
