using HearthDb;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.Assets;

namespace Hearthstone_Deck_Tracker.Hearthstone;

public class BattlegroundsDb
{
	private readonly Dictionary<int, Dictionary<Race, List<Card>>> _cardsByTier = new();
	private readonly Dictionary<int, Dictionary<Race, List<Card>>> _solosExclusiveCardsByTier = new();
	private readonly Dictionary<int, Dictionary<Race, List<Card>>> _duosExclusiveCardsByTier = new();
	private readonly List<HearthDb.Card> _spells = new();
	private readonly Dictionary<int, List<Card>> _spellsByTier = new();
	private readonly Dictionary<int, List<Card>> _solosExclusiveSpellsByTier = new();
	private readonly Dictionary<int, List<Card>> _duosExclusiveSpellsByTier = new();
	private readonly List<HearthDb.Card> _buddies = new();
	private readonly Dictionary<int, List<Card>> _buddiesByTier = new();
	private readonly Dictionary<int, List<Card>> _solosExclusiveBuddiesByTier = new();
	private readonly Dictionary<int, List<Card>> _duosExclusiveBuddiesByTier = new();

	public HashSet<Race> Races { get; } = new();

	public BattlegroundsDb() : this(Remote.BattlegroundsLiveMetaPeriod.Data)
	{
		Remote.BattlegroundsLiveMetaPeriod.Loaded += Update;
		CardDefsManager.CardsChanged += () =>
		{
			Update(Remote.BattlegroundsLiveMetaPeriod.Data);
		};
	}

	internal BattlegroundsDb(RemoteData.MetaPeriod? metaPeriod)
	{
		Update(metaPeriod);
	}

	private class TagLookup
	{
		private readonly Dictionary<(int DbfId, GameTag Tag), int> _overrides = new();

		public TagLookup(List<RemoteData.TagOverride>? tagOverrides)
		{
			if(tagOverrides == null)
				return;
			foreach(var tagOverride in tagOverrides)
				_overrides[(tagOverride.DbfId, tagOverride.Tag)] = tagOverride.Value;
		}

		public int GetTag(HearthDb.Card card, GameTag tag)
			=> _overrides.TryGetValue((card.DbfId, tag), out var value) ? value : card.Entity.GetTag(tag);

		public Race GetRace(HearthDb.Card card) => (Race)GetTag(card, GameTag.CARDRACE);

		// HearthDb resolves the secondary race from a per-race marker tag being present at all, so an
		// override can only remove one by setting that tag to 0
		public Race GetSecondaryRace(HearthDb.Card card)
		{
			var race = GetRace(card);
			foreach(var tag in card.Entity.Tags)
			{
				if(!RaceUtils.TagRaceMap.TryGetValue(tag.EnumId, out var secondaryRace) || secondaryRace == race)
					continue;
				if(_overrides.TryGetValue((card.DbfId, (GameTag)tag.EnumId), out var value) && value == 0)
					continue;
				return secondaryRace;
			}
			return Race.INVALID;
		}
	}

	private IEnumerable<Race> GetRaces(HearthDb.Card card, TagLookup tags)
	{
		var race = tags.GetRace(card);
		if(race == Race.INVALID)
		{
			var racesInText = Races
				.Where(x => x != Race.ALL && x != Race.INVALID)
				.Where(x => card.GetLocText(Locale.enUS)?.Contains(HearthDbConverter.RaceConverter(x)) ?? false)
				.ToList();
			if(racesInText.Count == 1)
			{
				yield return racesInText.Single();
				yield break;
			}
		}
		yield return race;
		var secondaryRace = tags.GetSecondaryRace(card);
		if(secondaryRace != Race.INVALID)
			yield return secondaryRace;
	}

