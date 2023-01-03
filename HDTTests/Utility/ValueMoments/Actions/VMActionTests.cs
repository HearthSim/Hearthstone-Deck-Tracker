using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker;
using NuGet;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;
using static Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.VMActions;
using Hearthstone_Deck_Tracker.Utility.ValueMoments;

namespace HDTTests.Utility.ValueMoments.Actions
{
	[TestClass]
	public class VMActionTests
	{
		[TestInitialize]
		public void TestInitialize()
		{
			Config.Instance.ResetAll();
		}

		[TestMethod]
		public void VMAction_ActionId()
		{
			var action = EndMatchAction.Create(new Dictionary<HearthstoneExtraData, object>());

			Assert.AreEqual("end match action hdt_hs-constructed", action.ActionId);
		}

		[TestMethod]
		public void VMAction_ActionIdWithSubFranchise()
		{
			var action = EndMatchAction.Create(
				new Dictionary<HearthstoneExtraData, object>(),
				new Dictionary<string, object>
				{
					{ ValueMomentsConstants.SubFranchiseProperty, new [] { "Arena"  } }
				}
			);

			Assert.AreEqual("end match action hdt_hs-constructed_arena", action.ActionId);
		}

		[TestMethod]
		public void VMAction_ActionIdWithAllFranchise()
		{
			var action = new InstallAction();

			Assert.AreEqual("install hdt_hs-constructed", action.ActionId);
		}

		[TestMethod]
		public void VMAction_MixpanelPayloadReturnsCorrect()
		{
			var action = EndMatchAction.Create(new Dictionary<HearthstoneExtraData, object>());
			DailyEventsCount.Instance.Clear(action.ActionId);
			// Recreate action to update daily occurrences
			action = EndMatchAction.Create(new Dictionary<HearthstoneExtraData, object>());

			var expectedDict = new Dictionary<string, object> {
				{ "action_name", "end_match" },
				{ "action_type", "End Match Action" },
				{ "action_source", "app" },
				{ "domain", "hsreplay.net" },
				{ "franchise", new [] { "HS-Constructed" } },
				{ "free_value_moments", new [] { "Overlay Decklist Visible" }},
				{ "paid_value_moments", new string[]{} },
				{ "has_free_value_moment", true },
				{ "has_paid_value_moment", false },
				{ "cur_daily_occurrences", 1 },
				{ "max_daily_occurrences", 1 },
				{ "card_language", "en" },
				{ "appearance_language", "en" },
				{ "hdt_plugins", new string[]{ } },
				{ "hdt_general_settings_enabled", new []{
					"upload_my_collection_automatically",
					"upload_replays_automatically",
					"share_notification",
					"overlay_hide_if_hs_in_background",
					"overlay_menu_hide_if_hs_in_background",
					"card_tooltips",
					"analytics_submit_anonymous_data",
					"show_news_bar"
				}},
				{ "hdt_general_settings_disabled", new []{
					"overlay_hide_completely",
					"start_with_windows",
					"start_minimized",
					"close_to_tray",
					"minimize_to_tray"
				}},
				{ "hdt_hsconstructed_settings_enabled", new []{ "hide_timers" }},
				{ "hdt_hsconstructed_settings_disabled", new []{ "hide_decks" }}
			};

			var mixpanelPayload = action.MixpanelPayload;
			// Remove some properties to avoid issues when running tests on CI
			mixpanelPayload.Remove("is_authenticated");
			mixpanelPayload.Remove("screen_height");
			mixpanelPayload.Remove("screen_width");

			Assert.AreEqual(
				$"{JsonConvert.SerializeObject(expectedDict)}",
				$"{JsonConvert.SerializeObject(mixpanelPayload)}"
			);
		}

