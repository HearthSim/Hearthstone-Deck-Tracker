using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
    public class BobHandler
    {
        public void Handle(string logLine, HsGameState gameState)
        {
            if (logLine.StartsWith("[Bob] ---RegisterScreenPractice---"))
                gameState.GameHandler.SetGameMode(GameMode.Practice);
            else if (logLine.StartsWith("[Bob] ---RegisterScreenTourneys---"))
                gameState.GameHandler.SetGameMode(GameMode.Casual);
            else if (logLine.StartsWith("[Bob] ---RegisterScreenForge---"))
            {
                gameState.GameHandler.SetGameMode(GameMode.Arena);
                Game.ResetArenaCards();
            }
            else if (logLine.StartsWith("[Bob] ---RegisterScreenFriendly---"))
                gameState.GameHandler.SetGameMode(GameMode.Friendly);
            else if (logLine.StartsWith("[Bob] ---RegisterScreenBox---"))
            {
                //game ended -  back in menu
                if (Game.CurrentGameMode == GameMode.Spectator)
                    gameState.GameEnd();
            }
            else if (logLine.StartsWith("[Bob] ---RegisterFriendChallenge---"))
                gameState.GameHandler.HandleInMenu();
            else if (logLine.StartsWith("[Bob] ---RegisterScreenCollectionManager---"))
                gameState.GameHandler.ResetConstructedImporting();
            else if (logLine.StartsWith("[Bob] ---RegisterProfileNotices---"))
                gameState.GameLoaded = true;
        }
    }
}