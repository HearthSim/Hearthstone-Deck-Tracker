using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Hearthstone.RelatedCardsSystem
{
	[TestClass]
	public class RelatedCardsManagerTests
	{
		[TestMethod]
		public void AllRegisteredCardIdsExist()
		{
			var manager = new RelatedCardsManager();
			var cardIds = manager.RelatedCards.Keys
				.Concat(manager.HighlightCards.Keys)
				.Concat(manager.SpellSchoolTutorCards.Keys)
				.Concat(manager.CardGeneratorCards.Keys)
				.Where(id => id != "")
				.Distinct();

			var unknown = cardIds.Where(id => !HearthDb.Cards.All.ContainsKey(id)).OrderBy(id => id).ToList();

			CollectionAssert.AreEqual(new string[0], unknown, string.Join(", ", unknown));
		}
	}
}
