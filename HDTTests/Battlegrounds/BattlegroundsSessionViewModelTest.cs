using System;
using System.Collections.Generic;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Session;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Hearthstone_Deck_Tracker.Utility.Battlegrounds.BattlegroundsLastGames;

namespace HDTTests.Battlegrounds
{
	[TestClass]
	public class BattlegroundsSessionViewModelTest
	{
		private static List<GameItem> RecentSession(int rating, int ratingAfter, bool duos)
		{
			var startTime = DateTime.Now.AddMinutes(-30);
			return new List<GameItem>
			{
				new GameItem(
					startTime.ToString("o"), startTime.AddMinutes(20).ToString("o"), "TB_BaconShop_HERO_01",
					rating, ratingAfter, 3, Array.Empty<Entity>(), false, "1_2", duos
				)
			};
		}

		[TestMethod]
		public void SessionGames_AreKeptWhenTheSoloRatingIsFarBelowTheDuosRating()
		{
			var ratingInfo = new BattlegroundRatingInfo { Rating = 0, DuosRating = 6000 };

			var sessionGames = BattlegroundsSessionViewModel.GetSessionGames(RecentSession(5900, 6000, true), ratingInfo, true);

			Assert.AreEqual(1, sessionGames.Count);
		}

		[TestMethod]
		public void SessionGames_AreClearedWhenTheDuosRatingWasReset()
		{
			var ratingInfo = new BattlegroundRatingInfo { Rating = 6000, DuosRating = 100 };

			var sessionGames = BattlegroundsSessionViewModel.GetSessionGames(RecentSession(5900, 6000, true), ratingInfo, true);

			Assert.AreEqual(0, sessionGames.Count);
		}

		[TestMethod]
		public void SessionGames_AreClearedWhenTheSoloRatingWasReset()
		{
			var ratingInfo = new BattlegroundRatingInfo { Rating = 100, DuosRating = 0 };

			var sessionGames = BattlegroundsSessionViewModel.GetSessionGames(RecentSession(5900, 6000, false), ratingInfo, false);

			Assert.AreEqual(0, sessionGames.Count);
		}

		[TestMethod]
		public void CurrentRating_UsesThePostGameRating()
		{
			var viewModel = new BattlegroundsSessionViewModel();

			viewModel.UpdateCurrentRating(712, 665);

			Assert.AreEqual("712", viewModel.BgRatingCurrent);
		}

		[TestMethod]
		public void CurrentRating_FallsBackToTheClientRatingWhenThePostGameRatingIsUnavailable()
		{
			var viewModel = new BattlegroundsSessionViewModel();

			viewModel.UpdateCurrentRating(0, 665);

			Assert.AreEqual("665", viewModel.BgRatingCurrent);
		}

		[TestMethod]
		public void CurrentRating_IsKeptWhenNoRatingIsAvailableAtAll()
		{
			var viewModel = new BattlegroundsSessionViewModel();
			viewModel.UpdateCurrentRating(665, null);

			viewModel.UpdateCurrentRating(0, null);

			Assert.AreEqual("665", viewModel.BgRatingCurrent);
		}
	}
}
