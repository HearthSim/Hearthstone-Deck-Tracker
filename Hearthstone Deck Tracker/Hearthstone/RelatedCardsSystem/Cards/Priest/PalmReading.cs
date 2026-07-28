namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Discover a spell. Reduce the Cost of spells in your hand by (1)."
public class PalmReading : Renew
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.PalmReading;
}
