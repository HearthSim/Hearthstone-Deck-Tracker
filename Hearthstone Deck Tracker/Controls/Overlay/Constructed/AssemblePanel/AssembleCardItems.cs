using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Controls.Tooltips;
using Hearthstone_Deck_Tracker.Hearthstone.CardExtraInfo;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Assemble;
using Hearthstone_Deck_Tracker.Utility.Assets;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.AssemblePanel;

public abstract class AssembleCardItem : ICardTooltip
{
	private CardAssetViewModel? _assetViewModel;

	protected AssembleCardItem(Hearthstone.Card card)
	{
		Card = card;
	}

	public Hearthstone.Card Card { get; }

	public CardAssetViewModel AssetViewModel =>
		_assetViewModel ??= new CardAssetViewModel(Card, CardAssetType.FullImage);

	public abstract void UpdateTooltip(CardTooltipViewModel viewModel);
}

/// <summary>
/// A middle card of an Assemble pool. Clicking it shows the options offered with it.
/// </summary>
public class AssembleMiddleItem : AssembleCardItem
{
	public AssembleMiddleItem(AssembleMiddleCard middle) : base(middle.Card)
	{
		Middle = middle;
	}

	public AssembleMiddleCard Middle { get; }

	public override void UpdateTooltip(CardTooltipViewModel viewModel) => Card.UpdateTooltip(viewModel);
}

/// <summary>
/// An option offered with a middle card. Its tooltip shows the two minions it can make: the middle
/// card's trigger with the option's effect, and the option's trigger with the middle card's effect.
/// </summary>
public class AssembleOptionItem : AssembleCardItem
{
	private readonly Hearthstone.Card _middle;

	public AssembleOptionItem(Hearthstone.Card option, Hearthstone.Card middle) : base(option)
	{
		_middle = middle;
	}

	public override void UpdateTooltip(CardTooltipViewModel viewModel)
	{
		// The assembled minion is the middle card, with the half the player didn't pick from it.
		viewModel.ShowTriple = false;
		viewModel.Card = _middle;
		viewModel.AssembledText = new AssembledHalves(0, Card.DbfId).GetText(_middle);
		viewModel.AlternativeAssembledText = new AssembledHalves(Card.DbfId, 0).GetText(_middle);
		viewModel.HasRaceBanner = _middle.RaceEnum is not (null or Race.INVALID or Race.BLANK);
		viewModel.RelatedCards = null;
		viewModel.RelatedCardsSummary = null;
		viewModel.PoolStatistics = null;
		viewModel.HasLargePool = false;
	}
}
