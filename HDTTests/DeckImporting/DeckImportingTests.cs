using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.Importing.Game.ImportOptions;
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
		 *  - archived decks
		 *  - same vs different class
		 *  - golden vs nongolden
		 *  - existing deck as non-selected version
		 * 
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
				new { Local = TestData.LocalDeck1_DifferentCards, Remote = TestData.RemoteDeck1_DifferentCards },
			};

			foreach(var deckPair in deckPairs)
				DeckComparer.AssertAreEqual(deckPair.Local, deckPair.Remote);
		}

		[TestMethod]
		public void NoLocal_NoRemote()
		{
			Assert.AreEqual(0, _localDecks.Count);
			Assert.AreEqual(0, _remoteDecks.Count);

			var decks = DeckImporter.GetImportedDecks(_remoteDecks, _localDecks);
			Assert.AreEqual(0, decks.Count);

			DeckManager.ImportDecksTo(_localDecks, decks, false, true, true);
			Assert.AreEqual(0, _localDecks.Count);
		}

		[TestMethod]
		public void NoLocal_NewRemote()
		{
			_remoteDecks.Add(TestData.RemoteDeck1);
			Assert.AreEqual(0, _localDecks.Count);
			Assert.AreEqual(1, _remoteDecks.Count);

			var decks = DeckImporter.GetImportedDecks(_remoteDecks, _localDecks);

			Assert.AreEqual(1, decks.Count);
			DeckComparer.AssertAreEqual(TestData.RemoteDeck1, decks[0].Deck);
			Assert.IsTrue(decks[0].Import);
			Assert.AreEqual(1, decks[0].ImportOptions.Count());
			Assert.AreEqual(TestData.LocalDeck1.Class, decks[0].Class);
			Assert.AreEqual(0, decks[0].SelectedIndex);
			Assert.AreEqual(typeof(NewDeck), decks[0].SelectedImportOption.GetType());

			DeckManager.ImportDecksTo(_localDecks, decks, false, true, true);

			Assert.AreEqual(1, _localDecks.Count);
			DeckComparer.AssertAreEqual(TestData.LocalDeck1, _localDecks[0]);
		}

		[TestMethod]
		public void HasLocal_NewRemote_NewId_Distinct()
		{
			_localDecks.Add(TestData.LocalDeck1);
			_remoteDecks.Add(TestData.RemoteDeck2);
			Assert.AreEqual(1, _localDecks.Count);
			Assert.AreEqual(1, _remoteDecks.Count);

			var decks = DeckImporter.GetImportedDecks(_remoteDecks, _localDecks);

			Assert.AreEqual(1, decks.Count);
			DeckComparer.AssertAreEqual(TestData.RemoteDeck2, decks[0].Deck);
			Assert.IsTrue(decks[0].Import);
			Assert.AreEqual(1, decks[0].ImportOptions.Count());
			Assert.AreEqual(0, decks[0].SelectedIndex);
			Assert.AreEqual(typeof(NewDeck), decks[0].SelectedImportOption.GetType());

			DeckManager.ImportDecksTo(_localDecks, decks, false, true, true);

			Assert.AreEqual(2, _localDecks.Count);
			DeckComparer.AssertAreEqual(TestData.LocalDeck1, _localDecks[0]);
			DeckComparer.AssertAreEqual(TestData.LocalDeck2, _localDecks[1]);
		}

		[TestMethod]
		public void HasLocal_NewRemote_ExistingId_Distinct()
		{
			_localDecks.Add(TestData.LocalDeck1);
			_remoteDecks.Add(TestData.RemoteDeck1_DifferentCards);
			Assert.AreEqual(1, _localDecks.Count);
			Assert.AreEqual(1, _remoteDecks.Count);

			var decks = DeckImporter.GetImportedDecks(_remoteDecks, _localDecks);

			Assert.AreEqual(1, decks.Count);
			DeckComparer.AssertAreEqual(TestData.RemoteDeck1_DifferentCards, decks[0].Deck);
			Assert.IsTrue(decks[0].Import);
			Assert.AreEqual(2, decks[0].ImportOptions.Count());
			Assert.AreEqual(0, decks[0].SelectedIndex);
			Assert.AreEqual(typeof(NewDeck), decks[0].SelectedImportOption.GetType());

			DeckManager.ImportDecksTo(_localDecks, decks, false, true, true);

			Assert.AreEqual(2, _localDecks.Count);
			var localDeck = TestData.LocalDeck1;
			localDeck.HsId = 0;
			DeckComparer.AssertAreEqual(localDeck, _localDecks[0]);
			DeckComparer.AssertAreEqual(TestData.RemoteDeck1_DifferentCards, _localDecks[1]);
		}

		[TestMethod]
		public void HasLocal_NewRemote_NewId_MinorChanges()
		{
			var remoteDeck = TestData.RemoteDeck1_MinorChanges;
			remoteDeck.Id = 2;
			_localDecks.Add(TestData.LocalDeck1);
			_remoteDecks.Add(remoteDeck);
			Assert.AreEqual(1, _localDecks.Count);
			Assert.AreEqual(1, _remoteDecks.Count);

			var decks = DeckImporter.GetImportedDecks(_remoteDecks, _localDecks);

			Assert.AreEqual(1, decks.Count);
			DeckComparer.AssertAreEqual(remoteDeck, decks[0].Deck);
			Assert.IsTrue(decks[0].Import);
			Assert.AreEqual(2, decks[0].ImportOptions.Count());

			// The current behavior is to always create a new deck for a new id
			// this will likely change with the new data model
			Assert.AreEqual(typeof(NewDeck), decks[0].SelectedImportOption.GetType());

			DeckManager.ImportDecksTo(_localDecks, decks, false, true, true);

			Assert.AreEqual(2, _localDecks.Count);
			DeckComparer.AssertAreEqual(TestData.LocalDeck1, _localDecks[0]);
			DeckComparer.AssertAreEqual(remoteDeck, _localDecks[1]);
		}

		[TestMethod]
		public void HasLocal_NewRemote_ExistingId_MinorChanges()
		{
			_localDecks.Add(TestData.LocalDeck1);
			_remoteDecks.Add(TestData.RemoteDeck1_MinorChanges);
			Assert.AreEqual(1, _localDecks.Count);
			Assert.AreEqual(1, _remoteDecks.Count);

			var decks = DeckImporter.GetImportedDecks(_remoteDecks, _localDecks);

			Assert.AreEqual(1, decks.Count);
			DeckComparer.AssertAreEqual(TestData.RemoteDeck1_MinorChanges, decks[0].Deck);
			Assert.IsTrue(decks[0].Import);
			Assert.AreEqual(2, decks[0].ImportOptions.Count());
			var existingDeck = decks[0].SelectedImportOption as ExistingDeck;
			Assert.IsNotNull(existingDeck);
			Assert.AreEqual(1, existingDeck.NewVersion.Major);
			Assert.AreEqual(1, existingDeck.NewVersion.Minor);

			DeckManager.ImportDecksTo(_localDecks, decks, false, true, true);

			Assert.AreEqual(1, _localDecks.Count);
			DeckComparer.AssertAreEqual(TestData.LocalDeck1_MinorChanges, _localDecks[0]);
			Assert.AreEqual(1, _localDecks[0].Versions.Count);
			DeckComparer.AssertAreEqual(TestData.LocalDeck1, _localDecks[0].Versions[0]);
		}

		[TestMethod]
		public void HasLocal_NewRemote_NewId_MajorChanges()
		{
			var remoteDeck = TestData.RemoteDeck1_MajorChanges;
			remoteDeck.Id = 2;
			_localDecks.Add(TestData.LocalDeck1);
			_remoteDecks.Add(remoteDeck);
			Assert.AreEqual(1, _localDecks.Count);
			Assert.AreEqual(1, _remoteDecks.Count);

			var decks = DeckImporter.GetImportedDecks(_remoteDecks, _localDecks);

			Assert.AreEqual(1, decks.Count);
			DeckComparer.AssertAreEqual(remoteDeck, decks[0].Deck);
			Assert.IsTrue(decks[0].Import);
			Assert.AreEqual(2, decks[0].ImportOptions.Count());
			Assert.AreEqual(typeof(NewDeck), decks[0].SelectedImportOption.GetType());

			DeckManager.ImportDecksTo(_localDecks, decks, false, true, true);

			Assert.AreEqual(2, _localDecks.Count);
			DeckComparer.AssertAreEqual(TestData.LocalDeck1, _localDecks[0]);
			DeckComparer.AssertAreEqual(remoteDeck, _localDecks[1]);
		}

		[TestMethod]
		public void HasLocal_NewRemote_ExistingId_MajorChanges()
		{
			_localDecks.Add(TestData.LocalDeck1);
			_remoteDecks.Add(TestData.RemoteDeck1_MajorChanges);
			Assert.AreEqual(1, _localDecks.Count);
			Assert.AreEqual(1, _remoteDecks.Count);

			var decks = DeckImporter.GetImportedDecks(_remoteDecks, _localDecks);

			Assert.AreEqual(1, decks.Count);
			DeckComparer.AssertAreEqual(TestData.RemoteDeck1_MajorChanges, decks[0].Deck);
			Assert.IsTrue(decks[0].Import);
			Assert.AreEqual(2, decks[0].ImportOptions.Count());
			var existingDeck = decks[0].SelectedImportOption as ExistingDeck;
			Assert.IsNotNull(existingDeck);
			Assert.AreEqual(2, existingDeck.NewVersion.Major);
			Assert.AreEqual(0, existingDeck.NewVersion.Minor);

			DeckManager.ImportDecksTo(_localDecks, decks, false, true, true);

			Assert.AreEqual(1, _localDecks.Count);
			DeckComparer.AssertAreEqual(TestData.LocalDeck1_MajorChanges, _localDecks[0]);
			Assert.AreEqual(1, _localDecks[0].Versions.Count);
			DeckComparer.AssertAreEqual(TestData.LocalDeck1, _localDecks[0].Versions[0]);
		}

		[TestMethod]
		public void HasLocal_NewRemote_NewId_ExactMatch()
		{
			var remoteDeck = TestData.RemoteDeck1;
			remoteDeck.Id = 2;
			_localDecks.Add(TestData.LocalDeck1);
			_remoteDecks.Add(remoteDeck);
			Assert.AreEqual(1, _localDecks.Count);
			Assert.AreEqual(1, _remoteDecks.Count);

			var decks = DeckImporter.GetImportedDecks(_remoteDecks, _localDecks);

			Assert.AreEqual(1, decks.Count);
			DeckComparer.AssertAreEqual(remoteDeck, decks[0].Deck);

			var existingDeck = decks[0].SelectedImportOption as ExistingDeck;
			Assert.IsNotNull(existingDeck);
			Assert.AreEqual(0, existingDeck.NewVersion.Major);
			Assert.AreEqual(0, existingDeck.NewVersion.Minor);
			Assert.IsTrue(decks[0].Import);

			DeckManager.ImportDecksTo(_localDecks, decks, false, true, true);

			Assert.AreEqual(1, _localDecks.Count);
			DeckComparer.AssertAreEqual(remoteDeck, _localDecks[0]);
		}

		[TestMethod]
		public void HasLocal_NewRemote_ExistingId_ExactMatch()
		{
			_localDecks.Add(TestData.LocalDeck1);
			_remoteDecks.Add(TestData.RemoteDeck1);
			Assert.AreEqual(1, _localDecks.Count);
			Assert.AreEqual(1, _remoteDecks.Count);

			var decks = DeckImporter.GetImportedDecks(_remoteDecks, _localDecks);

			Assert.AreEqual(0, decks.Count);
			DeckManager.ImportDecksTo(_localDecks, decks, false, true, true);

			Assert.AreEqual(1, _localDecks.Count);
			DeckComparer.AssertAreEqual(TestData.LocalDeck1, _localDecks[0]);
		}

		[TestMethod]
		public void HasLocal_NoRemote()
		{
			_localDecks.Add(TestData.LocalDeck1);
			Assert.AreEqual(1, _localDecks.Count);
			Assert.AreEqual(0, _remoteDecks.Count);

			var decks = DeckImporter.GetImportedDecks(_remoteDecks, _localDecks);
			Assert.AreEqual(0, decks.Count);
		}
	}
}
