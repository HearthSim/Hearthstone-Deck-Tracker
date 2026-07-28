using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Casts When Drawn. Cast a random spell."
public class ScrollOfWonderToken : SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Mage.DeckofWonders_ScrollOfWonderToken;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}

// "Shuffle 5 Scrolls into your deck. When drawn, cast a random spell."
public class DeckOfWonders : SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.DeckOfWonders;
	public override int Picks() => 1;
	public override int EventCount() => 5;
	public override bool IsWithReplacement() => true;
}
