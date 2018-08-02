using System.Collections.Generic;
using System.Linq;
using HearthMirror.Objects;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class Collection
	{
		public Collection(ulong accountHi, ulong accountLo, BattleTag battleTag, HearthMirror.Objects.Collection collection)
		{
			AccountHi = accountHi;
			AccountLo = accountLo;
			BattleTag = $"{battleTag.Name}#{battleTag.Number}";
			Cards = new SortedDictionary<int, int[]>(
				collection.Cards.Select(x => new {Key=GetDbfId(x.Id), Card=x}).GroupBy(x => x.Key)
					.ToDictionary(x => x.Key,
						x => new[] { x.FirstOrDefault(c => !c.Card.Premium)?.Card.Count ?? 0, x.FirstOrDefault(c => c.Card.Premium)?.Card.Count ?? 0 }));
			FavoriteHeroes = new SortedDictionary<int, int>(collection.FavoriteHeroes.ToDictionary(x => x.Key, x => GetDbfId(x.Value.Id)));
			CardBacks = collection.CardBacks.OrderBy(x => x).ToList();
			FavoriteCardBack = collection.FavoriteCardBack;
			Dust = collection.Dust;
		}

		private static int GetDbfId(string cardId) => Database.GetCardFromId(cardId)?.DbfIf ?? 0;

		[JsonIgnore]
		public ulong AccountHi { get; }

		[JsonIgnore]
		public ulong AccountLo { get; }

		[JsonIgnore]
		public string BattleTag { get; }

		[JsonProperty("collection")]
		public SortedDictionary<int, int[]> Cards { get; }

		[JsonProperty("favorite_heroes")]
		public SortedDictionary<int, int> FavoriteHeroes { get; }

		[JsonProperty("cardbacks")]
		public List<int> CardBacks { get; }

		[JsonProperty("favorite_cardback")]
		public int FavoriteCardBack { get; }

		[JsonProperty("dust")]
		public int Dust { get; }

		public override int GetHashCode() => JsonConvert.SerializeObject(this).GetHashCode();
	}
}
