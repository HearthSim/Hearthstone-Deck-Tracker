using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Hearthstone_Deck_Tracker.Utility.Battlegrounds.BattlegroundsLastGames;

namespace HDTTests.Utility.Battlegrounds
{
	[TestClass]
	public class BattlegroundsLastGamesTests
	{
		private static GameItem Game(string player, bool duos) => new GameItem { Player = player, Duos = duos };

		[TestMethod]
		public void FilterPlayerGames_KeepsOnlyRequestedMode()
		{
			var games = new List<GameItem> { Game("me", true), Game("me", false) };

			var duos = FilterPlayerGames(games, "me", true);
			var solo = FilterPlayerGames(games, "me", false);

			Assert.AreEqual(1, duos.Count);
			Assert.IsTrue(duos[0].Duos);
			Assert.AreEqual(1, solo.Count);
			Assert.IsFalse(solo[0].Duos);
		}

		[TestMethod]
		public void FilterPlayerGames_ExcludesOtherPlayers()
		{
			var games = new List<GameItem> { Game("me", true), Game("someone-else", true) };

			var result = FilterPlayerGames(games, "me", true);

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("me", result[0].Player);
		}

		[TestMethod]
		public void FilterPlayerGames_IncludesGamesWithNoPlayerRecorded()
		{
			// legacy games saved without a player id are shared across accounts
			var games = new List<GameItem> { Game(null, true), Game("me", true) };

			var result = FilterPlayerGames(games, "me", true);

			Assert.AreEqual(2, result.Count);
		}
	}
}
