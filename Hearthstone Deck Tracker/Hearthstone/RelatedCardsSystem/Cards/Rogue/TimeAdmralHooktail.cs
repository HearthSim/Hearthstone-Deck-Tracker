using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class TimeAdmralHooktail: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.TimeAdmralHooktail;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => new List<Card?>()
	{
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Rogue.TimeAdmralHooktail_TimelessChestToken),
	};
}
