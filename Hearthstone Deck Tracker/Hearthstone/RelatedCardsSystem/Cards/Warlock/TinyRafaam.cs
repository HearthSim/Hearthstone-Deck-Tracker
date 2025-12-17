using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class TinyRafaam : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_TinyRafaamToken;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(_rafaams.Contains(card.Id));

	private readonly List<string> _rafaams = new List<string> {
		HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_TinyRafaamToken,
		HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_GreenRafaamToken,
		HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_MurlocRafaamToken,
		HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_ExplorerRafaamToken,
		HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_WarchiefRafaamToken,
		HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_CalamitousRafaamToken,
		HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_MindflayerRfaamToken,
		HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_GiantRafaamToken,
		HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_ArchmageRafaamToken,
		HearthDb.CardIds.Collectible.Warlock.TimethiefRafaam,
	};
}
