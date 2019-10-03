using HearthDb.Deckstrings;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

	}
}
