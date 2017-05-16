using System;
using System.Linq;
using HearthDb;
using Hearthstone_Deck_Tracker.Hearthstone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CardIds = HearthDb.CardIds;

namespace HDTTests.Hearthstone
{
	[TestClass]
	public class DeckSerializerTest
	{
		[TestMethod]
		public void TestMethod1()
		{
			var deck = new Deck();
			deck.Cards.Add(Database.GetCardFromId(CardIds.Collectible.Druid.AddledGrizzly));
			var aol = Database.GetCardFromId(CardIds.Collectible.Druid.AncientOfLore);
			aol.Count = 2;
			var aow = Database.GetCardFromId(CardIds.Collectible.Druid.AncientOfWar);
			aow.Count = 3;
			deck.Cards.Add(aol);
			deck.Cards.Add(aow);
			deck.Class = "Druid";
			var deckString = DeckSerializer.Serialize(deck);

			var deck2 = DeckSerializer.Deserialize(deckString);
			Assert.AreEqual(deck.Class, deck2.Class);
			Assert.AreEqual(deck.Cards.Count, deck2.Cards.Count);
			foreach(var card in deck.Cards)
				Assert.IsTrue(deck2.Cards.Any(c => c.Id == card.Id && c.Count == card.Count));

			var foo =
				DeckSerializer.DeserializeDeckString("AAECAR8G+LEChwTmwgKhwgLZwgK7BQzquwKJwwKOwwKTwwK5tAK1A/4MqALsuwLrB86uAu0JAA==");

			var deckString2 = DeckSerializer.Serialize(deck2);
			Assert.AreEqual(deckString, deckString2);

		}
	}
}
