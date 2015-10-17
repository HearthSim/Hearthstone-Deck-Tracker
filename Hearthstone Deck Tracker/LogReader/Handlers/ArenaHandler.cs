using System;
using System.Globalization;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

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
	            if(match.Success)
	            {
		            try
					{
						game.NewArenaCard(match.Groups["id"].Value);
					}
		            catch(Exception ex)
		            {
						Logger.WriteLine("Error adding arena card: " + ex, "ArenaHandler");
		            }
	            }
                else
                {
                    match = HsLogReaderConstants.NewChoiceRegex.Match(logLine);
                    if (match.Success)
                    {
	                    if(Database.GetHeroNameFromId(match.Groups["id"].Value, false) != null)
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

							try
							{
								game.NewArenaCard(cardId);
							}
							catch (Exception ex)
							{
								Logger.WriteLine("Error adding arena card: " + ex, "ArenaHandler");
							}

		                    _lastChoice = DateTime.Now;
		                    _lastChoiceId = cardId;
	                    }
                    }
                }
            }
        }
    }
}