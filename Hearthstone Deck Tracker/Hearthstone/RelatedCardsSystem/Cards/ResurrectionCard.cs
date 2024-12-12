using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards;

public abstract class ResurrectionCard: ICardWithRelatedCards
{
	public abstract string GetCardId();

	/// Don't usually show these cards for the opponent, because minions typically die all the time.
	public bool ShouldShowForOpponent(Player opponent) => false;

	protected abstract bool FilterCard(Card card);

	protected abstract bool ResurrectsMultipleCards();

	public List<Card?> GetRelatedCards(Player player)
	{
		 var cards = player.DeadMinionsCards.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player)).Where(card => card is Card c && FilterCard(c));

		 if(!ResurrectsMultipleCards())
		 {
			 // If the card resurrects only one card, we want to see a clean selection of possibilities we can hit without duplicates
			 cards = cards.Distinct();
		 }

		 // The order is not normally relevant because the card will pull from a random pool, so sort by cost as impact
		 return cards.OrderByDescending(x => x?.Cost ?? 0).ToList();
	}
}
