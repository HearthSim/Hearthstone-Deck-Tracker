#region

using System;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	public class RachelleHandler
	{
		public void Handle(string logLine, IHsGameState gameState, IGame game)
		{
			if(HsLogReaderConstants.CardAlreadyInCacheRegex.IsMatch(logLine))
			{
				var id = HsLogReaderConstants.CardAlreadyInCacheRegex.Match(logLine).Groups["id"].Value;
				if(game.CurrentGameMode == GameMode.Arena)
					gameState.GameHandler.HandlePossibleArenaCard(id);
				else
					gameState.GameHandler.HandlePossibleConstructedCard(id, false);
			}
			else if(HsLogReaderConstants.GoldProgressRegex.IsMatch(logLine)
				&& (DateTime.Now - gameState.LastGameStart) > TimeSpan.FromSeconds(10)
				&& game.CurrentGameMode != GameMode.Spectator)
			{

				int wins;
				var rawWins = HsLogReaderConstants.GoldProgressRegex.Match(logLine).Groups["wins"].Value;
				if(int.TryParse(rawWins, out wins))
				{
					var timeZone = GetTimeZoneInfo(game.CurrentRegion);
					if(timeZone != null)
					{
						UpdateGoldProgress(wins, game, timeZone);
					}
                }

			}
			else if(HsLogReaderConstants.DustRewardRegex.IsMatch(logLine))
			{
				int amount;
				if(int.TryParse(HsLogReaderConstants.DustRewardRegex.Match(logLine).Groups["amount"].Value, out amount))
					gameState.GameHandler.HandleDustReward(amount);
			}
			else if(HsLogReaderConstants.GoldRewardRegex.IsMatch(logLine))
			{
				int amount;
				if(int.TryParse(HsLogReaderConstants.GoldRewardRegex.Match(logLine).Groups["amount"].Value, out amount))
					gameState.GameHandler.HandleGoldReward(amount);
			}
		}

		private TimeZoneInfo GetTimeZoneInfo(Region region)
		{
			try
			{
				switch(region)
				{
					case Region.EU:
						return TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
					case Region.US:
						return TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
					case Region.ASIA:
						return TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
					case Region.CHINA:
						return TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
					default:
						Logger.WriteLine(string.Format("Could not get TimeZoneInfo for Region {0}", region), "RachelleHandler");
						return null;
				}
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error determining region: " + ex, "RachelleHandler");
			}
			return null;
		}

		private void UpdateGoldProgress(int wins, IGame game, TimeZoneInfo timeZone)
		{
			try
			{
				var region = (int)game.CurrentRegion - 1;
				var date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).Date;
				if(Config.Instance.GoldProgressLastReset[region].Date != date)
				{
					Config.Instance.GoldProgressTotal[region] = 0;
					Config.Instance.GoldProgressLastReset[region] = date;
				}
				Config.Instance.GoldProgress[region] = wins == 3 ? 0 : wins;
				if(wins == 3)
					Config.Instance.GoldProgressTotal[region] += 10;
				Config.Save();
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error updating GoldProgress: " + ex, "RachelleHandler");
			}
		}
	}
}