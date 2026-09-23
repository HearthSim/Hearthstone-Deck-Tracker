using HearthDb;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.Assets;
using HearthMirror.Objects;

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

	private BattlegroundsDb(BattlegroundsMinionPool pool, BattlegroundsDb fallback)
	{
		Update(pool, fallback);
	}

	/// <summary>
	/// Builds a database for the pool the game server sent for the current match. It is already specific to
	/// the game mode, so isDuos is ignored by all queries. Buddies are not part of the pool and are taken
	/// from the fallback.
	/// </summary>
	public static BattlegroundsDb FromMinionPool(BattlegroundsMinionPool pool, BattlegroundsDb fallback) => new(pool, fallback);

	private readonly HashSet<int> _bannedDbfIds = new();

	public bool IsBanned(int dbfId) => _bannedDbfIds.Contains(dbfId);

	/// <summary>
	/// The Dark Paradox in the minion pool, which rolls a different Dark Gift, stats and tier each game.
	/// </summary>
	public Card? DarkParadox { get; private set; }

	public int? DarkParadoxTier { get; private set; }

	private static bool IsDarkParadox(HearthDb.Card dbCard)
	{
		if(!Cards.All.TryGetValue(HearthDb.CardIds.NonCollectible.Neutral.DarkParadox, out var darkParadox))
			return false;
		return dbCard.DbfId == darkParadox.DbfId
			|| dbCard.Entity.GetTag(GameTag.BACON_EVOLUTION_CARD_ID) == darkParadox.DbfId;
	}

	private static readonly Dictionary<GameTag, Race> SubsetTagRaces = new()
	{
		{ GameTag.BACON_SUBSET_BEAST, Race.BEAST },
		{ GameTag.BACON_SUBSET_DEMON, Race.DEMON },
		{ GameTag.BACON_SUBSET_DRAGON, Race.DRAGON },
		{ GameTag.BACON_SUBSET_ELEMENTALS, Race.ELEMENTAL },
		{ GameTag.BACON_SUBSET_MECH, Race.MECHANICAL },
		{ GameTag.BACON_SUBSET_MURLOC, Race.MURLOC },
		{ GameTag.BACON_SUBSET_NAGA, Race.NAGA },
		{ GameTag.BACON_SUBSET_PIRATE, Race.PIRATE },
		{ GameTag.BACON_SUBSET_QUILLBOAR, Race.QUILBOAR },
		{ GameTag.BACON_SUBSET_UNDEAD, Race.UNDEAD },
		{ GameTag.BACON_SUBSET_ABERRATION, Race.ABERRATION },
	};

	private void Update(BattlegroundsMinionPool pool, BattlegroundsDb fallback)
	{
		var activeRaces = pool.ActiveMinionTypes.Cast<Race>().ToHashSet();
		Races.UnionWith(fallback.Races);
		Races.UnionWith(activeRaces);
		Races.Add(Race.INVALID);
		Races.Add(Race.ALL);

		foreach(var entry in pool.Cards)
		{
			if(!Cards.AllByDbfId.TryGetValue(entry.DbfId, out var dbCard))
				continue;
			var card = new Card(dbCard, true);
			if(entry.Banned)
			{
				_bannedDbfIds.Add(entry.DbfId);
				card.Count = 0;
			}
			// prefer this game's variant over the generic card, as only the variant knows the Dark Gift
			else if(IsDarkParadox(dbCard) && (DarkParadox == null || DarkParadox.Id == HearthDb.CardIds.NonCollectible.Neutral.DarkParadox))
			{
				DarkParadox = card;
				DarkParadoxTier = entry.Tier;
			}

			if(entry.CardType == (int)CardType.BATTLEGROUND_SPELL)
			{
				if(!_spellsByTier.ContainsKey(entry.Tier))
					_spellsByTier[entry.Tier] = new List<Card>();
				_spellsByTier[entry.Tier].Add(card);
				continue;
			}

			// match the in-game minion gallery, which also lists a minion under active tribes it is a subset of
			var races = entry.MinionTypes.Cast<Race>().ToHashSet();
			foreach(var subset in SubsetTagRaces)
			{
				if(activeRaces.Contains(subset.Value) && dbCard.Entity.GetTag(subset.Key) > 0)
					races.Add(subset.Value);
			}

			if(!_cardsByTier.ContainsKey(entry.Tier))
				_cardsByTier[entry.Tier] = new Dictionary<Race, List<Card>>();
			foreach(var race in races)
			{
				if(!_cardsByTier[entry.Tier].ContainsKey(race))
					_cardsByTier[entry.Tier][race] = new List<Card>();
				_cardsByTier[entry.Tier][race].Add(card);
			}
		}

		foreach(var tier in fallback._buddiesByTier)
			_buddiesByTier[tier.Key] = tier.Value;
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
		var raceList = races?.ToList() ?? new List<Race>();
		return GetCardsByRaces(raceList, isDuos, tier)
			.Where(card => keyword.Matches(card.GetTag, card.EnglishText))
			.Distinct()
			.ToList();
	}

	/// <summary>
	/// The cards that can be offered for the given races. Unlike the display queries, this leaves out banned cards.
	/// </summary>
	public List<Card> GetCardsByRaces(IReadOnlyCollection<Race> races, bool isDuos)
		=> GetCardsByRaces(races, isDuos, null).Where(card => !IsBanned(card.DbfId)).ToList();

	private IEnumerable<Card> GetCardsByRaces(IReadOnlyCollection<Race> races, bool isDuos, int? onlyTier)
	{
		var exclusiveCardsByTier = isDuos ? _duosExclusiveCardsByTier : _solosExclusiveCardsByTier;
		foreach(var cardsByTier in new[] { _cardsByTier, exclusiveCardsByTier })
		{
			foreach(var tier in cardsByTier)
			{
				if(onlyTier is int t && tier.Key != t)
					continue;
				foreach(var race in races)
				{
					if(tier.Value.TryGetValue(race, out var cards))
					{
						foreach(var card in cards)
							yield return card;
					}
				}
			}
		}
	}

	/// <summary>
	/// The spells that can be offered. Unlike the display queries, this leaves out banned spells.
	/// </summary>
	public List<Card> GetSpells(bool isDuos) => GetAllSpells(isDuos).Where(card => !IsBanned(card.DbfId)).ToList();

	private IEnumerable<Card> GetAllSpells(bool isDuos)
	{
		var exclusiveSpellsByTier = isDuos ? _duosExclusiveSpellsByTier : _solosExclusiveSpellsByTier;
		return _spellsByTier.Values.Concat(exclusiveSpellsByTier.Values).SelectMany(x => x);
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
		=> GetAllSpells(isDuos).Where(card => keyword.Matches(card.GetTag, card.EnglishText)).ToList();

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
