using Hearthstone_Deck_Tracker.Utility.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
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

		// A file can end up on disk without a matching cache-index entry: eviction removes the index
		// entry (and attempts to delete the file, which can fail while the image is in use), or the
		// LRU index is trimmed to its cap on load. Such a file must still be served rather than
		// treated as missing (which would show the Faceless placeholder and never recover).
		[TestMethod]
		public void TryGetAssetData_FileOnDiskWithoutCacheEntry_ServesFromDisk()
		{
			var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(path);
			File.WriteAllText(Path.Combine(path, "FOO.txt"), "hello");

			var assetDownloader = new AssetDownloader<string, string>(
				path, key => $"http://127.0.0.1:1/{key}", key => $"{key}.txt", bytes => Encoding.UTF8.GetString(bytes));

			var data = assetDownloader.TryGetAssetData("FOO");

			Assert.AreEqual("hello", data);

			Directory.Delete(path, true);
		}

		[TestMethod]
		public async Task GetAssetData_FileOnDiskWithoutCacheEntry_ServesFromDiskWithoutDownloading()
		{
			var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(path);
			File.WriteAllText(Path.Combine(path, "FOO.txt"), "hello");

			// The url points at a closed port: if this ever tries to download instead of reading the
			// existing file, it returns null and the assert fails.
			var assetDownloader = new AssetDownloader<string, string>(
				path, key => $"http://127.0.0.1:1/{key}", key => $"{key}.txt", bytes => Encoding.UTF8.GetString(bytes));

			var data = await assetDownloader.GetAssetData("FOO");

			Assert.AreEqual("hello", data);

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
