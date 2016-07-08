using System;
using System.Linq;
using Hearthstone_Deck_Tracker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Hearthstone
{
	[TestClass]
	public class ParseCardStringTest
	{
		[TestMethod]
		public void TestNoCount()
		{
			var deck = Helper.ParseCardString("Ragnaros the firelord");
			var card = deck.Cards.FirstOrDefault();
			Assert.AreEqual("EX1_298", card.Id);
			Assert.AreEqual(1, card.Count);
		}

		[TestMethod]
		public void TestOneCount()
		{
			var deck = Helper.ParseCardString("Ragnaros the firelord x1");
			var card = deck.Cards.FirstOrDefault();
			Assert.AreEqual("EX1_298", card.Id);
			Assert.AreEqual(1, card.Count);
		}

		[TestMethod]
		public void TestTwoCount()
		{
			var deck = Helper.ParseCardString("Ragnaros the firelord x2");
			var card = deck.Cards.FirstOrDefault();
			Assert.AreEqual("EX1_298", card.Id);
			Assert.AreEqual(2, card.Count);
		}

		[TestMethod]
		public void TestCommaNoCount()
		{
			var deck = Helper.ParseCardString("Ragnaros, Lightlord");
			var card = deck.Cards.FirstOrDefault();
			Assert.AreEqual("OG_229", card.Id);
			Assert.AreEqual(1, card.Count);
		}

		[TestMethod]
		public void TestCommaOneCount()
		{
			var deck = Helper.ParseCardString("Ragnaros, Lightlord x1");
			var card = deck.Cards.FirstOrDefault();
			Assert.AreEqual("OG_229", card.Id);
			Assert.AreEqual(1, card.Count);
		}

		[TestMethod]
		public void TestCommaTwoCount()
		{
			var deck = Helper.ParseCardString("Ragnaros, Lightlord x2");
			var card = deck.Cards.FirstOrDefault();
			Assert.AreEqual("OG_229", card.Id);
			Assert.AreEqual(2, card.Count);
		}
	}
}
