using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Counters;

public class MinionTypesPlayedThisGameCounter : NumericCounter
{
	public override string LocalizedName => LocUtil.Get("Counter_PlayedMinionTypes", useCardLanguage: true);

	protected override string? CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.TheOneAmalgamBand;

	public override string[] RelatedCards => new string[]
	{
		HearthDb.CardIds.Collectible.Shaman.MountainMap,
		HearthDb.CardIds.Collectible.Shaman.SpiritOfTheMountain,
		HearthDb.CardIds.Collectible.Warrior.PowerSlider,
		HearthDb.CardIds.Collectible.Neutral.TheOneAmalgamBand,
	};

	public static readonly Race[] MinionTypes =
	{
		Race.BEAST, Race.DEMON, Race.DRAENEI, Race.DRAGON, Race.ELEMENTAL, Race.MECHANICAL,
		Race.MURLOC, Race.NAGA, Race.PIRATE, Race.QUILBOAR, Race.TOTEM, Race.UNDEAD,
	};

	// One entry per typed minion played, with every type it could count as (ALL expands to
	// all types). The counter is the maximum matching between these entries and distinct
	// types: the game always resolves multi-type minions in whichever way maximizes the
	// count, so a Dragon/Mech alone counts 1, then flexes to the other type when a pure
	// Dragon or Mech is played later (2), and a third minion of either type adds nothing.
	private readonly List<(int EntityId, Race[] Types)> _playedMinions = new();

	public MinionTypesPlayedThisGameCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
	{
	}

	public override bool ShouldShow()
	{
		if(!Game.IsTraditionalHearthstoneMatch) return false;
		if(IsPlayerCounter)
			return InPlayerDeckOrKnown(RelatedCards);
		return Counter > 1 && OpponentMayHaveRelevantCards(true);
	}

	public override string[] GetCardsToDisplay()
	{
		return IsPlayerCounter ?
			GetCardsInDeckOrKnown(RelatedCards).ToArray() :
			FilterCardsByClassAndFormat(RelatedCards, Game.Opponent.OriginalClass);
	}

	public override bool IsDisplayValueLong => true;

	public override string ValueToShow()
	{
		if(Counter == 0)
			return LocUtil.Get("Counter_Spell_School_None", useCardLanguage: true);

		var matchedBy = BuildMatching();
		var unplayed = GetUnplayedTypes();
		var definite = matchedBy.Keys
			.Where(type => !unplayed.Contains(type))
			.Select(HearthDbConverter.GetLocalizedRace)
			.WhereNotNull()
			.OrderBy(x => x);
		var flexible = matchedBy
			.Where(kvp => unplayed.Contains(kvp.Key))
			.Select(kvp => FormatTypeSet(_playedMinions[kvp.Value].Types))
			.OrderBy(x => x);
		return $"{Counter} - {string.Join(", ", definite.Concat(flexible))}";
	}

	private static string FormatTypeSet(Race[] types) =>
		types.Length == MinionTypes.Length
			? HearthDbConverter.GetLocalizedRace(Race.ALL) ?? ""
			: string.Join("/", types.Select(HearthDbConverter.GetLocalizedRace).WhereNotNull());

	public HashSet<Race> GetUnplayedTypes()
	{
		var matchedBy = BuildMatching();
		var unplayed = new HashSet<Race>();
		foreach(var type in MinionTypes)
		{
			var copy = new Dictionary<Race, int>(matchedBy);
			var visited = new HashSet<Race> { type };
			if(!copy.TryGetValue(type, out var holder) || TryAugment(holder, visited, copy))
				unplayed.Add(type);
		}
		return unplayed;
	}

	public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
	{
		if(!Game.IsTraditionalHearthstoneMatch)
			return;

		if(entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
			return;

		if(!entity.IsMinion)
			return;

		if(tag == GameTag.CANT_PLAY && value > 0 && LastEntityToCount?.Id == entity.Id)
		{
			var last = _playedMinions.FindLastIndex(m => m.EntityId == entity.Id);
			if(last >= 0)
			{
				_playedMinions.RemoveAt(last);
				Counter = BuildMatching().Count;
			}
			return;
		}

		if(tag != GameTag.ZONE)
			return;

		if(value != (int)Zone.PLAY)
			return;

		if(prevValue != (int)Zone.HAND)
			return;

		if(gameState.CurrentBlock?.Type != "PLAY")
			return;

		var types = GetTypes(entity.LatestCard);
		if(types.Length == 0)
			return;

		_playedMinions.Add((entity.Id, types));
		LastEntityToCount = entity;
		Counter = BuildMatching().Count;
	}

	private static Race[] GetTypes(Card card)
	{
		if(card.IsAllRace())
			return MinionTypes;
		return new[] { card.RaceEnum, card.SecondaryRaceEnum }
			.OfType<Race>()
			.Where(MinionTypes.Contains)
			.ToArray();
	}

	// Maximum bipartite matching (Kuhn's augmenting paths) between played minions and
	// types; sizes are tiny (≤ 12 types), so recomputing per play is negligible.
	private Dictionary<Race, int> BuildMatching()
	{
		var matchedBy = new Dictionary<Race, int>();
		for(var i = 0; i < _playedMinions.Count; i++)
			TryAugment(i, new HashSet<Race>(), matchedBy);
		return matchedBy;
	}

	private bool TryAugment(int minion, HashSet<Race> visited, Dictionary<Race, int> matchedBy)
	{
		foreach(var type in _playedMinions[minion].Types)
		{
			if(!visited.Add(type))
				continue;
			if(!matchedBy.TryGetValue(type, out var holder) || TryAugment(holder, visited, matchedBy))
			{
				matchedBy[type] = minion;
				return true;
			}
		}
		return false;
	}
}
