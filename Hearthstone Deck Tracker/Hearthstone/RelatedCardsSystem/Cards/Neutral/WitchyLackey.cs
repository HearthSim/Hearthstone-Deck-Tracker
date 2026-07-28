namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Transform a friendly minion into one that costs (1) more."
// Non-collectible Lackey token.
public class WitchyLackey : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.WitchyLackey;
	protected override int CostOffset => 1;
	protected override bool AffectsAllTargets => false;
}
