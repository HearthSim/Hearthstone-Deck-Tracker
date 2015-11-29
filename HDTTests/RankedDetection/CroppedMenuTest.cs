using System.Drawing;
using Hearthstone_Deck_Tracker.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.RankedDetection
{
	[TestClass]
	public class CroppedMenuTest
	{
		private readonly string _testFiles = "RankedDetection/TestFiles/";

		// Test all menus fail (due to how legend detect works)

		[TestMethod]
		public void AllMenus_16_9() // at 16:9 area is same for all menus
		{
			Assert.IsTrue(MenuFails("16-9_Menus"));
		}

		[TestMethod]
		public void AiMenu()
		{
			Assert.IsTrue(MenuFails("AiMenu"));
		}

		[TestMethod]
		public void ArenaMenu()
		{
			Assert.IsTrue(MenuFails("Arena"));
		}

		[TestMethod]
		public void BrawlMenu()
		{
			Assert.IsTrue(MenuFails("Brawl"));
		}

		[TestMethod]
		public void CollectionMenu()
		{
			Assert.IsTrue(MenuFails("Collection"));
		}

		[TestMethod]
		public void PlayMenu()
		{
			Assert.IsTrue(MenuFails("PlayMenu"));
		}

		[TestMethod]
		public void MainMenu_16_9()
		{
			Assert.IsTrue(MenuFails("MainMenu_16-9"));
		}

		[TestMethod]
		public void MainMenu_4_3()
		{
			Assert.IsTrue(MenuFails("MainMenu_4-3"));
		}

		// Helper methods

		private bool MenuFails(string name)
		{
			var bmp = new Bitmap("RankedDetection/TestFiles/" + name + "_Opp.png");
			var opp = RankDetection.FindBest(bmp);
			bmp = new Bitmap("RankedDetection/TestFiles/" + name + "_Play.png");
			var play = RankDetection.FindBest(bmp);

			return opp == -1 && play == -1;
		}
	}
}
