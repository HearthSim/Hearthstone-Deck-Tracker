namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Battlecry: If you've cast 2 Shadow spells while holding this, choose a C'Thun ability to trigger."
public class DarkDeacon : CthunAbilityCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.DarkDeacon;
}
