namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Reveal a spell from your deck. Summon a random minion with the same Cost."
// One unknown revealed deck spell -> averaged mixture over the deck spells' cost buckets.
public class SpitefulSummoner : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.SpitefulSummoner;
	protected override int CostOffset => 0;
	protected override RelativeCostTargetSource TargetSource => RelativeCostTargetSource.DeckSpells;
	protected override bool AffectsAllTargets => false;
}
