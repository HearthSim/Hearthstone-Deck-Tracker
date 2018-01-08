using System;
using System.Linq;
using System.Threading;
using HearthWatcher.LogReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthWatcher.Test
{
	[TestClass]
	public class LogLineTests
	{
		[TestMethod]
		public void TimeStampTest()
		{
			var lines = new[]
			{
				"D 00:06:10.0000000 GameState",
				"D 00:06:10.0010000 GameState",
				"D 00:06:10 GameState.DebugPrintPower() -     tag=ZONE value=PLAY"
			}.Select(x => new LogLine("Power", x)).ToArray();
			Assert.AreEqual(lines[0].Time, lines[2].Time);
			Assert.IsTrue(lines[1].Time > lines[2].Time);
		}

		[TestMethod]
		public void LineContentTest()
		{
			const string line = "D 00:06:10.0010000 GameState.DebugPrintPower() -     tag=ZONE value=PLAY";
			var lineItem = new LogLine("Power", line);
			Assert.AreEqual("D " + lineItem.Time.ToString("HH:mm:ss.fffffff") + " " + lineItem.LineContent, line);
		}

		[TestMethod]
		public void DayRolloverTest()
		{
			if(DateTime.Now.AddSeconds(5).Date > DateTime.Now.Date)
				Thread.Sleep(5000);
			const string line = "D 23:59:59.9999999 GameState.DebugPrintPower() -     tag=ZONE value=PLAY";
			var lineItem = new LogLine("Power", line);
			Assert.AreEqual(lineItem.Time.Date, DateTime.Now.Date.AddDays(-1));
		}
	}
}
