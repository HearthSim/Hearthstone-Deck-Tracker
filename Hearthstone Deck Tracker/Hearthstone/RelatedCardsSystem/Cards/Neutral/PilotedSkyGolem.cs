using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Deathrattle: Summon a random 4-Cost minion."
public class PilotedSkyGolem : Cost4MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.PilotedSkyGolem;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
