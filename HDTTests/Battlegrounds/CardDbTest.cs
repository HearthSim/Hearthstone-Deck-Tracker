using System;
using System.Collections.Generic;
using System.Linq;
using BobsBuddy.Simulation;
using BobsBuddy.Utils;
using HearthDb;
using Hearthstone_Deck_Tracker.Hearthstone;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Battlegrounds
{
	[TestClass]
	public class SupportedCardsTest
	{
		[TestMethod]
		public void ValidateAllCurrentCardsAreKnown()
		{
			var simulator = new Simulator();
			var unknownCards = new List<string>();
			var textChangedCards = new List<string>();
			var battlegroundsCards = Cards.All.Values.Where(x => x.TechLevel != 0);

			foreach (var card in battlegroundsCards)
			{
				var result = SupportedCards.VerifyCardIsSupported(card);

				switch (result)
				{
					case SupportedCards.Result.UnknownCard:
						unknownCards.Add($"{card.Name} ({card.Id})");
						break;
					case SupportedCards.Result.TextChanged when simulator.MinionFactory.HasImplementationFor(card.Id):
						textChangedCards.Add($"{card.Name} ({card.Id})");
						break;
					case SupportedCards.Result.Supported:
						break;
				}
			}

			Assert.AreEqual(0, unknownCards.Count,
				$"Found unknown cards: {string.Join(", ", unknownCards)}");
			Assert.AreEqual(0, textChangedCards.Count,
				$"Found cards with changed text that have implementations: {string.Join(", ", textChangedCards)}");
		}
	}
}
