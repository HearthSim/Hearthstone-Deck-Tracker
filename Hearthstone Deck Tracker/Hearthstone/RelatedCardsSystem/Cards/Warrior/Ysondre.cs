using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Taunt. Deathrattle: Summon a random Dragon for each time Ysondre has died this game."
public class Ysondre : DragonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.Ysondre;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}
