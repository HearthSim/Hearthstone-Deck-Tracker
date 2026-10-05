namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Choose a friendly minion. Summon a random minion of the same Cost."
public class ConstellarsCalling : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.ConstellarsCalling;
	protected override int CostOffset => 0;
	protected override bool AffectsAllTargets => false;
}
