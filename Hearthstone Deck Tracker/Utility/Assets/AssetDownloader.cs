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
	public class AssetDownloader<T>
	{
		private readonly string _storageDestiniation;
		private readonly string _inProgressDestination;
		private readonly Func<T, string> _getUrl;
		private readonly Func<T, string> _getFilename;
		private readonly List<string> _succesfullyDownloadedImages = new List<string>();
		private readonly Dictionary<string, Task<bool>> _inProcessDownloads = new Dictionary<string, Task<bool>>();
		private readonly long? _maxSize = null;
		private readonly List<string> _lRUCache = new List<string>();

		private bool UseLRUCache => _maxSize != null;
		private string LRUCacheXMLPath => Path.Combine(_storageDestiniation, "Cache.xml");
		public string PlaceholderAssetPath { get; }

		/// <exception cref="ArgumentException">Thrown when directory cannot be accessed or created.</exception>
		/// <param name="storageDestination">Destination for assets to be stored.</param>

		public AssetDownloader(string storageDestination, Func<T, string> urlConverter, Func<T, string> fileNameConverter, long? maxSize = null, string placeholderAsset = null)
		{
			_storageDestiniation = storageDestination;
			_inProgressDestination = Path.Combine(storageDestination, "_inProgress");
			_getFilename = fileNameConverter;
			_getUrl = urlConverter;
			_maxSize = maxSize;
			if(UseLRUCache)
				_lRUCache = TryLoadCache();
			TryCreateDirectory(_storageDestiniation);
			TryCreateDirectory(_inProgressDestination);
			TryCleanDirectory(_inProgressDestination, true);
			_succesfullyDownloadedImages.AddRange(GetCurrentlyStoredFileNames());
			PlaceholderAssetPath = placeholderAsset ?? "pack://application:,,,/Resources/faceless_manipulator.png";
		}

		private List<string> TryLoadCache()
		{
			try
			{
				if(File.Exists(LRUCacheXMLPath))
					return XmlManager<List<string>>.Load(LRUCacheXMLPath);
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
			return new List<string>();
		}

		public void ClearStorage()
		{
			TryCleanDirectory(_inProgressDestination, true);
			TryCleanDirectory(_storageDestiniation, false);
			_succesfullyDownloadedImages.Clear();
			_lRUCache.Clear();
			SerializeLRUCache();
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

		private void TryDeleteFile(string filename)
		{
			var directory = new DirectoryInfo(_storageDestiniation);
			var file = directory.GetFiles().FirstOrDefault(x => x.Name == filename);
			if(file != null)
			{
				try
				{
					file.Delete();
					_lRUCache.Remove(filename);
					_succesfullyDownloadedImages.Remove(filename);
				}
				catch(IOException)
				{
					
				}
				catch(Exception e)
				{
					Log.Error($"Could not delete file {file.Name}: {e.Message}");
				}
			}
		}

		private void ManageLRUCache()
		{
			if(UseLRUCache)
			{
				try
				{
					if(_lRUCache.Count > _maxSize)
					{
						_lRUCache.GetRange((int)_maxSize.Value, _lRUCache.Count - (int)_maxSize.Value).ForEach(TryDeleteFile);
					}
					SerializeLRUCache();
				}
				catch(Exception ex)
				{

				}
			}
		}

		/// <exception cref="ArgumentNullException">Thrown if obj is null</exception>
		public Task<bool> DownloadAsset(T obj)
		{
			if(obj == null)
				throw new ArgumentNullException();
			ManageLRUCache();
			var filename = _getFilename(obj);
			if(_inProcessDownloads.TryGetValue(filename, out var inProgressDownload))
				return inProgressDownload;
			Log.Info($"Starting download for {filename}.");
			_inProcessDownloads[filename] = DownloadFileAsync(obj);
			return _inProcessDownloads[filename];
		}

		/// <exception cref="ArgumentNullException">Thrown if obj is null</exception>
		private async Task<bool> DownloadFileAsync(T obj)
		{
			if(obj == null)
				throw new ArgumentNullException();
			var filename = _getFilename(obj);
			var inProgressPath = InProgressPathFor(filename);
			try
			{
				using(var client = new WebClient())
				{
					var url = _getUrl(obj);
					var downloadTask = client.DownloadFileTaskAsync(url, inProgressPath);
					Log.Info($"Waiting to cleanup {filename}.");
					return await CleanupDownload(filename, downloadTask, inProgressPath, StoragePathFor(obj));
				}
			}
			catch(Exception e)
			{
				Log.Error($"Unable to download {filename}: {e.Message}");
				return false;
			}
		}

		private async Task<bool> CleanupDownload(string filename, Task toAwait, string inProgressPath, string finalPath)
		{
			await toAwait;
			_inProcessDownloads.Remove(filename);
			if(toAwait.IsCompletedSuccessfully())
			{
				_succesfullyDownloadedImages.Add(filename);
				try
				{
					if(File.Exists(finalPath))
						File.Delete(finalPath);
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
					Log.Error($"Couldn't delete {filename} at path {inProgressPath}: {e.Message}");
				}
			}
			return false;
		}

		public bool HasAsset(T obj)
		{
			if(obj == null)
				return false;
			var filename = _getFilename(obj);
			return _succesfullyDownloadedImages.Contains(filename);
		}

		/// <exception cref="ArgumentNullException">Thrown if obj is null</exception>
		public string StoragePathFor(T obj)
		{
			if(obj == null)
				throw new ArgumentNullException();
			var filename = _getFilename(obj);
			_lRUCache.Remove(filename);
			_lRUCache.Insert(0, filename);
			return Path.Combine(_storageDestiniation, filename);
		}

		private int _serializeLRUTracker = 0;
		private async void SerializeLRUCache()
		{
			
			var initialValue = ++_serializeLRUTracker;
			await Task.Delay(500);
			if(initialValue == _serializeLRUTracker)
			{
				_serializeLRUTracker = 0;
				XmlManager<List<string>>.Save(LRUCacheXMLPath, _lRUCache);
			}
		}

		private string InProgressPathFor(string filename) => Path.Combine(_inProgressDestination, filename);

		public IEnumerable<string> GetCurrentlyStoredFileNames() => GetStoredImagesContent().Select(Path.GetFileName);

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
