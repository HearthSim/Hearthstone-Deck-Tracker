using System;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
    public class RachelleHandler
    {
        /// <summary>
        /// [Rachelle] RewardUtils.GetViewableRewards() - processing reward [GoldRewardData: Amount=10 Origin=TOURNEY OriginData=0]
        /// </summary>
        private bool win3Times;

        /// <summary>
        /// [Rachelle] VictoryScreen.InitGoldRewardUI(): goldRewardState = INVALID
        /// </summary>
        private bool winCheck;

        /// <summary>
        /// count win times by 3 for a loop
        /// </summary>
        private int wins;

        public void Handle(string logLine, IHsGameState gameState, IGame game)
        {
            if (HsLogReaderConstants.CardAlreadyInCacheRegex.IsMatch(logLine))
            {
                var id = HsLogReaderConstants.CardAlreadyInCacheRegex.Match(logLine).Groups["id"].Value;
                if (game.CurrentGameMode == GameMode.Arena)
                    gameState.GameHandler.HandlePossibleArenaCard(id);
                else
                    gameState.GameHandler.HandlePossibleConstructedCard(id, false);
            }
            else if ((DateTime.Now - gameState.LastGameStart) > TimeSpan.FromSeconds(10)
                     && game.CurrentGameMode != GameMode.Spectator)
            {
                GoldTracking(logLine, game);
            }
            else if (HsLogReaderConstants.DustRewardRegex.IsMatch(logLine))
            {
                int amount;
                if (int.TryParse(HsLogReaderConstants.DustRewardRegex.Match(logLine).Groups["amount"].Value,
                    out amount))
                    gameState.GameHandler.HandleDustReward(amount);
            }
            else if (HsLogReaderConstants.GoldRewardRegex.IsMatch(logLine))
            {
                int amount;
                if (int.TryParse(HsLogReaderConstants.GoldRewardRegex.Match(logLine).Groups["amount"].Value,
                    out amount))
                    gameState.GameHandler.HandleGoldReward(amount);
            }
        }

        private void GoldTracking(string logLine, IGame game)
        {
            if (
                logLine.Equals(
                    "[Rachelle] RewardUtils.GetViewableRewards() - processing reward [GoldRewardData: Amount=10 Origin=TOURNEY OriginData=0]"))
            {
                win3Times = true;
            }
            if (logLine.Equals("[Rachelle] VictoryScreen.InitGoldRewardUI(): goldRewardState = INVALID"))
            {
                winCheck = true;
            }
            if (winCheck)
            {
                winCheck = false;
                TimeZoneInfo timeZone = null;
                switch (game.CurrentRegion)
                {
                    case Region.EU:
                        timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
                        break;
                    case Region.US:
                        timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                        break;
                    case Region.ASIA:
                        timeZone = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
                        break;
                    case Region.CHINA:
                        timeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                        break;
                }
                if (timeZone != null)
                {
                    var region = (int) game.CurrentRegion - 1;
                    wins = Config.Instance.GoldProgress[region];
                    wins++;
                    var date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).Date;
                    if (Config.Instance.GoldProgressLastReset[region].Date != date)
                    {
                        wins = 1;
                        Config.Instance.GoldProgressTotal[region] = 0;
                        Config.Instance.GoldProgressLastReset[region] = date;
                    }
                    if (win3Times)
                    {
                        if (wins >= 4)
                        {
                            Logger.WriteLine(string.Format("Current wins is {0},{1} wins did not get gold reward.", wins,
                                wins - 3));
                        }
                        wins = 0;
                        win3Times = false;
                        Config.Instance.GoldProgressTotal[region] += 10;
                    }
                    if (Config.Instance.GoldProgressTotal[region] == 100)
                    {
                        wins = 0;
                    }
                    Config.Instance.GoldProgress[region] = wins;
                    Config.Save();
                }
                else
                {
                    Logger.WriteLine(string.Format("Can not recognize current region :{0}.", game.CurrentRegion));
                }
            }
        }
    }
}