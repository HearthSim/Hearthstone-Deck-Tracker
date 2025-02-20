using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

public class RelatedCardsManager
{
	private Dictionary<string, ICardWithRelatedCards>? _relatedCards;
	private Dictionary<string, ICardWithHighlight>? _highlightCards;

	public Dictionary<string, ICardWithRelatedCards> RelatedCards => _relatedCards ??= InitializeRelatedCards();
	public Dictionary<string, ICardWithHighlight> HighlightCards  => _highlightCards ??= InitializeHighlightCards();

	private Dictionary<string, ICardWithRelatedCards> InitializeRelatedCards()
	{
		var (relatedCardsDict, highlightCardsDict ) = InitializeCards();
		_highlightCards = highlightCardsDict;
		return relatedCardsDict;
	}

	private Dictionary<string, ICardWithHighlight> InitializeHighlightCards()
	{
		var (relatedCardsDict, highlightCardsDict ) = InitializeCards();
		_relatedCards = relatedCardsDict;
		return highlightCardsDict;
	}

	private (Dictionary<string, ICardWithRelatedCards> , Dictionary<string, ICardWithHighlight>) InitializeCards()
	{
		var cards = Assembly.GetAssembly(typeof(ICard)).GetTypes()
			.Where(t => t.IsClass && !t.IsAbstract && typeof(ICard).IsAssignableFrom(t));

		var relatedCardsDict = new Dictionary<string, ICardWithRelatedCards>();
		var highlightCardsDict = new Dictionary<string, ICardWithHighlight>();

		foreach(var card in cards)
		{
			var cardInstance = Activator.CreateInstance(card) as ICard;

			if(cardInstance is ICardWithRelatedCards relatedCard)
			{
				relatedCardsDict[relatedCard.GetCardId()] = relatedCard;
			}

			if(cardInstance is ICardWithHighlight highlightCard)
			{
				highlightCardsDict[highlightCard.GetCardId()] = highlightCard;
			}
		}

		return (relatedCardsDict, highlightCardsDict);
	}

	public ICardWithHighlight? GetCardWithHighlight(string cardId)
	{
		return HighlightCards.TryGetValue(cardId, out var card) ? card : null;
	}

	public ICardWithRelatedCards? GetCardWithRelatedCards(string cardId)
	{
		return RelatedCards.TryGetValue(cardId, out var card) ? card : null;
	}

	public IEnumerable<Card> GetCardsOpponentMayHave(Player opponent)
	{
		return RelatedCards.Values.Where(card => card.ShouldShowForOpponent(opponent))
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
