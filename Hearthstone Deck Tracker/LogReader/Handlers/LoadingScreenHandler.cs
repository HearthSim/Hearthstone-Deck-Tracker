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
		public void Handle(string logLine, IHsGameState gameState, IGame game)
		{
			var match = HsLogReaderConstants.GameModeRegex.Match(logLine);
			if(!match.Success)
				return;
			game.CurrentMode = GetMode(match.Groups["curr"].Value);
			game.PreviousMode = GetMode(match.Groups["prev"].Value);

			var newMode = GetGameMode(game.CurrentMode) ?? GetGameMode(game.PreviousMode);
			if(newMode.HasValue && !(game.CurrentGameMode == Ranked && newMode.Value == Casual))
				game.CurrentGameMode = newMode.Value;
			if(game.PreviousMode == Mode.GAMEPLAY)
				gameState.GameHandler.HandleInMenu();
			switch(game.CurrentMode)
			{
				case Mode.COLLECTIONMANAGER:
				case Mode.TAVERN_BRAWL:
					gameState.GameHandler.ResetConstructedImporting();
					break;
				case Mode.DRAFT:
					game.ResetArenaCards();
					break;
			}
		}

		private GameMode? GetGameMode(Mode mode)
		{
			switch(mode)
			{
				case Mode.TOURNAMENT:
					return Casual;
				case Mode.FRIENDLY:
					return Friendly;
				case Mode.DRAFT:
					return Arena;
				case Mode.ADVENTURE:
					return Practice;
				case Mode.TAVERN_BRAWL:
					return Brawl;
				default:
					return null;
			}
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