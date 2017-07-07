using System;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HmDeck = HearthMirror.Objects.Deck;
using HmCard = HearthMirror.Objects.Card;

namespace HDTTests.DeckImporting
{
	public class DeckComparer
	{
		public static void AssertAreEqual(object deckObj1, object deckObj2)
		{
			Assert.AreEqual(GetDeckId(deckObj1), GetDeckId(deckObj2));
			Assert.AreEqual(GetDeckName(deckObj1), GetDeckName(deckObj2));
			Assert.AreEqual(GetDeckPlayerClass(deckObj1), GetDeckPlayerClass(deckObj2));
			Assert.AreEqual(GetDeckCardCount(deckObj1), GetDeckCardCount(deckObj2));
			Assert.AreEqual(GetDeckCardCountSum(deckObj1), GetDeckCardCountSum(deckObj2));
			if(deckObj1 is Deck deck1)
			{
				if(deckObj2 is Deck deck2)
				{
					foreach(var card in deck1.Cards)
						AssertContainsCard(deck2, card);
				}
				else if(deckObj2 is HmDeck hmDeck2)
				{
					foreach(var card in deck1.Cards)
						AssertContainsCard(hmDeck2, card);
				}
				else
					throw new ArgumentException("Invalid deck object", nameof(deckObj2));
			}
			else if(deckObj1 is HmDeck hmDeck1)
			{
				if(deckObj2 is Deck deck2)
				{
					foreach(var card in hmDeck1.Cards)
						AssertContainsCard(deck2, card);
				}
				else if(deckObj2 is HmDeck hmDeck2)
				{
					foreach(var card in hmDeck2.Cards)
						AssertContainsCard(hmDeck2, card);
				}
				else
					throw new ArgumentException("Invalid deck object", nameof(deckObj2));
			}
			else
				throw new ArgumentException("Invalid deck object", nameof(deckObj1));
		}

		private static long GetDeckId(object deckObj)
		{
			if(deckObj is Deck deck)
				return deck.HsId;
			if(deckObj is HmDeck hmDeck)
				return hmDeck.Id;
			throw new ArgumentException("Invalid deck object");
		}

		private static string GetDeckName(object deckObj)
		{
			if(deckObj is Deck deck)
				return deck.Name;
			if(deckObj is HmDeck hmDeck)
				return hmDeck.Name;
			throw new ArgumentException("Invalid deck object");
		}

		private static string GetDeckPlayerClass(object deckObj)
		{
			if(deckObj is Deck deck)
				return deck.Class.ToLower();
			if(deckObj is HmDeck hmDeck)
				return HearthDb.Cards.All[hmDeck.Hero].Class.ToString().ToLower();
			throw new ArgumentException("Invalid deck object");
		}

		private static int GetDeckCardCount(object deckObj)
		{
			if(deckObj is Deck deck)
				return deck.Cards.Count;
			if(deckObj is HmDeck hmDeck)
				return hmDeck.Cards.Count;
			throw new ArgumentException("Invalid deck object");
		}

		private static int GetDeckCardCountSum(object deckObj)
		{
			if(deckObj is Deck deck)
				return deck.Cards.Sum(x => x.Count);
			if(deckObj is HmDeck hmDeck)
				return hmDeck.Cards.Sum(x => x.Count);
			throw new ArgumentException("Invalid deck object");
		}

		private static void AssertContainsCard(object deckObj, object cardObj)
		{
			var id = GetCardId(cardObj);
			var count = GetCardCount(cardObj);
			if(deckObj is Deck deck)
				Assert.AreEqual(1, deck.Cards.Count(c => c.Id == id && c.Count == count));
			else if (deckObj is HmDeck hmDeck)
				Assert.AreEqual(1, hmDeck.Cards.Count(c => c.Id == id && c.Count == count));
			else
				throw new ArgumentException("Invalid deck object");
		}

		private static string GetCardId(object cardObj)
		{
			if(cardObj is Card card)
				return card.Id;
			if(cardObj is HmCard hmCard)
				return hmCard.Id;
			throw new ArgumentException("Invalid card object");
		}

		private static int GetCardCount(object cardObj)
		{
			if(cardObj is Card card)
				return card.Count;
			if(cardObj is HmCard hmCard)
				return hmCard.Count;
			throw new ArgumentException("Invalid card object");
		}
	}
}
