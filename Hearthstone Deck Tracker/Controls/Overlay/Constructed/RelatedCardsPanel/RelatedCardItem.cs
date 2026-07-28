using Hearthstone_Deck_Tracker.Controls.Tooltips;
using Hearthstone_Deck_Tracker.Utility.Assets;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.RelatedCardsPanel;

public class RelatedCardItem : ICardTooltip
{
	private CardAssetViewModel? _assetViewModel;
	private CardTileViewModel? _tileViewModel;

	public RelatedCardItem(Hearthstone.Card card)
	{
		Card = card;
	}

	public Hearthstone.Card Card { get; }

	public CardAssetViewModel AssetViewModel =>
		_assetViewModel ??= new CardAssetViewModel(Card, CardAssetType.FullImage);

	public CardTileViewModel TileViewModel => _tileViewModel ??= new CardTileViewModel(Card);

	public void UpdateTooltip(CardTooltipViewModel viewModel) => Card.UpdateTooltip(viewModel);
}
