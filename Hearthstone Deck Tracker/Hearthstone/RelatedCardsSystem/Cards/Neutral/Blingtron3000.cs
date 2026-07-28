using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Equip a random weapon for each player."
public class Blingtron3000 : WeaponPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Blingtron3000;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
