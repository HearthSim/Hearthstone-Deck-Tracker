using HearthWatcher.LogReader;
using Hearthstone_Deck_Tracker.LogReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.LogReader
{
	[TestClass]
	public class PowerLogFilterTests
	{
		private static bool Kept(string line)
		{
			var info = LogWatcherManager.PowerLogWatcherInfo;
			return info.Matches(new LogLine(info.Name, line).LineContent);
		}

		[TestMethod]
		public void KeepsTheLineShowingChoicesToThePlayer()
		{
			Assert.IsTrue(Kept("D 15:32:54.1304872 ChoiceCardMgr.WaitThenShowChoices() - id=2 BEGIN"));
		}

		[TestMethod]
		public void KeepsEntityChoices()
		{
			Assert.IsTrue(Kept("D 15:32:54.1304872 GameState.DebugPrintEntityChoices() - id=2 Player=BehEh#1355 TaskList= ChoiceType=GENERAL CountMin=1 CountMax=1"));
			Assert.IsTrue(Kept("D 15:32:54.0531008 PowerProcessor.EndCurrentTaskList() - m_currentTaskList=1297"));
		}

		[TestMethod]
		public void DropsUnrelatedPowerLines()
		{
			Assert.IsFalse(Kept("D 15:33:21.1187747 PowerProcessor.DoTaskListForCard() - unhandled BlockType PLAY for sourceEntity [entityName=Might of Stormwind id=1921 zone=GRAVEYARD zonePos=0 cardId=BG35_951 player=1]"));
			Assert.IsFalse(Kept("D 15:32:54.0475677 PowerTaskList.DebugDump() - ID=1297 ParentID=0 PreviousID=1295 TaskCount=3"));
		}
	}
}
