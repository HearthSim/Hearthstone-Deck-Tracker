namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Battlecry: Transform your minions into random ones that cost (2) more."
public class ThrallDeathseer : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.ThrallDeathseer;
	protected override int CostOffset => 2;
}

public class ThrallDeathseerCorePlaceholder : ThrallDeathseer
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.ThrallDeathseerCorePlaceholder;
}
