using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using NuGet;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Newtonsoft.Json;

namespace HDTTests.Utility.ValueMoments.Actions
{
	[TestClass]
	public class VMActionsTests
	{
		[TestMethod]
		public void VMAction_MixpanelPropertiesReturnsCorrect()
		{
			var action = new VMActions.FirstHSCollectionUploadAction(999);

			var expectedDict = new Dictionary<string, object> {
				{ "franchise", new [] { "HS-Constructed" } },
				{ "collection_size", 999 },
				{ "action_type", "First Collection Upload" },
				{ "action_source", "app" },
				{ "domain", "hsreplay.net" },
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
				}}
			};

			Assert.AreEqual(
				$"{JsonConvert.SerializeObject(expectedDict)}",
				$"{JsonConvert.SerializeObject(action.MixpanelProperties)}"
			);
		}

		[TestMethod]
		public void ToastAction_IncludesCorrectToastName()
		{
			var action = new VMActions.ToastAction(
				Franchise.HSConstructed,
				VMActions.ToastAction.ToastName.ConstructedCollectionUploaded
			);

			Assert.AreEqual(action.MixpanelProperties["toast"], "constructed_collection_uploaded");
		}

		[TestMethod]
		public void ClickAction_IncludesCorrectActionName()
		{
			var action = new VMActions.ClickAction(
				Franchise.HSConstructed,
				VMActions.ClickAction.ActionName.ScreenshotSaveToDisk
			);

			Assert.AreEqual(action.MixpanelProperties["action_name"], "screenshot: Save To Disk");
		}

		[TestMethod]
		public void CopyDeckAction_IncludesCorrectActionName()
		{
			var action = new VMActions.CopyDeckAction(
				Franchise.HSConstructed,
				VMActions.CopyDeckAction.ActionName.CopyIds
			);

			Assert.AreEqual(action.MixpanelProperties["action_name"], "Copy Ids to Clipboard");
		}
	}
}
