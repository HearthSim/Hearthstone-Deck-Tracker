using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthMirror;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Key = System.Tuple<ulong, ulong>;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class CollectionHelper
	{
		private static DateTime _lastUpdate;
		private static Key _lastUsedKey;
		private static readonly Dictionary<Key, Collection> Collections = new Dictionary<Key, Collection>();
		public static event Action OnCollectionChanged;

		public static async Task<Collection> GetCollection()
		{
			var key = await GetCurrentKey();
			if(key == null)
				return null;
			if(!Collections.TryGetValue(key, out var collection))
			{
				await UpdateCollection(key);
				Collections.TryGetValue(key, out collection);
			}
			return collection;
		}

		public static bool TryGetCollection(out Collection collection)
		{
			collection = null;
			var key = GetCurrentKey(false).Result;
			if(key == null)
				return false;
			return Collections.TryGetValue(key, out collection);
		}

		public static async Task UpdateCollection() => await UpdateCollection(await GetCurrentKey());

		private static async Task<bool> UpdateCollection(Key key, bool retry = true)
		{
			if(DateTime.Now - _lastUpdate < TimeSpan.FromSeconds(2) || key == null)
				return false;
			Log.Debug("Updating collection...");
			_lastUpdate = DateTime.Now;
			var data = await Task.Run(() => new
			{
				Collection = Reflection.GetFullCollection(), 
				BattleTag = Reflection.GetBattleTag()
			});
			if(data.Collection?.Cards.Any() ?? false)
			{
				Collections[key] = new Collection(key.Item1, key.Item2, data.BattleTag, data.Collection);
				OnCollectionChanged?.Invoke();
				Log.Debug("Updated collection!");
				return true;
			}
			if(retry)
			{
				Log.Warn("No collection found, retrying...");
				await Task.Delay(3000);
				return await UpdateCollection(key, false);
			}
			Log.Warn("No collection found");
			return false;
		}

		private static async Task<Key> GetCurrentKey(bool retry = true)
		{
			if(!Core.Game.IsRunning)
				return _lastUsedKey;
			var user = Reflection.GetAccountId();
			if(user == null)
			{
				if(_lastUsedKey == null && retry)
				{
					Log.Debug("User not found, retrying...");
					await Task.Delay(3000);
					return await GetCurrentKey(false);
				}
				return _lastUsedKey;
			}
			_lastUsedKey = new Key(user.Hi, user.Lo);
			return _lastUsedKey;
		}
	}
}
