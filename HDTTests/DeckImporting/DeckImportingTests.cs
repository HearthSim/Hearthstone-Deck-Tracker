using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Hearthstone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HmDeck = HearthMirror.Objects.Deck;

namespace HDTTests.DeckImporting
{
	[TestClass]
	public class DeckImportingTests
	{

		/*
		 * Deck importing test cases:
		 * 
		 * No local decks
		 *	- no decks in hearthstone
		 *	- new decks in hearthstone
		 *
		 * Local decks
		 *	- no decks in hearthstone
		 *	- new deck in hearthstone (new id, fully distinct cards from local decks)
		 *	- new deck in hearthstone (existing id, fully distinct cards from local decks)
		 *	- new deck in hearthstone (new id, minor changes from local deck)
		 *	- new deck in hearthstone (existing id, minor changes from local deck)
		 *	- new deck in hearthstone (new id, major changes from local deck)
		 *	- new deck in hearthstone (existing id, major changes from local deck)
		 *	- new deck in hearthstone (new id, cards exactly match local deck)
		 *	- new deck in hearthstone (existing id, cards exatly match local deck)
		 *  - new deck in hearthstone (less than 30 cards);
		 *  
		 *  - archived decks
		 *  - same vs different class
		 *  - golden vs nongolden
		 *	
		 *	"id": hearthstones internal id
		 *	"existing id": local deck with id
		 *	"new id" no local deck with id
		 *	"minor changes": less than 4 cards different
		 *	"major changes": less than 10 cards different
		 */

		private List<Deck> _localDecks;
		private List<HmDeck> _remoteDecks;

		[TestInitialize]
		public void Setup()
		{
			_localDecks = new List<Deck>();
			_remoteDecks = new List<HmDeck>();
		}

		[TestMethod]
		public void TestDataIsCorrect()
		{
			var deckPairs = new[]
			{
				new { Local = TestData.LocalDeck1, Remote = TestData.RemoteDeck1 },
				new { Local = TestData.LocalDeck2, Remote = TestData.RemoteDeck2 },
			};

			foreach(var deckPair in deckPairs)
				DeckComparer.AssertAreEqual(deckPair.Local, deckPair.Remote);
		}

		[TestMethod]
		public void NoLocal_NoRemote()
		{
			Assert.AreEqual(0, _localDecks.Count);
			Assert.AreEqual(0, _remoteDecks.Count);
			// var decks = GetImportableDecks(_remoteDecks, _localDecks);
			// Assert.AreEqual(0, decks.Count);
		}

		[TestMethod]
		public void NoLocal_NewRemote()
		{
			_remoteDecks.Add(TestData.RemoteDeck1);
			Assert.AreEqual(0, _localDecks.Count);
			Assert.AreEqual(1, _remoteDecks.Count);
			// var decks = GetImportableDecks(_remoteDecks, _localDecks);
			// Assert.AreEqual(1, decks.Count);
			// Assert.AreEqual(_remoteDeck.Id, decks[0].Id);
			// ImportDecks(decks, _localDecks);
			// Assert.AreEqual(1, _localDecks.Count);
			// Assert.AreEqual(_localDeck1.Id, _localDecks[0].Id);
		}
	}
}
