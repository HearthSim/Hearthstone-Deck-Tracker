using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class PterrordaxEgg: ICardWithRelatedCards
{
	protected readonly List<Card?> Token = new() {
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.PterrordaxEgg_JuvenilePterrordaxToken)
	};
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.PterrordaxEgg;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => Token;
}
