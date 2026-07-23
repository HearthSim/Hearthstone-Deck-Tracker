using System.Windows;
using Hearthstone_Deck_Tracker.Controls.Overlay.Arena;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Controls.Overlay.Arena
{
	// Locks the per-position layout math after the hero-option view models were
	// de-duplicated onto a shared base. MarginForPosition(position, top) must stay:
	//   pos 0 -> (0, top, HM, 0), pos 1 -> (0, top, 0, 0), pos 2 -> (HM, top, 0, 0)
	[TestClass]
	public class ArenaPickHeroOptionMarginTests
	{
		private static Thickness Margin(int position, double top, int horizontalMargin) =>
			new Thickness(position != 2 ? 0 : horizontalMargin, top, position != 0 ? 0 : horizontalMargin, 0);

		[TestMethod]
		public void DualClass_WithData_MatchesExpectedMargins()
		{
			for(var position = 0; position <= 2; position++)
			{
				var vm = new ArenaPickSingleDualClassHeroOptionViewModel(new ArenaHeroPickApiResponse.ResponseData(), false, position);
				Assert.AreEqual(Margin(position, 428, 55), vm.Margin, $"Margin at position {position}");
				Assert.AreEqual(Margin(position, 170, 55), vm.PlaqueViewModel.Margin, $"PlaqueViewModel.Margin at position {position}");
			}
		}

		[TestMethod]
		public void DualClass_Loading_MatchesExpectedMargins()
		{
			for(var position = 0; position <= 2; position++)
			{
				var vm = new ArenaPickSingleDualClassHeroOptionViewModel(false, position);
				Assert.AreEqual(Margin(position, 630, 55), vm.Margin, $"Margin at position {position}");
				Assert.AreEqual(Margin(position, 560, 55), vm.PlaqueViewModel.Margin, $"PlaqueViewModel.Margin at position {position}");
			}
		}

		[TestMethod]
		public void HeroPower_WithData_MatchesExpectedMargins()
		{
			for(var position = 0; position <= 2; position++)
			{
				var vm = new ArenaPickSingleHeroPowerOptionViewModel(new ArenaHeroPickApiResponse.ResponseData(), false, position);
				Assert.AreEqual(Margin(position, 630, 47), vm.Margin, $"Margin at position {position}");
				Assert.AreEqual(Margin(position, 560, 47), vm.PlaqueViewModel.Margin, $"PlaqueViewModel.Margin at position {position}");
			}
		}

		[TestMethod]
		public void HeroPower_Loading_MatchesExpectedMargins()
		{
			for(var position = 0; position <= 2; position++)
			{
				var vm = new ArenaPickSingleHeroPowerOptionViewModel(false, position);
				Assert.AreEqual(Margin(position, 630, 47), vm.Margin, $"Margin at position {position}");
				Assert.AreEqual(Margin(position, 560, 47), vm.PlaqueViewModel.Margin, $"PlaqueViewModel.Margin at position {position}");
			}
		}
	}
}
