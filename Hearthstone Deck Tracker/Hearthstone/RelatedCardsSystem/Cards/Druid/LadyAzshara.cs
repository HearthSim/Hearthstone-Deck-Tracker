using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Fill your hand with random Temporary spells. They cast twice." (Temporary is post-pick modifier, ignored for pool)
public class TheWellOfEternityToken : SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Druid.LadyAzshara_TheWellOfEternityToken2;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}
