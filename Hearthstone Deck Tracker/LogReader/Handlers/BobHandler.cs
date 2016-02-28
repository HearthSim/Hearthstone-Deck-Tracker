#region

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
			if(logLine == HsLogReaderConstants.ReconnectMessage)
				game.StorePowerLog();
		}
	}
}