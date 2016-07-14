using System;
using System.Net;
using System.Text;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Utility.Analytics
{
	internal class Influx
	{
		private const string Url = "https://metrics.hearthsim.net:8086/write?db=hsreplaynet&precision=s&u=hdt&p=GPPHbmJQtC87FAAR";

		public static void OnAppStart(Version version, LoginType loginType, bool isNew)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			WritePoint(new InfluxPointBuilder("hdt_app_start").Tag("version", version.ToVersionString()).Tag("login_type", loginType).Tag("new", isNew).Build());
		}


		public static void OnGameEnd(BnetGameType gameType)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			WritePoint(new InfluxPointBuilder("hdt_played_game_counter").Tag("game_type", gameType).Build());
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
					var response = (HttpWebResponse)await request.GetResponseAsync();
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
	}
}