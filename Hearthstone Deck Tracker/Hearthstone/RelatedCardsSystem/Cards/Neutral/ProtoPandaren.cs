using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "After a card enters your hand except from this, give both players a random spell."
public class ProtoPandaren : SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ProtoPandaren;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
