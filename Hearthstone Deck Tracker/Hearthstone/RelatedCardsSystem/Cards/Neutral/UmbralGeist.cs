using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Deathrattle: Add a random Shadow spell to your hand."
public class UmbralGeist : ShadowSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.UmbralGeist;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
