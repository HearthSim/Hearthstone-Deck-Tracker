namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "In 2 turns, choose a C'Thun ability to trigger."
public class RitualOfCthun : CthunAbilityCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.RitualOfCthun;
}
