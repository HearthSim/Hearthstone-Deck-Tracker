#region

using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	public class NetHandler
	{
		public void Handle(string logLine, IHsGameState gameState, IGame game)
		{
			var match = HsLogReaderConstants.ConnectionRegex.Match(logLine);
			if(match.Success)
			{
				game.MetaData.ServerAddress = match.Groups["address"].Value;
				game.MetaData.ClientId = match.Groups["client"].Value;
				game.MetaData.GameId = match.Groups["game"].Value;
				game.MetaData.SpectateKey = match.Groups["spectateKey"].Value;

				gameState.Reset();
				gameState.GameHandler.HandleGameStart();
				gameState.GameLoaded = true;
			}
		}
	}
}