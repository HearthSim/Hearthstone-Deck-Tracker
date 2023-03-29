using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;
using Newtonsoft.Json.Linq;
using NuGet;

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
			var action = new EndMatchHearthstoneAction(123, "foo", GameResult.Win, GameMode.Practice, GameType.GT_VS_AI, 1);

			Assert.AreEqual("end match action hdt_hs-constructed", action.Id);
		}

		[TestMethod]
		public void VMAction_ActionIdWithSubFranchise()
		{
			var action = new EndMatchHearthstoneAction(123, "foo", GameResult.Win, GameMode.Arena, GameType.GT_ARENA, 1);

			Assert.AreEqual("end match action hdt_hs-constructed_arena", action.Id);
		}

		[TestMethod]
		public void VMAction_ActionIdWithAllFranchise()
		{
			var action = new InstallAction();

			Assert.AreEqual("install hdt_hs-constructed", action.Id);
		}
		
		[TestMethod]
		public void VMAction_MixpanelPayloadReturnsCorrect()
		{
			DailyEventsCount.Instance.Clear("end match action hdt_hs-constructed");
			var action = new EndMatchHearthstoneAction(123, "foo", GameResult.Win, GameMode.Practice, GameType.GT_VS_AI, 1);

			var expectedDict = new Dictionary<string, object> {
				{ "action_type", "End Match Action" },
				{ "hero_dbf_id", 123 },
				{ "hero_name", "foo" },
				{ "match_result", 1 },
				{ "game_type", 1 },
				{ "star_level", 1 },
				{ "hdt_hsconstructed_settings_enabled", new []{ "hide_timers" }},
				{ "hdt_hsconstructed_settings_disabled", new []{ "hide_decks" }},
				{ "action_source", "app" },
				{ "action_name", "end_match" },
				{ "domain", "hsreplay.net" },
				{ "franchise", new [] { "HS-Constructed" } },
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
				{ "cur_daily_occurrences", 1 },
				{ "max_daily_occurrences", 1 },
				{ "free_value_moments", new [] { "Overlay Decklist Visible" }},
				{ "paid_value_moments", new string[]{} },
				{ "has_free_value_moment", true },
				{ "has_paid_value_moment", false }
			};

			var mixpanelPayload = JObject.Parse(JsonConvert.SerializeObject(action));
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
			var action = new EndMatchHearthstoneAction(123, "foo", GameResult.Win, GameMode.Practice, GameType.GT_VS_AI, 5);

			var mixpanelPayload = JObject.Parse(JsonConvert.SerializeObject(action));
			Assert.AreEqual(5, mixpanelPayload["star_level"]);
			Assert.IsTrue(mixpanelPayload.ContainsKey("hdt_hsconstructed_settings_enabled"));
			Assert.IsTrue(mixpanelPayload.ContainsKey("hdt_hsconstructed_settings_disabled"));
		}

		[TestMethod]
		public void EndMatchAction_BattlegroundsIncludesExclusiveData()
		{
			var action = new EndMatchBattlegroundsAction(123, "foo", 1, GameType.GT_BATTLEGROUNDS, 5000, new GameMetrics());

			var mixpanelPayload = JObject.Parse(JsonConvert.SerializeObject(action));
			Assert.AreEqual(mixpanelPayload["battlegrounds_rating"], 5000);
			Assert.IsTrue(mixpanelPayload.ContainsKey("hdt_battlegrounds_settings_enabled"));
			Assert.IsTrue(mixpanelPayload.ContainsKey("hdt_battlegrounds_settings_disabled"));
			
			Assert.IsFalse(mixpanelPayload.ContainsKey("tier7_hero_overlay_displayed"));
			Assert.IsFalse(mixpanelPayload.ContainsKey("tier7_quest_overlay_displayed"));
		}

		[TestMethod]
		public void EndMatchAction_MercenariesIncludesExclusiveData()
		{
			var gameMetrics = new GameMetrics();
			gameMetrics.IncrementMercenariesHoversOpponentMercToShowAbility();
			gameMetrics.IncrementMercenariesHoversOpponentMercToShowAbility();
			var action = new EndMatchMercenariesAction(GameResult.Win, GameType.GT_VS_AI, gameMetrics);

			var mixpanelPayload = JObject.Parse(JsonConvert.SerializeObject(action));
			Assert.AreEqual(mixpanelPayload["num_hover_opponent_merc_ability"], 2);
			Assert.IsTrue(mixpanelPayload.ContainsKey("hdt_mercenaries_settings_enabled"));
			Assert.IsTrue(mixpanelPayload.ContainsKey("hdt_mercenaries_settings_disabled"));
		}

		[TestMethod]
		public void InstallAction_IncludesExclusiveData()
		{
			var action = new InstallAction();

			var mixpanelPayload = JObject.Parse(JsonConvert.SerializeObject(action));
			Assert.AreEqual("First App Start", mixpanelPayload["action_type"]);
			Assert.AreEqual(
				$"{JsonConvert.SerializeObject(new[] { "HS-Constructed", "Battlegrounds", "Mercenaries" })}",
				$"{JsonConvert.SerializeObject(mixpanelPayload["franchise"])}"
			);
		}

		[TestMethod]
		public void FirstHSCollectionUploadAction_IncludesExclusiveData()
		{
			var action = new FirstHSCollectionUploadAction(999);

			var mixpanelPayload = JObject.Parse(JsonConvert.SerializeObject(action));
			Assert.AreEqual("First Collection Upload", mixpanelPayload["action_type"]);
			Assert.AreEqual(999, mixpanelPayload["collection_size"]);
		}

		[TestMethod]
		public void ToastAction_IncludesExclusiveData()
		{
			var action = new ToastAction(
				Franchise.HSConstructed,
				ToastAction.Toast.ConstructedCollectionUploaded
			);

			var mixpanelPayload = JObject.Parse(JsonConvert.SerializeObject(action));
			Assert.AreEqual("constructed_collection_uploaded", mixpanelPayload["toast"]);
		}

		[TestMethod]
		public void ClickAction_IncludesExclusiveData()
		{
			var action = new ClickAction(
				Franchise.HSConstructed,
				ClickAction.Action.StatsArena,
				new[] { SubFranchise.Arena }
			);

			var mixpanelPayload = JObject.Parse(JsonConvert.SerializeObject(action));
			Assert.AreEqual("stats: Arena", mixpanelPayload["action_name"]);
			Assert.AreEqual("[\"Arena\"]", JsonConvert.SerializeObject(mixpanelPayload["sub_franchise"]));
			Assert.IsTrue(mixpanelPayload.ContainsKey("hdt_personal_stats_settings_enabled"));
			Assert.IsTrue(mixpanelPayload.ContainsKey("hdt_personal_stats_settings_disabled"));
		}

		[TestMethod]
		public void CopyDeckAction_IncludesExclusiveData()
		{
			var action = new CopyDeckAction(
				Franchise.HSConstructed,
				CopyDeckAction.Action.CopyCode
			);

			var mixpanelPayload = JObject.Parse(JsonConvert.SerializeObject(action));
			Assert.AreEqual("Copy Code", mixpanelPayload["action_name"]);
			Assert.IsTrue(mixpanelPayload.ContainsKey("hdt_personal_stats_settings_enabled"));
			Assert.IsTrue(mixpanelPayload.ContainsKey("hdt_personal_stats_settings_disabled"));
		}
	}
}