		[TestMethod]
		public void EndMatchAction_HearthstoneIncludesExclusiveData()
		{
			var action = EndMatchAction.Create(new Dictionary<HearthstoneExtraData, object>
			{
				{ HearthstoneExtraData.StarLevel, 5 }
			});

			Assert.AreEqual(5, action.MixpanelPayload["star_level"]);
			Assert.IsTrue(action.MixpanelPayload.ContainsKey("hdt_hsconstructed_settings_enabled"));
			Assert.IsTrue(action.MixpanelPayload.ContainsKey("hdt_hsconstructed_settings_disabled"));
		}

		[TestMethod]
		public void EndMatchAction_BattlegroundsIncludesExclusiveData()
		{
			var action = EndMatchAction.Create(new Dictionary<BattlegroundsExtraData, object>
			{
				{ BattlegroundsExtraData.BattlegroundsRating, 5000 }
			});

			Assert.AreEqual(action.MixpanelPayload["battlegrounds_rating"], 5000);
			Assert.IsTrue(action.MixpanelPayload.ContainsKey("hdt_battlegrounds_settings_enabled"));
			Assert.IsTrue(action.MixpanelPayload.ContainsKey("hdt_battlegrounds_settings_disabled"));
		}

		[TestMethod]
		public void EndMatchAction_MercenariesIncludesExclusiveData()
		{
			var action = EndMatchAction.Create(new Dictionary<MercenariesExtraData, object>
			{
				{ MercenariesExtraData.NumHoverOpponentMercAbility, 13 }
			});

			Assert.AreEqual(action.MixpanelPayload["num_hover_opponent_merc_ability"], 13);
			Assert.IsTrue(action.MixpanelPayload.ContainsKey("hdt_mercenaries_settings_enabled"));
			Assert.IsTrue(action.MixpanelPayload.ContainsKey("hdt_mercenaries_settings_disabled"));
		}

		[TestMethod]
		public void InstallAction_IncludesExclusiveData()
		{
			var action = new InstallAction();

			Assert.AreEqual("First App Start", action.MixpanelPayload["action_type"]);
			Assert.AreEqual(
				$"{JsonConvert.SerializeObject(new[] { "HS-Constructed", "Battlegrounds", "Mercenaries" })}",
				$"{JsonConvert.SerializeObject(action.MixpanelPayload["franchise"])}"
			);
		}

		[TestMethod]
		public void FirstHSCollectionUploadAction_IncludesExclusiveData()
		{
			var action = new FirstHSCollectionUploadAction(999);

			Assert.AreEqual("First Collection Upload", action.MixpanelPayload["action_type"]);
			Assert.AreEqual(999, action.MixpanelPayload["collection_size"]);
		}

		[TestMethod]
		public void ToastAction_IncludesExclusiveData()
		{
			var action = new ToastAction(
				Franchise.HSConstructed,
				ToastAction.ToastName.ConstructedCollectionUploaded
			);

			Assert.AreEqual("constructed_collection_uploaded", action.MixpanelPayload["toast"]);
		}

		[TestMethod]
		public void ClickAction_IncludesExclusiveData()
		{
			var action = new ClickAction(
				Franchise.HSConstructed,
				ClickAction.ActionName.ScreenshotSaveToDisk
			);

			Assert.AreEqual("screenshot: Save To Disk", action.MixpanelPayload["action_name"]);
			Assert.IsTrue(action.MixpanelPayload.ContainsKey("hdt_personal_stats_settings_enabled"));
			Assert.IsTrue(action.MixpanelPayload.ContainsKey("hdt_personal_stats_settings_disabled"));
		}

		[TestMethod]
		public void CopyDeckAction_IncludesExclusiveData()
		{
			var action = new CopyDeckAction(
				Franchise.HSConstructed,
				CopyDeckAction.ActionName.CopyIds
			);

			Assert.AreEqual("Copy Ids to Clipboard", action.MixpanelPayload["action_name"]);
			Assert.IsTrue(action.MixpanelPayload.ContainsKey("hdt_personal_stats_settings_enabled"));
			Assert.IsTrue(action.MixpanelPayload.ContainsKey("hdt_personal_stats_settings_disabled"));
		}
	}
}
