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

	public BattlegroundsDb()
	{
		Update(Remote.Config.Data?.BattlegroundsTagOverrides);
		Remote.Config.Loaded += d => Update(d?.BattlegroundsTagOverrides);
		CardDefsManager.CardsChanged += () =>
		{
			Update(Remote.Config.Data?.BattlegroundsTagOverrides);
		};
	}

	private void Update(List<RemoteData.TagOverride>? tagOverrides)
	{
		var overrides = new Dictionary<int, Tuple<GameTag, int>>();
		if (tagOverrides != null)
		{
			foreach(var tagOverride in tagOverrides)
				overrides[tagOverride.DbfId] = new Tuple<GameTag, int>(tagOverride.Tag, tagOverride.Value);
		}

		int GetTag(HearthDb.Card card, GameTag tag)
		{
			if(overrides.TryGetValue(card.DbfId, out var tagOverride) && tagOverride.Item1 == tag) return tagOverride.Item2;
			return card.Entity.GetTag(tag);
		}

		var baconCards = Cards.All.Values
			.Where(x =>
				GetTag(x, GameTag.TECH_LEVEL) > 0
				&& GetTag(x, GameTag.IS_BACON_POOL_MINION) > 0
			)
			.ToList();

		Races.Clear();
		foreach(var race in baconCards.Select(x => x.Race))
			Races.Add(race);

		_cardsByTier.Clear();
		_solosExclusiveCardsByTier.Clear();
		_duosExclusiveCardsByTier.Clear();
		foreach(var card in baconCards)
		{
			var tier = GetTag(card, GameTag.TECH_LEVEL);
			var duosExclusive = GetTag(card, GameTag.IS_BACON_DUOS_EXCLUSIVE);
			// the game doesn't actually set this ever to a negative value, but we use that as a sentinel
			// value to hide Solos-exclusive cards in Duos
			var targetDict = (
				duosExclusive > 0 ? _duosExclusiveCardsByTier :
				duosExclusive < 0 ? _solosExclusiveCardsByTier :
				_cardsByTier
			);
			if(!targetDict.ContainsKey(tier))
				targetDict[tier] = new Dictionary<Race, List<Card>>();
			foreach(var race in new HashSet<Race>(GetRaces(card)))
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
				GetTag(x, GameTag.TECH_LEVEL) > 0
				&& x.Type == CardType.BATTLEGROUND_SPELL
				&& GetTag(x, GameTag.IS_BACON_POOL_SPELL) == 1
			)));
		foreach(var card in _spells)
		{
			var tier = GetTag(card, GameTag.TECH_LEVEL);
			var duosExclusive = GetTag(card, GameTag.IS_BACON_DUOS_EXCLUSIVE);
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
		_buddies.AddRange(Cards.All.Values.Where(x => GetTag(x, GameTag.BACON_BUDDY) == 1 && GetTag(x, GameTag.BACON_TRIPLED_BASE_MINION_ID) == 0));
		foreach(var card in _buddies)
		{
			var tier = GetTag(card, GameTag.TECH_LEVEL);
			var duosExclusive = GetTag(card, GameTag.IS_BACON_DUOS_EXCLUSIVE);
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

	private IEnumerable<Race> GetRaces(HearthDb.Card card)
	{
		if(card.Race == Race.INVALID)
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
		yield return card.Race;
		if(card.SecondaryRace != Race.INVALID)
			yield return card.SecondaryRace;
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

	public List<Card> GetCards(int tier, GameTag keyword, IEnumerable<Race>? races, bool isDuos)
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
		GameTag keyword)
	{
		if (!cardsByTier.TryGetValue(tier, out var cards))
			return new List<Card>();

		return cards
			.Where(card =>
				card.GetTag(keyword) > 0 ||
				(keyword != GameTag.IS_BACON_POOL_SPELL && (card.EnglishText?.Contains(HearthDbConverter.GetLocalizedKeyword(keyword)) ?? false)))
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

	public List<Card> GetSpells(GameTag keyword, bool isDuos)
	{
		var availableSpells = new List<Card>();
		foreach(var card in _spells)
		{
			var duosExclusive = card.Entity.GetTag(GameTag.IS_BACON_DUOS_EXCLUSIVE);

			if(duosExclusive > 0 && isDuos)
				continue;
			if(duosExclusive < 0 && !isDuos)
				continue;

			if (card.Entity.GetTag(keyword) > 0 ||
			    (keyword != GameTag.IS_BACON_POOL_SPELL && (card.GetLocText(Locale.enUS)?.Contains(HearthDbConverter.GetLocalizedKeyword(keyword)) ?? false)))
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
