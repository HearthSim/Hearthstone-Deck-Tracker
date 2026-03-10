using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class StolenPower : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.StolenPower;

	private readonly List<Card?> _shatterCards = new List<Card?> {
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Druid.WildwoodCircle),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Hunter.SupplyRun),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Mage.ArcaneFlow),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Paladin.FlightManeuvers),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Priest.Schism),
	};

	public bool ShouldShowForOpponent(Player opponent) => false;
	public List<Card?> GetRelatedCards(Player player) => _shatterCards;
}
