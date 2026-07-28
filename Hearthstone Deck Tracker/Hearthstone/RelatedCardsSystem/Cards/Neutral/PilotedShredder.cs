using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Deathrattle: Summon a random 2-Cost minion."
public class PilotedShredder : Cost2MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.PilotedShredder;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}
