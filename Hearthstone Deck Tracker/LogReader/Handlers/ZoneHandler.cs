using System;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

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

	            var card = Database.GetCardFromId(id);
                if(card != null && card.Type == "Hero")
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