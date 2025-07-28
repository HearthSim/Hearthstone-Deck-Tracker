using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class KiriChosenOfElune: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.KiriChosenOfElune;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new()
		{
			Database.GetCardFromId(HearthDb.CardIds.Collectible.Druid.SolarEclipse),
			Database.GetCardFromId(HearthDb.CardIds.Collectible.Druid.LunarEclipse),
		};
}
