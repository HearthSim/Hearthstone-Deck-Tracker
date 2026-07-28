using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Rewind Battlecry: Both players equip a random weapon. Give yours +1/+1."
public class StadiumAnnouncer : WeaponPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.StadiumAnnouncer;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
