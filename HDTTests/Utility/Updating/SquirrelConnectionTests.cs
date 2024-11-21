using System;
using System.Linq;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Utility.Updating;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Utility.Updating
{
	[TestClass]
	public class SquirrelConnectionTests
	{

		[TestMethod]
		public void SquirrelConnection_HasUrlForAllSources()
		{
			foreach(var value in Enum.GetValues(typeof(SquirrelRemote)).Cast<SquirrelRemote>())
			{
				try
				{
					_ = SquirrelConnection.SquirrelRemoteUrls[value];
				}
				catch(Exception e)
				{
					Assert.Fail(e.Message);
				}
			}
		}

		[TestMethod]
		public void GetCurrentRemote_ReturnsExpectedValue()
		{
			Config.Instance.SquirrelRemote = (int)SquirrelRemote.Github;
			var (remote, url) = SquirrelConnection.GetCurrentRemote();
			Assert.AreEqual(remote, SquirrelRemote.Github);

			Config.Instance.SquirrelRemote = (int)SquirrelRemote.AwsHongKong;
			(remote, url) = SquirrelConnection.GetCurrentRemote();
			Assert.AreEqual(remote, SquirrelRemote.AwsHongKong);

			Config.Instance.SquirrelRemote = 12345;
			(remote, url) = SquirrelConnection.GetCurrentRemote();
			Assert.AreEqual(remote, SquirrelRemote.Github); // invalid value defaults to github
		}
	}

}
