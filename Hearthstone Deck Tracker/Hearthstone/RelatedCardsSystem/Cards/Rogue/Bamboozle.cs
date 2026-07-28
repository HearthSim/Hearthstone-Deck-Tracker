namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Secret: When one of your minions is attacked, transform it into a random one that costs (3) more."
// Which minion gets attacked is unknown, so friendly minions are candidates and the
// summary averages over them.
public class Bamboozle : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.Bamboozle;
	protected override int CostOffset => 3;
	protected override bool AffectsAllTargets => false;
}
