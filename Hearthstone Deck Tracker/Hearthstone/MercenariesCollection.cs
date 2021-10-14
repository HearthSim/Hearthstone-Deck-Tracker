using HearthMirror;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class MercenariesCollection
	{
		private static Dictionary<int, long> _coinsByMercenary = new Dictionary<int, long>();

		public static List<MercenaryCollectionEntry> Update()
		{
			var deltas = new List<MercenaryCollectionEntry>();
			var data = Reflection.GetMercenariesInCollection();
			if(data == null)
				return deltas;
			foreach(var merc in data)
			{
				var delta = _coinsByMercenary.TryGetValue(merc.Id, out var existing)
					? (merc.CurrencyAmount - existing)
					: merc.CurrencyAmount;
				if(delta > 0)
					deltas.Add(new MercenaryCollectionEntry() { Id = merc.Id, Coins = (int)delta });
				_coinsByMercenary[merc.Id] = merc.CurrencyAmount;
			}
			return deltas;
		}
	}

	public class MercenaryCollectionEntry
	{
		public int Id { get; set; }
		public int Coins { get; set; }
	}
}
