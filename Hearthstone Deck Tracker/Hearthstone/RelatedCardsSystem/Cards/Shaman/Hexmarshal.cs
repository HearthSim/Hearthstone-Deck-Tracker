using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Battlecry: Get a random spell that costs (5) or more. If your deck started with no spells, it costs (5) less."
public class Hexmarshal : CostAtLeast5SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Hexmarshal;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
