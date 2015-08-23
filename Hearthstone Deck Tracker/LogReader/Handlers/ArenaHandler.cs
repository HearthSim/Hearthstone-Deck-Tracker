using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
    public class ArenaHandler
    {
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
                        if (GameV2.GetHeroNameFromId(match.Groups["id"].Value, false) != null)
                            game.NewArenaDeck(match.Groups["id"].Value);
                        else
                            game.NewArenaCard(match.Groups["id"].Value);
                    }
                }
            }
        }
    }
}