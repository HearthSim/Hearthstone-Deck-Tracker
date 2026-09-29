using HearthDb.Enums;
using HearthMirror;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

		private static readonly TimeSpan AvailableRacesTimeout = TimeSpan.FromSeconds(15);

		// the memory read can come up empty right after the game starts, so keep trying for a while
		// (null if it keeps failing, OperationCanceledException once the game is over so callers can stop quietly)
		public static async Task<HashSet<Race>?> WaitForAvailableRaces()
		{
			CancellationToken token;
			lock(PendingRacesWaitLock)
				token = _gameplayCancellation.Token;

			// without a game id the races can't be cached, and a game change can't be told apart from the id arriving
			if(await WaitForCurrentGameId(token) is not Guid gameId)
				return null;

			// another caller may have read them since the shared wait's last attempt
			if(_availableRacesCache.TryGetValue(gameId, out var cachedRaces))
				return cachedRaces;

			var races = await GetOrStartSharedRacesWait(gameId, () => WaitForAvailableRaces(
				() => GetAvailableRaces(gameId),
				() => IsCurrentGame(gameId),
				AvailableRacesTimeout,
				TimeSpan.FromMilliseconds(250),
				token
			));

			if(!IsCurrentGame(gameId))
				throw new OperationCanceledException();

			return races;
		}

		// leaving gameplay ends every wait still in flight, instead of letting them run into the next game
		public static void CancelAvailableRacesWaits()
		{
			CancellationTokenSource cancelled;
			lock(PendingRacesWaitLock)
			{
				cancelled = _gameplayCancellation;
				_gameplayCancellation = new CancellationTokenSource();
				_pendingRacesWait = null;
			}
			cancelled.Cancel();
		}

		private static bool IsCurrentGame(Guid gameId) => !Core.Game.IsInMenu && Core.Game.CurrentGameStats?.GameId == gameId;

		private static async Task<Guid?> WaitForCurrentGameId(CancellationToken token)
		{
			var stopwatch = Stopwatch.StartNew();
			while(true)
			{
				if(Core.Game.CurrentGameStats?.GameId is Guid gameId)
					return gameId;
				if(Core.Game.IsInMenu)
					throw new OperationCanceledException();
				if(stopwatch.Elapsed >= AvailableRacesTimeout)
					return null;
				await Task.Delay(250, token);
			}
		}

		private static readonly object PendingRacesWaitLock = new();
		private static CancellationTokenSource _gameplayCancellation = new();
		private static (Guid GameId, Task<HashSet<Race>?> Task)? _pendingRacesWait;

		internal static Task<HashSet<Race>?> GetOrStartSharedRacesWait(Guid gameId, Func<Task<HashSet<Race>?>> start)
		{
			lock(PendingRacesWaitLock)
			{
				// a finished wait is not reused, so a caller after a timeout gets a fresh attempt
				if(_pendingRacesWait is { } pending && pending.GameId == gameId && !pending.Task.IsCompleted)
					return pending.Task;

				var task = start();
				_pendingRacesWait = (gameId, task);
				return task;
			}
		}

		internal static async Task<HashSet<Race>?> WaitForAvailableRaces(
			Func<HashSet<Race>?> read,
			Func<bool> isSameGame,
			TimeSpan timeout,
			TimeSpan initialDelay,
			CancellationToken token
		)
		{
			var stopwatch = Stopwatch.StartNew();
			var delay = initialDelay;
			var maxDelay = TimeSpan.FromSeconds(2);
			while(true)
			{
				token.ThrowIfCancellationRequested();
				var races = read();
				if(races != null)
					return races;
				if(!isSameGame())
					throw new OperationCanceledException();
				if(stopwatch.Elapsed >= timeout)
					return null;
				await Task.Delay(delay, token);
				delay = TimeSpan.FromTicks(Math.Min(delay.Ticks * 2, maxDelay.Ticks));
			}
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

		public static int? GetBattlegroundsDeityDbfId(Entity? game)
		{
			if(game == null) return null; // defensive to protect against wrong type of Core.Game.GameEntity
			var deityDbfId = game.GetTag(GameTag.BACON_GLOBAL_OLD_GOD_DBID);
			if(deityDbfId > 0)
				return deityDbfId;
			return null;
		}
		private static readonly List<BattlegroundsKeyword> _availableKeywords = new()
		{
			new TagKeyword(GameTag.BATTLECRY, "GameTag_Battlecry"),
			new TagKeyword(GameTag.DEATHRATTLE, "GameTag_Deathrattle"),
			new TagKeyword(GameTag.AVENGE, "GameTag_BGAvenge"),
			new TagKeyword(GameTag.BACON_RALLY, "GameTag_BGRally"),
			new TagKeyword(GameTag.DIVINE_SHIELD, "GameTag_DivineShield"),
			new TagKeyword(GameTag.TAUNT, "GameTag_Taunt"),
			new TagKeyword(GameTag.END_OF_TURN_TRIGGER, "GameTag_EndOfTurn"),
			new TagKeyword(GameTag.START_OF_COMBAT, "GameTag_StartOfCombat"),
			new TagKeyword(GameTag.REBORN, "GameTag_Reborn"),
			new TagKeyword(GameTag.CHOOSE_ONE, "GameTag_ChooseOne"),
			new TagKeyword(GameTag.MODULAR, "GameTag_Modular"),
			new TagKeyword(GameTag.VENOMOUS, "GameTag_Venomous"),
			new TagKeyword(GameTag.BACON_ACTIVATE_TOOLTIP, "GameTag_BGActivate"),
			new MentionedKeyword("Battlegrounds_Browser_Filter_Lockbox") { RequiredRace = Race.PIRATE },
		};

		public static List<BattlegroundsKeyword> GetAvailableKeywords(IEnumerable<Race>? availableRaces)
		{
			if(availableRaces is null)
				return _availableKeywords;
			var races = availableRaces as IReadOnlyCollection<Race> ?? availableRaces.ToList();
			return _availableKeywords
				.Where(x => x.RequiredRace is not { } required || races.Contains(required))
				.ToList();
		}
	}
}
