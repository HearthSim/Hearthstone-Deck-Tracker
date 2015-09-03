using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
    public class ZoneHandler
    {
        public void Handle(string logLine, IHsGameState gameState)
        {
            if (HsLogReaderConstants.CardMovementRegex.IsMatch(logLine))
            {
                var match = HsLogReaderConstants.CardMovementRegex.Match(logLine);

                var id = match.Groups["Id"].Value.Trim();
                var from = match.Groups["from"].Value.Trim();
                var to = match.Groups["to"].Value.Trim();

                if (id.Contains("HERO") || (id.Contains("NAX") && id.Contains("_01")) || id.StartsWith("BRMA"))
                {
                    if (!from.Contains("PLAY"))
                    {
                        if (to.Contains("FRIENDLY"))
                            gameState.GameHandler.SetPlayerHero(Database.GetHeroNameFromId(id, false));
                        else if (to.Contains("OPPOSING"))
                            gameState.GameHandler.SetOpponentHero(Database.GetHeroNameFromId(id, false));
                    }
                }
            }
        }
    }
}