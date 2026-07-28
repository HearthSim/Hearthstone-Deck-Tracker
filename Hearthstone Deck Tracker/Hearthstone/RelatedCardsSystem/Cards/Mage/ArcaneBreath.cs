namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Deal $2 damage to a minion. If you're holding a Dragon, Discover a spell."
public class ArcaneBreath : RunedOrb
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.ArcaneBreath;
}
