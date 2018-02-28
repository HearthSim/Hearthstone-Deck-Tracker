using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.HsReplay.Enums;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal sealed class Account : INotifyPropertyChanged
	{
		public static string CacheFilePath => Path.Combine(Config.Instance.DataDir, "hsreplay.cache");

		private static readonly Lazy<Account> Lazy = new Lazy<Account>(Load);
		private bool? _tokenClaimed;

		public event Action TokenClaimedChanged;

		private Account()
		{
			HSReplayNetOAuth.AccountDataUpdated += () =>
			{
				Update(HSReplayNetOAuth.AccountData.Id, HSReplayNetOAuth.AccountData.Username);
			};
			HSReplayNetOAuth.UploadTokenClaimed += () =>
			{
				TokenClaimed = true;
				Save();
			};
		}

		public void Update(int id, string username)
		{
			Id = id;
			Username = username;
			LastUpdated = DateTime.Now;
			OnPropertyChanged(nameof(Status));
			Save();
		}

		public void Reset()
		{
			UploadTokenHistory.Write("Deleting token");
			UploadToken = string.Empty;
			CollectionState.Clear();
			Update(0, null);
		}

		public static Account Instance => Lazy.Value;

		public string UploadToken { get; set; }

		public bool? TokenClaimed
		{
			get => _tokenClaimed;
			set
			{
				if(value != _tokenClaimed)
				{
					_tokenClaimed = value;
					TokenClaimedChanged?.Invoke();
				}
			}
		}

		[JsonIgnore]
		public AccountStatus Status => Id == 0 ? AccountStatus.Anonymous : AccountStatus.Registered;

		public string Username { get; set; }

		public int Id { get; set; }

		public DateTime LastUpdated { get; set; }

		public Dictionary<string, SyncState> CollectionState { get; set; } = new Dictionary<string, SyncState>();

		public override string ToString()
		{
			return $"Id={Id}, Username={Username}, Token=****-{UploadToken.Split('-').Last()}";
		}

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
					return JsonConvert.DeserializeObject<Account>(sr.ReadToEnd()) ?? new Account();
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return new Account();
			}
		}

		public class SyncState
		{
			public DateTime Date { get; set; }
			public int Hash { get; set; }

			public SyncState(int hash)
			{
				Hash = hash;
				Date = DateTime.Now;
			}

			public SyncState()
			{
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
