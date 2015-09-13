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
		private const string GoldRewardLogLine =
			"RewardUtils.GetViewableRewards() - processing reward [GoldRewardData: Amount=10 Origin=TOURNEY OriginData=0]";

		private const string ProgressLogLine = "VictoryScreen.InitGoldRewardUI(): goldRewardState = INVALID";
		private const int WinsRequiredForReward = 3;
		private const int GoldRewardAmount = 10;
		private const int TotalMaxGoldReward = 100;
		private readonly GameMode[] _goldTrackingGameModes = {GameMode.Casual, GameMode.Ranked, GameMode.Brawl};
		private bool _receivedGoldReward;

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
			else if((DateTime.Now - gameState.LastGameStart) > TimeSpan.FromSeconds(10))
			{
				if(logLine.Contains(GoldRewardLogLine))
					_receivedGoldReward = true;
				else if(logLine.Contains(ProgressLogLine))
					HandleGoldProgress(game);
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

		private async void HandleGoldProgress(IGame game)
		{
			Logger.WriteLine("Updating GoldProgress, waiting for GameModeDetection...", "RachelleHandler");
			await game.GameModeDetection(300);
			if(_goldTrackingGameModes.All(mode => game.CurrentGameMode != mode))
			{
				Logger.WriteLine(string.Format("Current GameMode is {0} - cancelled GoldProgress update.", game.CurrentGameMode), "RachelleHandler");
				return;
			}

			var timeZone = GetTimeZoneInfo(game.CurrentRegion);
			if(timeZone != null)
				UpdateGoldProgress(game, timeZone);
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

		private void UpdateGoldProgress(IGame game, TimeZoneInfo timeZone)
		{
			try
			{
				var regionIndex = (int)game.CurrentRegion - 1;
				var previousGoldProgress = GetGoldProgressString(regionIndex);
				var wins = Config.Instance.GoldProgress[regionIndex] + 1;
				var date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).Date;
				if(Config.Instance.GoldProgressLastReset[regionIndex].Date != date)
				{
					wins = 1;
					Config.Instance.GoldProgressTotal[regionIndex] = 0;
					Config.Instance.GoldProgressLastReset[regionIndex] = date;
				}
				if(_receivedGoldReward)
				{
					wins = 0;
					_receivedGoldReward = false;
					Config.Instance.GoldProgressTotal[regionIndex] += GoldRewardAmount;
				}
				if(Config.Instance.GoldProgressTotal[regionIndex] == TotalMaxGoldReward)
					wins = 0;
				Config.Instance.GoldProgress[regionIndex] = wins;
				Config.Save();
				Logger.WriteLine(string.Format("Updated GoldProgress from {0} to {1} for region {2}.", previousGoldProgress, GetGoldProgressString(regionIndex), game.CurrentRegion));
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error updating GoldProgress: " + ex, "RachelleHandler");
			}
		}

		private string GetGoldProgressString(int regionIndex)
		{
			return string.Format("{0}/{1} - {2}/{3}", Config.Instance.GoldProgress[regionIndex], WinsRequiredForReward,
			                     Config.Instance.GoldProgressTotal[regionIndex], TotalMaxGoldReward);
		}
	}
}