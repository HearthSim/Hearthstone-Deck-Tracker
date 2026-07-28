using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Add 2 random Taunt minions to your hand."
public class InFormation : TauntMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.InFormation;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;
}
