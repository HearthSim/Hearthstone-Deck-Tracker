using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Taunt. Deathrattle: Get 2 random Fel spells."
public class WhisperingStone : FelSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.WhisperingStone;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;
}
