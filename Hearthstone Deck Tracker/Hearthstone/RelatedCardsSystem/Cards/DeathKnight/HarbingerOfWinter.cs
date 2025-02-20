using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class HarbingerOfWinter : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.HarbingerOfWinterCore;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FROST);
}

public class HarbingerOfWinterCore : HarbingerOfWinter
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Deathknight.HarbingerOfWinterCore;

}
