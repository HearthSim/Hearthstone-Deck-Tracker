namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class WyvernsSlumber: DormantDreadseedsGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.WyvernsSlumber;

	public bool ShouldShowForOpponent(Player opponent) => false;
}
