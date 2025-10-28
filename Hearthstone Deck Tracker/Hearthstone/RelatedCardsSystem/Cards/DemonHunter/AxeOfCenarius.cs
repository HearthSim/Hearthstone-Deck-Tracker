using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class AxeOfCenarius : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Demonhunter.Broxigar_AxeOfCenariusToken;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(_portals.Contains(card.Id));

	private readonly HashSet<string> _portals = new()
	{
		HearthDb.CardIds.NonCollectible.Demonhunter.Broxigar_FirstPortalToArgusToken,
		HearthDb.CardIds.NonCollectible.Demonhunter.Broxigar_SecondPortalToArgusToken,
		HearthDb.CardIds.NonCollectible.Demonhunter.Broxigar_ThirdPortalToArgusToken,
		HearthDb.CardIds.NonCollectible.Demonhunter.Broxigar_FinalPortalToArgusToken
	};
}
