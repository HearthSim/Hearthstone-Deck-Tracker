using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
    public class PowerHandler
    {
        public void Handle(string logLine, IHsGameState gameState, IGame game)
        {
            if ((logLine.Contains("Begin Spectating") || logLine.Contains("Start Spectator") || gameState.FoundSpectatorStart) && game.IsInMenu)
            {
                gameState.GameHandler.SetGameMode(GameMode.Spectator);
                gameState.FoundSpectatorStart = false;
            }
            else if (logLine.Contains("End Spectator"))
            {
                gameState.GameHandler.SetGameMode(GameMode.Spectator);
                gameState.GameHandler.HandleGameEnd();
            }
        }
    }
}