using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.RelatedCardsPanel;

public class CostFilterButton : ViewModel
{
	public int Cost { get; }
	public string Label => Cost.ToString();

	public bool IsActive
	{
		get => GetProp(false);
		set => SetProp(value);
	}

	public CostFilterButton(int cost)
	{
		Cost = cost;
	}
}
