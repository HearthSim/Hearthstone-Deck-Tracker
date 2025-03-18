namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class GrimHarvest: DormantDreadseedsGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.GrimHarvest;

	public bool ShouldShowForOpponent(Player opponent) => false;
}
