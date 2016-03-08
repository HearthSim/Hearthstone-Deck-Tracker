#region

using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Replay;
using static Hearthstone_Deck_Tracker.Enums.GAME_TAG;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	internal class TagChangeHandler
	{
		private readonly TagChangeActions _tagChangeActions = new TagChangeActions();
		private readonly Queue<Action> _creationTagActionQueue = new Queue<Action>();

		public void TagChange(IHsGameState gameState, string rawTag, int id, string rawValue, IGame game, bool isCreationTag = false)
		{
			var tag = LogReaderHelper.ParseEnum<GAME_TAG>(rawTag);
			var value = LogReaderHelper.ParseTag(tag, rawValue);
			TagChange(gameState, tag, id, value, game, isCreationTag);
		}

		public void TagChange(IHsGameState gameState, GAME_TAG tag, int id, int value, IGame game, bool isCreationTag = false)
		{
			if(gameState.LastId != id)
			{
				if(gameState.ProposedKeyPoint != null)
				{
					ReplayMaker.Generate(gameState.ProposedKeyPoint.Type, gameState.ProposedKeyPoint.Id, gameState.ProposedKeyPoint.Player, game);
					gameState.ProposedKeyPoint = null;
				}
			}
			gameState.LastId = id;
			if(id > gameState.MaxId)
				gameState.MaxId = id;
			if(!game.Entities.ContainsKey(id))
				game.Entities.Add(id, new Entity(id));

			if(!gameState.DeterminedPlayers)
			{
				var entity = game.Entities[id];
				if(tag == CONTROLLER && entity.IsInHand && string.IsNullOrEmpty(entity.CardId))
					DeterminePlayers(gameState, game, value);
			}

			var prevValue = game.Entities[id].GetTag(tag);
			game.Entities[id].SetTag(tag, value);

			if(isCreationTag)
				_tagChangeActions.FindAction(tag, game, gameState, id, value, prevValue)?.Enqueue(_creationTagActionQueue);
			else
				_tagChangeActions.FindAction(tag, game, gameState, id, value, prevValue)?.Invoke();
		}
		

		public void InvokeQueuedActions()
		{
			while(_creationTagActionQueue.Any())
				_creationTagActionQueue.Dequeue().Invoke();
		}

		public void ClearQueuedActions() => _creationTagActionQueue.Clear();

		internal void DeterminePlayers(IHsGameState gameState, IGame game, int playerId, bool isOpponentId = true)
		{
			if(isOpponentId)
			{
				game.Entities.FirstOrDefault(e => e.Value.GetTag(PLAYER_ID) == 1).Value?.SetPlayer(playerId != 1);
				game.Entities.FirstOrDefault(e => e.Value.GetTag(PLAYER_ID) == 2).Value?.SetPlayer(playerId == 1);
				game.Player.Id = playerId % 2 + 1;
				game.Opponent.Id = playerId;
			}
			else
			{
				game.Entities.FirstOrDefault(e => e.Value.GetTag(PLAYER_ID) == 1).Value?.SetPlayer(playerId == 1);
				game.Entities.FirstOrDefault(e => e.Value.GetTag(PLAYER_ID) == 2).Value?.SetPlayer(playerId != 1);
				game.Player.Id = playerId;
				game.Opponent.Id = playerId % 2 + 1;
			}
			if(gameState.WasInProgress)
			{
				var playerName = game.GetStoredPlayerName(game.Player.Id);
				if(!string.IsNullOrEmpty(playerName))
					game.Player.Name = playerName;
				var opponentName = game.GetStoredPlayerName(game.Opponent.Id);
				if(!string.IsNullOrEmpty(opponentName))
					game.Opponent.Name = opponentName;
			}
			gameState.DeterminedPlayers = game.PlayerEntity != null;
		}
	}

	public static class ActionExtensions
	{
		public static void Enqueue(this Action action, Queue<Action> queue) => queue.Enqueue(action);
	}
}