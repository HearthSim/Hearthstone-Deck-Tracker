using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Utility.Assets
{
	public class AssetDownloader<T, U>
	{
		private readonly string _storageDestination;
		private readonly Func<T, string> _getUrl;
		private readonly Func<T, string> _getFilename;
		private readonly Dictionary<string, Task<bool>> _inProgressDownloads = new();
		private readonly long? _maxCacheSize;
		private readonly Func<byte[], U> _dataConverter;
		private readonly LRUCache<U> _lruCache;
		private readonly Dictionary<string, LRUCache<U>.Entry> _lruLookup = new();

		private string CacheFilePath => Path.Combine(_storageDestination, "Cache.xml");

		private readonly string _placeholderAssetPath;
		private U? _placeholderAsset;
		public U? PlaceholderAsset
		{
			get
			{
				if(_placeholderAsset == null)
				{
					try
					{
						byte[] bytes;
						if(_placeholderAssetPath.StartsWith("pack://application"))
						{
							var stream = Application.GetResourceStream( new Uri(_placeholderAssetPath));
							if(stream == null)
								return default;
							using var ms = new MemoryStream();
							stream.Stream.CopyTo(ms);
							bytes = ms.ToArray();
						}
						else
						{
							bytes = File.ReadAllBytes(_placeholderAssetPath);
						}
						_placeholderAsset = _dataConverter(bytes);
					}
					catch
					{
						return default;
					}
				}

				return _placeholderAsset;
			}
		}

		/// <exception cref="ArgumentException">Thrown when directory cannot be accessed or created.</exception>
		/// <param name="storageDestination">Destination for assets to be stored.</param>

		public AssetDownloader(
			string storageDestination,
			Func<T, string> urlConverter,
			Func<T, string> fileNameConverter,
			Func<byte[], U> dataConverter,
			long? maxCacheSize = null,
			string? placeholderAsset = null)
		{
			_storageDestination = storageDestination;
			_getFilename = fileNameConverter;
			_getUrl = urlConverter;
			_maxCacheSize = maxCacheSize;
			_dataConverter = dataConverter;

			_lruCache = TryLoadCache();
			foreach (var entry in _lruCache)
				_lruLookup[entry.File] = entry;

			TryCreateDirectory(_storageDestination);
			_placeholderAssetPath = placeholderAsset ?? "pack://application:,,,/Resources/faceless_manipulator.png";
		}

		private LRUCache<U> TryLoadCache()
		{
			try
			{
				if(File.Exists(CacheFilePath))
					return XmlManager<LRUCache<U>>.Load(CacheFilePath);
			}
			catch(Exception e)
			{
				Log.Error(e);
			}

			return new LRUCache<U>();
		}

		public void ClearStorage()
		{
			TryCleanDirectory(_storageDestination, false);
			_lruCache.Clear();
			_lruLookup.Clear();
			SerializeLRUCache();
		}

		public void InvalidateCachedAssets()
		{
			foreach(var entry in _lruCache)
				entry.Validated = false;
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

		private void TryDeleteFile(LRUCache<U>.Entry entry)
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
				if(_lruCache.Count > _maxCacheSize)
					_lruCache.GetRange((int)_maxCacheSize.Value, _lruCache.Count - (int)_maxCacheSize.Value).ForEach(TryDeleteFile);

				SerializeLRUCache();
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		/// <exception cref="ArgumentNullException">Thrown if obj is null</exception>
		private Task<bool> DownloadAsset(T obj)
		{
			if(obj == null)
				throw new ArgumentNullException();
			var filename = _getFilename(obj);
			ManageLRUCache();
			if(_inProgressDownloads.TryGetValue(filename, out var inProgressDownload))
				return inProgressDownload;
			_inProgressDownloads[filename] = DownloadFileAsync(obj);
			return _inProgressDownloads[filename];
		}

		/// <exception cref="ArgumentNullException">Thrown if obj is null</exception>
		private async Task<bool> DownloadFileAsync(T obj)
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
					return true;
				}

				if(response.StatusCode != HttpStatusCode.OK)
				{
					Log.Error(
						$"Failed to download {filename}: {response.StatusCode}");
					return false;
				}

				var bytes = await response.Content.ReadAsByteArrayAsync();
				try
				{
					using var fs = new FileStream(StoragePathFor(obj), FileMode.Create);
					await fs.WriteAsync(bytes, 0, bytes.Length);
				}
				catch(Exception e)
				{
					Log.Error($"Unable to write {filename}: {e.Message}");
				}

				var data = _dataConverter(bytes);

				var etag = response.Headers.ETag.Tag;
				if(entry == null)
				{
					entry = new LRUCache<U>.Entry(filename, etag);
					_lruCache.Insert(0, entry);
					_lruLookup[filename] = entry;
				}
				else
				{
					entry.ETag = etag;
				}

				entry.Validated = true;
				entry.Data = data;

				SerializeLRUCache();
				return true;
			}
			catch(WebException e)
			{
				Log.Error($"Unable to download {filename}: {e.Message}");
				return false;
			}
			catch(Exception e)
			{
				Log.Error(
					$"Unknown Error while trying to download {filename}: {e.Message}");
				return false;
			}
			finally
			{
				_inProgressDownloads.Remove(filename);
			}
		}

		public async Task<U?> GetAssetData(T obj)
		{
			if(obj == null)
				return default;
			var filename = _getFilename(obj);
			if(!_lruLookup.TryGetValue(filename, out var entry))
			{
				var success = await DownloadAsset(obj);
				if(!success)
					return default;
				if(!_lruLookup.TryGetValue(filename, out entry))
					return default;
			}

			if(!entry.Validated)
			{
				DownloadAsset(obj).Forget();
				entry.Validated = true;
			}

			if(entry.Data != null)
				return entry.Data;

			// Probably fine to do sync for now, the file we are dealing with
			// are pretty small. This keeps us from reading the same file
			// multiple times without any extra logic.
			return LoadAssetFromDiskSync(obj);
		}

		public U? TryGetAssetData(T obj)
		{
			if(!_lruLookup.TryGetValue(_getFilename(obj), out var entry))
				return default;
			if(entry.Data == null)
			{
				// This will populate entry.Data
				LoadAssetFromDiskSync(obj);
			}

			if(!entry.Validated)
			{
				DownloadAsset(obj).Forget();
				entry.Validated = true;
			}

			return entry.Data;
		}

		private U? LoadAssetFromDiskSync(T obj)
		{
			var filename = _getFilename(obj);
			try
			{
				var bytes = File.ReadAllBytes(StoragePathFor(obj));
				var data = _dataConverter(bytes);
				if(_lruLookup.TryGetValue(filename, out var entry))
					entry.Data = data;
				return data;
			}
			catch(Exception e)
			{
				Log.Error($"Unable to read {_getFilename(obj)}: {e.Message}");
				if(_lruLookup.TryGetValue(filename, out var entry))
				{
					_lruCache.Remove(entry);
					_lruLookup.Remove(filename);
				}

				return default;
			}
		}

		/// <exception cref="ArgumentNullException">Thrown if obj is null</exception>
		private string StoragePathFor(T obj)
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

		private int _serializeLRUTracker = 0;
		private async void SerializeLRUCache()
		{

			var initialValue = ++_serializeLRUTracker;
			await Task.Delay(500);
			if(initialValue == _serializeLRUTracker)
			{
				_serializeLRUTracker = 0;
				XmlManager<LRUCache<U>>.Save(CacheFilePath, _lruCache);
			}
		}
	}
}

[XmlType("LRUCache")]
[XmlRoot("LRUCache")]
public class LRUCache<T> : List<LRUCache<T>.Entry>
{
	public class Entry
	{
		[XmlAttribute("file")]
		public string File { get; set; } = "";

		[XmlAttribute("etag")]
		public string ETag { get; set; } = "";

		[XmlIgnore]
		public T? Data { get; set; }

		[XmlIgnore]
		public bool Validated { get; set; }

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
