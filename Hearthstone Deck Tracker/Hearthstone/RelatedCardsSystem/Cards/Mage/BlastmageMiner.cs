namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Excavate a treasure. For each card in your hand, deal 1 damage to a random enemy."
public class BlastmageMiner : ExcavateCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.BlastmageMiner;
}
