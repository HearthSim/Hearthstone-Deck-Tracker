using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HDTTests.ImageDownloading
{
	[TestClass]
	public class AssetDownloaderTests
	{
		const string WispCardId = HearthDb.CardIds.Collectible.Neutral.Wisp;

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AssetDownloader_FailsWhenInitializedToInvalidPath()
		{
			AssetDownloader assetDownloader = new AssetDownloader("", "", null, null);
			Assert.Fail();
		}

		[TestMethod]
		public void AssetDownloader_GeneratesCorrectFileLocation()
		{
			AssetDownloader assetDownloader = new AssetDownloader(Path.GetTempPath(), "jpg", null, (string cardId) => $"{cardId}");
			Assert.AreEqual(Path.Combine(Path.GetTempPath(), "testFile.jpg"), assetDownloader.StoragePathFor("testFile"));
		}

		[TestMethod]
		public void AssetDownloader_ReturnsTrueIfImageExists()
		{
			var assetDownloader = new AssetDownloader(Path.GetTempPath(), "jpg", "https://art.hearthstonejson.com/v1/256x", (string cardId) => $"{cardId}");
			var awaiting = assetDownloader.DownloadAsset(WispCardId);
			Task.WaitAny(awaiting, Task.Delay(10000));
			Assert.IsTrue(awaiting.Result);
		}

		[TestMethod]
		public void AssetDownloader_ReturnsTrueIfImageDoesNotExist()
		{
			var assetDownloader = new AssetDownloader(Path.GetTempPath(), "jpg", "https://art.hearthstonejson.com/v1/256x", (string cardId) => $"{cardId}");
			var awaiting = assetDownloader.DownloadAsset("");
			Task.WaitAny(awaiting, Task.Delay(10000));
			Assert.IsFalse(awaiting.Result);
		}
	}
}
