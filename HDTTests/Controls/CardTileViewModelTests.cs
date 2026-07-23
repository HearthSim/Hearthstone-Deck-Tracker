using Hearthstone_Deck_Tracker.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;

namespace HDTTests.Controls
{
	[TestClass]
	public class CardTileViewModelTests
	{
		[TestMethod]
		public void IsLegendaryIconVisible_TrueForConstructedLegendary()
		{
			var brann = new Card(HearthDb.Cards.All["LOE_077"]);
			var viewModel = new CardTileViewModel(brann);
			Assert.IsTrue(viewModel.IsLegendaryIconVisible);
		}

		[TestMethod]
		public void IsLegendaryIconVisible_FalseForBattlegroundsCard()
		{
			var brann = new Card(HearthDb.Cards.All["BG_LOE_077"], baconCard: true);
			var viewModel = new CardTileViewModel(brann);
			Assert.IsFalse(viewModel.IsLegendaryIconVisible);
		}
	}
}
