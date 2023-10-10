using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility
{
	public class DataLoader<T>
	{
		private readonly Func<Task<T>> _load;
		private T? _data;
		private bool _loading;

		public event Action<T>? Loaded;

		public T? Data => TryGetData(out var data) ? data : default;

		public bool TryGetData(out T? data)
		{
			if(_data == null)
				Load();
			data = _data;
			return data != null;
		}

		public async void Load()
		{
			if(_loading)
				return;
			_loading = true;
			_data = await _load();
			Loaded?.Invoke(_data);
			_loading = false;
		}

		public DataLoader(Func<Task<T>> load)
		{
			_load = load;
		}

		public static DataLoader<T?> FromDisk(string path, Func<string, T> deserializer)
		{
			return new DataLoader<T?>(async () =>
			{
				try
				{
					using(var sr = new StreamReader(path))
					{
						var data = await sr.ReadToEndAsync();
						return deserializer(data);
					}
				}
				catch(Exception e)
				{
					Log.Error(e);
					return default;
				}
			});
		}

		public static DataLoader<T?> JsonFromDisk(string path) 
			=> FromDisk(path, JsonConvert.DeserializeObject<T>);

		public static DataLoader<T?> FromWeb(string url, Func<string, T> deserializer, string? contentType)
		{
			return new DataLoader<T?>(async () =>
			{
				try
				{
					using(var client = new WebClient())
					{
						client.Headers.Add("User-Agent", Helper.GetUserAgent());
						if(contentType != null)
							client.Headers.Add("accept", contentType);
						var data = await client.DownloadStringTaskAsync(url);
						return deserializer(data);
					}
				}
				catch(Exception e)
				{
					Log.Error(e);
					return default;
				}
			});
		}

		public static DataLoader<T?> JsonFromWeb(string url) 
			=> FromWeb(url, JsonConvert.DeserializeObject<T>, "application/json");
	}
}
