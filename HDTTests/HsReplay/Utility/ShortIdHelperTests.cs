using HearthDb.Deckstrings;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.Linq;
using CardIds = HearthDb.CardIds;
using Deck = Hearthstone_Deck_Tracker.Hearthstone.Deck;

namespace HDTTests.HsReplay.Utility
{
	[TestClass]
	public class ShortIdHelperTests
	{
		[TestMethod]
		public void BasicMageDeck_ValidShortId()
		{
			var hearthDbDeck = DeckSerializer.Deserialize("AAECAf0EAA9NWtgBtAK7AosDqwS0BOAE+wSWBYoH7AeeCZYNAA==");
			var deck = HearthDbConverter.FromHearthDbDeck(hearthDbDeck);
			var shortid = ShortIdHelper.GetShortId(deck);
			Assert.AreEqual(shortid, "KtYDbAkM8IjSSyLMWQn5zb");
		}

		[TestMethod]
		public void ComboPriest_ValidShortId()
		{
			var hearthDbDeck = DeckSerializer.Deserialize("AAECAa0GBqUJ+wzl9wLQ/gKnhwOppQMM+ALlBPYH1QjRCtIK8gz3DK+lA9KlA/2nA4SoAwA=");
			var deck = HearthDbConverter.FromHearthDbDeck(hearthDbDeck);
			var shortid = ShortIdHelper.GetShortId(deck);
			Assert.AreEqual(shortid, "0gsPp02q8ajKsGVstRwLpb");
		}

		[TestMethod]
		public void HighlanderHunter_ValidShortId()
		{
			var hearthDbDeck = DeckSerializer.Deserialize("AAECAR8engGoArUDxwOHBMkErgbFCNsJ7Qn+DJjwAp7wAu/xAqCAA6eCA5uFA/WJA+aWA/mWA76YA7acA56dA/yjA+WkA5+lA6KlA6alA4SnA5+3AwAA");
			var deck = HearthDbConverter.FromHearthDbDeck(hearthDbDeck);
			var shortid = ShortIdHelper.GetShortId(deck);
			Assert.AreEqual(shortid, "O2VpFDQokIrwww8iOmE3Lc");
		}

		[TestMethod]
		public void CardIdCollisionDeck_ValidShortId()
		{
			var hearthDbDeck = DeckSerializer.Deserialize("AAEBAf0EHnGeAcABwwG7Au4CqwSWBewF9w2JDroWwxbXtgLrugLYuwLZuwKHvQLBwQKP0wKi0wLu9gKnggP1iQP8owOSpAO+pAO/pAPdqQP0qwMAAA==");
			var deck = HearthDbConverter.FromHearthDbDeck(hearthDbDeck);
			var shortid = ShortIdHelper.GetShortId(deck);
			Assert.AreEqual(shortid, "mA9O7wXvzxMsx1KO7EuFve");
		}

		[TestMethod]
		public void ETCDeck_ValidShortId()
		{
			var hearthDbDeck = DeckSerializer.Deserialize("AAECAfHhBAjlsASJ5gSP7QSX7wSE9gTipAX9xAXIxwUQlrcE9OME/eMEieQElOQEh/YEsvcEs/cEtvoEq4AFopkF8cIF4MgF1c4Fj+QF0Z4GAAEDuNkE/cQF/+EE/cQF76IF/cQFAAA=");
			var deck = HearthDbConverter.FromHearthDbDeck(hearthDbDeck);
			var shortid = ShortIdHelper.GetShortId(deck);
			Assert.AreEqual(shortid, "QqdKc0MXat74LUQguHM3N");
		}

		[TestMethod]
		public void MultipleSideboards_ValidShortId()
		{
			var cards = new[] { Database.GetCardFromId(CardIds.Collectible.Druid.ClawLegacy) };
			var sideboards = new[]
			{
				new Sideboard(
					CardIds.Collectible.Neutral.ETCBandManager,
					new[]
					{
						Database.GetCardFromId(CardIds.Collectible.Neutral.Mechathun),
						Database.GetCardFromId(CardIds.Collectible.Neutral.YshaarjRageUnbound),
						Database.GetCardFromId(CardIds.Collectible.Neutral.NzothTheCorruptor)
					}.ToList()
				),
				new Sideboard(
					CardIds.Collectible.Neutral.Mechathun,
					new[]
					{
						Database.GetCardFromId(CardIds.Collectible.Druid.ClawLegacy),
						Database.GetCardFromId(CardIds.Collectible.Neutral.YshaarjRageUnbound),
						Database.GetCardFromId(CardIds.Collectible.Neutral.NzothTheCorruptor)
					}.ToList()
				)
			};

			var shortid = ShortIdHelper.GetShortId(new Deck
			{
				Cards = new ObservableCollection<Card>(cards),
				Sideboards = sideboards.ToList()
			});
			Assert.AreEqual(shortid, ShortIdHelper.GetShortId(new Deck
			{
				Cards = new ObservableCollection<Card>(cards),
				Sideboards = sideboards.Reverse().ToList()
			}));
			Assert.AreEqual(shortid, "1Q9hEG0dTelPtyDIIBbgYd");
		}
	}
}
