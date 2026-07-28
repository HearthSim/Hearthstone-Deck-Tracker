using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Summon a random 1-Cost minion for your opponent."
public class GravelsnoutKnight : Cost1MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.GravelsnoutKnight;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
