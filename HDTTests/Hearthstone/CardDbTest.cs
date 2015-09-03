using System.Linq;
using System.IO;
using Hearthstone_Deck_Tracker.Hearthstone;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Hearthstone
{
	[TestClass]
	public class CardDBTest
	{
		// Test collectable card count
		[TestMethod]
		public void TestTotalCollectableCards()
		{
			// 3.0.0.9786 - TGT
			Assert.AreEqual(698, Database.GetActualCards().Count);
		}

		// Dreadscale card has unusual id ending in 't', some tests to check it is recognized
		[TestMethod]
		public void TestDreadscaleFromId()
		{
			var card = Database.GetCardFromId("AT_063t");
			Assert.AreEqual("Dreadscale", card.Name);
		}
		[TestMethod]
		public void TestDreadscaleInGetActual()
		{
			var db = Database.GetActualCards();
			var found = db.Any<Card>(c => c.LocalizedName.ToLowerInvariant().Contains("dreadscale"));
			Assert.IsTrue(found);
		}
		[TestMethod]
		public void TestDreadscaleIsActual()
		{
			Card c = new Card { Id = "AT_063t", Name = "Dreadscale", Type = "Minion" };
			Assert.IsTrue(Database.IsActualCard(c));
		}

		[TestMethod]
		public void TestHeroSkins()
		{
			var Alleria = Database.GetHeroNameFromId("HERO_05a");
			Assert.AreEqual("Hunter", Alleria);

			var AlleriaPower = Database.GetCardFromId("DS1h_292_H1");
			Assert.AreEqual("Steady Shot", AlleriaPower.Name);
		}

		[TestMethod]
		public void TestBrawlCards()
		{
			var Rotten = Database.GetCardFromId("TB_008");
			Assert.AreEqual("Rotten Banana", Rotten.Name);

			var Moira = Database.GetCardFromId("BRMC_87");
			Assert.AreEqual("Moira Bronzebeard", Moira.Name);
		}

		[TestMethod]
		public void TestCardImages()
		{
			foreach(var card in Database.GetActualCards())
			{
				Assert.IsTrue(File.Exists("Images/" + card.CardFileName + ".png"), card.Name);
			}
		}
	}
}
