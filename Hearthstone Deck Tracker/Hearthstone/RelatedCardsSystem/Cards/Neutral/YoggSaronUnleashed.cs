
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Titan After this uses an ability, cast two random spells."
public class YoggSaronUnleashed : SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.YoggSaronUnleashed;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;
}
