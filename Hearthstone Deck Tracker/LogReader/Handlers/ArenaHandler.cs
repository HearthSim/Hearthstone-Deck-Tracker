#region

using System;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static Hearthstone_Deck_Tracker.LogReader.HsLogReaderConstants;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	public class ArenaHandler
	{
		private DateTime _lastChoice = DateTime.MinValue;
		private string _lastChoiceId = "";

		public void Handle(string logLine, IHsGameState gameState, IGame game)
		{
			var match = ExistingHeroRegex.Match(logLine);
			if(match.Success)
				game.NewArenaDeck(match.Groups["id"].Value);
			else
			{
				match = ExistingCardRegex.Match(logLine);
				if(match.Success)
				{
					try
					{
						game.NewArenaCard(match.Groups["id"].Value);
					}
					catch(Exception ex)
					{
						Log.Error("Error adding arena card: " + ex);
					}
				}
				else
				{
					match = NewChoiceRegex.Match(logLine);
					if(!match.Success)
						return;
					if(Database.GetHeroNameFromId(match.Groups["id"].Value, false) != null)
						game.NewArenaDeck(match.Groups["id"].Value);
					else
					{
						var cardId = match.Groups["id"].Value;
							var timeSinceLastChoice = DateTime.Now.Subtract(_lastChoice).TotalMilliseconds;

						if(_lastChoiceId == cardId && timeSinceLastChoice < 1000)
						{
							Log.Warn($"Card with the same ID ({cardId}) was chosen less {timeSinceLastChoice} ms ago. Ignoring.");
							return;
						}

						try
						{
							game.NewArenaCard(cardId);
						}
						catch(Exception ex)
						{
							Log.Error("Error adding arena card: " + ex);
						}

						_lastChoice = DateTime.Now;
						_lastChoiceId = cardId;
					}
				}
			}
		}
	}
}