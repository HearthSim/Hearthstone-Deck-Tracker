#region

using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	public class BobHandler
	{
		public void Handle(string logLine, IHsGameState gameState, IGame game)
		{
			var match = HsLogReaderConstants.LegendRankRegex.Match(logLine);
			if(match.Success)
			{
				var rank = int.Parse(match.Groups["rank"].Value);
				game.MetaData.LegendRank = rank;
				return;
			}
			if(!logLine.Contains("---Register"))
				return;

			if(logLine.Contains("---RegisterScreenBox---"))
			{
				if(game.CurrentGameMode == GameMode.Spectator)
					gameState.GameEnd();
			}
			else if(logLine.Contains("---RegisterScreenForge---"))
				game.ResetArenaCards();
			else if(logLine.Contains("---RegisterProfileNotices---"))
				gameState.GameLoaded = true;
			else if(logLine.Contains("---RegisterFriendChallenge---"))
				gameState.GameHandler.HandleInMenu();
			else if(logLine.Contains("---RegisterScreenCollectionManager---"))
				gameState.GameHandler.ResetConstructedImporting();
		}
	}
}