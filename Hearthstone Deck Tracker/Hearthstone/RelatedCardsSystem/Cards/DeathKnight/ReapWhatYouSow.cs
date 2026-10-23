namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Deal 3 damage. Excavate a treasure."
public class ReapWhatYouSow : ExcavateCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.ReapWhatYouSow;
}
