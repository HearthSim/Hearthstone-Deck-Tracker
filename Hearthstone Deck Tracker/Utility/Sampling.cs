using System;

namespace Hearthstone_Deck_Tracker.Utility;

public static class Sampling
{
	private static readonly Random Random = new();

	public static bool ShouldSample(double rate)
	{
		if(rate <= 0)
			return false;
		// System.Random is not thread-safe
		lock(Random)
			return Random.NextDouble() < rate;
	}
}
