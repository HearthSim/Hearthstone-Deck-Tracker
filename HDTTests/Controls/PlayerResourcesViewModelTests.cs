using System.Linq;
using Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.PlayerResourcesWidget;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Controls
{
	[TestClass]
	public class PlayerResourcesViewModelTests
	{
		private const string GoldIcon = "/Images/coin-cost.png";
		private const string ManaIcon = "/Images/mana.png";

		[TestMethod]
		public void MaxGold_NotShownWhenNotProvided()
		{
			var viewModel = new PlayerResourcesViewModel();
			viewModel.Initialize(30, 10, 10);

			viewModel.UpdatePlayerResourcesWidget(30, 10, 10);

			Assert.IsFalse(viewModel.HasVisibleResources);
		}

		[TestMethod]
		public void MaxGold_ShownWhenProvided()
		{
			var viewModel = new PlayerResourcesViewModel();
			viewModel.Initialize(30, 10, 10);

			viewModel.UpdatePlayerResourcesWidget(30, 10, 10, maxGold: 12);

			var gold = viewModel.ChangedResources.Single();
			Assert.AreEqual(GoldIcon, gold.Icon);
			Assert.AreEqual(12, gold.Value);
		}

		[TestMethod]
		public void MaxGold_DisappearsWhenNoLongerProvided()
		{
			var viewModel = new PlayerResourcesViewModel();
			viewModel.Initialize(30, 10, 10);

			viewModel.UpdatePlayerResourcesWidget(30, 10, 10, maxGold: 12);
			viewModel.UpdatePlayerResourcesWidget(30, 10, 10);

			Assert.IsFalse(viewModel.HasVisibleResources);
		}

		[TestMethod]
		public void MaxMana_StaysVisibleAfterDeviatingFromInitial()
		{
			var viewModel = new PlayerResourcesViewModel();
			viewModel.Initialize(30, 10, 10);

			viewModel.UpdatePlayerResourcesWidget(30, 12, 10);
			viewModel.UpdatePlayerResourcesWidget(30, 10, 10);

			var mana = viewModel.ChangedResources.Single();
			Assert.AreEqual(ManaIcon, mana.Icon);
			Assert.AreEqual(10, mana.Value);
		}

		[TestMethod]
		public void Initialize_ClearsMaxGoldFromPreviousGame()
		{
			var viewModel = new PlayerResourcesViewModel();
			viewModel.Initialize(30, 10, 10);
			viewModel.UpdatePlayerResourcesWidget(30, 10, 10, maxGold: 12);

			viewModel.Initialize(30, 10, 10);

			Assert.IsFalse(viewModel.HasVisibleResources);
		}

		[TestMethod]
		public void Initialize_ClearsConstructedResourcesFromPreviousGame()
		{
			var viewModel = new PlayerResourcesViewModel();
			viewModel.Initialize(30, 10, 10);
			viewModel.UpdatePlayerResourcesWidget(40, 12, 12, corpsesLeft: 3);

			viewModel.Initialize(30, 10, 10);

			Assert.IsFalse(viewModel.HasVisibleResources);
		}
	}
}
