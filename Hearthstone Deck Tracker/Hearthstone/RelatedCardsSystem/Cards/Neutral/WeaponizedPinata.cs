using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Deathrattle: Add a random Legendary minion to your hand."
public class WeaponizedPinata : LegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.WeaponizedPiñata;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
