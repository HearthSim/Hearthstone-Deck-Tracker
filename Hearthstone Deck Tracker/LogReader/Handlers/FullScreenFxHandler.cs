#region

using System;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	public class FullScreenFxHandler
	{
		public void Handle(LogLineItem logLine, IGame game)
		{
			var match = HsLogReaderConstants.BeginBlurRegex.Match(logLine.Line);
			if(match.Success && game.IsInMenu
			   && (game.CurrentMode == Mode.TAVERN_BRAWL || game.CurrentMode == Mode.TOURNAMENT || game.CurrentMode == Mode.DRAFT))
			{
				game.MetaData.EnqueueTime = logLine.Time;
				Log.Info($"Now in queue ({logLine.Time})");
			}
		}
	}
}