using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BobsBuddy.Simulation;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

public class RelatedCardsManager
{
	private Dictionary<string, ICardWithRelatedCards>? _relatedCards;
	private Dictionary<string, ICardWithHighlight>? _highlightCards;
	private Dictionary<string, ISpellSchoolTutor>? _spellSchoolTutorCards;
	private Dictionary<string, ICardGenerator>? _cardGeneratorCards;

	public Dictionary<string, ICardWithRelatedCards> RelatedCards => _relatedCards ??= InitializeRelatedCards();
	public Dictionary<string, ICardWithHighlight> HighlightCards  => _highlightCards ??= InitializeHighlightCards();
	public Dictionary<string, ISpellSchoolTutor> SpellSchoolTutorCards  => _spellSchoolTutorCards ??= InitializeSpellSchoolTutorCards();
	public Dictionary<string, ICardGenerator> CardGeneratorCards  => _cardGeneratorCards ??= InitializeCardGeneratorCards();


	private Dictionary<string, ICardWithRelatedCards> InitializeRelatedCards()
	{
		var (relatedCardsDict, highlightCardsDict, spellSchoolTutorCardsDict, generatorsDict ) = InitializeCards();
		_highlightCards = highlightCardsDict;
		_spellSchoolTutorCards = spellSchoolTutorCardsDict;
		_cardGeneratorCards = generatorsDict;
		return relatedCardsDict;
	}

	private Dictionary<string, ICardWithHighlight> InitializeHighlightCards()
	{
		var (relatedCardsDict, highlightCardsDict, spellSchoolTutorCardsDict, generatorsDict ) = InitializeCards();
		_relatedCards = relatedCardsDict;
		_spellSchoolTutorCards = spellSchoolTutorCardsDict;
		_cardGeneratorCards = generatorsDict;
		return highlightCardsDict;
	}

	private Dictionary<string, ISpellSchoolTutor> InitializeSpellSchoolTutorCards()
	{
		var (relatedCardsDict, highlightCardsDict, spellSchoolTutorCardsDict, generatorsDict ) = InitializeCards();
		_relatedCards = relatedCardsDict;
		_highlightCards = highlightCardsDict;
		_cardGeneratorCards = generatorsDict;
		return spellSchoolTutorCardsDict;
	}

	private Dictionary<string, ICardGenerator> InitializeCardGeneratorCards()
	{
		var (relatedCardsDict, highlightCardsDict, spellSchoolTutorCardsDict, generatorsDict ) = InitializeCards();
		_relatedCards = relatedCardsDict;
		_highlightCards = highlightCardsDict;
		_spellSchoolTutorCards = spellSchoolTutorCardsDict;
		return generatorsDict;
	}

	private (
		Dictionary<string, ICardWithRelatedCards>,
		Dictionary<string, ICardWithHighlight>,
		Dictionary<string, ISpellSchoolTutor>,
		Dictionary<string, ICardGenerator> )
		InitializeCards()
	{
		var cards = Assembly.GetAssembly(typeof(ICard)).GetTypes()
			.Where(t => t.IsClass && !t.IsAbstract && typeof(ICard).IsAssignableFrom(t));

		var relatedCardsDict = new Dictionary<string, ICardWithRelatedCards>();
		var highlightCardsDict = new Dictionary<string, ICardWithHighlight>();
		var spellSchoolTutorCardsDict = new Dictionary<string, ISpellSchoolTutor>();
		var cardGeneratorCardsDict = new Dictionary<string, ICardGenerator>();


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

			if(cardInstance is ISpellSchoolTutor tutor)
			{
				spellSchoolTutorCardsDict[tutor.GetCardId()] = tutor;
			}

			if(cardInstance is ICardGenerator generator)
			{
				cardGeneratorCardsDict[generator.GetCardId()] = generator;
			}
		}

		return (relatedCardsDict, highlightCardsDict, spellSchoolTutorCardsDict, cardGeneratorCardsDict);
	}

	public ICardWithHighlight? GetCardWithHighlight(string cardId)
	{
		return HighlightCards.TryGetValue(cardId, out var card) ? card : null;
	}

	public ICardWithRelatedCards? GetCardWithRelatedCards(string cardId)
	{
		return RelatedCards.TryGetValue(cardId, out var card) ? card : null;
	}

	public ISpellSchoolTutor? GetSpellSchoolTutor(string cardId)
	{
		return SpellSchoolTutorCards.TryGetValue(cardId, out var card) ? card : null;
	}

	public IEnumerable<Card> GetCardsOpponentMayHave(Player opponent, GameType gameType, FormatType format)
	{
		return RelatedCards.Values.Where(card => card.ShouldShowForOpponent(opponent) && card.IsCardLegal(gameType, format))
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
