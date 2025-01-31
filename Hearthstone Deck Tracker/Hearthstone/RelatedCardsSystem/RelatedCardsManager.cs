using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

public class RelatedCardsManager
{
	private Dictionary<string, ICardWithRelatedCards>? _relatedCards;
	public Dictionary<string, ICardWithRelatedCards> Cards => _relatedCards ??= Initialize();

	private Dictionary<string, ICardWithRelatedCards> Initialize()
	{
		var cards = Assembly.GetAssembly(typeof(ICardWithRelatedCards)).GetTypes()
			.Where(t => t.IsClass && !t.IsAbstract && typeof(ICardWithRelatedCards).IsAssignableFrom(t));

		var dict = new Dictionary<string, ICardWithRelatedCards>();
		foreach(var card in cards)
		{
			if (Activator.CreateInstance(card) is not ICardWithRelatedCards cardWithRelatedCards)
				continue;
			dict[cardWithRelatedCards.GetCardId()] = cardWithRelatedCards;
		}

		return dict;
	}

	public ICardWithRelatedCards GetCardWithRelatedCards(string cardId)
	{
		return Cards.TryGetValue(cardId, out var card) ? card : Cards[""];
	}

	public IEnumerable<Card> GetCardsOpponentMayHave(Player opponent)
	{
		return Cards.Values.Where(card => card.ShouldShowForOpponent(opponent))
			.Select(card =>
			{
				var c =  Database.GetCardFromId(card.GetCardId());
				if(c != null)
				{
					// Used for related cards tooltip
					c.ControllerPlayer = opponent;
				}
				return c;
			}).WhereNotNull();
	}
}
