using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	internal class SendChoicesHandler
	{
		private Choice? Choice;

		public void SendChoices(int id, ChoiceType choiceType, IHsGameState gameState, IGame game)
		{
			Choice = new Choice(id, choiceType);
		}

		public void SendChoice(int index, int entityId, IHsGameState gameState, IGame game)
		{
			if(Choice is null) return;

			if(game.Entities.TryGetValue(entityId, out var entity))
				Choice.AttachChosenEntity(index, entity);
		}

		public void Flush(IHsGameState gameState, IGame game)
		{
			if(Choice is null)  return;

			gameState.GameHandler?.HandlePlayerSendChoices(Choice);
			Choice = null;
		}
	}
}
