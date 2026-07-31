using System.Collections.Generic;
using System.Linq;
using HearthWatcher.LogReader;
using Hearthstone_Deck_Tracker.LogReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.LogReader
{
	[TestClass]
	public class RewoundEntityCreationFilterTests
	{
		private RewoundEntityCreationFilter _filter;

		[TestInitialize]
		public void Setup() => _filter = new RewoundEntityCreationFilter();

		private static LogLine Power(string content) =>
			new LogLine("Power", "D 14:04:05.1234567 " + content);

		private List<string> Kept(params string[] contents) =>
			contents.Where(x => _filter.KeepInPowerLog(Power(x))).ToList();

		[TestMethod]
		public void KeepsEntityCreationWithItsTags()
		{
			var kept = Kept(
				"GameState.DebugPrintPower() -         FULL_ENTITY - Creating ID=221 CardID=",
				"GameState.DebugPrintPower() -             tag=ZONE value=HAND",
				"GameState.DebugPrintPower() -             tag=CONTROLLER value=2",
				"GameState.DebugPrintPower() -             tag=ENTITY_ID value=221",
				"GameState.DebugPrintPower() -             tag=ZONE_POSITION value=7"
			);

			Assert.AreEqual(5, kept.Count);
		}

		[TestMethod]
		public void DropsTheRewoundPlayBlock()
		{
			var kept = Kept(
				"GameState.DebugPrintPower() - BLOCK_START BlockType=PLAY Entity=[id=122 zone=HAND cardId= player=2] EffectIndex=0 Target=0 SubOption=-1",
				"GameState.DebugPrintPower() -     TAG_CHANGE Entity=McBanterFace#1422 tag=RESOURCES_USED value=4",
				"GameState.DebugPrintPower() - BLOCK_END"
			);

			Assert.AreEqual(0, kept.Count);
		}

		[TestMethod]
		public void DropsTagsThatDoNotBelongToACreation()
		{
			var kept = Kept(
				"GameState.DebugPrintPower() -     SHOW_ENTITY - Updating Entity=218 CardID=MEND_504e",
				"GameState.DebugPrintPower() -         tag=CARDTYPE value=ENCHANTMENT"
			);

			Assert.AreEqual(0, kept.Count);
		}

		[TestMethod]
		public void StopsKeepingTagsOnceTheCreationEnded()
		{
			var kept = Kept(
				"GameState.DebugPrintPower() -     FULL_ENTITY - Creating ID=221 CardID=",
				"GameState.DebugPrintPower() -         tag=ZONE value=HAND",
				"GameState.DebugPrintPower() -     TAG_CHANGE Entity=221 tag=ZONE_POSITION value=7",
				"GameState.DebugPrintPower() -         tag=CARDTYPE value=MINION"
			);

			Assert.AreEqual(2, kept.Count);
			CollectionAssert.AreEqual(
				new[]
				{
					"GameState.DebugPrintPower() -     FULL_ENTITY - Creating ID=221 CardID=",
					"GameState.DebugPrintPower() -         tag=ZONE value=HAND"
				},
				kept
			);
		}

		[TestMethod]
		public void DropsTagsIndentedAtOrAboveTheCreation()
		{
			var kept = Kept(
				"GameState.DebugPrintPower() -         FULL_ENTITY - Creating ID=221 CardID=",
				"GameState.DebugPrintPower() -         tag=ZONE value=HAND",
				"GameState.DebugPrintPower() -     tag=CONTROLLER value=2"
			);

			Assert.AreEqual(1, kept.Count);
		}

		[TestMethod]
		public void KeepsConsecutiveCreations()
		{
			var kept = Kept(
				"GameState.DebugPrintPower() -     FULL_ENTITY - Creating ID=221 CardID=",
				"GameState.DebugPrintPower() -         tag=ZONE value=HAND",
				"GameState.DebugPrintPower() -     FULL_ENTITY - Creating ID=222 CardID=",
				"GameState.DebugPrintPower() -         tag=ZONE value=PLAY"
			);

			Assert.AreEqual(4, kept.Count);
		}

		[TestMethod]
		public void DropsNonGameStateAndNonPowerLines()
		{
			Assert.IsFalse(_filter.KeepInPowerLog(
				Power("PowerTaskList.DebugPrintPower() -     FULL_ENTITY - Creating ID=221 CardID=")));
			Assert.IsFalse(_filter.KeepInPowerLog(
				new LogLine("LoadingScreen", "D 14:04:05.1234567 LoadingScreen.OnSceneLoaded() - prevMode=GAMEPLAY")));
		}

		[TestMethod]
		public void ResetForgetsAnOpenCreation()
		{
			Assert.IsTrue(_filter.KeepInPowerLog(
				Power("GameState.DebugPrintPower() -     FULL_ENTITY - Creating ID=221 CardID=")));

			_filter.Reset();

			Assert.IsFalse(_filter.KeepInPowerLog(
				Power("GameState.DebugPrintPower() -         tag=ZONE value=HAND")));
		}
	}
}
