namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Battlecry: Transform a friendly minion into a random one that costs (1) more."
public class MasterOfEvolution : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.MasterOfEvolution;
	protected override int CostOffset => 1;
	protected override bool AffectsAllTargets => false;
}
