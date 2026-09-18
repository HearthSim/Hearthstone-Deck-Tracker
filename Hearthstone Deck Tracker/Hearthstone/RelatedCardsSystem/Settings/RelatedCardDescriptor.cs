using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Utility.Assets;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Settings;

/// <summary>
/// UI metadata for one row of the related cards options page: one card, however many card ids it
/// has (see <see cref="RelatedCardCatalog"/>).
/// </summary>
/// <remarks>
/// Reads through to the <see cref="Card"/> instead of caching the name, so a card-language change is
/// picked up without rebuilding anything.
/// </remarks>
public class RelatedCardDescriptor
{
	private readonly Card _card;

	public RelatedCardDescriptor(Card card, IReadOnlyList<string> cardIds)
	{
		_card = card;
		CardId = card.Id;
		CardIds = cardIds;
	}

	/// <summary>The representative id: the one the override is stored under, and the one the row displays.</summary>
	public string CardId { get; }

	/// <summary>Every registered id of this card, the representative first.</summary>
	public IReadOnlyList<string> CardIds { get; }

	public string CardIdsText => string.Join(", ", CardIds);

	public string DisplayName => _card.LocalizedName ?? CardId;

	public CardAssetViewModel CardAsset => new(_card, CardAssetType.Portrait);
}
