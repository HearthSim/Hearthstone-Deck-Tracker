using System;
using Hearthstone_Deck_Tracker.LogReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Hearthstone
{
	[TestClass]
	public class LogReadingTest
	{
		private const string BlockStartSubOption = "D 21:36:06.7807683 GameState.DebugPrintPower() - BLOCK_START BlockType=PLAY Entity=[name=I Know a Guy id=59 zone=HAND zonePos=2 cardId=CFM_940 player=2] EffectCardId= EffectIndex=0 Target=0 SubOption=0";
		private const string BlockStartTriggerKeyword = "D 22:09:45.3580003 GameState.DebugPrintPower() - BLOCK_START BlockType=TRIGGER Entity=[name=UNKNOWN ENTITY [cardType=INVALID] id=79 zone=SETASIDE zonePos=0 cardId= player=1] EffectCardId= EffectIndex=-1 Target=0 SubOption=-1 TriggerKeyword=TAG_ONE_TURN_EFFECT";
		private const string BlockStartTargetEntity = "D 22:10:02.7179900 GameState.DebugPrintPower() - BLOCK_START BlockType=PLAY Entity=[name=player name id=65 zone=PLAY zonePos=0 cardId=CS2_034 player=1] EffectCardId= EffectIndex=0 Target=[name=[player name id=27 zone=PLAY zonePos=0 cardId=EX1_096 player=1] SubOption=0";
		private const string TagChangeDefChange = "D 22:05:13.2737190 GameState.DebugPrintPower() -     TAG_CHANGE Entity=GameEntity tag=NEXT_STEP value=MAIN_READY";
		private const string TagChangeDefChange2 = "D 00:05:53.4164397 GameState.DebugPrintPower() - TAG_CHANGE Entity=90 tag=MODULAR_ENTITY_PART_1 value=41622 DEF CHANGE";

		[TestMethod]
		public void TestBlockStartSubOption()
		{
			//TODO: Fix suboption/triggerKeyword matching - suboption is not used currently
			var subOptionMatch = LogConstants.PowerTaskList.BlockStartRegex.Match(BlockStartSubOption);
			Assert.IsTrue(subOptionMatch.Success);
			Assert.AreEqual("PLAY", subOptionMatch.Groups["type"].Value);
			Assert.AreEqual("59", subOptionMatch.Groups["id"].Value);
			Assert.AreEqual("CFM_940", subOptionMatch.Groups["Id"].Value);
			Assert.AreEqual("0 ", subOptionMatch.Groups["target"].Value);
			Assert.AreEqual("0", subOptionMatch.Groups["subOption"].Value);

			var triggerKeywordMatch = LogConstants.PowerTaskList.BlockStartRegex.Match(BlockStartTriggerKeyword);
			Assert.IsTrue(triggerKeywordMatch.Success);
			Assert.AreEqual("TRIGGER", triggerKeywordMatch.Groups["type"].Value);
			Assert.AreEqual("79", triggerKeywordMatch.Groups["id"].Value);
			Assert.AreEqual(string.Empty, triggerKeywordMatch.Groups["Id"].Value);
			Assert.AreEqual("0 ", triggerKeywordMatch.Groups["target"].Value);
			Assert.AreEqual("-1 TriggerKeyword=TAG_ONE_TURN_EFFECT", triggerKeywordMatch.Groups["subOption"].Value);

			var triggerTargetEntity = LogConstants.PowerTaskList.BlockStartRegex.Match(BlockStartTargetEntity);
			Assert.IsTrue(triggerTargetEntity.Success);
			Assert.AreEqual("PLAY", triggerTargetEntity.Groups["type"].Value);
			Assert.AreEqual("65", triggerTargetEntity.Groups["id"].Value);
			Assert.AreEqual("CS2_034", triggerTargetEntity.Groups["Id"].Value);
			Assert.AreEqual("[name=[player name id=27 zone=PLAY zonePos=0 cardId=EX1_096 player=1] ", triggerTargetEntity.Groups["target"].Value);
			Assert.AreEqual("0", triggerTargetEntity.Groups["subOption"].Value);
		}

		[TestMethod]
		public void TestTagChange()
		{
			var match = LogConstants.PowerTaskList.TagChangeRegex.Match(TagChangeDefChange);
			Assert.IsTrue(match.Success);
			Assert.AreEqual("GameEntity", match.Groups["entity"].Value);
			Assert.AreEqual("NEXT_STEP", match.Groups["tag"].Value);
			Assert.AreEqual("MAIN_READY", match.Groups["value"].Value);

			var match2 = LogConstants.PowerTaskList.TagChangeRegex.Match(TagChangeDefChange2);
			Assert.IsTrue(match2.Success);
			Assert.AreEqual("90", match2.Groups["entity"].Value);
			Assert.AreEqual("MODULAR_ENTITY_PART_1", match2.Groups["tag"].Value);
			Assert.AreEqual("41622", match2.Groups["value"].Value);
		}
	}
}
