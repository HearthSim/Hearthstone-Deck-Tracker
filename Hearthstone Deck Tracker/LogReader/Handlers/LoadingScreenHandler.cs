#region

using System;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	public class LoadingScreenHandler
	{
		private DateTime _lastAutoImport;
		public void Handle(LogLineItem logLine, IHsGameState gameState, IGame game)
		{
			var match = HsLogReaderConstants.GameModeRegex.Match(logLine.Line);
			if(!match.Success)
				return;
			game.CurrentMode = GetMode(match.Groups["curr"].Value);
			game.PreviousMode = GetMode(match.Groups["prev"].Value);

			if((DateTime.Now - logLine.Time).TotalSeconds < 5 && _lastAutoImport < logLine.Time && game.CurrentMode == Mode.TOURNAMENT)
			{
				_lastAutoImport = logLine.Time;
				var decks = DeckImporter.FromConstructed();
				if(decks.Any() && (Config.Instance.ConstructedAutoImportNew || Config.Instance.ConstructedAutoUpdate))
					DeckManager.ImportDecks(decks, false, Config.Instance.ConstructedAutoImportNew, Config.Instance.ConstructedAutoUpdate);
			}

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