using Hearthstone_Deck_Tracker.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Utility
{
	[TestClass]
	public class KeyedPoolTests
	{
		private class Item : IPoolItem
		{
			public int ReuseCount;
			public int ReturnCount;
			public void OnReuseFromPool() => ReuseCount++;
			public void OnReturnToPool() => ReturnCount++;
		}

		[TestMethod]
		public void GetOrCreate_SameKey_ReturnsInstanceLastReturnedUnderThatKey()
		{
			var pool = new KeyedPool<Item>(10);
			var a = pool.GetOrCreate("a");
			var b = pool.GetOrCreate("b");
			pool.Return("a", a);
			pool.Return("b", b);
			Assert.AreSame(a, pool.GetOrCreate("a"));
			Assert.AreSame(b, pool.GetOrCreate("b"));
		}

		[TestMethod]
		public void GetOrCreate_UnknownKey_FallsBackToAnyPooledInstance()
		{
			var pool = new KeyedPool<Item>(10);
			var item = pool.GetOrCreate("a");
			pool.Return("a", item);
			Assert.AreSame(item, pool.GetOrCreate("b"));
		}

		[TestMethod]
		public void GetOrCreate_EmptyPool_CreatesNewInstance()
		{
			var pool = new KeyedPool<Item>(10);
			var item = pool.GetOrCreate("a");
			Assert.AreEqual(0, item.ReuseCount);
			Assert.AreEqual(0, item.ReturnCount);
		}

		[TestMethod]
		public void Return_OverCapacity_DropsInstanceInsteadOfPooling()
		{
			var pool = new KeyedPool<Item>(1);
			var a = pool.GetOrCreate("a");
			var b = pool.GetOrCreate("b");
			pool.Return("a", a);
			pool.Return("b", b);
			Assert.AreEqual(0, b.ReturnCount);
			Assert.AreSame(a, pool.GetOrCreate("b"));
		}

		[TestMethod]
		public void PoolCallbacks_FireOnReturnAndReuse()
		{
			var pool = new KeyedPool<Item>(10);
			var item = pool.GetOrCreate("a");
			pool.Return("a", item);
			Assert.AreEqual(1, item.ReturnCount);
			pool.GetOrCreate("a");
			Assert.AreEqual(1, item.ReuseCount);
		}

		[TestMethod]
		public void SameKeyPooledTwice_ReturnsBothInstances()
		{
			var pool = new KeyedPool<Item>(10);
			var first = pool.GetOrCreate("a");
			var second = pool.GetOrCreate("a");
			pool.Return("a", first);
			pool.Return("a", second);
			var got = new[] { pool.GetOrCreate("a"), pool.GetOrCreate("a") };
			CollectionAssert.AreEquivalent(new[] { first, second }, got);
		}
	}
}
