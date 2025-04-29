using HearthDb.Enums;
using HearthMirror;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using System;
using System.Collections.Generic;
using System.Linq;
using static HearthDb.CardIds;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public static class BattlegroundsUtils
	{
		private static readonly Dictionary<Guid, HashSet<Race>> _availableRacesCache = new Dictionary<Guid, HashSet<Race>>();

		const string UntransformedArannaCardid = NonCollectible.Neutral.ArannaStarseekerTavernBrawl1;
		const string TransformedArannaCardid = NonCollectible.Neutral.ArannaStarseeker_ArannaUnleashedTokenTavernBrawl;

		const string UntransformedQueenAzshara = NonCollectible.Neutral.QueenAzsharaBATTLEGROUNDS;
		const string TransformedQueenAzshara = NonCollectible.Neutral.QueenAzshara_NagaQueenAzsharaToken;

		private static readonly Dictionary<string, string> TransformableHeroCardidTable = new Dictionary<string, string>()
		{
			{ TransformedArannaCardid, UntransformedArannaCardid },
			{ TransformedQueenAzshara, UntransformedQueenAzshara }
		};

		public static HashSet<Race>? GetAvailableRaces()
		{
			return GetAvailableRaces(Core.Game.CurrentGameStats?.GameId);
		}

		public static HashSet<Race>? GetAvailableRaces(Guid? gameId)
		{
			var currentGameId = Core.Game.CurrentGameStats?.GameId;

			// Return from cache, if available
			if(
				(gameId ?? currentGameId) is Guid requestedGameId &&
				_availableRacesCache.TryGetValue(requestedGameId, out var cachedRaces)
			)
			{
				return cachedRaces;
			}

			// Not cached, so we need to get it from the game

			// If a specific game is requested, and we can't ensure it's the current game, we have to give up
			if(gameId.HasValue && (!currentGameId.HasValue || gameId.Value != currentGameId.Value))
				return null;

			// Otherwise get data from the current game
			var races = ReadAvailableRacesFromMemory();
			if(races is null)
				return null;

			// If we know the current game id, cache it as such
			if(currentGameId.HasValue)
				_availableRacesCache[currentGameId.Value] = races;

			return races;
		}

		private static HashSet<Race>? ReadAvailableRacesFromMemory()
		{
			var races = Reflection.Client.GetAvailableBattlegroundsRaces();
			if(races == null)
				return null;

			var hashSet = new HashSet<Race>(races.Cast<Race>());

			// Before initialized this contains only contains Race.INVALID
			if(hashSet.Count > 1 || hashSet.SingleOrDefault() != Race.INVALID)
				return hashSet;

			return null;
		}

		public static string GetOriginalHeroId(string heroId) => TransformableHeroCardidTable.TryGetValue(heroId, out var mapped) ? mapped : heroId;

		public static HashSet<int> GetAvailableTiers(string? anomalyCardId)
		{
			return anomalyCardId switch
			{
				NonCollectible.Neutral.BigLeague => new HashSet<int> { 3, 4, 5, 6 },
				NonCollectible.Neutral.HowToEven => new HashSet<int> { 2, 4, 6 },
				NonCollectible.Neutral.LittleLeague => new HashSet<int> { 1, 2, 3, 4 },
				NonCollectible.Neutral.SecretsOfNorgannon => new HashSet<int> { 1, 2, 3, 4, 5, 6, 7 },
				NonCollectible.Neutral.ValuationInflation => new HashSet<int> { 2, 3, 4, 5, 6 },
				NonCollectible.Neutral.WhatAreTheOdds => new HashSet<int> { 1, 3, 5 },
				_ => new HashSet<int> { 1, 2, 3, 4, 5, 6 },
			};
		}

		public static int? GetBattlegroundsAnomalyDbfId(Entity? game)
		{
			if(game == null) return null; // defensive to protect against wrong type of Core.Game.GameEntity
			var anomalyDbfId = game.GetTag(GameTag.BACON_GLOBAL_ANOMALY_DBID);
			if (anomalyDbfId > 0)
				return anomalyDbfId;
			return null;
		}
		public static List<GameTag> GetAvailableKeywords()
		{
			return new List<GameTag>()
			{
				GameTag.BATTLECRY,
				GameTag.DEATHRATTLE,
				GameTag.DIVINE_SHIELD,
				GameTag.TAUNT,
				GameTag.END_OF_TURN_TRIGGER,
				GameTag.START_OF_COMBAT,
				GameTag.REBORN,
				GameTag.CHOOSE_ONE,
				GameTag.MODULAR,
				GameTag.VENOMOUS
			};
		}
	}
}
