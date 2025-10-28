using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

public class GladiatorialCombat : ICardWithHighlight, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warrior.GladiatorialCombat;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion");

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => new List<Card?>()
		{ Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warrior.GladiatorialCombat_ColiseumTigerToken) };
}
