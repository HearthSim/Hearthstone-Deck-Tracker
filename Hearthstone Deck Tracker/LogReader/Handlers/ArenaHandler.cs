#region

using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using HearthWatcher.LogReader;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	public class ArenaHandler
	{
		public void Handle(LogLine logLine, IHsGameState gameState, IGame game)
		{
			if(logLine.Line.Contains("IN_REWARDS") && game.CurrentMode == Mode.DRAFT)
				Watchers.ArenaWatcher.Update();
		}
	}
}
