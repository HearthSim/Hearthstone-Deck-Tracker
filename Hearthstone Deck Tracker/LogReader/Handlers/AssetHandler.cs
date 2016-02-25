#region

using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using static Hearthstone_Deck_Tracker.LogReader.HsLogReaderConstants;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	public class AssetHandler
	{
		public void Handle(string logLine, IHsGameState gameState, IGame game)
		{
			if(!UnloadCardRegex.IsMatch(logLine))
				return;
			var id = UnloadCardRegex.Match(logLine).Groups["id"].Value;
			if(game.CurrentMode == Mode.DRAFT && game.PreviousMode == Mode.HUB)
				gameState.GameHandler.HandlePossibleArenaCard(id);
			else if((game.CurrentMode == Mode.COLLECTIONMANAGER || game.CurrentMode == Mode.TAVERN_BRAWL) && game.PreviousMode == Mode.HUB)
				gameState.GameHandler.HandlePossibleConstructedCard(id, true);
		}
	}
}