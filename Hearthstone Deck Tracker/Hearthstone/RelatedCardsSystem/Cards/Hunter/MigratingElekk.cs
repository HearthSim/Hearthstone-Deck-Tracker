namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Taunt. Battlecry: Replace your future Animal Companions with random Beasts that cost (1) more."
public class MigratingElekk : AnimalCompanionUpgradeCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.MigratingElekk;
	protected override int CostOffset => 1;
}
