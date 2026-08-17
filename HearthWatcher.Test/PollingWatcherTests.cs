using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthWatcher.Test
{
	[TestClass]
	public class PollingWatcherTests
	{
		private RecordingSynchronizationContext _context;
		private TestWatcher _watcher;

		[TestInitialize]
		public void Initialize()
		{
			_context = new RecordingSynchronizationContext();
			PollingWatcher.SetEventContext(_context);
		}

		[TestCleanup]
		public void Cleanup()
		{
			_watcher?.Stop();
			PollingWatcher.SetEventContext(null);
		}

		[TestMethod]
		public async Task EventsFromLoopAreDeliveredThroughContext()
		{
			var values = new List<int>();
			_watcher = new TestWatcher(w =>
			{
				w.RaiseEvent(w.Ticks);
				return false;
			});
			_watcher.Event += values.Add;
			_watcher.Run();

			await WaitUntil(() => values.Count >= 3);
			Assert.IsTrue(_context.PostCount >= 3);
		}

		[TestMethod]
		public void EventsRaisedOnEventThreadAreDeliveredInline()
		{
			var values = new List<int>();
			_watcher = new TestWatcher(w => false);
			_watcher.Event += values.Add;

			_watcher.RaiseEvent(42);

			Assert.AreEqual(1, values.Count);
			Assert.AreEqual(42, values[0]);
			Assert.AreEqual(0, _context.PostCount);
		}

		[TestMethod]
		public async Task StopTerminatesLoop()
		{
			_watcher = new TestWatcher(w => false);
			_watcher.Run();
			await WaitUntil(() => _watcher.Ticks >= 1);

			_watcher.Stop();
			await WaitUntil(() => _watcher.LoopEnds >= 1);

			var ticks = _watcher.Ticks;
			await Task.Delay(50);
			Assert.AreEqual(ticks, _watcher.Ticks);
		}

		[TestMethod]
		public async Task StopThenRunRestartsWithoutConcurrentLoops()
		{
			_watcher = new TestWatcher(w => false);
			_watcher.Run();
			await WaitUntil(() => _watcher.Ticks >= 1);

			for(var i = 0; i < 10; i++)
			{
				_watcher.Stop();
				_watcher.Run();
			}

			_watcher.Stop();
			await WaitUntil(() => _watcher.LoopEnds >= 1);

			var ticks = _watcher.Ticks;
			_watcher.Run();
			await WaitUntil(() => _watcher.LoopStarts >= 2);
			await WaitUntil(() => _watcher.Ticks > ticks);
			Assert.IsFalse(_watcher.ObservedReentrancy);
		}

		[TestMethod]
		public async Task SelfStopStopsLoopAndRunRestartsIt()
		{
			_watcher = new TestWatcher(w => true);
			_watcher.Run();
			await WaitUntil(() => _watcher.LoopEnds >= 1);

			var ticks = _watcher.Ticks;
			await Task.Delay(50);
			Assert.AreEqual(ticks, _watcher.Ticks);

			_watcher.Run();
			await WaitUntil(() => _watcher.LoopEnds >= 2);
			Assert.IsTrue(_watcher.Ticks > ticks);
			Assert.IsTrue(_watcher.LoopStarts >= 2);
		}

		[TestMethod]
		public async Task MultipleEventsPerTickArriveInOrder()
		{
			var values = new List<int>();
			_watcher = new TestWatcher(w =>
			{
				w.RaiseEvent(w.Ticks * 2);
				w.RaiseEvent(w.Ticks * 2 + 1);
				return false;
			});
			_watcher.Event += values.Add;
			_watcher.Run();

			await WaitUntil(() => values.Count >= 6);
			_watcher.Stop();

			var snapshot = values.Take(6).ToList();
			CollectionAssert.AreEqual(snapshot.OrderBy(x => x).ToList(), snapshot);
		}

		private static async Task WaitUntil(Func<bool> condition, int timeoutMs = 5000)
		{
			var start = Environment.TickCount;
			while(!condition())
			{
				if(Environment.TickCount - start > timeoutMs)
					Assert.Fail("Timed out waiting for condition");
				await Task.Delay(10);
			}
		}

		private class TestWatcher : PollingWatcher
		{
			private readonly Func<TestWatcher, bool> _tick;
			private int _ticks;
			private int _loopStarts;
			private int _loopEnds;
			private int _inTick;

			public TestWatcher(Func<TestWatcher, bool> tick, int delay = 1) : base(delay)
			{
				_tick = tick;
			}

			public int Ticks => Volatile.Read(ref _ticks);
			public int LoopStarts => Volatile.Read(ref _loopStarts);
			public int LoopEnds => Volatile.Read(ref _loopEnds);
			public bool ObservedReentrancy { get; private set; }

			public event Action<int> Event;

			public void RaiseEvent(int value) => Dispatch(() => Event?.Invoke(value));

			protected override Task<bool> TickAsync()
			{
				if(Interlocked.Exchange(ref _inTick, 1) == 1)
					ObservedReentrancy = true;
				Interlocked.Increment(ref _ticks);
				var result = _tick(this);
				Interlocked.Exchange(ref _inTick, 0);
				return Task.FromResult(result);
			}

			protected override void OnLoopStart() => Interlocked.Increment(ref _loopStarts);
			protected override void OnLoopEnd() => Interlocked.Increment(ref _loopEnds);
		}

		private class RecordingSynchronizationContext : SynchronizationContext
		{
			private int _postCount;

			public int PostCount => Volatile.Read(ref _postCount);

			public override void Post(SendOrPostCallback d, object state)
			{
				Interlocked.Increment(ref _postCount);
				d(state);
			}
		}
	}
}
