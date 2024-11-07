using Hearthstone_Deck_Tracker.Utility.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker;

namespace HDTTests.ImageDownloading
{
	[TestClass]
	public class AssetDownloaderTests
	{
		const string ValidCardId = HearthDb.CardIds.Collectible.Neutral.Wisp;
		const string InvalidCardId = "FOO_BAR";

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AssetDownloader_FailsWhenInitializedToInvalidPath()
		{
			new AssetDownloader<string, string>("", key => "", key => "", data => "");
			Assert.Fail();
		}

		[TestMethod]
		public async Task AssetDownloader_AssetExists_LoadsAndCachesAssets()
		{
			var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			var assetDownloader = new AssetDownloader<string, BitmapImage>(path, key => $"https://art.hearthstonejson.com/v1/256x/{key}.jpg", key => $"{key}.jpg", Helper.BitmapImageFromBytes);

			// Returns null if the asset does not exist in memory or on disk
			var asset = assetDownloader.TryGetAssetData(ValidCardId);
			Assert.IsNull(asset); // does not yet exist

			var task = assetDownloader.GetAssetData(ValidCardId);
			await Task.WhenAny(task, Task.Delay(10000));
			Assert.IsNotNull(task.Result);
			var filePath = Path.Combine(path, $"{ValidCardId}.jpg");

			var file = new FileInfo(filePath);
			Assert.IsTrue(file.Exists);
			Assert.IsTrue(file.Length > 0);

			// Rename file to verify that we a) don't lock it, and b) don't need it anymore
			var altFilePath = Path.Combine(path, $"_{ValidCardId}.jpg");
			file.MoveTo(altFilePath);

			// Serves from memory cache without trying to access it on disk
			task = assetDownloader.GetAssetData(ValidCardId);
			await Task.WhenAny(task, Task.Delay(10000));
			Assert.IsNotNull(task.Result);

			asset = assetDownloader.TryGetAssetData(ValidCardId);
			Assert.IsNotNull(asset); // Asset exists in memory now!

			// File on disk has not been recreated
			Assert.IsTrue(File.Exists(altFilePath));
			Assert.IsFalse(File.Exists(filePath));


			// Rename back
			File.Move(altFilePath, filePath);

			// Wait for LRU cache to be written to disk
			await Task.Delay(1000);

			// Second downloader should just load it from disk
			var assetDownloader2 = new AssetDownloader<string, BitmapImage>(path, key => $"https://art.hearthstonejson.com/v1/256x/{key}.jpg", key => $"{key}.jpg", Helper.BitmapImageFromBytes);

			// TryGetAssetData will not make a web request and instead only attempt
			// to load from disk
			var asset2 = assetDownloader2.TryGetAssetData(ValidCardId);
			Assert.IsNotNull(asset2);

			Directory.Delete(path, true);
		}

		[TestMethod]
		public async Task AssetDownloader_AssetDoesNotExist_ReturnsNull()
		{
			var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			var assetDownloader = new AssetDownloader<string, BitmapImage>(path, key => $"https://art.hearthstonejson.com/v1/256x/{key}.jpg", key => $"{key}.jpg", Helper.BitmapImageFromBytes);
			var task = assetDownloader.GetAssetData(InvalidCardId);
			await Task.WhenAny(task, Task.Delay(10000));
			Assert.IsNull(task.Result);
			var file = new FileInfo(Path.Combine(path, $"{InvalidCardId}.jpg"));
			Assert.IsFalse(file.Exists);

			Directory.Delete(path, true);
		}
	}
}
