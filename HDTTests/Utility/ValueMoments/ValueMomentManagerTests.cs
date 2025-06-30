using System;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Utility.ValueMoments;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;
using NuGet;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Mercenaries_Deck_Tracker.Utility.ValueMoments.Actions;

namespace HDTTests.Utility.ValueMoments
{
	[TestClass]
	public class ValueMomentManagerTests
	{

		[TestInitialize]
		public void TestInitialize()
		{
			Config.Instance.ResetAll();
		}

		[TestMethod]
		public void GetValueMoments_ReturnsCopyDeckValueMoment()
		{
			var action = new CopyDeckAction(Franchise.HSConstructed, CopyDeckAction.Action.CopyAll);
			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.CopyDeck
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsShareDeckValueMoment()
		{
			var action = new ClickAction(Franchise.HSConstructed, ClickAction.Action.ScreenshotCopyToClipboard);
			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.ShareDeck
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);

			action = new ClickAction(Franchise.HSConstructed, ClickAction.Action.ScreenshotSaveToDisk);
			valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.ShareDeck
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);

			action = new ClickAction(Franchise.HSConstructed, ClickAction.Action.ScreenshotUploadToImgur);
			valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.ShareDeck
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsPersonalStatsValueMoment()
		{
			var action = new ClickAction(Franchise.HSConstructed, ClickAction.Action.StatsArena);
			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.PersonalStats
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);

