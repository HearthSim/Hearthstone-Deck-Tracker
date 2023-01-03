using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker;
using NuGet;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;
using static Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.VMActions;

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
		public void VMAction_MixpanelPayloadReturnsCorrect()
		{
			var action = EndMatchAction.Create(new Dictionary<HearthstoneExtraData, object>());
			DailyEventsCount.Instance.Clear(action.EventId);
			// Recreate action to update daily occurrences
			action = EndMatchAction.Create(new Dictionary<HearthstoneExtraData, object>());

			var expectedDict = new Dictionary<string, object> {
				{ "franchise", new [] { "HS-Constructed" } },
				{ "action_name", "end_match" },
				{ "action_type", "End Match Action" },
				{ "action_source", "app" },
				{ "domain", "hsreplay.net" },
				{ "cur_daily_occurrences", 1 },
				{ "max_daily_occurrences", 1 },
				{ "is_authenticated", true },
				{ "screen_height", 1080 },
				{ "screen_width", 1920 },
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

			Assert.AreEqual(
				$"{JsonConvert.SerializeObject(expectedDict)}",
				$"{JsonConvert.SerializeObject(action.MixpanelPayload)}"
			);
		}

		[TestMethod]
		public void EndMatchAction_HearthstoneIncludesExclusiveData()
		{
			var action = EndMatchAction.Create(new Dictionary<HearthstoneExtraData, object>
			{
				{ HearthstoneExtraData.StarLevel, 5 }
			});

			Assert.AreEqual(action.MixpanelPayload["star_level"], 5);
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
		public void FirstHSCollectionUploadAction_IncludesExclusiveData()
		{
			var action = new FirstHSCollectionUploadAction(999);

			Assert.AreEqual(action.MixpanelPayload["action_type"], "First Collection Upload");
			Assert.AreEqual(action.MixpanelPayload["collection_size"], 999);
		}

		[TestMethod]
		public void ToastAction_IncludesExclusiveData()
		{
			var action = new ToastAction(
				Franchise.HSConstructed,
				ToastAction.ToastName.ConstructedCollectionUploaded
			);

			Assert.AreEqual(action.MixpanelPayload["toast"], "constructed_collection_uploaded");
		}

		[TestMethod]
		public void ClickAction_IncludesExclusiveData()
		{
			var action = new ClickAction(
				Franchise.HSConstructed,
				ClickAction.ActionName.ScreenshotSaveToDisk
			);

			Assert.AreEqual(action.MixpanelPayload["action_name"], "screenshot: Save To Disk");
		}

		[TestMethod]
		public void CopyDeckAction_IncludesExclusiveData()
		{
			var action = new CopyDeckAction(
				Franchise.HSConstructed,
				CopyDeckAction.ActionName.CopyIds
			);

			Assert.AreEqual(action.MixpanelPayload["action_name"], "Copy Ids to Clipboard");
		}
	}
}
