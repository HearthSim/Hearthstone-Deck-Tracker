using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HearthDb.Enums;

namespace HDTTests.Hearthstone
{
	[TestClass]
	public class DeckTest
	{
		[TestMethod]
		public void SubtractOperator001()
		{
			Deck d1 = new Deck();
			Deck d2 = new Deck();
			//just in d1
			d1.Cards.Add(new Card("ID_1", "", Rarity.FREE, "", "ID 1", 0, "", 0, 1, "", "", 0, 0, "", new string[] { }, 0, "", ""));
			//in both but diff count
			d1.Cards.Add(new Card("ID_2", "", Rarity.FREE, "", "ID 2", 0, "", 0, 2, "", "", 0, 0, "", new string[] { }, 0, "", ""));
			d2.Cards.Add(new Card("ID_2", "", Rarity.FREE, "", "ID 2", 0, "", 0, 3, "", "", 0, 0, "", new string[] { }, 0, "", ""));
			//just in d2
			d2.Cards.Add(new Card("ID_3", "", Rarity.FREE, "", "ID 3", 0, "", 0, 2, "", "", 0, 0, "", new string[] { }, 0, "", ""));
			//in bth and same cont
			d1.Cards.Add(new Card("ID_4", "", Rarity.FREE, "", "ID 4", 0, "", 0, 5, "", "", 0, 0, "", new string[] { }, 0, "", ""));
			d2.Cards.Add(new Card("ID_4", "", Rarity.FREE, "", "ID 4", 0, "", 0, 5, "", "", 0, 0, "", new string[] { }, 0, "", ""));

			IEnumerable<Card> result = d1 - d2;

			Assert.IsNotNull(result);
			foreach (Card c in result)
			{
				Console.Out.WriteLine(c.ToString());
			}
			Assert.AreEqual(3, result.Count());
			Assert.AreEqual(1, result.Where(c => c.Id == "ID_1" && c.Count == 1).Count(), "ID 1, expected count 1");
			Assert.AreEqual(1, result.Where(c => c.Id == "ID_2" && c.Count == -1).Count(), "ID 2, expected count -1");
			Assert.AreEqual(1, result.Where(c => c.Id == "ID_3" && c.Count == -2).Count(), "ID 3, expected count -2");
		}
	}
}
