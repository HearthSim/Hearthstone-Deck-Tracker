using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Add 5 random Taunt minions to your hand. They are Temporary."
public class StandardizedPack : TauntMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.StandardizedPack;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 5;
}
