using System;
using Hearthstone_Deck_Tracker.Stats;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests
{
	[TestClass]
	public class GameStatsTest
	{
		[TestMethod]
		public void AgeStringTest()
		{
		    var gameStats = new GameStats {StartTime = DateTime.Now};

		    Assert.AreEqual("0 minutes ago", gameStats.Age);

			gameStats.StartTime = DateTime.Now.AddSeconds(59);
			Assert.AreEqual("0 minutes ago", gameStats.Age);

			gameStats.StartTime = DateTime.Now.AddMinutes(-1);
			Assert.AreEqual("1 minute ago", gameStats.Age);

			gameStats.StartTime = DateTime.Now.AddMinutes(-1).AddSeconds(-59);
			Assert.AreEqual("1 minute ago", gameStats.Age);

			gameStats.StartTime = DateTime.Now.AddMinutes(-2);
			Assert.AreEqual("2 minutes ago", gameStats.Age);

			gameStats.StartTime = DateTime.Now.AddMinutes(-59);
			Assert.AreEqual("59 minutes ago", gameStats.Age);

			gameStats.StartTime = DateTime.Now.AddHours(-1);
			Assert.AreEqual("1 hour ago", gameStats.Age);

			gameStats.StartTime = DateTime.Now.AddHours(-1).AddMinutes(-59);
			Assert.AreEqual("1 hour ago", gameStats.Age);

			gameStats.StartTime = DateTime.Now.AddHours(-2);
			Assert.AreEqual("2 hours ago", gameStats.Age);

			gameStats.StartTime = DateTime.Now.AddHours(-23);
			Assert.AreEqual("23 hours ago", gameStats.Age);

			gameStats.StartTime = DateTime.Now.AddHours(-23).AddMinutes(-59);
			Assert.AreEqual("23 hours ago", gameStats.Age);

			gameStats.StartTime = DateTime.Now.AddDays(-1);
			Assert.AreEqual("1 day ago", gameStats.Age);

			gameStats.StartTime = DateTime.Now.AddDays(-1).AddHours(-23);
			Assert.AreEqual("1 day ago", gameStats.Age);

			gameStats.StartTime = DateTime.Now.AddDays(-2);
			Assert.AreEqual("2 days ago", gameStats.Age);
		}
	}
}
