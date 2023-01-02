using Hearthstone_Deck_Tracker.Utility.ValueMoments;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using NuGet;

namespace HDTTests.Utility.ValueMoments
{
	[TestClass]
	public class ValueMomentManagerTests
	{
		[TestInitialize]
		public void TestInitialize()
		{
			DailyEventsCount.Instance.Clear("Foo");
		}

		[TestMethod]
		public void GetValueMoments_ReturnsCopyDeckValueMoment()
		{
			var action = new VMActions.CopyDeckAction(new Dictionary<string, object>());
			var valueMoment = ValueMomentManager.GetValueMoments(action).First();
			Assert.IsTrue(valueMoment.Name == ValueMoment.VMName.CopyDeck);
			Assert.IsTrue(valueMoment.IsFree);
		}
		
		[TestMethod]
		public void GetValueMoments_ReturnsShareDeckValueMoment()
		{
			var action = new VMActions.ClickAction(new Dictionary<string, object>
			{
				{ "action_name", VMActions.ClickAction.ActionName.ScreenshotCopyToClipboard }
			});
			var valueMoment = ValueMomentManager.GetValueMoments(action).First();
			Assert.IsTrue(valueMoment.Name == ValueMoment.VMName.ShareDeck);
			Assert.IsTrue(valueMoment.IsFree);

			action = new VMActions.ClickAction(new Dictionary<string, object>
			{
				{ "action_name", VMActions.ClickAction.ActionName.ScreenshotSaveToDisk }
			});
			valueMoment = ValueMomentManager.GetValueMoments(action).First();
			Assert.IsTrue(valueMoment.Name == ValueMoment.VMName.ShareDeck);
			Assert.IsTrue(valueMoment.IsFree);

			action = new VMActions.ClickAction(new Dictionary<string, object>
			{
				{ "action_name", VMActions.ClickAction.ActionName.ScreenshotUploadToImgur }
			});
			valueMoment = ValueMomentManager.GetValueMoments(action).First();
			Assert.IsTrue(valueMoment.Name == ValueMoment.VMName.ShareDeck);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsPersonalStatsValueMoment()
		{
			var action = new VMActions.ClickAction(new Dictionary<string, object>
			{
				{ "action_name", VMActions.ClickAction.ActionName.StatsArena }
			});
			var valueMoment = ValueMomentManager.GetValueMoments(action).First();
			Assert.IsTrue(valueMoment.Name == ValueMoment.VMName.PersonalStats);
			Assert.IsTrue(valueMoment.IsFree);

			action = new VMActions.ClickAction(new Dictionary<string, object>
			{
				{ "action_name", VMActions.ClickAction.ActionName.StatsConstructed }
			});
			valueMoment = ValueMomentManager.GetValueMoments(action).First();
			Assert.IsTrue(valueMoment.Name == ValueMoment.VMName.PersonalStats);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsDecklistVisibleValueMoment()
		{
			var action = new VMActions.EndMatchAction(new Dictionary<string, object>
			{
				{ "franchise", new [] { Franchise.HSConstructed } },
				{ "hdt_general_settings_enabled", new string[] { } },
				{ "hdt_general_settings_disabled", new [] { ValueMomentUtils.OVERLAY_HIDE_COMPLETELY } }
			});
			var valueMoment = ValueMomentManager.GetValueMoments(action).First();
			Assert.IsTrue(valueMoment.Name == ValueMoment.VMName.DecklistVisible);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsDecklistVisibleValueMomentSpectate()
		{
			var action = new VMActions.EndSpectateMatchAction(new Dictionary<string, object>
			{
				{ "franchise", new [] { Franchise.HSConstructed } },
				{ "hdt_general_settings_enabled", new string[] { } },
				{ "hdt_general_settings_disabled", new [] { ValueMomentUtils.OVERLAY_HIDE_COMPLETELY } }
			});
			var valueMoment = ValueMomentManager.GetValueMoments(action).First();
			Assert.IsTrue(valueMoment.Name == ValueMoment.VMName.DecklistVisible);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsBGBobsBuddyValueMoment()
		{
			var action = new VMActions.EndMatchAction(new Dictionary<string, object>
			{
				{ "franchise", new [] { Franchise.Battlegrounds } },
				{ "hdt_battlegrounds_settings_enabled", new []
					{
						ValueMomentUtils.BB_COMBAT_SIMULATIONS,
						ValueMomentUtils.BB_RESULTS_DURING_COMBAT,
					}
				}
			});
			var valueMoment = ValueMomentManager.GetValueMoments(action).First();
			Assert.IsTrue(valueMoment.Name == ValueMoment.VMName.BGBobsBuddy);
			Assert.IsTrue(valueMoment.IsFree);

			action = new VMActions.EndMatchAction(new Dictionary<string, object>
			{
				{ "franchise", new [] { Franchise.Battlegrounds } },
				{ "hdt_battlegrounds_settings_enabled", new []
					{
						ValueMomentUtils.BB_COMBAT_SIMULATIONS,
						ValueMomentUtils.BB_RESULTS_DURING_SHOPPING,
					}
				}
			});
			valueMoment = ValueMomentManager.GetValueMoments(action).First();
			Assert.IsTrue(valueMoment.Name == ValueMoment.VMName.BGBobsBuddy);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsBGSessionRecapValueMoment()
		{
			var action = new VMActions.EndMatchAction(new Dictionary<string, object>
			{
				{ "franchise", new [] { Franchise.Battlegrounds } },
				{ "hdt_battlegrounds_settings_enabled", new [] { ValueMomentUtils.SESSION_RECAP } }
			});
			var valueMoment = ValueMomentManager.GetValueMoments(action).First();
			Assert.IsTrue(valueMoment.Name == ValueMoment.VMName.BGSessionRecap);
			Assert.IsTrue(valueMoment.IsFree);

			action = new VMActions.EndMatchAction(new Dictionary<string, object>
			{
				{ "franchise", new [] { Franchise.Battlegrounds } },
				{ "hdt_battlegrounds_settings_enabled", new [] { ValueMomentUtils.SESSION_RECAP_BETWEEN_GAMES } }
			});
			valueMoment = ValueMomentManager.GetValueMoments(action).First();
			Assert.IsTrue(valueMoment.Name == ValueMoment.VMName.BGSessionRecap);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsBGMinionBrowserValueMoment()
		{
			var action = new VMActions.EndMatchAction(new Dictionary<string, object>
			{
				{ "franchise", new [] { Franchise.Battlegrounds } },
				{ "hdt_battlegrounds_settings_enabled", new string[] {} },
				{ "num_click_battlegrounds_minion_tab", 1 }
			});
			var valueMoment = ValueMomentManager.GetValueMoments(action).First();

			Assert.IsTrue(valueMoment.Name == ValueMoment.VMName.BGMinionBrowser);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsBGMinionBrowserValueMomentSpectate()
		{
			var action = new VMActions.EndSpectateMatchAction(new Dictionary<string, object>
			{
				{ "franchise", new [] { Franchise.Battlegrounds } },
				{ "hdt_battlegrounds_settings_enabled", new string[] {} },
				{ "num_click_battlegrounds_minion_tab", 1 }
			});
			var valueMoment = ValueMomentManager.GetValueMoments(action).First();

			Assert.IsTrue(valueMoment.Name == ValueMoment.VMName.BGMinionBrowser);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsMercOpponentAbilitiesValueMoment()
		{
			var action = new VMActions.EndMatchAction(new Dictionary<string, object>
			{
				{ "franchise", new [] { Franchise.Mercenaries } },
				{ "num_hover_opponent_merc_ability", 1 }
			});
			var valueMoment = ValueMomentManager.GetValueMoments(action).First();

			Assert.IsTrue(valueMoment.Name == ValueMoment.VMName.MercOpponentAbilities);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsMercFriendlyTasksValueMoment()
		{
			var action = new VMActions.EndMatchAction(new Dictionary<string, object>
			{
				{ "franchise", new [] { Franchise.Mercenaries } },
				{ "num_hover_opponent_merc_ability", 0 },
				{ "num_hover_merc_task_overlay", 1 }
			});
			var valueMoment = ValueMomentManager.GetValueMoments(action).First();

			Assert.IsTrue(valueMoment.Name == ValueMoment.VMName.MercFriendlyTasks);
			Assert.IsTrue(valueMoment.IsFree);
		}

		[TestMethod]
		public void GetValueMoments_ReturnsMercFriendlyTasksValueMomentSpectate()
		{
			var action = new VMActions.EndSpectateMatchAction(new Dictionary<string, object>
			{
				{ "franchise", new [] { Franchise.Mercenaries } },
				{ "num_hover_opponent_merc_ability", 0 },
				{ "num_hover_merc_task_overlay", 1 }
			});
			var valueMoment = ValueMomentManager.GetValueMoments(action).First();

			Assert.IsTrue(valueMoment.Name == ValueMoment.VMName.MercFriendlyTasks);
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
			var action = new VMActions.ToastAction(new Dictionary<string, object>
			{
				{ "cur_daily_occurrences", 10000 }
			});

			Assert.IsTrue(ValueMomentManager.ShouldSendEventToMixPanel(action, new List<ValueMoment>()));
		}

		[TestMethod]
		public void ShouldSendEventToMixPanel_ReturnsTrueForActionWithFewDailyOccurrences()
		{
			var action = new VMActions.CopyDeckAction(new Dictionary<string, object>
			{
				{ "cur_daily_occurrences", 1 }
			});
			
			Assert.IsTrue(ValueMomentManager.ShouldSendEventToMixPanel(action, new List<ValueMoment>()));
		}

		[TestMethod]
		public void ShouldSendEventToMixPanel_ReturnsFalseForActionWithExceededDailyOccurrences()
		{
			var action = new VMActions.CopyDeckAction(new Dictionary<string, object>
			{
				{ "cur_daily_occurrences", 12 }
			});

			Assert.IsFalse(ValueMomentManager.ShouldSendEventToMixPanel(action, new List<ValueMoment>()));
		}

		[TestMethod]
		public void ShouldSendEventToMixPanel_ReturnsTrueForForValueMomentWithFewDailyOccurrences()
		{
			var action = new VMActions.CopyDeckAction(new Dictionary<string, object>
			{
				{ "cur_daily_occurrences", 2 },
			});
			var valueMoments = new List<ValueMoment> { new ValueMoment("Foo", ValueMoment.VMKind.Free, 1) };
			
			Assert.IsTrue(ValueMomentManager.ShouldSendEventToMixPanel(action, valueMoments));
		}

		[TestMethod]
		public void ShouldSendEventToMixPanel_ReturnsFalseForForValueMomentWithExceededDailyOccurrences()
		{
			var action = new VMActions.CopyDeckAction(new Dictionary<string, object>
			{
				{ "cur_daily_occurrences", 12 },
			});
			var valueMoments = new List<ValueMoment> { new ValueMoment("Foo", ValueMoment.VMKind.Free, 1) };
			DailyEventsCount.Instance.UpdateEventDailyCount("Foo");
			DailyEventsCount.Instance.UpdateEventDailyCount("Foo");
			
			Assert.IsFalse(ValueMomentManager.ShouldSendEventToMixPanel(action, valueMoments));
		}
	}
}
