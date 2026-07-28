using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Whenever this enters your hand from the battlefield, summon two random 2-Cost minions."
// 2-Cost minion pool (two draws) inherited from DistressSignal.
public class HarbingerOfTheBlighted : Cost2MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.HarbingerOfTheBlighted;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;
}
