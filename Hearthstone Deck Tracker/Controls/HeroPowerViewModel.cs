using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Controls.Tooltips;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls;

public class HeroPowerViewModel : ViewModel, ICardTooltip
{
	public int? Cost
	{
		get => GetProp<int?>(null);
		set => SetProp(value);
	}

	public bool IsCoinCost
	{
		get => GetProp(false);
		set => SetProp(value);
	}

	public Hearthstone.Card? Card
	{
		get => GetProp<Hearthstone.Card?>(null);
		set
		{
			SetProp(value);
			CardPortrait = new CardAssetViewModel(value, CardAssetType.Portrait);
		}
	}

	public CardAssetViewModel? CardPortrait
	{
		get => GetProp<CardAssetViewModel?>(null);
		private set => SetProp(value);
	}

	public void UpdateTooltip(CardTooltipViewModel viewModel)
	{
		viewModel.Card = Card;
	}
}
