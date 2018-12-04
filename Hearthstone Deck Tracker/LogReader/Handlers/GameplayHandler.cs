#region

using System;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static System.TimeZoneInfo;
using static Hearthstone_Deck_Tracker.Enums.Region;
using static Hearthstone_Deck_Tracker.LogReader.LogConstants;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	public class GameplayHandler
	{
		public void Handle(string logLine, IHsGameState gameState, IGame game)
		{
			if(!GoldProgressRegex.IsMatch(logLine) || (DateTime.Now - gameState.LastGameStart) <= TimeSpan.FromSeconds(10)
				|| game.CurrentGameMode == GameMode.Spectator)
				return;
			var rawWins = GoldProgressRegex.Match(logLine).Groups["wins"].Value;
			if(!int.TryParse(rawWins, out int wins))
				return;
			UpdateGoldProgress(wins, game);
		}

		private static TimeZoneInfo GetTimeZoneInfo(Region region)
		{
			try
			{
				switch(region)
				{
					case EU:
						return FindSystemTimeZoneById("Central European Standard Time");
					case US:
						return FindSystemTimeZoneById("Pacific Standard Time");
					case ASIA:
						return FindSystemTimeZoneById("Korea Standard Time");
					case CHINA:
						return FindSystemTimeZoneById("China Standard Time");
					default:
						Log.Error($"Could not get TimeZoneInfo for Region {region}");
						return null;
				}
			}
			catch(Exception ex)
			{
				Log.Error("Error determining region: " + ex);
			}
			return null;
		}

		public static void ResetGoldProgress(Region regionEnum, bool saveConfig)
		{
			var timeZone = GetTimeZoneInfo(regionEnum);
			if(timeZone == null)
				return;
			var date = ConvertTimeFromUtc(DateTime.UtcNow, timeZone).Date;
			var region = (int)regionEnum - 1;
			bool reset = false;
			lock(Config.Instance.GoldProgressLastReset)
			{
				if(Config.Instance.GoldProgressLastReset[region].Date != date)
				{
					Config.Instance.GoldProgressTotal[region] = 0;
					Config.Instance.GoldProgressLastReset[region] = date;
					reset = true;
				}
			}
			if(saveConfig && reset)
				Config.Save();
		}

		private void UpdateGoldProgress(int wins, IGame game)
		{
			try
			{
				var regionEnum = game.CurrentRegion;
				var region = (int)regionEnum - 1;
				ResetGoldProgress(regionEnum, false);
				Config.Instance.GoldProgress[region] = wins == 3 ? 0 : wins;
				if(wins == 3)
					Config.Instance.GoldProgressTotal[region] += 10;
				Config.Save();
			}
			catch(Exception ex)
			{
				Log.Error("Error updating GoldProgress: " + ex);
			}
		}
	}
}
