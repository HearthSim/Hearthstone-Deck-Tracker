using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	public class NetHandler
	{
		public void Handle(string logLine, IGame game)
		{
			var match = HsLogReaderConstants.ConnectionRegex.Match(logLine);
			if(match.Success)
			{
				game.MetaData.ServerAddress = match.Groups["address"].Value;
				game.MetaData.ClientId = match.Groups["client"].Value;
				game.MetaData.GameId = match.Groups["game"].Value;
				game.MetaData.SpectateKey = match.Groups["spectateKey"].Value;
			}
		}
	}
}