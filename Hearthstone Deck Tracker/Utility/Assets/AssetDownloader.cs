using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Utility.Assets
{
	public class AssetDownloader
	{
		private string _storageDestiniation;
		private string _inProgressDestination;
		private string _url;
		private string _fileExtension;
		private Func<string, string> _keyConverter;
		private List<string> _succesfullyDownloadedImages = new List<string>();
		private Dictionary<string, Task<bool>> _inProcessDownloads = new Dictionary<string, Task<bool>>();

		/// <exception cref="ArgumentException">Thrown when directory cannot be accessed or created.</exception>
		/// <param name="storageDestination">Destination for assets to be stored.</param>

		public AssetDownloader(string storageDestination, string url, string fileExtension, Func<string, string> keyConverter)
		{
			_storageDestiniation = storageDestination;
			_inProgressDestination = Path.Combine(storageDestination, "_inProgress");
			_url = url;
			_fileExtension = fileExtension;
			_keyConverter = keyConverter;
			TryCreateDirectory(_storageDestiniation);
			TryCreateDirectory(_inProgressDestination);
			TryCleanDirectory(_inProgressDestination, true);
			_succesfullyDownloadedImages.AddRange(GetCurrentlyStoredFileNames());
		}

		public void ClearStorage()
		{
			TryCleanDirectory(_inProgressDestination, true);
			TryCleanDirectory(_storageDestiniation, false);
			_succesfullyDownloadedImages.Clear();
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

		void TryCleanDirectory(string path, bool deleteDirs)
		{
			DirectoryInfo directory = new DirectoryInfo(path);

			foreach(FileInfo file in directory.GetFiles())
			{
				try
				{
					file.Delete();
				}
				catch(Exception e)
				{
					Log.Error($"Could not delete file {file.Name}: {e.Message}");
				}
			}
			if(deleteDirs)
			{
				foreach(DirectoryInfo dir in directory.GetDirectories())
				{
					try
					{
						dir.Delete(true);
					}
					catch(Exception e)
					{
						Log.Error($"Could not delete directory {dir.Name}: {e.Message}");
					}
				}
			}
		}

		public Task<bool> DownloadAsset(string fileKey)
		{
			if(_inProcessDownloads.TryGetValue(fileKey, out var inProgressDownload))
				return inProgressDownload;
			Log.Info($"Starting download for {fileKey}.");
			_inProcessDownloads[fileKey] = DownloadFileAsync(fileKey);
			return _inProcessDownloads[fileKey];
		}

		private async Task<bool> DownloadFileAsync(string fileKey)
		{
			var inProgressPath = InProgressPathFor(fileKey);
			try
			{
				using(WebClient client = new WebClient())
				{
					var downloadTask = client.DownloadFileTaskAsync($"{_url}/{_keyConverter(fileKey)}.{_fileExtension}", inProgressPath);
					Log.Info($"Waiting to cleanup {fileKey}.");
					return await CleanupDownload(fileKey, downloadTask, inProgressPath, StoragePathFor(fileKey));
				}
			}
			catch(Exception e)
			{
				Log.Error($"Unable to download {fileKey}: {e.Message}");
				return false;
			}
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

		public string StoragePathFor(string fileKey) => Path.Combine(_storageDestiniation, $"{fileKey}.{_fileExtension}");

		private string InProgressPathFor(string fileKey) => Path.Combine(_inProgressDestination, $"{fileKey}.{_fileExtension}");

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

		public bool FileIsSuccessfullyDownloaded(string fileName) => _succesfullyDownloadedImages.Contains(fileName);

	}
}
