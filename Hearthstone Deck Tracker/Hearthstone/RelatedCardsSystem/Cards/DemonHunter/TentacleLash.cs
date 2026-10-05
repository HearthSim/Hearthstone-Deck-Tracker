namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "After your hero attacks, reduce the Cost of C'Thun by (1). Choose a C'Thun ability to trigger."
public class TentacleLash : CthunAbilityCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.TentacleLash;
}
