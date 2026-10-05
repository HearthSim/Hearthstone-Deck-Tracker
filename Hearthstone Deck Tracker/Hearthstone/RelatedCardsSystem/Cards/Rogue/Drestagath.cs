using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Deathrattle: Draw an Old God. If you can't, get a random Old God."
public class Drestagath : ICardWithHighlight, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.Drestagath;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.HasTag(GameTag.OLD_GOD));

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.Collectible.Neutral.MastermindYoggSaron),
			Database.GetCardFromId(HearthDb.CardIds.Collectible.Neutral.CthunEyeOfTheEmpire),
			Database.GetCardFromId(HearthDb.CardIds.Collectible.Neutral.NzothDreamsEnd),
			Database.GetCardFromId(HearthDb.CardIds.Collectible.Neutral.YshaarjTitanBane),
		};
}
