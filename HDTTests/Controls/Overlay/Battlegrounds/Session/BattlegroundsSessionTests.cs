using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Session;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Hearthstone_Deck_Tracker.Utility.Battlegrounds.BattlegroundsLastGames;

namespace HDTTests.Controls.Overlay.Battlegrounds.Session
{
	[TestClass]
	public class BattlegroundsSessionTests
	{
		// Unspecified so ToString("o")/Parse round-trip without a UTC offset
		private static readonly DateTime Base = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Unspecified);

		private static GameItem Game(double startHoursFromBase, int rating, int ratingAfter, double durationMinutes = 20)
		{
			var start = Base.AddHours(startHoursFromBase);
			return new GameItem
			{
				StartTime = start.ToString("o"),
				EndTime = start.AddMinutes(durationMinutes).ToString("o"),
				Rating = rating,
				RatingAfter = ratingAfter,
			};
		}

		[TestMethod]
		public void GetSessionGames_KeepsAllGames_WithinSingleSession()
		{
			var games = new List<GameItem> { Game(0, 6000, 6050), Game(0.5, 6050, 6100) };
			var now = Base.AddHours(0.9);

			var session = BattlegroundsSessionViewModel.GetSessionGames(games, 6100, now);

			Assert.AreEqual(2, session.Count);
		}

		[TestMethod]
		public void GetSessionGames_SplitsSession_OnGapOverTwoHours()
		{
			var games = new List<GameItem> { Game(0, 6000, 6050), Game(3, 6050, 6100) };
			var now = Base.AddHours(3.5);

			var session = BattlegroundsSessionViewModel.GetSessionGames(games, 6100, now);

			Assert.AreEqual(1, session.Count);
			Assert.AreEqual(games[1].StartTime, session[0].StartTime);
		}

		[TestMethod]
		public void GetSessionGames_DoesNotWipe_WhenCurrentDuosRatingMatchesLastGame()
		{
			// the reset check must use the duos rating, not the (low/zero) solo rating
			var games = new List<GameItem> { Game(0, 6000, 6050), Game(0.5, 6050, 6100) };
			var now = Base.AddHours(0.9);

			var session = BattlegroundsSessionViewModel.GetSessionGames(games, 6100, now);

			Assert.AreEqual(2, session.Count);
		}

		[TestMethod]
		public void GetSessionGames_WipesSession_OnGenuineMmrReset()
		{
			var games = new List<GameItem> { Game(0, 6000, 6100) };
			var now = Base.AddHours(0.5);

			var session = BattlegroundsSessionViewModel.GetSessionGames(games, 100, now);

			Assert.AreEqual(0, session.Count);
		}

		[TestMethod]
		public void GetSessionGames_WipesStaleSession_WhenLastGameOlderThanTwoHours()
		{
			var games = new List<GameItem> { Game(0, 6000, 6050) };
			var now = Base.AddHours(3);

			var session = BattlegroundsSessionViewModel.GetSessionGames(games, 6050, now);

			Assert.AreEqual(0, session.Count);
		}
	}
}
