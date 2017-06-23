#region

using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace HDTTests.Hearthstone
{
	[TestClass]
	public class GameTimeTest
	{
		private readonly GameTime _gameTime = new GameTime();
		private WaitForDurationHelper _helper;

		[TestInitialize]
		public void TestInitialize()
		{
			_gameTime.Time = DateTime.MinValue;
			_helper = new WaitForDurationHelper(_gameTime);
		}

		[TestMethod]
		public void WaitForDuration_Delay()
		{
			var value1 = false;
			var value2 = false;
			_helper.New(100, () => value1 = true);
			_helper.New(200, () => value2 = true);
			Assert.IsFalse(value1);
			Assert.IsFalse(value2);
			_gameTime.Time += TimeSpan.FromMilliseconds(50);
			Assert.IsFalse(value1);
			Assert.IsFalse(value2);
			_gameTime.Time += TimeSpan.FromMilliseconds(100);
			Assert.IsTrue(value1);
			Assert.IsFalse(value2);
			_gameTime.Time += TimeSpan.FromMilliseconds(100);
			Assert.IsTrue(value1);
			Assert.IsTrue(value2);
		}

		[TestMethod]
		public void WaitForDuration_OrderOfExecution()
		{
			var values = new List<int>();
			_helper.New(300, () => values.Add(3));
			_helper.New(200, () => values.Add(2));
			_helper.New(100, () => values.Add(1));
			Assert.IsFalse(values.Any());
			_gameTime.Time += TimeSpan.FromMilliseconds(50);
			Assert.IsFalse(values.Any());
			_gameTime.Time += TimeSpan.FromMilliseconds(500);
			Assert.AreEqual(3, values.Count);
			Assert.AreEqual(1, values[0]);
			Assert.AreEqual(2, values[1]);
			Assert.AreEqual(3, values[2]);
		}

		public class WaitForDurationHelper
		{
			private readonly GameTime _gameTime;

			public WaitForDurationHelper(GameTime gameTime)
			{
				_gameTime = gameTime;
			}

			public async void New(int ms, Action action)
			{
				await _gameTime.WaitForDuration(ms);
				action.Invoke();
			}
		}
	}
}
