namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Transform a friendly minion into a random minion with the same Cost."
// One chosen friendly minion -> averaged mixture over its cost bucket.
public class Recombobulator : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Recombobulator;
	protected override int CostOffset => 0;
	protected override bool AffectsAllTargets => false;
}
