namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Give a friendly minion 'Deathrattle: Summon a random minion that costs (1) more.'"
public class BigBadVoodoo : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.BigBadVoodoo;
	protected override int CostOffset => 1;
	protected override bool AffectsAllTargets => false;
}
