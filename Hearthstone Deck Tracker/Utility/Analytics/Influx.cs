using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Utility.Analytics
{
	internal class Influx
	{
		private const string Url = "https://metrics.hearthsim.net:8086/write?db=hsreplaynet&precision=s&u=hdt&p=GPPHbmJQtC87FAAR";
		private static DateTime _appStartTime;
		private static bool _new;

		public static void OnAppStart(Version version, bool isNew, int startupDuration)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			_appStartTime = DateTime.Now;
			_new = isNew;
			var point = new InfluxPointBuilder("hdt_app_start")
				.Tag("version", version.ToVersionString(true))
				.Tag("new", isNew)
				.Tag("auto_upload", Config.Instance.HsReplayAutoUpload)
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
				.Field("session_duration_seconds", (int)sessionDuration);
#if(SQUIRREL)
			point.Tag("squirrel", true);
#else
			point.Tag("squirrel", false);
#endif
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

		private static async void WritePoint(InfluxPoint point)
		{
			try
			{
				var request = (HttpWebRequest)WebRequest.Create(Url);
				request.ContentType = "text/plain";
				request.Method = "POST";
				using(var stream = await request.GetRequestStreamAsync())
				{
					var line = point.ToLineProtocol();
					Log.Debug(line);
					stream.Write(Encoding.UTF8.GetBytes(line), 0, line.Length);
				}
				try
				{
					using(var response = (HttpWebResponse)await request.GetResponseAsync())
						Log.Debug(response.StatusCode.ToString());
				}
				catch(WebException e)
				{
					Log.Debug(e.Status.ToString());
				}
			}
			catch(Exception ex)
			{
				Log.Debug(ex.ToString());
			}
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
	}
}
