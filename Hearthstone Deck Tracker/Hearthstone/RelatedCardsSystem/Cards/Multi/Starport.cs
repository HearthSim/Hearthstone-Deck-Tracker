using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Multi;

public class Starport: ICardWithRelatedCards
{
	private readonly List<Card?> _starshipPieces = new List<Card?>
	{
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Invalid.Starport_Viking),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Invalid.Starport_Liberator),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Invalid.Starport_Raven2),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Invalid.Starport_Banshee2),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Invalid.Starport_Medivac2)
	};

	public string GetCardId() => HearthDb.CardIds.Collectible.Invalid.Starport;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => _starshipPieces;
}
