using System;
using System.Text.RegularExpressions;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.LogReader
{
    public class AssetHandler
    {
        public void Handle(string logLine, IHsGameState gameState, IGame game)
        {
            if (gameState.AwaitingRankedDetection)
            {
                gameState.LastAssetUnload = DateTime.Now;
                gameState.WaitingForFirstAssetUnload = false;
            }
            if (logLine.Contains("Medal_Ranked_"))
            {
                var match = Regex.Match(logLine, "Medal_Ranked_(?<rank>(\\d+))");
                if (match.Success)
                {
                    int rank;
                    if (int.TryParse(match.Groups["rank"].Value, out rank))
                        gameState.GameHandler.SetRank(rank);
                }
            }
            else if (logLine.Contains("rank_window"))
            {
                gameState.FoundRanked = true;
                gameState.GameHandler.SetGameMode(GameMode.Ranked);
            }
            else if (HsLogReaderConstants.UnloadCardRegex.IsMatch(logLine))
            {
                var id = HsLogReaderConstants.UnloadCardRegex.Match(logLine).Groups["id"].Value;
                if (game.CurrentGameMode == GameMode.Arena)
                    gameState.GameHandler.HandlePossibleArenaCard(id);
                else
                    gameState.GameHandler.HandlePossibleConstructedCard(id, true);
            }
            else if (HsLogReaderConstants.UnloadBrawlAsset.IsMatch(logLine))
            {
                gameState.GameHandler.SetGameMode(GameMode.Brawl);
            }
        }
    }
}