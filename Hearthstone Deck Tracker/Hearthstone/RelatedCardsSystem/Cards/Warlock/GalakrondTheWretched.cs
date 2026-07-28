
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Battlecry: Summon 1 random Demon."
public class GalakrondTheWretched : DemonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.GalakrondTheWretched;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
