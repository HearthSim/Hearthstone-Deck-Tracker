using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Utility.Analytics
{
	internal class Influx
	{
		private static DateTime _appStartTime;
		private static bool _new;
		private static int? _pctHsReplayData;
		private static int? _pctHsReplayDataTotal;
		private static readonly List<int> MainWindowActivations = new List<int>();
		private static DateTime? _lastMainWindowActivation;
		private static DateTime _oAuthInitiated;

		public static void OnAppStart(Version version, bool isNew, bool authenticated, bool premium, int startupDuration, int numPlugins)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			_appStartTime = DateTime.Now;
			_new = isNew;
			var point = new InfluxPointBuilder("hdt_app_start")
				.Tag("version", version.ToVersionString(true))
				.Tag("new", isNew)
				.Tag("authenticated", authenticated)
				.Tag("premium", premium)
				.Tag("collection_syncing", Config.Instance.SyncCollection)
				.Tag("collections_uploaded", Account.Instance.CollectionState.Count)
				.Tag("auto_upload", Config.Instance.HsReplayAutoUpload)
				.Tag("lang_card", Config.Instance.SelectedLanguage)
				.Tag("lang_ui", Config.Instance.Localization.ToString())
				.Field("num_plugins", numPlugins)
				.Field("startup_duration", startupDuration);
#if(SQUIRREL)
			point.Tag("squirrel", true);
#else
			point.Tag("squirrel", false);
#endif
			WritePoint(point.Build());
		}

		public static void OnAppExit(Version version)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			var sessionDuration = (DateTime.Now - _appStartTime).TotalSeconds;
			var point = new InfluxPointBuilder("hdt_app_exit")
				.Tag("version", version.ToVersionString(true))
				.Tag("new", _new)
				.Tag("stats_window_used", Core.StatsOverviewInitialized)
				.Field("session_duration_seconds", (int)sessionDuration);
#if(SQUIRREL)
			point.Tag("squirrel", true);
#else
			point.Tag("squirrel", false);
#endif

			if(_pctHsReplayDataTotal.HasValue)
				point.Field("pct_hsreplay_data_total", _pctHsReplayDataTotal.Value);
			if(_pctHsReplayData.HasValue)
				point.Field("pct_hsreplay_data_last14d", _pctHsReplayData.Value);

			if(_lastMainWindowActivation != null)
				OnMainWindowDeactivated();
			point.Field("window_activations", MainWindowActivations.Count);
			point.Field("window_active_duration", (int)MainWindowActivations.Average());

			WritePoint(point.Build());
		}

		public static void OnHsReplayAutoUploadChanged(bool newState)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			WritePoint(new InfluxPointBuilder("hdt_hsreplay_autoupload_changed").Tag("new_state", newState).Build());
		}

		public static void OnHighMemoryUsage(long mem)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			WritePoint(new InfluxPointBuilder("hdt_memory_usage", false).Tag("os", Regex.Escape(Helper.GetWindowsVersion()))
				.Tag("net", Helper.GetInstalledDotNetVersion()).Field("MB", mem).Build());
		}

		public static void OnUnevenPermissions()
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			WritePoint(new InfluxPointBuilder("hdt_uneven_permissions", false).Tag("os", Regex.Escape(Helper.GetWindowsVersion()))
				.Tag("net", Helper.GetInstalledDotNetVersion()).Build());
		}

		public static void OnPluginLoaded(IPlugin plugin, TimeSpan startupTime)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			var point = new InfluxPointBuilder("hdt_plugin_loaded", false)
				.Tag("name", plugin.Name)
				.Tag("version", plugin.Version.ToVersionString())
				.Field("startup_time", (int)startupTime.TotalMilliseconds);
			WritePoint(point.Build());
		}

		public static void OnPluginLoadingError(IPlugin plugin)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			var point = new InfluxPointBuilder("hdt_plugin_loading_error", false)
				.Tag("name", plugin.Name)
				.Tag("version", plugin.Version.ToVersionString());
			WritePoint(point.Build());
		}

		public static void OnGameUploadFailed(WebExceptionStatus status = WebExceptionStatus.UnknownError)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			WritePoint(new InfluxPointBuilder("hdt_hsreplay_upload_failed_counter").Tag("status", status).Build());
		}

		public static void OnEndOfGameUploadError(string reason)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			WritePoint(new InfluxPointBuilder("hdt_end_of_game_upload_error").Tag("reason", Regex.Escape(reason)).Build());
		}

		public static void OnCollectionSyncingBannerClicked(bool authenticated, bool collectionSynced)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			var point = new InfluxPointBuilder("hdt_collection_syncing_banner_interaction")
				.Tag("type", "click")
				.Tag("authenticated", authenticated)
				.Tag("collection_synced", collectionSynced);
			WritePoint(point.Build());
		}

		public static void OnCollectionSyncingBannerClosed()
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			var point = new InfluxPointBuilder("hdt_collection_syncing_banner_interaction")
				.Tag("type", "close");
			WritePoint(point.Build());
		}

		public static void OnBlizzardAccountClaimed(bool success)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			var point = new InfluxPointBuilder("hdt_collection_syncing_account_claimed")
				.Tag("success", success);
			WritePoint(point.Build());
		}

		public static void OnCollectionSynced(bool success)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			var point = new InfluxPointBuilder("hdt_collection_syncing_uploaded")
				.Tag("success", success);
			WritePoint(point.Build());
		}

		public static void OnCollectionSyncingEnabled(bool enabled)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			var point = new InfluxPointBuilder("hdt_collection_syncing_enabled_changed")
				.Tag("enabled", enabled);
			WritePoint(point.Build());
		}

		public static void OnOAuthLoginInitiated()
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			_oAuthInitiated = DateTime.Now;
		}

		public static void OnOAuthLoginComplete(HSReplayNetHelper.AuthenticationErrorType error)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			var point = new InfluxPointBuilder("hdt_oauth_login")
				.Tag("error", error)
				.Field("duration_ms", (int)(DateTime.Now - _oAuthInitiated).TotalMilliseconds);
			WritePoint(point.Build());

		}

		public static void OnOAuthLogout()
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			var point = new InfluxPointBuilder("hdt_oauth_logout");
			WritePoint(point.Build());
		}

		public static void OnHsReplayDataLoaded()
		{
			try
			{
				var constructedDecks = DeckList.Instance.Decks.Where(x => !x.IsArenaDeck && !x.IsDungeonDeck).ToList();
				if(constructedDecks.Count == 0)
					return;

				var available = HsReplayDataManager.Decks.AvailableDecks;
				bool HasData(Deck deck) => available.Contains(deck.ShortId);
				int PctHasData(IReadOnlyCollection<Deck> decks) => (int)Math.Round(100.0 * decks.Count(HasData) / decks.Count);

				_pctHsReplayDataTotal = PctHasData(constructedDecks);

				var decksPlayed = constructedDecks.Where(x => DateTime.Now - x.LastPlayed < TimeSpan.FromDays(14)).ToList();
				if(decksPlayed.Count > 0)
					_pctHsReplayData = PctHasData(decksPlayed);
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		public static void OnMainWindowActivated()
		{
			_lastMainWindowActivation = DateTime.Now;
		}

		public static void OnMainWindowDeactivated()
		{
			if(_lastMainWindowActivation == null)
				return;
			var duration = DateTime.Now - _lastMainWindowActivation.Value;
			MainWindowActivations.Add((int)duration.TotalSeconds);
			_lastMainWindowActivation = null;
		}

		private static async void WritePoint(InfluxPoint point)
		{
			try
			{
				using(var client = new UdpClient())
				{
					var line = point.ToLineProtocol();
					var data = Encoding.UTF8.GetBytes(line);
					var length = await client.SendAsync(data, data.Length, "metrics.hearthsim.net", 8091);
					Log.Debug(line + " - " +  length);
				}
			}
			catch(Exception ex)
			{
				Log.Debug(ex.ToString());
			}
		}
	}
}
