using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Battlecry: Get 4 random spells. They cost (1) less for each time you've Excavated this game (but not less than 1)."
public class TheAzeriteScorpion : SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Rogue.KoboldMiner_TheAzeriteScorpionToken;
	public override int Picks() => 1;
	public override int EventCount() => 4;
	public override bool IsWithReplacement() => true;
}
