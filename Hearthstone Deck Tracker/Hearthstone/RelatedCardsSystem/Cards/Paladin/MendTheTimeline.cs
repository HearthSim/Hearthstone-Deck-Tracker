using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Rewind Get 2 random Holy spells. Restore Health to your hero equal to their Costs."
public class MendTheTimeline : HolySpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.MendTheTimeline;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;
}
