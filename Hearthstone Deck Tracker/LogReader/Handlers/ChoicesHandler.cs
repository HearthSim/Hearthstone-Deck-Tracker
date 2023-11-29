using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using System;
using HearthDb.Enums;
using static Hearthstone_Deck_Tracker.LogReader.LogConstants.Choices;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	internal class ChoicesHandler
	{
		SendChoicesHandler _sendChoicesHandler = new SendChoicesHandler();

		public void Handle(string logLine, IHsGameState gameState, IGame game)
		{
			if(SendChoicesHeaderRegex.IsMatch(logLine))
			{
				var match = SendChoicesHeaderRegex.Match(logLine);
				var choiceId = int.Parse(match.Groups["id"].Value);
				if(!Enum.TryParse(match.Groups["choiceType"].Value, out ChoiceType choiceType))
					choiceType = ChoiceType.INVALID;
				_sendChoicesHandler.SendChoices(choiceId, choiceType, gameState, game);
			}
			else if(SendChoicesBodyRegex.IsMatch(logLine))
			{
				var match = SendChoicesBodyRegex.Match(logLine);
				var index = int.Parse(match.Groups["index"].Value);
				var entityId = int.Parse(match.Groups["id"].Value);
				_sendChoicesHandler.SendChoice(index, entityId, gameState, game);
			}
			else
			{
				// Terminate the current Choice.
				_sendChoicesHandler.Flush(gameState, game);
			}
		}
	}
}