			action = new ClickAction(Franchise.HSConstructed, ClickAction.Action.StatsConstructed);
			valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.PersonalStats
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsDecklistVisibleValueMoment()
		{
			var action = new EndMatchHearthstoneAction(123, "foo", GameResult.Win, GameMode.Practice, GameType.GT_VS_AI, 1, new GameMetrics());
			Assert.IsFalse(action.GeneralSettings.OverlayHideCompletely);

			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.HSDecklistVisible
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsDecklistVisibleValueMomentSpectate()
		{
			var action = new EndSpectateMatchHearthstoneAction(123, "foo", GameResult.Win, GameMode.Practice, GameType.GT_VS_AI, 1, new GameMetrics());
			Assert.IsFalse(action.GeneralSettings.OverlayHideCompletely);

			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.HSDecklistVisible
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsBGBobsBuddyValueMoment()
		{
			var action = new EndMatchBattlegroundsAction(123, "foo", 1, 2, GameType.GT_BATTLEGROUNDS, 5000, new GameMetrics());

			Assert.IsTrue(action.BattlegroundsSettings.BobsBuddyCombatSimulations);
			Assert.IsTrue(action.BattlegroundsSettings.BobsBuddyResultsDuringCombat);

			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.BGBobsBuddy
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);

			Config.Instance.ShowBobsBuddyDuringCombat = false;
			Config.Instance.ShowBobsBuddyDuringShopping = true;

			action = new EndMatchBattlegroundsAction(123, "foo", 1, 2, GameType.GT_BATTLEGROUNDS, 5000, new GameMetrics());

			Assert.IsTrue(action.BattlegroundsSettings.BobsBuddyCombatSimulations);
			Assert.IsTrue(action.BattlegroundsSettings.BobsBuddyResultsDuringShopping);

			valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.BGBobsBuddy
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsBGSessionRecapValueMoment()
		{
			var action = new EndMatchBattlegroundsAction(123, "foo", 1, 2, GameType.GT_BATTLEGROUNDS, 5000, new GameMetrics());

			Assert.IsTrue(action.BattlegroundsSettings.SessionRecap);

			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.BGSessionRecap
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);

			action = new EndMatchBattlegroundsAction(123, "foo", 1, 2, GameType.GT_BATTLEGROUNDS, 5000, new GameMetrics());

			Assert.IsTrue(action.BattlegroundsSettings.SessionRecapBetweenGames);

			valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.BGSessionRecap
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsBGMinionBrowserValueMoment()
		{
			var gameMetrics = new GameMetrics();
			gameMetrics.IncrementBattlegroundsMinionsTiersClick();
			var action = new EndMatchBattlegroundsAction(123, "foo", 1, 2, GameType.GT_BATTLEGROUNDS, 5000, gameMetrics);

			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.BGMinionBrowser
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsBGMinionBrowserValueMomentSpectate()
		{
			var gameMetrics = new GameMetrics();
			gameMetrics.IncrementBattlegroundsMinionsTiersClick();
			var action = new EndSpectateMatchBattlegroundsAction(123, "foo", 1, 2, GameType.GT_BATTLEGROUNDS, 5000, gameMetrics);

			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.BGMinionBrowser
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsMercOpponentAbilitiesValueMoment()
		{
			var gameMetrics = new GameMetrics();
			gameMetrics.IncrementMercenariesHoversOpponentMercToShowAbility();
			var action = new EndMatchMercenariesAction(GameResult.Win, GameType.GT_VS_AI, gameMetrics);

			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.MercOpponentAbilities
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsMercFriendlyTasksValueMoment()
		{
			var gameMetrics = new GameMetrics();
			gameMetrics.IncrementMercenariesTaskHoverDuringMatch();
			var action = new EndMatchMercenariesAction(GameResult.Win, GameType.GT_VS_AI, gameMetrics);

			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.MercFriendlyTasks
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsMercFriendlyTasksValueMomentSpectate()
		{
			var gameMetrics = new GameMetrics();
			gameMetrics.IncrementMercenariesTaskHoverDuringMatch();
			var action = new EndSpectateMatchMercenariesAction(GameResult.Win, GameType.GT_VS_AI, gameMetrics);

			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.MercFriendlyTasks
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void ShouldSendEventToMixPanel_ReturnsTrueForActionsWithoutMaxOccurrences()
		{
			var action = new ToastAction(Franchise.HSConstructed, ToastAction.Toast.Mulligan);
			DailyEventsCount.Instance.SetEventDailyCount(action.Id, 10000);
			// Recreate action to update daily occurrences
			action = new ToastAction(Franchise.HSConstructed, ToastAction.Toast.Mulligan);

			Assert.IsTrue(ValueMomentManager.ShouldSendEventToMixPanel(action, new List<ValueMoment>()));
		}

		[TestMethod]
		public void ShouldSendEventToMixPanel_ReturnsTrueForActionWithFewDailyOccurrences()
		{
			var action = new CopyDeckAction(Franchise.HSConstructed, CopyDeckAction.Action.CopyAll);
			DailyEventsCount.Instance.Clear(action.Id);
			// Recreate action to update daily occurrences
			action = new CopyDeckAction(Franchise.HSConstructed, CopyDeckAction.Action.CopyAll);

			Assert.IsTrue(ValueMomentManager.ShouldSendEventToMixPanel(action, new List<ValueMoment>()));
		}

		[TestMethod]
		public void ShouldSendEventToMixPanel_ReturnsFalseForActionWithExceededDailyOccurrences()
		{
			var action = new CopyDeckAction(Franchise.HSConstructed, CopyDeckAction.Action.CopyAll);
			DailyEventsCount.Instance.SetEventDailyCount(action.Id, 10);
			// Recreate action to update daily occurrences
			action = new CopyDeckAction(Franchise.HSConstructed, CopyDeckAction.Action.CopyAll);

			Assert.IsFalse(ValueMomentManager.ShouldSendEventToMixPanel(action, new List<ValueMoment>()));
		}

		[TestMethod]
		public void ShouldSendEventToMixPanel_ReturnsTrueForForValueMomentWithFewDailyOccurrences()
		{
			var action = new CopyDeckAction(Franchise.HSConstructed, CopyDeckAction.Action.CopyAll);
			DailyEventsCount.Instance.SetEventDailyCount(action.Id, 0);
			// Recreate action to update daily occurrences
			action = new CopyDeckAction(Franchise.HSConstructed, CopyDeckAction.Action.CopyAll);

			var valueMoments = new List<ValueMoment> { new ValueMoment("Foo", ValueMoment.VMKind.Free, 1) };
			DailyEventsCount.Instance.Clear("Foo");

			Assert.IsTrue(ValueMomentManager.ShouldSendEventToMixPanel(action, valueMoments));
		}

		[TestMethod]
		public void ShouldSendEventToMixPanel_ReturnsFalseForForValueMomentWithExceededDailyOccurrences()
		{
			var action = new CopyDeckAction(Franchise.HSConstructed, CopyDeckAction.Action.CopyAll);
			DailyEventsCount.Instance.SetEventDailyCount(action.Id, 10);
			// Recreate action to update daily occurrences
			action = new CopyDeckAction(Franchise.HSConstructed, CopyDeckAction.Action.CopyAll);

			var valueMoments = new List<ValueMoment> { new ValueMoment("Foo", ValueMoment.VMKind.Free, 1) };
			DailyEventsCount.Instance.SetEventDailyCount("Foo", 2);

			Assert.IsFalse(ValueMomentManager.ShouldSendEventToMixPanel(action, valueMoments));
		}
	}
}
