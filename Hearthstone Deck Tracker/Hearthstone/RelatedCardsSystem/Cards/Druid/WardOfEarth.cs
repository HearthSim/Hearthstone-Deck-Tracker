using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Gain 5 Armor. Summon a random 5-Cost minion and give it Taunt."
public class WardOfEarth : Cost5MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.WardOfEarth;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
