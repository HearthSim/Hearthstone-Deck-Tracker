using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Summon a random 4-Cost minion and give it Taunt. Kindred: Do it again."
public class GravedawnVoidbulb : Cost4MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.GravedawnVoidbulb;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
