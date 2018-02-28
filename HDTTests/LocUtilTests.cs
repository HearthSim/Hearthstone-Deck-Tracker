using System;
using Hearthstone_Deck_Tracker.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests
{
	[TestClass]
	public class LocUtilTests
	{
		[TestMethod]
		public void AgeTest()
		{
			Assert.AreEqual("0 minutes ago", LocUtil.GetAge(DateTime.Now));
			Assert.AreEqual("0 minutes ago", LocUtil.GetAge(DateTime.Now.AddSeconds(59)));
			Assert.AreEqual("1 minute ago", LocUtil.GetAge(DateTime.Now.AddMinutes(-1)));
			Assert.AreEqual("1 minute ago", LocUtil.GetAge(DateTime.Now.AddMinutes(-1).AddSeconds(-59)));
			Assert.AreEqual("2 minutes ago", LocUtil.GetAge(DateTime.Now.AddMinutes(-2)));
			Assert.AreEqual("59 minutes ago", LocUtil.GetAge(DateTime.Now.AddMinutes(-59)));
			Assert.AreEqual("1 hour ago", LocUtil.GetAge(DateTime.Now.AddHours(-1)));
			Assert.AreEqual("1 hour ago", LocUtil.GetAge(DateTime.Now.AddHours(-1).AddMinutes(-59)));
			Assert.AreEqual("2 hours ago", LocUtil.GetAge(DateTime.Now.AddHours(-2)));
			Assert.AreEqual("23 hours ago", LocUtil.GetAge(DateTime.Now.AddHours(-23)));
			Assert.AreEqual("23 hours ago", LocUtil.GetAge(DateTime.Now.AddHours(-23).AddMinutes(-59)));
			Assert.AreEqual("1 day ago", LocUtil.GetAge(DateTime.Now.AddDays(-1)));
			Assert.AreEqual("1 day ago", LocUtil.GetAge(DateTime.Now.AddDays(-1).AddHours(-23)));
			Assert.AreEqual("2 days ago", LocUtil.GetAge(DateTime.Now.AddDays(-2)));
		}
	}
}
