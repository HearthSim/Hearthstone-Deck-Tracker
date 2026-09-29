using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Battlegrounds
{
	[TestClass]
	public class BattlegroundsAvailableRacesTest
	{
		private static readonly HashSet<Race> Races = new HashSet<Race> { Race.BEAST, Race.DEMON, Race.MURLOC, Race.PIRATE, Race.UNDEAD };

		private static Task<HashSet<Race>> Wait(
			Func<HashSet<Race>> read,
			Func<bool> isSameGame = null,
			int timeoutMs = 1000,
			int initialDelayMs = 1,
			CancellationToken token = default(CancellationToken)
		)
			=> BattlegroundsUtils.WaitForAvailableRaces(
				read,
				isSameGame ?? (() => true),
				TimeSpan.FromMilliseconds(timeoutMs),
				TimeSpan.FromMilliseconds(initialDelayMs),
				token
			);

		private static async Task AssertCancelled(Task task)
		{
			try
			{
				await task;
			}
			catch(OperationCanceledException)
			{
				return;
			}
			Assert.Fail("Expected the wait to be cancelled");
		}

		[TestMethod]
		public async Task WaitForAvailableRaces_RetriesUntilTheReadSucceeds()
		{
			var reads = 0;

			var races = await Wait(() => ++reads < 4 ? null : Races);

			Assert.AreSame(Races, races);
			Assert.AreEqual(4, reads);
		}

		[TestMethod]
		public async Task WaitForAvailableRaces_GivesUpAfterTimeout()
		{
			var races = await Wait(() => null, timeoutMs: 50);

			Assert.IsNull(races);
		}

		[TestMethod]
		public async Task WaitForAvailableRaces_IsCancelledWhenTheGameChanges()
		{
			var reads = 0;

			await AssertCancelled(Wait(() =>
			{
				reads++;
				return null;
			}, () => reads < 3, timeoutMs: 60_000));

			Assert.AreEqual(3, reads);
		}

		[TestMethod]
		public async Task WaitForAvailableRaces_IsCancelledWithoutWaitingForTheNextRead()
		{
			var cancellation = new CancellationTokenSource();
			var stopwatch = Stopwatch.StartNew();

			var wait = Wait(() => null, timeoutMs: 60_000, initialDelayMs: 60_000, token: cancellation.Token);
			cancellation.CancelAfter(20);
			await AssertCancelled(wait);

			Assert.IsTrue(stopwatch.Elapsed < TimeSpan.FromSeconds(5));
		}

		[TestMethod]
		public void SharedRacesWait_ConcurrentCallersShareOneWaitUntilItFinishes()
		{
			var gameId = Guid.NewGuid();
			var starts = 0;
			var pending = new TaskCompletionSource<HashSet<Race>>();
			Func<Task<HashSet<Race>>> start = () =>
			{
				starts++;
				return pending.Task;
			};

			var first = BattlegroundsUtils.GetOrStartSharedRacesWait(gameId, start);
			var second = BattlegroundsUtils.GetOrStartSharedRacesWait(gameId, start);

			Assert.AreSame(first, second);
			Assert.AreEqual(1, starts);

			pending.SetResult(null);
			pending = new TaskCompletionSource<HashSet<Race>>();
			var retry = BattlegroundsUtils.GetOrStartSharedRacesWait(gameId, start);

			Assert.AreNotSame(first, retry);
			Assert.AreEqual(2, starts);
		}

		[TestMethod]
		public void SharedRacesWait_NewGameStartsItsOwnWait()
		{
			var starts = 0;
			Func<Task<HashSet<Race>>> start = () =>
			{
				starts++;
				return new TaskCompletionSource<HashSet<Race>>().Task;
			};

			var previousGame = BattlegroundsUtils.GetOrStartSharedRacesWait(Guid.NewGuid(), start);
			var currentGame = BattlegroundsUtils.GetOrStartSharedRacesWait(Guid.NewGuid(), start);

			Assert.AreNotSame(previousGame, currentGame);
			Assert.AreEqual(2, starts);
		}
	}
}
