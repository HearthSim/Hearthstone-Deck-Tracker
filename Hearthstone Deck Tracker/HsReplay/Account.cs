using System;
using System.IO;
using Hearthstone_Deck_Tracker.HsReplay.Enums;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal sealed class Account
	{
		public static string CacheFilePath => Path.Combine(Config.Instance.DataDir, "hsreplay.cache");

		private static readonly Lazy<Account> Lazy = new Lazy<Account>(Load);

		private Account()
		{
		}

		public static Account Instance => Lazy.Value;

		public string UploadToken { get; set; }
		public AccountStatus Status { get; set; }
		public string Username { get; set; }
		public int Id { get; set; }
		public DateTime LastUpdated { get; set; }

		public static bool Save()
		{
			var json = JsonConvert.SerializeObject(Instance);
			try
			{
				using(var sw = new StreamWriter(CacheFilePath))
					sw.WriteLine(json);
				return true;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return false;
			}
		}


		public static void DeleteCacheFile()
		{
			if(!File.Exists(CacheFilePath))
				return;
			try
			{
				File.Delete(CacheFilePath);
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		private static Account Load()
		{
			if(!File.Exists(CacheFilePath))
				return new Account();
			try
			{
				using(var sr = new StreamReader(CacheFilePath))
					return JsonConvert.DeserializeObject<Account>(sr.ReadToEnd());
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return new Account();
			}
		}
	}
}
