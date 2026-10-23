namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Battlecry: Excavate a treasure. It costs (0)."
public class SkeletonCrew : ExcavateCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.SkeletonCrew;
}
