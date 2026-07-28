using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "After your hero attacks, summon a random 4-Cost minion."
public class GeniusOfMimironToken2 : Cost4MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.GeniusOfMimironToken2;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
