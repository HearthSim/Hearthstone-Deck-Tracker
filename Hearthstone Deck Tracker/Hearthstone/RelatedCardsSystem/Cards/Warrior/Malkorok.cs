
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Battlecry: Equip a random weapon."
public class Malkorok : WeaponPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.Malkorok;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
