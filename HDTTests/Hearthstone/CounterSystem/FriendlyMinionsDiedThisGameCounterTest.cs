using System.Linq;
using System.Reflection;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Hearthstone.CounterSystem
{
	[TestClass]
	public class FriendlyMinionsDiedThisGameCounterTest
	{
		[TestCleanup]
		public void Cleanup()
		{
			SetActiveDeckWithoutEvents(null);
		}

		[TestMethod]
		public void ShouldShowForPlayerDeckWithMalorneTheWaywatcher()
		{
			SetActiveDeckWithoutEvents(new Deck
			{
				Cards =
				{
					new Card(HearthDb.CardIds.Collectible.Neutral.MalorneTheWaywatcher)
				}
			});

			var counter = new FriendlyMinionsDiedThisGameCounter(true, new GameV2());

			Assert.IsTrue(counter.ShouldShow());
			CollectionAssert.AreEqual(
				new[] { HearthDb.CardIds.Collectible.Mage.Aessina },
				counter.GetCardsToDisplay().ToArray()
			);
		}

		private static void SetActiveDeckWithoutEvents(Deck deck)
		{
			typeof(DeckList)
				.GetField("_activeDeck", BindingFlags.Instance | BindingFlags.NonPublic)
				.SetValue(DeckList.Instance, deck);
		}
	}
}
