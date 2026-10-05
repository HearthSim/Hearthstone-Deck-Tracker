using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Deathrattle: Summon two random 1-Cost minions. Give your minions +3 Attack."
public class XelnothBehemoth : Cost1MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.XelnothBehemoth;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;
}
