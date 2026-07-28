using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Reopen a location. Give it "Deathrattle: Summon a random 3-Cost minion.""
public class WelcomeHome : Cost3MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.WelcomeHome;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
