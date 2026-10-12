using System;
using System.Linq;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Utility.Updating;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Utility.Updating
{
	[TestClass]
	public class UpdaterMirrorsTests
	{

		[TestMethod]
		public void MirrorUrls_HasUrlForAllMirrors()
		{
			foreach(var value in Enum.GetValues(typeof(UpdateMirror)).Cast<UpdateMirror>())
			{
				try
				{
					_ = Updater.MirrorUrls[value];
				}
				catch(Exception e)
				{
					Assert.Fail(e.Message);
				}
			}
		}

		[TestMethod]
		public void GetCurrentMirror_ReturnsExpectedValue()
		{
			Config.Instance.SquirrelRemote = (int)UpdateMirror.Github;
			var (mirror, url) = Updater.GetCurrentMirror();
			Assert.AreEqual(mirror, UpdateMirror.Github);

			Config.Instance.SquirrelRemote = (int)UpdateMirror.AwsHongKong;
			(mirror, url) = Updater.GetCurrentMirror();
			Assert.AreEqual(mirror, UpdateMirror.AwsHongKong);

			Config.Instance.SquirrelRemote = 12345;
			(mirror, url) = Updater.GetCurrentMirror();
			Assert.AreEqual(mirror, UpdateMirror.Github); // invalid value defaults to github
		}
	}

}
