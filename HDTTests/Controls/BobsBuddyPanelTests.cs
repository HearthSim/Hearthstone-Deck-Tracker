using System.Globalization;
using System.Threading;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Controls
{
	[TestClass]
	public class BobsBuddyPanelTests
	{
		[TestMethod]
		public void Percent_WithoutAtLeast_HasNoPrefix()
		{
			Assert.AreEqual("62.5%", BobsBuddyPanel.Percent(0.625, false));
		}

		[TestMethod]
		public void Percent_WithAtLeast_IsPrefixed()
		{
			Assert.AreEqual("≥62.5%", BobsBuddyPanel.Percent(0.625, true));
		}

		[TestMethod]
		public void Percent_OfOne_IsStillRendered()
		{
			Assert.AreEqual("100%", BobsBuddyPanel.Percent(1, false));
		}

		[TestMethod]
		public void Percent_IsFormattedTheSameRegardlessOfTheThreadCulture()
		{
			Assert.AreEqual(PercentUnder("en-US", 0.625), PercentUnder("de-DE", 0.625));
		}

		private static string PercentUnder(string culture, double value)
		{
			var previousCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
			try
			{
				return BobsBuddyPanel.Percent(value, false);
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = previousCulture;
			}
		}
	}
}
