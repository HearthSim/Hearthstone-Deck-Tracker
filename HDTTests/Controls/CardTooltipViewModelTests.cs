using Hearthstone_Deck_Tracker.Controls.Tooltips;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;

namespace HDTTests.Controls
{
	[TestClass]
	public class CardTooltipViewModelTests
	{
		private static void Hover(CardTooltipViewModel viewModel, Card card)
		{
			viewModel.Card = card;
			viewModel.ShowTriple = card.BaconCard;
		}

		[TestMethod]
		public void BattlegroundsMinion_ShowsNormalCardWithTripleAlongside()
		{
			var viewModel = new CardTooltipViewModel();
			Hover(viewModel, new Card(HearthDb.Cards.All["BG19_010"], baconCard: true));
			Assert.AreEqual("BG19_010", viewModel.PrimaryCard?.Id);
			Assert.AreEqual("BG19_010_G", viewModel.SecondaryCard?.Id);
		}

		[TestMethod]
		public void OnlyGoldInGuideMinion_ShowsGoldenCardAndNothingElse()
		{
			var viewModel = new CardTooltipViewModel();
			Hover(viewModel, new Card(HearthDb.Cards.All["BG32_236"], baconCard: true));
			Assert.AreEqual("BG32_236_G", viewModel.PrimaryCard?.Id);
			Assert.IsNull(viewModel.SecondaryCard);
		}

		[TestMethod]
		public void ConstructedCardAfterBattlegroundsMinion_ClearsTriple()
		{
			var viewModel = new CardTooltipViewModel();
			Hover(viewModel, new Card(HearthDb.Cards.All["BG19_010"], baconCard: true));
			Hover(viewModel, new Card(HearthDb.Cards.All["LOE_077"]));
			Assert.AreEqual("LOE_077", viewModel.PrimaryCard?.Id);
			Assert.IsNull(viewModel.SecondaryCard);
		}
	}
}
