using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

public enum HighlightColor
{
	None,
	Teal,
	Orange,
	Green,
}

public static class HighlightColorHelper
{
	private static readonly Dictionary<int, HighlightColor> _colorMapping = new()
	{
		{ 0, HighlightColor.Teal },
		{ 1, HighlightColor.Orange },
		{ 2, HighlightColor.Green },
	};

	public static HighlightColor GetHighlightColor(params bool[] conditions)
	{
		if (conditions.Length == 0)
			return HighlightColor.None;

		for (var i = 0; i < conditions.Length; i++)
		{
			if(!conditions[i]) continue;
			if (_colorMapping.TryGetValue(i, out var color))
			{
				return color;
			}
		}

		return HighlightColor.None;
	}
}
