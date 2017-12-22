using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone;
using HmDeck = HearthMirror.Objects.Deck;
using HmCard = HearthMirror.Objects.Card;

namespace HDTTests.DeckImporting
{
	public class DataGenerator
	{
		public static Card GetCard(string cardId, int count = 2) => new Card(HearthDb.Cards.Collectible[cardId]) { Count = count };

		public static HmCard GetHmCard(string cardId, int count = 2) => new HmCard(cardId, count, false);

		public static IEnumerable<T> GetCards<T>(string[] cardIds, Func<string, int, T> func) => GetCards(cardIds, new string[0], func);

		public static IEnumerable<T> GetCards<T>(string[] cardIds, string[] singles, Func<string, int, T> func)
		{
			int GetCount(string id) => singles.Contains(id) ? 1 : 2;
			return cardIds.Select(c => func(c, GetCount(c)));
		}

		public static Deck GetDeck(long id, string playerClass, string name, string[] cards, string[] singles = null)
		{
			return new Deck
			{
				HsId = id,
				Class = playerClass,
				Name = name,
				Cards = new ObservableCollection<Card>(GetCards(cards, singles ?? new string[0], GetCard))
			};
		}

		public static HmDeck GetHmDeck(long id, string heroId, string name, string[] cards, string[] singles = null)
		{
			return new HmDeck
			{
				Id = id,
				Hero = heroId,
				Name = name,
				Cards = new List<HmCard>(GetCards(cards, singles ?? new string[0], GetHmCard))
			};
		}
	}
}