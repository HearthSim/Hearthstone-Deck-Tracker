using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Add 5 random cards from other classes to your hand. They are Temporary."
public class TwistedPack : OffClassCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.TwistedPack;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 5;
}
