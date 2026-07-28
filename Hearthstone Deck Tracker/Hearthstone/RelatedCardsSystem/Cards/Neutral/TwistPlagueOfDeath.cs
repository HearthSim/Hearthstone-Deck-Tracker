using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "When 3 friendly minions die, summon a random minion."
public class EternalTombToken : MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.TwistPlagueofDeath_EternalTombToken;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
