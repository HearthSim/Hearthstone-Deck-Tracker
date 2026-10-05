using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Summon two 1/1 Mech Beasts with Charge."
public class ClockworkBoars : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.ClockworkBoars;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Hunter.ClockworkBoars_ClockworkBoarToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Hunter.ClockworkBoars_ClockworkBoarToken),
		};
}
