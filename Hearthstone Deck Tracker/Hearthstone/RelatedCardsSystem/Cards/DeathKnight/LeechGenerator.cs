using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public abstract class LeechGenerator
{
	protected readonly List<Card?> Leech = new() {
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Deathknight.HideousHusk_BloatedLeechToken),
	};

	public List<Card?> GetRelatedCards(Player player) =>
		Leech;
}
