using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class ChiaDrake : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Druid.ChiaDrake;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.TypeEnum == CardType.SPELL);
}

public class ChiaDrakeMini : ChiaDrake
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Druid.ChiaDrake_ChiaDrakeToken;
}
