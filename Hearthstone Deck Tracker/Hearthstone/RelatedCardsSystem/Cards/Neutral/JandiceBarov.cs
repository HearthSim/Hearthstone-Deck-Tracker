
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Summon two random 5-Cost minions. Secretly pick one that dies when it takes damage."
public class JandiceBarov : Cost5MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.JandiceBarov;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;
}
