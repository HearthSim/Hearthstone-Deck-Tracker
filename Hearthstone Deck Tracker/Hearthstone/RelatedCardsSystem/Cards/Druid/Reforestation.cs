using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class Reforestation : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Druid.Reforestation;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card.Type == "Minion",
			card.Type == "Spell"
		);
}

public class ReforestationToken : Reforestation
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Druid.Reforestation_ReforestationToken;

}
