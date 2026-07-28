using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Taunt Deathrattle: Get a random Rewind card."
public class TimeMachine : RewindCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.TimeMachine;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
