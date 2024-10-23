using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

public class RelatedCardsManager
{
	public readonly Dictionary<string, ICardWithRelatedCards> Cards = new();

	private void Initialize()
	{
		var cards = Assembly.GetAssembly(typeof(ICardWithRelatedCards)).GetTypes()
			.Where(t => t.IsClass && !t.IsAbstract && typeof(ICardWithRelatedCards).IsAssignableFrom(t));

		foreach(var card in cards)
		{
			if (Activator.CreateInstance(card) is not ICardWithRelatedCards cardWithRelatedCards) continue;
			Cards.Add(cardWithRelatedCards.GetCardId(), cardWithRelatedCards);
		}
	}

	public void Reset()
	{
		if (Cards.Count == 0) Initialize();
	}

	public ICardWithRelatedCards GetCardWithRelatedCards(string cardId)
	{
		return Cards.TryGetValue(cardId, out var card) ? card : Cards[""];
	}

	public IEnumerable<Card> GetCardsOpponentMayHave(Player opponent)
	{
		return Cards.Values.Where(card => card.ShouldShowForOpponent(opponent))
			.Select(card => Database.GetCardFromId(card.GetCardId())).WhereNotNull();
	}
}
