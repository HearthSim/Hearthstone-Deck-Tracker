using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class FandralStaghelm : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Druid.FandralStaghelm;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.CHOOSE_ONE) > 0);
}

public class FandralStaghelmCorePlaceholder : FandralStaghelm
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.FandralStaghelmCore;
}
