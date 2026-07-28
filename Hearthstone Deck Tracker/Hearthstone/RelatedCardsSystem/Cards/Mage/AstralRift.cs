using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Add 2 random minions to your hand."
public class AstralRift : MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.AstralRift;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;
}
