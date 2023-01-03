using System;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Utility.ValueMoments;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using NuGet;
using static Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.VMActions;

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
			var action = new CopyDeckAction(Franchise.HSConstructed, CopyDeckAction.ActionName.CopyAll);
			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.CopyDeck
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}
		
		[TestMethod]
		public void GetValueMoments_ReturnsShareDeckValueMoment()
		{
			var action = new ClickAction(Franchise.HSConstructed, ClickAction.ActionName.ScreenshotCopyToClipboard);
			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.ShareDeck
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);

			action = new ClickAction(Franchise.HSConstructed, ClickAction.ActionName.ScreenshotSaveToDisk);
			valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.ShareDeck
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);

			action = new ClickAction(Franchise.HSConstructed, ClickAction.ActionName.ScreenshotUploadToImgur);
			valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.ShareDeck
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsPersonalStatsValueMoment()
		{
			var action = new ClickAction(Franchise.HSConstructed, ClickAction.ActionName.StatsArena);
			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.PersonalStats
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);

			action = new ClickAction(Franchise.HSConstructed, ClickAction.ActionName.StatsConstructed);
			valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.PersonalStats
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsDecklistVisibleValueMoment()
		{
			var action = EndMatchAction.Create(new Dictionary<HearthstoneExtraData, object>());
			var hdtGeneralSettingsDisabled = action.ClientProperties.HDTGeneralSettingsDisabled;
			Assert.IsTrue(hdtGeneralSettingsDisabled.Contains(HDTGeneralSettings.OverlayHideCompletely));

			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.DecklistVisible
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsDecklistVisibleValueMomentSpectate()
		{
			var action = EndSpectateMatchAction.Create(new Dictionary<HearthstoneExtraData, object>());
			var hdtGeneralSettingsDisabled = action.ClientProperties.HDTGeneralSettingsDisabled;
			Assert.IsTrue(hdtGeneralSettingsDisabled.Contains(HDTGeneralSettings.OverlayHideCompletely));

			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.DecklistVisible
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsBGBobsBuddyValueMoment()
		{
			var action = EndMatchAction.Create(new Dictionary<BattlegroundsExtraData, object>());
			var bgsSettingsEnabled = action.FranchiseProperties?.BattlegroundsSettingsEnabled;
			if (bgsSettingsEnabled == null)
				throw new Exception();

			Assert.IsTrue(bgsSettingsEnabled.Contains(BattlegroundsSettings.BobsBuddyCombatSimulations));
			Assert.IsTrue(bgsSettingsEnabled.Contains(BattlegroundsSettings.BobsBuddyResultsDuringCombat));

			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.BGBobsBuddy
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);

			Config.Instance.ShowBobsBuddyDuringCombat = false;
			Config.Instance.ShowBobsBuddyDuringShopping = true;

			action = EndMatchAction.Create(new Dictionary<BattlegroundsExtraData, object>());
			bgsSettingsEnabled = action.FranchiseProperties?.BattlegroundsSettingsEnabled;
			if(bgsSettingsEnabled == null)
				throw new Exception();

			Assert.IsTrue(bgsSettingsEnabled.Contains(BattlegroundsSettings.BobsBuddyCombatSimulations));
			Assert.IsTrue(bgsSettingsEnabled.Contains(BattlegroundsSettings.BobsBuddyResultsDuringShopping));

			valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.BGBobsBuddy
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsBGSessionRecapValueMoment()
		{
			var action = EndMatchAction.Create(new Dictionary<BattlegroundsExtraData, object>());
			var bgsSettingsEnabled = action.FranchiseProperties?.BattlegroundsSettingsEnabled;
			if(bgsSettingsEnabled == null)
				throw new Exception();

			Assert.IsTrue(bgsSettingsEnabled.Contains(BattlegroundsSettings.SessionRecap));

			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.BGSessionRecap
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		
			action = EndMatchAction.Create(new Dictionary<BattlegroundsExtraData, object>());
			bgsSettingsEnabled = action.FranchiseProperties?.BattlegroundsSettingsEnabled;
			if(bgsSettingsEnabled == null)
				throw new Exception();

			Assert.IsTrue(bgsSettingsEnabled.Contains(BattlegroundsSettings.SessionRecapBetweenGames));

			valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.BGSessionRecap
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}
		
		[TestMethod]
		public void GetValueMoments_ReturnsBGMinionBrowserValueMoment()
		{
			var action = EndMatchAction.Create(new Dictionary<BattlegroundsExtraData, object>
			{
				{ BattlegroundsExtraData.NumClickBattlegroundsMinionTab, 1 }
			});
			
			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.BGMinionBrowser
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}
		
		[TestMethod]
		public void GetValueMoments_ReturnsBGMinionBrowserValueMomentSpectate()
		{
			var action = EndSpectateMatchAction.Create(new Dictionary<BattlegroundsExtraData, object>
			{
				{ BattlegroundsExtraData.NumClickBattlegroundsMinionTab, 1 }
			});

			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.BGMinionBrowser
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}
		
		[TestMethod]
		public void GetValueMoments_ReturnsMercOpponentAbilitiesValueMoment()
		{
			var action = EndMatchAction.Create(new Dictionary<MercenariesExtraData, object>
			{
				{ MercenariesExtraData.NumHoverOpponentMercAbility, 1 }
			});

			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.MercOpponentAbilities
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}
		
		[TestMethod]
		public void GetValueMoments_ReturnsMercFriendlyTasksValueMoment()
		{
			var action = EndMatchAction.Create(new Dictionary<MercenariesExtraData, object>
			{
				{ MercenariesExtraData.NumHoverOpponentMercAbility, 0 },
				{ MercenariesExtraData.NumHoverMercTaskOverlay, 1 }
			});

			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.MercFriendlyTasks
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}
		
		[TestMethod]
		public void GetValueMoments_ReturnsMercFriendlyTasksValueMomentSpectate()
		{
			var action = EndSpectateMatchAction.Create(new Dictionary<MercenariesExtraData, object>
			{
				{ MercenariesExtraData.NumHoverOpponentMercAbility, 0 },
				{ MercenariesExtraData.NumHoverMercTaskOverlay, 1 }
			});

			var valueMoment = ValueMomentManager.GetValueMoments(action).FirstOrDefault(
				vm => vm.Name == ValueMoment.VMName.MercFriendlyTasks
			);

			Assert.IsNotNull(valueMoment);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMomentsProperties_ReturnsFreeValueMoment()
		{
			var vms = new List<ValueMoment>
			{
				new ValueMoment("", ValueMoment.VMKind.Free, 1)
			};
			var result = ValueMomentManager.GetValueMomentsProperties(vms);
			Assert.IsTrue((bool)result["has_free_value_moment"]);
			Assert.IsTrue(!((List<string>)result["free_value_moments"]).IsEmpty());
		}

		[TestMethod]
		public void GetValueMomentsProperties_ReturnsPaidValueMoment()
		{
			var vms = new List<ValueMoment>
			{
				new ValueMoment("", ValueMoment.VMKind.Paid, 1)
			};
			var result = ValueMomentManager.GetValueMomentsProperties(vms);
			Assert.IsTrue((bool)result["has_paid_value_moment"]);
			Assert.IsTrue(!((List<string>)result["paid_value_moments"]).IsEmpty());
		}

		[TestMethod]
		public void GetValueMomentsProperties_ReturnsFreeAndPaidValueMoment()
		{
			var vms = new List<ValueMoment>
			{
				new ValueMoment("", ValueMoment.VMKind.Free, 1),
				new ValueMoment("", ValueMoment.VMKind.Paid, 1)
			};
			var result = ValueMomentManager.GetValueMomentsProperties(vms);
			Assert.IsTrue((bool)result["has_free_value_moment"]);
			Assert.IsTrue((bool)result["has_paid_value_moment"]);
			Assert.IsTrue(!((List<string>)result["free_value_moments"]).IsEmpty());
			Assert.IsTrue(!((List<string>)result["paid_value_moments"]).IsEmpty());
		}

		[TestMethod]
		public void ShouldSendEventToMixPanel_ReturnsTrueForActionsWithoutMaxOccurrences()
		{
			var action = new ToastAction(Franchise.HSConstructed, ToastAction.ToastName.Mulligan);
			DailyEventsCount.Instance.SetEventDailyCount(action.EventId, 10000);
			// Recreate action to update daily occurrences
			action = new ToastAction(Franchise.HSConstructed, ToastAction.ToastName.Mulligan);

			Assert.IsTrue(ValueMomentManager.ShouldSendEventToMixPanel(action, new List<ValueMoment>()));
		}

		[TestMethod]
		public void ShouldSendEventToMixPanel_ReturnsTrueForActionWithFewDailyOccurrences()
		{
			var action = new CopyDeckAction(Franchise.HSConstructed, CopyDeckAction.ActionName.CopyAll);
			DailyEventsCount.Instance.Clear(action.EventId);
			// Recreate action to update daily occurrences
			action = new CopyDeckAction(Franchise.HSConstructed, CopyDeckAction.ActionName.CopyAll);
			
			Assert.IsTrue(ValueMomentManager.ShouldSendEventToMixPanel(action, new List<ValueMoment>()));
		}

		[TestMethod]
		public void ShouldSendEventToMixPanel_ReturnsFalseForActionWithExceededDailyOccurrences()
		{
			var action = new CopyDeckAction(Franchise.HSConstructed, CopyDeckAction.ActionName.CopyAll);
			DailyEventsCount.Instance.SetEventDailyCount(action.EventId, 10);
			// Recreate action to update daily occurrences
			action = new CopyDeckAction(Franchise.HSConstructed, CopyDeckAction.ActionName.CopyAll);

			Assert.IsFalse(ValueMomentManager.ShouldSendEventToMixPanel(action, new List<ValueMoment>()));
		}

		[TestMethod]
		public void ShouldSendEventToMixPanel_ReturnsTrueForForValueMomentWithFewDailyOccurrences()
		{
			var action = new CopyDeckAction(Franchise.HSConstructed, CopyDeckAction.ActionName.CopyAll);
			DailyEventsCount.Instance.SetEventDailyCount(action.EventId, 0);
			// Recreate action to update daily occurrences
			action = new CopyDeckAction(Franchise.HSConstructed, CopyDeckAction.ActionName.CopyAll);

			var valueMoments = new List<ValueMoment> { new ValueMoment("Foo", ValueMoment.VMKind.Free, 1) };
			DailyEventsCount.Instance.Clear("Foo");

			Assert.IsTrue(ValueMomentManager.ShouldSendEventToMixPanel(action, valueMoments));
		}

		[TestMethod]
		public void ShouldSendEventToMixPanel_ReturnsFalseForForValueMomentWithExceededDailyOccurrences()
		{
			var action = new CopyDeckAction(Franchise.HSConstructed, CopyDeckAction.ActionName.CopyAll);
			DailyEventsCount.Instance.SetEventDailyCount(action.EventId, 10);
			// Recreate action to update daily occurrences
			action = new CopyDeckAction(Franchise.HSConstructed, CopyDeckAction.ActionName.CopyAll);

			var valueMoments = new List<ValueMoment> { new ValueMoment("Foo", ValueMoment.VMKind.Free, 1) };
			DailyEventsCount.Instance.SetEventDailyCount("Foo", 2);

			Assert.IsFalse(ValueMomentManager.ShouldSendEventToMixPanel(action, valueMoments));
		}
	}
}
