using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "At the end of your turn, summon two random Elementals."
public class ElementalRift : ElementalMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Shaman.ElementalRift;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;
}
