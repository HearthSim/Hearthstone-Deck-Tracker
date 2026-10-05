using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry and Deathrattle: Cast a random 3-Cost spell."
public class AstralMediator : Cost3SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.AstralMediator;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;
}
