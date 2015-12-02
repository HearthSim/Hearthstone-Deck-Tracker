using System;
using System.Text.RegularExpressions;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.LogReader
{
    public class AssetHandler
    {
        public void Handle(string logLine, IHsGameState gameState, IGame game)
        {
            if (HsLogReaderConstants.UnloadCardRegex.IsMatch(logLine))
            {
                var id = HsLogReaderConstants.UnloadCardRegex.Match(logLine).Groups["id"].Value;
                if (game.CurrentGameMode == GameMode.Arena)
                    gameState.GameHandler.HandlePossibleArenaCard(id);
                else
                    gameState.GameHandler.HandlePossibleConstructedCard(id, true);
            }
        }
    }
}