using System;
using Hearthstone_Deck_Tracker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Utility
{
	[TestClass]
	public class HelperTests
	{
		[TestMethod]
		public void ToPrettyNumber_ReturnsCorrectNumber()
		{
			Assert.AreEqual(880, Helper.ToPrettyNumber(885));
			Assert.AreEqual(8200, Helper.ToPrettyNumber(8293));
			Assert.AreEqual(85000, Helper.ToPrettyNumber(85277));
		}
	}
}
