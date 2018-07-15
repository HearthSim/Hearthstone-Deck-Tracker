using System.Linq;
using System.IO;
using Hearthstone_Deck_Tracker.Hearthstone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CardIds = HearthDb.CardIds;

namespace HDTTests.Hearthstone
{
	[TestClass]
	public class CardDBTest
	{
		// Dreadscale card has unusual id ending in 't', some tests to check it is recognized
		[TestMethod]
		public void TestDreadscaleFromId()
		{
			var card = Database.GetCardFromId("AT_063t");
			Assert.AreEqual("Dreadscale", card.Name);
		}

		[TestMethod]
		public void TestMurlocTinyFinInGetActual()
		{
			var db = Database.GetActualCards();
			var found = db.Any<Card>(c => c.LocalizedName.ToLowerInvariant().Contains("murloc tinyfin"));
			Assert.IsTrue(found);
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
		public void GetFromName_CollectibleByDefault()
		{
			var card = Database.GetCardFromName("Baron Geddon");
			Assert.AreEqual("EX1_249", card.Id);
		}

		[TestMethod]
		public void GetFromName_AllWithParam()
		{
			var card = Database.GetCardFromName("Baron Geddon", collectible: false);
			Assert.IsTrue(card.Id.Contains("BRMA05"));
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
		public void TestCardBarImages()
		{
			foreach(var card in Database.GetActualCards())
				Assert.IsTrue(File.Exists("../../../../Resources/Tiles/" + card.Id + ".png"), card.Name);
		}

		[TestMethod]
		public void DungeonBossTest()
		{
			var name = Database.GetHeroNameFromId(CardIds.NonCollectible.Warlock.XolTheUnscathedHeroic);
			Assert.AreEqual("Xol the Unscathed", name);
		}

		[TestMethod]
		public void TestEntourageCards()
		{
			var lichKing = Database.GetCardFromName("The Lich King");
			Assert.AreEqual(8, lichKing.EntourageCardIds.Length);

			var angryChicken = Database.GetCardFromName("Angry Chicken");
			Assert.AreEqual(0, angryChicken.EntourageCardIds.Length);

			var kalimos = Database.GetCardFromName("Kalimos, Primal Lord");
			Assert.AreEqual(4, kalimos.EntourageCardIds.Length);
		}
	}
}
