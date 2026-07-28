using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Deathrattle: If your deck has no Neutral cards, get a random Paladin card. It costs (2) less."
public class ScarletBruiser : PaladinCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.ScarletBruiser;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
