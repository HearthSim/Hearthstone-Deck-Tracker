using System;

namespace Hearthstone_Deck_Tracker.Utility.Exceptions;

public class CompositionStatsException : Exception
{
	public CompositionStatsException(string message) : base(message)
	{
	}
}
