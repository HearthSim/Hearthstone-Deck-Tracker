using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.RelatedCardsPanel;

public class KeywordFilterButton : ViewModel
{
	public string KeywordName { get; }
	public string Label { get; }
	public int CardCount { get; }

	public bool IsActive
	{
		get => GetProp(false);
		set => SetProp(value);
	}

	public KeywordFilterButton(string keywordName, string label, int cardCount)
	{
		KeywordName = keywordName;
		Label = label;
		CardCount = cardCount;
	}
}
