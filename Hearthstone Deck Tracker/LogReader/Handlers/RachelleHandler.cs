using System;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
    public class RachelleHandler
    {
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
            else if (HsLogReaderConstants.GoldProgressRegex.IsMatch(logLine)
                     && (DateTime.Now - gameState.LastGameStart) > TimeSpan.FromSeconds(10)
                     && game.CurrentGameMode != GameMode.Spectator)
            {
                int wins;
                var rawWins = HsLogReaderConstants.GoldProgressRegex.Match(logLine).Groups["wins"].Value;
                if (int.TryParse(rawWins, out wins))
                {
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
                    }
                    if (timeZone != null)
                    {
                        var region = (int)game.CurrentRegion - 1;
                        var date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).Date;
                        if (Config.Instance.GoldProgressLastReset[region].Date != date)
                        {
                            Config.Instance.GoldProgressTotal[region] = 0;
                            Config.Instance.GoldProgressLastReset[region] = date;
                        }
                        Config.Instance.GoldProgress[region] = wins == 3 ? 0 : wins;
                        if (wins == 3)
                            Config.Instance.GoldProgressTotal[region] += 10;
                        Config.Save();
                    }
                }
            }
            else if (HsLogReaderConstants.DustRewardRegex.IsMatch(logLine))
            {
                int amount;
                if (int.TryParse(HsLogReaderConstants.DustRewardRegex.Match(logLine).Groups["amount"].Value, out amount))
                    gameState.GameHandler.HandleDustReward(amount);
            }
            else if (HsLogReaderConstants.GoldRewardRegex.IsMatch(logLine))
            {
                int amount;
                if (int.TryParse(HsLogReaderConstants.GoldRewardRegex.Match(logLine).Groups["amount"].Value, out amount))
                    gameState.GameHandler.HandleGoldReward(amount);
            }
        }
    }
}