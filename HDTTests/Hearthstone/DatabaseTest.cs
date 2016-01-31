#region

using Hearthstone_Deck_Tracker.Hearthstone;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace HDTTests.Hearthstone
{
	[TestClass]
	public class DatabaseTest
	{
		[TestMethod]
		public void IsHero_Rexxar()
		{
			Assert.IsTrue(Database.IsHero("HERO_05"));
		}

		[TestMethod]
		public void IsHero_Alleria()
		{
			Assert.IsTrue(Database.IsHero("HERO_05a"));
		}

		[TestMethod]
		public void IsHero_Ragnaros()
		{
			Assert.IsTrue(Database.IsHero("BRM_027h"));
		}

		[TestMethod]
		public void IsHero_SkelesaurusHex()
		{
			Assert.IsTrue(Database.IsHero("LOEA13_1h"));
		}

		[TestMethod]
		public void IsNoHero_RagnarosCollectible()
		{
			Assert.IsFalse(Database.IsHero("EX1_298"));
		}
	}
}