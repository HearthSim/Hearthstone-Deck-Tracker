using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Taunt Deathrattle: Summon a random 2-Cost minion."
public class LivingStone : Cost2MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_LivingStoneToken;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
