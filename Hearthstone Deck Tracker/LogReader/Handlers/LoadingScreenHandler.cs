#region

using System;
using System.Diagnostics;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using static Hearthstone_Deck_Tracker.Enums.GameMode;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	public class LoadingScreenHandler
	{
		public void Handle(LogLineItem logLine, IHsGameState gameState, IGame game)
		{
			var match = HsLogReaderConstants.GameModeRegex.Match(logLine.Line);
			if(!match.Success)
				return;
			game.CurrentMode = GetMode(match.Groups["curr"].Value);
			game.PreviousMode = GetMode(match.Groups["prev"].Value);

			if(game.PreviousMode == Mode.GAMEPLAY)
				gameState.GameHandler.HandleInMenu();
		}

		private Mode GetMode(string modeString)
		{
			Mode mode;
			if(Enum.TryParse(modeString, out mode))
				return mode;
			return Mode.INVALID;
		}
	}
}