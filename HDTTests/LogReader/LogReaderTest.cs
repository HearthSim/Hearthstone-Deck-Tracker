using System;
using Hearthstone_Deck_Tracker.LogReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.LogReader
{
	[TestClass]
	public class LogReaderTest
	{
		private Hearthstone_Deck_Tracker.LogReader.LogReader _powerLogReader;
		private Hearthstone_Deck_Tracker.LogReader.LogReader _bobLogReader;

		private LogReaderInfo PowerLogReaderInfo
		{
			get { return new LogReaderInfo { Name = "Power", StartsWithFilters = new[] { "GameState." }, ContainsFilters = new[] { "Begin Spectating", "Start Spectator", "End Spectator" }, FilePath = "LogReader/TestFiles/Test1_Power.log.txt" }; }
		}

		private LogReaderInfo BobLogReaderInfo
		{
			get { return new LogReaderInfo { Name = "Bob", FilePath = "LogReader/TestFiles/Test1_Bob.log.txt" }; }
		}
		[TestInitialize]
		public void Setup()
		{
			_powerLogReader = new Hearthstone_Deck_Tracker.LogReader.LogReader(PowerLogReaderInfo);
			_bobLogReader = new Hearthstone_Deck_Tracker.LogReader.LogReader(BobLogReaderInfo);
		}

		[TestMethod]
		public void EntryPointTest()
		{
			var powerEntryStartOnly = _powerLogReader.FindEntryPoint("GameState.DebugPrintPower() - CREATE_GAME"); //10:00
			var powerEntryStartEnd = _powerLogReader.FindEntryPoint(new[] { "GameState.DebugPrintPower() - CREATE_GAME", "tag=GOLD_REWARD_STATE" }); //10:00, 10:10
			var bobEntry = _bobLogReader.FindEntryPoint("legend rank"); //10:09

			Assert.IsTrue(powerEntryStartEnd > powerEntryStartOnly, "powerEntryStartEnd > powerEntryStartOnly");
			Assert.IsTrue(powerEntryStartEnd > bobEntry, "powerEntryStartEnd > bobEntry");
			Assert.IsTrue(bobEntry > powerEntryStartOnly, "bobEntry > powerEntryStartOnly");
		}
	}
}