	internal void Update(RemoteData.MetaPeriod? metaPeriod)
	{
		var tags = new TagLookup(metaPeriod?.TagOverrides);

		var baconCards = Cards.All.Values
			.Where(x =>
				tags.GetTag(x, GameTag.TECH_LEVEL) > 0
				// explicitly check for == 1, as Rot Hide Gnoll has 2 but is not in the pool
				&& tags.GetTag(x, GameTag.IS_BACON_POOL_MINION) == 1
			)
			.ToList();

		// the card data can carry minions of a tribe that is not in rotation (yet), so the meta period
		// decides which tribes exist and the card scan is only the fallback until it has loaded
		Races.Clear();
		if(metaPeriod?.MinionTypes is { } minionTypes)
		{
			Races.UnionWith(minionTypes);
			Races.Add(Race.INVALID);
			Races.Add(Race.ALL);
		}
		else
		{
			foreach(var race in baconCards.Select(tags.GetRace))
				Races.Add(race);
		}

		_cardsByTier.Clear();
		_solosExclusiveCardsByTier.Clear();
		_duosExclusiveCardsByTier.Clear();
		foreach(var card in baconCards)
		{
			var tier = tags.GetTag(card, GameTag.TECH_LEVEL);
			var duosExclusive = tags.GetTag(card, GameTag.IS_BACON_DUOS_EXCLUSIVE);
			// the game doesn't actually set this ever to a negative value, but we use that as a sentinel
			// value to hide Solos-exclusive cards in Duos
			var targetDict = (
				duosExclusive > 0 ? _duosExclusiveCardsByTier :
				duosExclusive < 0 ? _solosExclusiveCardsByTier :
				_cardsByTier
			);
			if(!targetDict.ContainsKey(tier))
				targetDict[tier] = new Dictionary<Race, List<Card>>();
			foreach(var race in new HashSet<Race>(GetRaces(card, tags)))
			{
				if(!targetDict[tier].ContainsKey(race))
					targetDict[tier][race] = new List<Card>();
				targetDict[tier][race].Add(new Card(card, true));
			}
		}

		_spells.Clear();
		_spellsByTier.Clear();
		_solosExclusiveSpellsByTier.Clear();
		_duosExclusiveSpellsByTier.Clear();
		_spells.AddRange(Cards.All.Values
			.Where(x => (
				tags.GetTag(x, GameTag.TECH_LEVEL) > 0
				&& x.Type == CardType.BATTLEGROUND_SPELL
				&& tags.GetTag(x, GameTag.IS_BACON_POOL_SPELL) == 1
			)));
		foreach(var card in _spells)
		{
			var tier = tags.GetTag(card, GameTag.TECH_LEVEL);
			var duosExclusive = tags.GetTag(card, GameTag.IS_BACON_DUOS_EXCLUSIVE);
			var targetDict = (
				duosExclusive > 0 ? _duosExclusiveSpellsByTier :
				duosExclusive < 0 ? _solosExclusiveSpellsByTier :
				_spellsByTier
			);
			if(!targetDict.ContainsKey(tier))
				targetDict[tier] = new List<Card>();
			targetDict[tier].Add(new Card(card, true));
		}

		_buddies.Clear();
		_buddiesByTier.Clear();
		_solosExclusiveBuddiesByTier.Clear();
		_duosExclusiveBuddiesByTier.Clear();
		_buddies.AddRange(Cards.All.Values.Where(x => tags.GetTag(x, GameTag.BACON_BUDDY) == 1 && tags.GetTag(x, GameTag.BACON_TRIPLED_BASE_MINION_ID) == 0));
		foreach(var card in _buddies)
		{
			var tier = tags.GetTag(card, GameTag.TECH_LEVEL);
			var duosExclusive = tags.GetTag(card, GameTag.IS_BACON_DUOS_EXCLUSIVE);
			var targetDict = (
				duosExclusive > 0 ? _duosExclusiveBuddiesByTier :
				duosExclusive < 0 ? _solosExclusiveBuddiesByTier :
				_buddiesByTier
			);
			if(!targetDict.ContainsKey(tier))
				targetDict[tier] = new List<Card>();
			targetDict[tier].Add(new Card(card, true));
		}
	}

	public List<Card> GetCards(int tier, Race race, bool isDuos)
	{
		var cards = (
			_cardsByTier.TryGetValue(tier, out var cardsByRace) &&
			cardsByRace.TryGetValue(race, out var defaultCards)
		) ? defaultCards : new List<Card>();

		var exclusiveCardsByTier = isDuos ? _duosExclusiveCardsByTier : _solosExclusiveCardsByTier;
		var exclusiveCards = (
			exclusiveCardsByTier.TryGetValue(tier, out var exclusiveCardsByRace) &&
		    exclusiveCardsByRace.TryGetValue(race, out var theExclusiveCards)
		) ? theExclusiveCards : new List<Card>();

		return cards.Concat(exclusiveCards).ToList();
	}

