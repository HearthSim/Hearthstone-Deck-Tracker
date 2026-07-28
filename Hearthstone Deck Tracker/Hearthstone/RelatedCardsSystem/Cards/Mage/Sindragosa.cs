using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Deathrattle: Add a random Legendary minion to your hand."
public class FrozenChampionToken : LegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Mage.Sindragosa_FrozenChampionToken;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
