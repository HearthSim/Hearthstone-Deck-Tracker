namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

public class StatBar
{
	public string Label { get; }
	public double BarHeight { get; }
	public bool IsMedian { get; }

	public StatBar(string label, double barHeight, bool isMedian)
	{
		Label = label;
		BarHeight = barHeight;
		IsMedian = isMedian;
	}
}
