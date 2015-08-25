using System;
using System.Globalization;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
    public class ArenaHandler
    {
	    private DateTime _lastChoice = DateTime.MinValue;
	    private string _lastChoiceId = "";

        public void Handle(string logLine, IHsGameState gameState, IGame game)
        {
            var match = HsLogReaderConstants.ExistingHeroRegex.Match(logLine);
            if (match.Success)
                game.NewArenaDeck(match.Groups["id"].Value);
            else
            {
                match = HsLogReaderConstants.ExistingCardRegex.Match(logLine);
                if (match.Success)
                    game.NewArenaCard(match.Groups["id"].Value);
                else
                {
                    match = HsLogReaderConstants.NewChoiceRegex.Match(logLine);
                    if (match.Success)
                    {
	                    if(GameV2.GetHeroNameFromId(match.Groups["id"].Value, false) != null)
		                    game.NewArenaDeck(match.Groups["id"].Value);
	                    else
	                    {
							var cardId = match.Groups["id"].Value;
		                    var timeSinceLastChoice = DateTime.Now.Subtract(_lastChoice).Milliseconds;

                            if(_lastChoiceId == cardId &&  timeSinceLastChoice < 1000)
		                    {
			                    Logger.WriteLine(string.Format("Card with the same ID ({0}) was chosen less {1} ms ago. Ignoring.", cardId, timeSinceLastChoice));
			                    return;
		                    }

                            game.NewArenaCard(cardId);

		                    _lastChoice = DateTime.Now;
		                    _lastChoiceId = cardId;
	                    }
                    }
                }
            }
        }
    }
}