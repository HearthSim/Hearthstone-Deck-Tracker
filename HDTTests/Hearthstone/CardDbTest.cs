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
			Assert.AreEqual("CORE_EX1_249", card.Id);
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
			{
				if(card.Id == "LOOT_526e")
				{
					// Art for this appears to be missing as of 2020-02-26
					continue;
				}
				if(card.Id == "CORE_BOT_451" || card.Id == "CORE_DMF_060")
				{
					// Art for this appears to be missing as of 2023-08-22
					continue;
				}
				if(
					card.Id == "CORE_CS2_024" ||
					card.Id == "CORE_EX1_316" ||
					card.Id == "CORE_RLK_063" ||
					card.Id == "CORE_RLK_086" ||
					card.Id == "CORE_RLK_122" ||
					card.Id == "CORE_VAN_EX1_563" ||
					card.Id == "LEG_RLK_753"
				)
				{
					// Art for these cards appears to be missing as of 2024-03-12
					continue;
				}
				Assert.IsTrue(File.Exists("../../../../Resources/Tiles/" + card.Id + ".png"), card.Id);
			}
		}

		[TestMethod]
		public void DungeonBossTest()
		{
			var name = Database.GetHeroNameFromId(CardIds.NonCollectible.Warlock.XolTheUnscathedHeroic);
			Assert.AreEqual("Xol the Unscathed", name);
		}
	}
}
