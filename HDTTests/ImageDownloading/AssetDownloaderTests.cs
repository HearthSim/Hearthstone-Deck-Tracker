using Hearthstone_Deck_Tracker.Utility.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
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
			AssetDownloader<string> assetDownloader = new AssetDownloader<string>("", key => "", key => "");
			Assert.Fail();
		}

		[TestMethod]
		public void AssetDownloader_GeneratesCorrectFileLocation()
		{
			AssetDownloader<string> assetDownloader = new AssetDownloader<string>(Path.GetTempPath(), key => "", key => $"{key}.jpg");
			Assert.AreEqual(Path.Combine(Path.GetTempPath(), "testFile.jpg"), assetDownloader.StoragePathFor("testFile"));
		}

		[TestMethod]
		public void AssetDownloader_ReturnsTrueIfImageExists()
		{
			var assetDownloader = new AssetDownloader<string>(Path.GetTempPath(), key => $"https://art.hearthstonejson.com/v1/256x/{key}.jpg", key => $"{key}.jpg");
			var awaiting = assetDownloader.DownloadAsset(WispCardId);
			Task.WaitAny(awaiting, Task.Delay(10000));
			Assert.IsTrue(awaiting.Result);
		}

		[TestMethod]
		public void AssetDownloader_ReturnsTrueIfImageDoesNotExist()
		{
			var assetDownloader = new AssetDownloader<string>(Path.GetTempPath(), key => $"https://art.hearthstonejson.com/v1/256x/{key}.jpg", key => $"{key}.jpg");
			var awaiting = assetDownloader.DownloadAsset("foo");
			Task.WaitAny(awaiting, Task.Delay(10000));
			Assert.IsFalse(awaiting.Result);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AssetDownloader_DownloadAsset_Null_ThrowsException()
		{
			var assetDownloader = new AssetDownloader<string>(Path.GetTempPath(), key => "", key => $"{key}.jpg");
			var awaiting = assetDownloader.DownloadAsset(null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AssetDownloader_StoragePathFor_Null_ThrowsException()
		{
			var assetDownloader = new AssetDownloader<string>(Path.GetTempPath(), key => "", key => $"{key}.jpg");
			var awaiting = assetDownloader.StoragePathFor(null);
		}
	}
}
