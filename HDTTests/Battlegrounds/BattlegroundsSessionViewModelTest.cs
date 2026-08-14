using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Session;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Battlegrounds
{
	[TestClass]
	public class BattlegroundsSessionViewModelTest
	{
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
