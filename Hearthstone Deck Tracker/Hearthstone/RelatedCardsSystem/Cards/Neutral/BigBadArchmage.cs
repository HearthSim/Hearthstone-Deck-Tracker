using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "At the end of your turn, summon a random 6-Cost minion."
public class BigBadArchmage : Cost6MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.BigBadArchmage;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
