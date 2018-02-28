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
			var key = GetCurrentKey();
			if(key == null)
				return null;
			if(!Collections.TryGetValue(key, out var collection))
			{
				await UpdateCollection(key);
				Collections.TryGetValue(key, out collection);
			}
			return collection;
		}

		public static async Task UpdateCollection() => await UpdateCollection(GetCurrentKey());

		private static async Task<bool> UpdateCollection(Key key)
		{
			if(DateTime.Now - _lastUpdate < TimeSpan.FromSeconds(2) || key == null)
				return false;
			Log.Debug("Updating collection...");
			_lastUpdate = DateTime.Now;
			var collection = await Task.Run(() => Reflection.GetFullCollection());
			if(collection?.Cards.Any() ?? false)
			{
				Collections[key] = new Collection(key.Item1, key.Item2, collection);
				OnCollectionChanged?.Invoke();
				Log.Debug("Updated collection!");
				return true;
			}
			Log.Warn("No collection found");
			return false;
		}

		private static Key GetCurrentKey()
		{
			if(!Core.Game.IsRunning)
				return _lastUsedKey;
			var user = Reflection.GetAccountId();
			if(user == null)
				return _lastUsedKey;
			_lastUsedKey = new Key(user.Hi, user.Lo);
			return _lastUsedKey;
		}
	}
}
