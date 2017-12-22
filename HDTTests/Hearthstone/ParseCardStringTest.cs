using System;
using System.Linq;
using Hearthstone_Deck_Tracker.Importing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Hearthstone
{
	[TestClass]
	public class ParseCardStringTest
	{
		[TestMethod]
		public void TestNeutralOnly()
		{
			var deck = StringImporter.Import("Ragnaros the firelord");
			Assert.IsNull(deck);
		}

		[TestMethod]
		public void TestNoCount()
		{
			var deck = StringImporter.Import("Ragnaros, Lightlord");
			var card = deck.Cards.FirstOrDefault();
			Assert.AreEqual("OG_229", card.Id);
			Assert.AreEqual(1, card.Count);
		}

		[TestMethod]
		public void TestOneCount()
		{
			var deck = StringImporter.Import("Ragnaros, Lightlord x1");
			var card = deck.Cards.FirstOrDefault();
			Assert.AreEqual("OG_229", card.Id);
			Assert.AreEqual(1, card.Count);
		}

		[TestMethod]
		public void TestTwoCount()
		{
			var deck = StringImporter.Import("Ragnaros, Lightlord x2");
			var card = deck.Cards.FirstOrDefault();
			Assert.AreEqual("OG_229", card.Id);
			Assert.AreEqual(2, card.Count);
		}

		[TestMethod]
		public void TestCommaNoCount()
		{
			var deck = StringImporter.Import("Ragnaros, Lightlord");
			var card = deck.Cards.FirstOrDefault();
			Assert.AreEqual("OG_229", card.Id);
			Assert.AreEqual(1, card.Count);
		}

		[TestMethod]
		public void TestCommaOneCount()
		{
			var deck = StringImporter.Import("Ragnaros, Lightlord x1");
			var card = deck.Cards.FirstOrDefault();
			Assert.AreEqual("OG_229", card.Id);
			Assert.AreEqual(1, card.Count);
		}

		[TestMethod]
		public void TestCommaTwoCount()
		{
			var deck = StringImporter.Import("Ragnaros, Lightlord x2");
			var card = deck.Cards.FirstOrDefault();
			Assert.AreEqual("OG_229", card.Id);
			Assert.AreEqual(2, card.Count);
		}
	}
}
