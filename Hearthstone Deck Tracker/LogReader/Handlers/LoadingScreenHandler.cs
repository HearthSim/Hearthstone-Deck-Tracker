#region

using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	class LoadingScreenHandler
	{
		public void Handle(string logLine, IHsGameState gameState, IGame game)
		{
			var match = HsLogReaderConstants.GameModeRegex.Match(logLine);
			if(match.Success)
			{
				var prev = match.Groups["prev"].Value;
				var newMode = GetGameMode(match.Groups["curr"].Value) ?? GetGameMode(prev);
				if(newMode.HasValue && !(game.CurrentGameMode == GameMode.Ranked && newMode.Value == GameMode.Casual))
					game.CurrentGameMode = newMode.Value;
				if(prev == "GAMEPLAY")
					gameState.GameHandler.HandleInMenu();
			}
		}

		private GameMode? GetGameMode(string mode)
		{
			switch(mode)
			{
				case "ADVENTURE":
					return GameMode.Practice;
				case "TAVERN_BRAWL":
					return GameMode.Brawl;
				case "TOURNAMENT":
					return GameMode.Casual;
				case "DRAFT":
					return GameMode.Arena;
				case "FRIENDLY":
					return GameMode.Friendly;
				default:
					return null;
			}
		}
	}
}