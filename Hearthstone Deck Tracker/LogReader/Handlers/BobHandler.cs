using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
    public class BobHandler
    {
        public void Handle(string logLine, IHsGameState gameState, IGame game)
        {
            if (logLine[9] != 'R' && logLine[16] != 'r') // [Bob] ---Register
                return;

            if (logLine.Length == 29 && logLine[23] == 'B' && logLine[25] == 'x') // ---RegisterScreenBox---
            {
                if (game.CurrentGameMode == GameMode.Spectator)
                    gameState.GameEnd();
            }
            else if (logLine.Length == 31 && logLine[23] == 'F' && logLine[27] == 'e') // ---RegisterScreenForge---
            {
                gameState.GameHandler.SetGameMode(GameMode.Arena);
                game.ResetArenaCards();
            }
            else if (logLine.Length == 34)
            {
                if (logLine[23] == 'P' && logLine[30] == 'e') // ---RegisterScreenPractice---
                    gameState.GameHandler.SetGameMode(GameMode.Practice);
                else if (logLine[23] == 'T' && logLine[30] == 's') // ---RegisterScreenTourneys---
                    gameState.GameHandler.SetGameMode(GameMode.Casual);
                else if (logLine[23] == 'F' && logLine[30] == 'y') // ---RegisterScreenFriendly---
                    gameState.GameHandler.SetGameMode(GameMode.Friendly);
                else if (logLine[23] == 'e' && logLine[24] == 'N' && logLine[30] == 's') // RegisterProfileNotices
                    gameState.GameLoaded = true;
            }
            else if (logLine.Length == 35 && logLine[17] == 'F' && logLine[22] == 'd' && logLine[23] == 'C') // RegisterFriendChallenge
            {
                gameState.GameHandler.HandleInMenu();
            }
            else if (logLine.Length == 43 && logLine[23] == 'C' && logLine[32] == 'n' && logLine[33] == 'M' && logLine[39] == 'r')// ---RegisterScreenCollectionManager---
                gameState.GameHandler.ResetConstructedImporting();
        }
    }
}
