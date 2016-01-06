#region

using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using static Hearthstone_Deck_Tracker.Enums.GameMode;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	class LoadingScreenHandler
	{
		public void Handle(string logLine, IHsGameState gameState, IGame game)
		{
			var match = HsLogReaderConstants.GameModeRegex.Match(logLine);
			if(!match.Success)
				return;
			var prev = match.Groups["prev"].Value;
			var newMode = GetGameMode(match.Groups["curr"].Value) ?? GetGameMode(prev);
			if(newMode.HasValue && !(game.CurrentGameMode == Ranked && newMode.Value == Casual))
				game.CurrentGameMode = newMode.Value;
			if(prev == "GAMEPLAY")
				gameState.GameHandler.HandleInMenu();
		}

		private GameMode? GetGameMode(string mode)
		{
			switch(mode)
			{
				case "ADVENTURE":
					return Practice;
				case "TAVERN_BRAWL":
					return Brawl;
				case "TOURNAMENT":
					return Casual;
				case "DRAFT":
					return Arena;
				case "FRIENDLY":
					return Friendly;
				default:
					return null;
			}
		}
	}
}