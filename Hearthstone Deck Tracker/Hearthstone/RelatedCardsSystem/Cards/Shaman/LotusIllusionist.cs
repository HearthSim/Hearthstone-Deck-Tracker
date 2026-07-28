
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "After this minion attacks a hero, transform it into a random 6-Cost minion."
public class LotusIllusionist : Cost6MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.LotusIllusionist;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
