using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class Mothership: ICardWithRelatedCards
{
	private List<Card?>? _cache = null;
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.Mothership;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player){
		if(_cache != null) return _cache;

		var cardId = GetCardId();

		_cache = HearthDb.Cards.Collectible.Values
			.Where(c =>
				c.Entity.GetTag(GameTag.PROTOSS) > 0 &&
				c.Type == CardType.MINION &&
				c.Id != cardId)
			.Select(c => new Card(c))
			.OrderBy(card => card.Cost)
			.ToList()!;

		return _cache;

	}
}
