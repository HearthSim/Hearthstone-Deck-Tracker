using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthMirror;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Key = System.Tuple<ulong, ulong>;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public static class CollectionHelpers
	{
		public static CollectionHelper<Collection> Hearthstone { get; } = new CollectionHelper<Collection>(LoadCollection);
		public static CollectionHelper<MercenariesCollection> Mercenaries { get; } = new CollectionHelper<MercenariesCollection>(LoadMercenariesCollection);

		private static async Task<Collection?> LoadCollection(Key key)
		{
			var data = await Task.Run(() => new
			{
				Collection = Reflection.Client.GetFullCollection(), 
				BattleTag = Reflection.Client.GetBattleTag()
			});
			if(data.Collection?.Cards.Any() ?? false)
			{
				return new Collection(key.Item1, key.Item2, data.BattleTag, data.Collection);
			}
			return null;
		}

		private static async Task<MercenariesCollection?> LoadMercenariesCollection(Key key)
		{
			var data = await Task.Run(() => new
			{
				Collection = Reflection.Client.GetMercenariesCollection(), 
				BattleTag = Reflection.Client.GetBattleTag()
			});
			if(data.Collection?.Any() ?? false)
			{
				return new MercenariesCollection(key.Item1, key.Item2, data.BattleTag, data.Collection);
			}
			return null;
		}
	}

	public class CollectionHelper<T> where T : CollectionBase
	{
		private DateTime _lastUpdate;
		private Key? _lastUsedKey;
		private readonly Dictionary<Key, T> Collections = new Dictionary<Key, T>();
		private readonly Func<Key, Task<T?>> _loadCollection;

		public event Action? OnCollectionChanged;

		public CollectionHelper(Func<Key, Task<T?>> loadCollection)
		{
			_loadCollection = loadCollection;
		}

		public async Task<T?> GetCollection()
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

		public bool TryGetCollection(out T? collection)
		{
			collection = null;
			var key = GetCurrentKey(false).Result;
			if(key == null)
				return false;
			return Collections.TryGetValue(key, out collection);
		}

		public async Task UpdateCollection() => await UpdateCollection(await GetCurrentKey());

		private async Task<bool> UpdateCollection(Key? key, bool retry = true)
		{
			if(key == null)
				return false;
			if(DateTime.Now - _lastUpdate < TimeSpan.FromSeconds(2) || key == null)
				return false;
			Log.Info("Updating collection...");
			_lastUpdate = DateTime.Now;
			var data = await _loadCollection(key);
			if(data != null)
			{
				Collections[key] = data;
				OnCollectionChanged?.Invoke();
				Log.Info("Updated collection!");
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

		private async Task<Key?> GetCurrentKey(bool retry = true)
		{
			if(!Core.Game.IsRunning)
				return _lastUsedKey;
			var user = Reflection.Client.GetAccountId();
			if(user == null)
			{
				if(_lastUsedKey == null && retry)
				{
					Log.Info("User not found, retrying...");
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
