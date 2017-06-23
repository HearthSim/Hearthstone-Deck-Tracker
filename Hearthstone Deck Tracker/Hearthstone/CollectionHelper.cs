using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthMirror;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class CollectionHelper
	{
		private static DateTime _lastUpdate;
		private static bool _awaitingUpdate;
		private static string _lastUsedKey;
		private static readonly Dictionary<string, Dictionary<string, int>> Collections = new Dictionary<string, Dictionary<string, int>>();
		public static event Action OnCollectionChanged;

		public static async Task<Dictionary<string, int>> GetCollection()
		{
			var key = GetCurrentKey();
			if(key == null)
			{
				_awaitingUpdate = true;
				return null;
			}
			if(!Collections.TryGetValue(key, out var collection))
			{
				await UpdateCollection(key);
				_awaitingUpdate = !Collections.TryGetValue(key, out collection);
			}
			else
				_awaitingUpdate = false;
			return collection;
		}

		public static async Task UpdateCollection() => await UpdateCollection(GetCurrentKey());

		private static async Task<bool> UpdateCollection(string key)
		{
			if(DateTime.Now - _lastUpdate < TimeSpan.FromSeconds(2) || key == null)
				return false;
			Log.Info("Updating collection...");
			_lastUpdate = DateTime.Now;
			var collection = await Task.Run(() => Reflection.GetCollection()?.GroupBy(x => x.Id)
				.ToDictionary(x => x.Key, x => x.Sum(c => c.Count)));
			if(collection?.Any() ?? false)
			{
				Collections[key] = collection;
				OnCollectionChanged?.Invoke();
				Log.Info("Updated collection!");
				return true;
			}
			Log.Info("No collection found");
			return false;
		}

		public static bool IsAwaitingUpdate => !Collections.Any() && _awaitingUpdate;

		private static string GetCurrentKey()
		{
			if(!Core.Game.IsRunning)
				return _lastUsedKey;
			var user = Reflection.GetAccountId();
			if(user == null)
				return _lastUsedKey;
			_lastUsedKey = $"{user.Hi}{user.Lo}";
			return _lastUsedKey;
		}

		public static async Task TryUpdateCollection()
		{
			Log.Info("Trying to update collection...");
			string key = null;
			for(var i = 0; i < 5; i++)
			{
				key = GetCurrentKey();
				if(key != null)
				{
					Log.Info($"Got key after {i + 1} tries");
					break;
				}
				await Task.Delay(5000);
			}
			if(key == null)
				return;
			for(var i = 0; i < 5; i++)
			{
				if(await UpdateCollection(key))
				{
					Log.Info($"Got collection after {i + 1} tries");
					return;
				}
				await Task.Delay(5000);
			}
			Log.Warn("Failed to update collection");
		}
	}
}
