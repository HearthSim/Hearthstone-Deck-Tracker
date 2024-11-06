using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Hearthstone_Deck_Tracker.Utility.Assets
{
	public class AssetDownloader<T>
	{
		private readonly string _storageDestination;
		private readonly Func<T, string> _getUrl;
		private readonly Func<T, string> _getFilename;
		private readonly Dictionary<string, Task<bool>> _inProcessDownloads = new();
		private readonly long? _maxSize;
		private readonly LRUCache _lruCache;
		private readonly Dictionary<string, LRUCache.Entry> _lruLookup = new();
		private readonly HashSet<string> _validated = new();

		private string CacheFilePath => Path.Combine(_storageDestination, "Cache.xml");
		public string PlaceholderAssetPath { get; }

		/// <exception cref="ArgumentException">Thrown when directory cannot be accessed or created.</exception>
		/// <param name="storageDestination">Destination for assets to be stored.</param>

		public AssetDownloader(string storageDestination, Func<T, string> urlConverter, Func<T, string> fileNameConverter, long? maxSize = null, string? placeholderAsset = null)
		{
			_storageDestination = storageDestination;
			_getFilename = fileNameConverter;
			_getUrl = urlConverter;
			_maxSize = maxSize;

			_lruCache = TryLoadCache();
			foreach (var entry in _lruCache)
				_lruLookup[entry.File] = entry;

			TryCreateDirectory(_storageDestination);
			PlaceholderAssetPath = placeholderAsset ?? "pack://application:,,,/Resources/faceless_manipulator.png";
		}

		private LRUCache TryLoadCache()
		{
			try
			{
				if(File.Exists(CacheFilePath))
					return XmlManager<LRUCache>.Load(CacheFilePath);
			}
			catch(Exception e)
			{
				Log.Error(e);
			}

			return new LRUCache();
		}

		public void ClearStorage()
		{
			TryCleanDirectory(_storageDestination, false);
			_lruCache.Clear();
			SerializeLRUCache();
		}

		public void ValidateCachedAssets()
		{
			_validated.Clear();
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
			var directory = new DirectoryInfo(path);

			foreach(var file in directory.GetFiles())
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
				foreach(var dir in directory.GetDirectories())
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

		private void TryDeleteFile(LRUCache.Entry entry)
		{
			try
			{
				File.Delete(Path.Combine(_storageDestination, entry.File));
			}
			catch(IOException)
			{
			}
			catch(Exception e)
			{
				Log.Error($"Could not delete file {entry.File}: {e.Message}");
			}
			_lruCache.Remove(entry);
			_lruLookup.Remove(entry.File);
		}

		private void ManageLRUCache()
		{
			try
			{
				if(_lruCache.Count > _maxSize)
					_lruCache.GetRange((int)_maxSize.Value, _lruCache.Count - (int)_maxSize.Value).ForEach(TryDeleteFile);

				SerializeLRUCache();
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		/// <exception cref="ArgumentNullException">Thrown if obj is null</exception>
		public Task<bool> DownloadAsset(T obj, bool isCacheUpdate = false)
		{
			if(obj == null)
				throw new ArgumentNullException();
			ManageLRUCache();
			var filename = _getFilename(obj);
			if(_inProcessDownloads.TryGetValue(filename, out var inProgressDownload))
				return inProgressDownload;
			_validated.Add(filename);
			_inProcessDownloads[filename] = DownloadFileAsync(obj, isCacheUpdate);
			return _inProcessDownloads[filename];
		}

		/// <exception cref="ArgumentNullException">Thrown if obj is null</exception>
		private async Task<bool> DownloadFileAsync(T obj, bool isCacheUpdate)
		{
			if(obj == null)
				throw new ArgumentNullException();
			var filename = _getFilename(obj);

			try
			{
				using HttpRequestMessage request = new(HttpMethod.Get, _getUrl(obj));
				if(_lruLookup.TryGetValue(filename, out var entry))
					request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(entry.ETag));
				//Log.Debug($"Starting download for {filename} (isCacheUpdate={isCacheUpdate}).");
				var response = await Core.HttpClient.SendAsync(request);
				if(response.StatusCode == HttpStatusCode.NotModified)
				{
					//Log.Debug($"{filename} not modified");
					if(entry == null)
						return false;
					entry.Stale = false;
					SerializeLRUCache();
					return true;
				}

				if(response.StatusCode != HttpStatusCode.OK)
				{
					Log.Error($"Failed to download {filename}: {response.StatusCode}");
					return false;
				}

				using var fs = new FileStream(StoragePathFor(obj), FileMode.Create);
				await response.Content.CopyToAsync(fs);

				_inProcessDownloads.Remove(filename);
				var etag = response.Headers.ETag.Tag;
				if(entry == null)
				{
					entry = new LRUCache.Entry(filename, etag);
					_lruCache.Add(entry);
					_lruLookup[filename] = entry;
				}
				else
				{
					entry.ETag = etag;
					entry.Stale = false;
				}

				SerializeLRUCache();
				return true;
			}
			catch(WebException e)
			{
				Log.Error($"Unable to download {filename}: {e.Message}");
				return false;
			}
			catch(IOException e)
			{
				// Writing most likely failed because it was already in use.
				// We will mark the entry as stale and force an update next time.
				if(isCacheUpdate
				   && _lruLookup.TryGetValue(filename, out var entry))
				{
					entry.Stale = true;
					//Log.Debug($"Unable to write {filename}, marking stale");
					return true;
				}
				Log.Error($"Unable to write {filename}: {e.Message}");
				return false;
			}
			catch(Exception e)
			{
				Log.Error($"Unknown Error while trying to download {filename}: {e.Message}");
				return false;
			}
		}


		public bool HasAsset(T obj)
		{
			if(obj == null)
				return false;
			var filename = _getFilename(obj);
			if(!_lruLookup.TryGetValue(filename, out var entry))
				return false;

			// We validate images once per session / when _validated is cleared
			// (usually on Hearthstone start)
			if(!_validated.Contains(filename))
			{
				DownloadAsset(obj, true);
				_validated.Add(filename);
				if(entry.Stale)
				{
					// Pretend we don't have the asset to force an update.
					// DownloadAsset will overwrite the current file.
					return false;
				}
			}
			return true;
		}

		/// <exception cref="ArgumentNullException">Thrown if obj is null</exception>
		public string StoragePathFor(T obj)
		{
			if(obj == null)
				throw new ArgumentNullException();
			var filename = _getFilename(obj);
			if(_lruLookup.TryGetValue(filename, out var entry))
			{
				_lruCache.Remove(entry);
				_lruCache.Insert(0, entry);
			}
			return Path.Combine(_storageDestination, filename);
		}

		public async Task<string?> TryGetStoragePathFor(T obj)
		{
			try
			{
				if(!HasAsset(obj))
					await DownloadAsset(obj);
				return !HasAsset(obj) ? null : StoragePathFor(obj);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}

		private int _serializeLRUTracker = 0;
		private async void SerializeLRUCache()
		{

			var initialValue = ++_serializeLRUTracker;
			await Task.Delay(500);
			if(initialValue == _serializeLRUTracker)
			{
				_serializeLRUTracker = 0;
				XmlManager<LRUCache>.Save(CacheFilePath, _lruCache);
			}
		}
	}
}

[XmlType("LRUCache")]
[XmlRoot("LRUCache")]
public class LRUCache : List<LRUCache.Entry>
{
	public class Entry
	{
		[XmlAttribute("file")]
		public string File { get; set; } = "";

		[XmlAttribute("etag")]
		public string ETag { get; set; } = "";

		[XmlAttribute("stale")]
		public bool Stale { get; set; }

		public bool ShouldSerializeStale() => Stale;

		public Entry(string file, string eTag)
		{
			ETag = eTag;
			File = file;
		}

		public Entry()
		{
		}
	}
}
