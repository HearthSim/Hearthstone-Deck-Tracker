using System.IO;
using Hearthstone_Deck_Tracker.Hearthstone;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Hearthstone
{
	[TestClass]
	public class CardDBTest
	{
		/*
		 *  Correct for patch 2.7.0.9166
		 */

		[TestMethod]
		public void TestTotalCollectableCards()
		{
			Assert.AreEqual(566, GameV2.GetActualCards().Count);
		}

		[TestMethod]
		public void TestHeroSkins()
		{
			var Alleria = GameV2.GetHeroNameFromId("HERO_05a");
			Assert.AreEqual("Hunter", Alleria);

			var AlleriaPower = GameV2.GetCardFromId("DS1h_292_H1");
			Assert.AreEqual("Steady Shot", AlleriaPower.Name);
		}

		[TestMethod]
		public void TestBrawlCards()
		{
			var Rotten = GameV2.GetCardFromId("TB_008");
			Assert.AreEqual("Rotten Banana", Rotten.Name);

			var Moira = GameV2.GetCardFromId("BRMC_87");
			Assert.AreEqual("Moira Bronzebeard", Moira.Name);
		}

		[TestMethod]
		public void TestCardImages()
		{
			foreach(var card in GameV2.GetActualCards())
			{
				Assert.IsTrue(File.Exists("Images/" + card.CardFileName + ".png"), card.Name);
			}
		}
	}
}
