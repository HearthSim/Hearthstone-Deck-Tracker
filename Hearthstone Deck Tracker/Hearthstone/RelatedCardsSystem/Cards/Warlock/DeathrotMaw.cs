using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class DeathrotMaw: ICardWithRelatedCards
{
	private readonly List<Card?> FelBeasts = new List<Card?> {
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warlock.EscapetheUnderfel_FelscreamerToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warlock.EscapetheUnderfel_FelraptorToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warlock.EscapetheUnderfel_FelhornToken),
	};
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.DeathrotMaw;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => FelBeasts;
}
