using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class RaidingParty : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Rogue.RaidingParty;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.IsPirate(), card.Type == "Weapon");
}

public class RaidingPartyCore : RaidingParty
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.RaidingPartyCore;
}
