namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Transform a friendly minion into one that costs (2) more. Infuse (4): Transform all friendly minions instead."
// Modeled as the un-infused, single-target version.
public class ConvincingDisguise : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.ConvincingDisguise;
	protected override int CostOffset => 2;
	protected override bool AffectsAllTargets => false;
}

public class ConvincingDisguiseCorePlaceholder : ConvincingDisguise
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.ConvincingDisguiseCorePlaceholder;
}