	public List<Card> GetCards(int tier, BattlegroundsKeyword keyword, IEnumerable<Race>? races, bool isDuos)
	{
		var availableCards = GetCardsByRaces(races?.ToList() ?? new List<Race>(), isDuos);
		var cardsByTier = availableCards
			.GroupBy(card => card.GetTag(GameTag.TECH_LEVEL))
			.ToDictionary(
				group => group.Key,
				group => group.ToList()
			);
		return GetFilteredCardsByTierAndKeyword(cardsByTier, tier, keyword).ToList();
	}

	private List<Card> GetFilteredCardsByTierAndKeyword(Dictionary<int,List<Card>> cardsByTier, int tier,
		BattlegroundsKeyword keyword)
	{
		if (!cardsByTier.TryGetValue(tier, out var cards))
			return new List<Card>();

		return cards
			.Where(card => keyword.Matches(card.GetTag, card.EnglishText))
			.Distinct()
			.ToList();
	}

	public List<Card> GetCardsByRaces(IReadOnlyCollection<Race> races, bool isDuos)
	{
		var cards = new List<Card>();

		foreach (var tier in _cardsByTier.Values)
		{
			foreach (var race in races)
			{
				if (tier.TryGetValue(race, out var tierCards))
				{
					cards.AddRange(tierCards);
				}
			}
		}

		foreach (var tier in isDuos ? _duosExclusiveCardsByTier.Values : _solosExclusiveCardsByTier.Values)
		{
			foreach (var race in races)
			{
				if (tier.TryGetValue(race, out var exclusiveCards))
				{
					cards.AddRange(exclusiveCards);
				}
			}
		}

		return cards;
	}

	public List<Card> GetSpells(bool isDuos)
	{
		var allSpells = new List<Card>();

		foreach (var tierEntry in _spellsByTier)
		{
			allSpells.AddRange(tierEntry.Value);
		}

		var exclusiveSpellsDict = isDuos ? _duosExclusiveSpellsByTier : _solosExclusiveSpellsByTier;
		foreach (var tierEntry in exclusiveSpellsDict)
		{
			allSpells.AddRange(tierEntry.Value);
		}

		return allSpells;
	}

	public List<Card> GetSpells(int tier, bool isDuos)
	{
		var spells = (
			_spellsByTier.TryGetValue(tier, out var defaultSpells)
				? defaultSpells : new List<Card>()
		);

		var exclusiveSpells = (
			isDuos ? _duosExclusiveSpellsByTier : _solosExclusiveSpellsByTier
		).TryGetValue(tier, out var theExclusiveSpells) ? theExclusiveSpells : new List<Card>();

		return spells.Concat(exclusiveSpells).ToList();
	}

	public List<Card> GetSpells(BattlegroundsKeyword keyword, bool isDuos)
	{
		var availableSpells = new List<Card>();
		foreach(var card in _spells)
		{
			var duosExclusive = card.Entity.GetTag(GameTag.IS_BACON_DUOS_EXCLUSIVE);

			if(duosExclusive > 0 && isDuos)
				continue;
			if(duosExclusive < 0 && !isDuos)
				continue;

			if(keyword.Matches(card.Entity.GetTag, card.GetLocText(Locale.enUS)))
			{
				availableSpells.Add(new Card(card, true));
			}
		}
		return availableSpells;
	}

	public List<Card> GetBuddies(int tier, bool isDuos)
	{
		var buddies = (
			_buddiesByTier.TryGetValue(tier, out var defaultBuddies)
				? defaultBuddies : new List<Card>()
		);

		var exclusiveBuddies = (
			isDuos ? _duosExclusiveBuddiesByTier : _solosExclusiveBuddiesByTier
		).TryGetValue(tier, out var theExclusiveBuddies) ? theExclusiveBuddies : new List<Card>();

		return buddies.Concat(exclusiveBuddies).ToList();
	}
}
