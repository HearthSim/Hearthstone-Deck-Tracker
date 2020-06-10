using HearthDb;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class BattlegroundsDb
	{
		private Dictionary<int, Dictionary<Race, List<Card>>> _cardsByTier = new Dictionary<int, Dictionary<Race, List<Card>>>();

		public HashSet<Race> Races { get; } = new HashSet<Race>();

		public BattlegroundsDb()
		{
			Update(RemoteConfig.Instance.Data?.BattlegroundsTagOverrides);
			RemoteConfig.Instance.Loaded += d => Update(d?.BattlegroundsTagOverrides);
		}

		private void Update(List<RemoteConfig.ConfigData.TagOverride> tagOverrides)
		{
			var overrides = new Dictionary<int, Tuple<GameTag, int>>();
			if (tagOverrides != null)
			{
				foreach(var tagOverride in tagOverrides)
					overrides[tagOverride.DbfId] = new Tuple<GameTag, int>(tagOverride.Tag, tagOverride.Value);
			}
			Func<HearthDb.Card, GameTag, int> getTag = (HearthDb.Card card, GameTag tag) =>
			{
				if(overrides.TryGetValue(card.DbfId, out var tagOverride) && tagOverride.Item1 == tag)
					return tagOverride.Item2;
				return card.Entity.GetTag(tag);
			};

			var baconCards = Cards.All.Values
				.Where(x => getTag(x, GameTag.TECH_LEVEL) > 0 && getTag(x, GameTag.IS_BACON_POOL_MINION) > 0);

			Races.Clear();
			foreach(var race in baconCards.Select(x => x.Race))
				Races.Add(race);

			_cardsByTier.Clear();
			foreach(var card in baconCards)
			{
				var tier = getTag(card, GameTag.TECH_LEVEL);
				if(!_cardsByTier.ContainsKey(tier))
					_cardsByTier[tier] = new Dictionary<Race, List<Card>>();
				var race = GetRace(card);
				if(!_cardsByTier[tier].ContainsKey(race))
					_cardsByTier[tier][race] = new List<Card>();
				_cardsByTier[tier][race].Add(new Card(card, true));
			}
		}

		private Race GetRace(HearthDb.Card card)
		{
			var racesInText = Races
				.Where(x => x != Race.ALL && x != Race.INVALID)
				.Where(x => card.GetLocText(Locale.enUS)?.Contains(HearthDbConverter.RaceConverter(x)) ?? false)
				.ToList();
			if(racesInText.Count == 1)
				return racesInText.Single();
			return card.Race;
		}

		public List<Card> GetCards(int tier, Race race)
		{
			if(!_cardsByTier.TryGetValue(tier, out var cardsByRace))
				return new List<Card>();
			if(!cardsByRace.TryGetValue(race, out var cards))
				return new List<Card>();
			return cards;
		}
	}
}
