using System;
using System.IO;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.HsReplay.Data
{
	public class JsonCache<T>
	{
		private readonly string _fileName;

		public JsonCache(string fileName)
		{
			_fileName = fileName;
		}

		protected string CacheFilePath => Path.Combine(Config.Instance.DataDir, _fileName);

		protected async Task<T> LoadFromDisk()
		{
			var cacheFile = new FileInfo(CacheFilePath);
			if(!cacheFile.Exists)
				return default(T);
			try
			{
				using(var sr = new StreamReader(CacheFilePath))
				{
					var data = await sr.ReadToEndAsync();
					return JsonConvert.DeserializeObject<T>(data);
				}
			}
			catch(Exception e)
			{
				Log.Error(e);
				return default(T);
			}
		}

		protected async Task<bool> WriteToDisk(T data)
		{
			try
			{
				using(var sw = new StreamWriter(CacheFilePath))
					await sw.WriteAsync(JsonConvert.SerializeObject(data));
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
			return true;
		}
	}
}
