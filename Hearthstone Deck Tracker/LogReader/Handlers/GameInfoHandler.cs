using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static Hearthstone_Deck_Tracker.LogReader.LogConstants.GameInfo;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers;

public class GameInfoHandler
{
	public void Handle(string logLine, IHsGameState gameState, IGame game)
	{
		if(PlayerRegex.IsMatch(logLine))
		{
			var match = PlayerRegex.Match(logLine);
			var playerId = int.Parse(match.Groups["playerId"].Value);
			var playerName = match.Groups["playerName"].Value;
			if(playerName != "UNKNOWN HUMAN PLAYER")
			{
				gameState.PlayerIdsByPlayerName[playerName] = playerId;
			}
		}
	}
}
