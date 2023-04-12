using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDTTests.Hearthstone
{
	[TestClass]
	public class HearthDbConverterTest
	{
		private readonly CardSet[] _invalidSets =
		{
			CardSet.TAVERNS_OF_TIME,
			CardSet.BATTLEGROUNDS,
			CardSet.LETTUCE,
			CardSet.MERCENARIES_DEV,
			CardSet.PLACEHOLDER_202204
		};

		[TestMethod]
		public void TestSetDictIncludeNewSets()
		{
			foreach (var setCode in Enum.GetValues(typeof(CardSet)))
				if ((int)setCode > 1000 && !_invalidSets.Contains((CardSet)setCode))
					Assert.IsTrue(
						HearthDbConverter.SetDict.ContainsKey((int)setCode),
						$"Missing name for {(CardSet)setCode}"
					);
		}
	}
}
