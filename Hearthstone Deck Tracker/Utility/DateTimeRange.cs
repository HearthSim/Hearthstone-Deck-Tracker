using System;

namespace Hearthstone_Deck_Tracker.Utility;

public class DateTimeRange
{
	public DateTime Start { get; }
	public DateTime End { get; }

	public DateTimeRange(DateTime start, DateTime end)
	{
		if (end < start)
			throw new ArgumentException("End must be >= Start");

		Start = start;
		End = end;
	}

	public bool Contains(DateTime value) => value >= Start && value <= End;
}
