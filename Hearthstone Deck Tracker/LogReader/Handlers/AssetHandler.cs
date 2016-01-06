#region

using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using static Hearthstone_Deck_Tracker.LogReader.HsLogReaderConstants;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader
{
	public class AssetHandler
	{
		public void Handle(string logLine, IHsGameState gameState, IGame game)
		{
			if(!UnloadCardRegex.IsMatch(logLine))
				return;
			var id = UnloadCardRegex.Match(logLine).Groups["id"].Value;
			if(game.CurrentGameMode == GameMode.Arena)
				gameState.GameHandler.HandlePossibleArenaCard(id);
			else
				gameState.GameHandler.HandlePossibleConstructedCard(id, true);
		}
	}
}