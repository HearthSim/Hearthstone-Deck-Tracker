using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Deathrattle: Summon a random 7-Cost minion."
public class PlaguedProtodrake : Cost7MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.PlaguedProtodrake;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
