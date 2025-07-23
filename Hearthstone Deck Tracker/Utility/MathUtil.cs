using System;

namespace Hearthstone_Deck_Tracker.Utility;

public static class MathUtil
{
	public static double CubicEaseIn(double value)
	{
		return value * value * value;
	}

	public static double Lerp(double from, double to, double value)
	{
		return (1 - value) * from + value * to;
	}

	public static double InverseLerp(double value, double min, double max)
	{
		return (value - min) / (max - min);
	}

	public static double Remap(double value, double inMin, double inMax, double outMin, double outMax)
	{
		var t = InverseLerp(value, inMin, inMax);
		return Lerp(outMin, outMax, t);
	}

	public static double Clamp(double value, double min, double max)
	{
		return value < min ? min : value > max ? max : value;
	}
}
