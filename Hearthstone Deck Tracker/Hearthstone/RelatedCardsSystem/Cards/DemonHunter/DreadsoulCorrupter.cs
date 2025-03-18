namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class DreadsoulCorrupter: DormantDreadseedsGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.DreadsoulCorrupter;

	public bool ShouldShowForOpponent(Player opponent) => false;
}
