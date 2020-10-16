using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Utility.Assets
{
	public class AssetDownloader
	{
		private string _storageDestiniation;
		private string _inProgressDestination;
		private string _url;
		private Func<string, string> _keyConverter;
		private static List<string> _succesfullyDownloadedImages = new List<string>();
		private static Dictionary<string, Task<bool>> _inProcessDownloads = new Dictionary<string, Task<bool>>();

		/// <exception cref="ArgumentException">Thrown when directory cannot be accessed or created.</exception>
		/// <param name="storageDestination">Destination for assets to be stored.</param>

		public AssetDownloader(string storageDestination, string url, Func<string, string> keyConverter)
		{
			_storageDestiniation = storageDestination;
			_inProgressDestination = Path.Combine(storageDestination, "_inProgress");
			_url = url;
			_keyConverter = keyConverter;
			TryCreateDirectory(_storageDestiniation);
			TryCreateDirectory(_inProgressDestination);
			TryCleanDirectory(_inProgressDestination);
			_succesfullyDownloadedImages.AddRange(GetCurrentlyStoredFileNames());
		}

		void TryCreateDirectory(string path)
		{
			try
			{
				if(!Directory.Exists(path))
					Directory.CreateDirectory(path);
			}
			catch(Exception e)
			{
				throw new ArgumentException($"Could not create new directory {path}:", e);
			}
		}

		void TryCleanDirectory(string path)
		{
			DirectoryInfo directory = new DirectoryInfo(path);
			try
			{

				foreach(FileInfo file in directory.GetFiles())
				{
					file.Delete();
				}
				foreach(DirectoryInfo dir in directory.GetDirectories())
				{
					dir.Delete(true);
				}
			}
			catch(Exception e)
			{
				Log.Error($"Could not clean directory {path}: {e.Message}");
			}
		}

		public Task<bool> DownloadAsset(string fileKey)
		{
			if(_inProcessDownloads.TryGetValue(fileKey, out var inProgressDownload))
				return inProgressDownload;
			var storagePath = StoragePathFor(fileKey);
			var inProgressPath = InProgressPathFor(fileKey);
			Log.Info($"Starting download for {fileKey}.");
			_inProcessDownloads[fileKey] = Task.Run(async () => {
				try
				{
					using(WebClient client = new WebClient())
					{
						var downloadTask = client.DownloadFileTaskAsync($"{_url}/{_keyConverter(fileKey)}", inProgressPath);
						Log.Info($"Waiting to cleanup {fileKey}.");
						return await CleanupDownload(fileKey, downloadTask, inProgressPath, storagePath);
					}
				}
				catch(Exception e)
				{
					Log.Error($"Unable to download {fileKey}: {e.Message}");
					return false;
				}
			});
			return _inProcessDownloads[fileKey];
		}

		private async Task<bool> CleanupDownload(string fileKey, Task toAwait, string inProgressPath, string finalPath)
		{
			await toAwait;
			_inProcessDownloads.Remove(fileKey);
			if(toAwait.IsCompletedSuccessfully())
			{
				_succesfullyDownloadedImages.Add(fileKey);
				try
				{
					File.Move(inProgressPath, finalPath);
					return true;
				}
				catch(Exception e)
				{
					Log.Error($"Could not move {inProgressPath} to {finalPath}: {e.Message}");
				}
			}
			else
			{
				try
				{
					File.Delete(inProgressPath);
				}
				catch(Exception e)
				{
					Log.Error($"Couldn't delete {fileKey} at path {inProgressPath}: {e.Message}");
				}
			}
			return false;
		}

		public bool HasAsset(string fileName) => _succesfullyDownloadedImages.Contains(fileName);

		private bool AssetDownloadStartedOrFinished(string fileName) => _succesfullyDownloadedImages.Contains(fileName) || _inProcessDownloads.ContainsKey(fileName);

		public string StoragePathFor(string fileKey) => Path.Combine(_storageDestiniation, _keyConverter(fileKey));

		private string InProgressPathFor(string fileKey) => Path.Combine(_inProgressDestination, _keyConverter(fileKey));

		public IEnumerable<string> GetCurrentlyStoredFileNames() => GetStoredImagesContent().Select(Path.GetFileNameWithoutExtension);

		private List<string> GetStoredImagesContent()
		{
			try
			{
				return Directory.GetFiles(_storageDestiniation).ToList();
			}
			catch(Exception e)
			{
				Log.Error($"Could not read the content of directory {_storageDestiniation}: {e.Message}");
				return null;
			}
		}

		public static bool FileIsSuccessfullyDownloaded(string fileName) => _succesfullyDownloadedImages.Contains(fileName);

	}
}
