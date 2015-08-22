using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
    public class BobHandler
    {
        public void Handle(string logLine, IHsGameState gameState, GameV2 game)
        {
            if (logLine[9] == 'R' && logLine[16] == 'r') // [Bob] ---Register
            {
                if (logLine[17] == 'S' && logLine[22] == 'n') // [Bob] ---RegisterScreen
                {
                    if (logLine[23] == 'P' && logLine[30] == 'e') // ---RegisterScreenPractice---
                        gameState.GameHandler.SetGameMode(GameMode.Practice);
                    else if (logLine[23] == 'T' && logLine[30] == 's') // ---RegisterScreenTourneys---
                        gameState.GameHandler.SetGameMode(GameMode.Casual);
                    else if (logLine[23] == 'F' && logLine[30] == 'y') // ---RegisterScreenFriendly---
                        gameState.GameHandler.SetGameMode(GameMode.Friendly);
                    else if (logLine[23] == 'C' && logLine[30] == 'n' && logLine[31] == 'M' && logLine[37] == 'r') // ---RegisterScreenCollectionManager---
                        gameState.GameHandler.ResetConstructedImporting();
                    else if (logLine[23] == 'F' && logLine[27] == 'e') // ---RegisterScreenForge---
                    {
                        gameState.GameHandler.SetGameMode(GameMode.Arena);
                        game.ResetArenaCards();
                    }
                    else if (logLine[23] == 'B' && logLine[25] == 'x') // ---RegisterScreenBox---
                    {
                        if (game.CurrentGameMode == GameMode.Spectator)
                            gameState.GameEnd();
                    }
                }
                else if (logLine[17] == 'F' && logLine[22] == 'd' && logLine[23] == 'C') // RegisterFriendChallenge
                {
                    gameState.GameHandler.HandleInMenu();
                }
                else if (logLine[17] == 'P' && logLine[23] == 'e' && logLine[24] == 'N') // RegisterProfileNotices
                {
                    gameState.GameLoaded = true;
                }
            }
        }
    }
}