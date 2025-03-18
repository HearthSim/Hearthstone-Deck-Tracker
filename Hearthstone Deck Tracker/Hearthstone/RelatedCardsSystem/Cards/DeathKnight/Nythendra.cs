using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class Nythendra: ICardWithRelatedCards
{
	protected readonly List<Card?> NythendricBeetle = new() {
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Deathknight.Nythendra_NythendricBeetleToken),
	};
	public string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.Nythendra;

	public List<Card?> GetRelatedCards(Player player) => NythendricBeetle;

	public bool ShouldShowForOpponent(Player opponent) => false;
}
