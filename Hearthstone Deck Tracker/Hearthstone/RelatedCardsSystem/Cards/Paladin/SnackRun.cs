namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Discover a spell. Restore Health to your hero equal to its Cost."
// Same effect and pool as IvoryKnight.
public class SnackRun : IvoryKnightKARA
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.SnackRun;
}
