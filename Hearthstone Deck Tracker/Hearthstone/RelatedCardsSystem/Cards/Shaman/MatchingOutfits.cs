namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Transform a minion into a random one that costs (1) more, then summon a copy of it."
public class MatchingOutfits : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.MatchingOutfits;
	protected override int CostOffset => 1;
	protected override bool AffectsAllTargets => false;
}
