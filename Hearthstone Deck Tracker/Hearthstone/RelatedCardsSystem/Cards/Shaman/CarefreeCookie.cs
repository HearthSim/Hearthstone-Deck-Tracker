namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Demon Hunter Tourist. After a friendly minion dies, summon a random minion that costs (1) more."
// Which minion dies next is unknown, so friendly minions are candidates and the summary
// averages over them.
public class CarefreeCookie : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.CarefreeCookie;
	protected override int CostOffset => 1;
	protected override bool AffectsAllTargets => false;
}
