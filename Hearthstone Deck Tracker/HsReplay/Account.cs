using System;
using System.IO;
using Hearthstone_Deck_Tracker.HsReplay.Enums;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal sealed class Account
	{
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
				using(var sw = new StreamWriter(Constants.CacheFilePath))
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
			if(!File.Exists(Constants.CacheFilePath))
				return;
			try
			{
				File.Delete(Constants.CacheFilePath);
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		private static Account Load()
		{
			if(!File.Exists(Constants.CacheFilePath))
				return new Account();
			try
			{
				using(var sr = new StreamReader(Constants.CacheFilePath))
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
