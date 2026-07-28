using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Summon four random 5-Cost minions. Make them 2/2."
public class TinyToys : Cost5MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.TinyToys;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 4;
}
