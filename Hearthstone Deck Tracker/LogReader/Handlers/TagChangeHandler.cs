#region

using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Replay;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	internal class TagChangeHandler
	{
		private readonly TagChangeActions _tagChangeActions = new TagChangeActions();
		private readonly Queue<Tuple<int, Action>> _creationTagActionQueue = new Queue<Tuple<int, Action>>();

		public void TagChange(IHsGameState gameState, string rawTag, int id, string rawValue, IGame game, bool isCreationTag = false)
		{
			var tag = LogReaderHelper.ParseEnum<GameTag>(rawTag);
			var value = LogReaderHelper.ParseTag(tag, rawValue);
			TagChange(gameState, tag, id, value, game, isCreationTag);
		}

		public void TagChange(IHsGameState gameState, GameTag tag, int id, int value, IGame game, bool isCreationTag = false)
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

			var prevValue = game.Entities[id].GetTag(tag);
			game.Entities[id].SetTag(tag, value);

			if(isCreationTag)
			{
				var action = _tagChangeActions.FindAction(tag, game, gameState, id, value, prevValue);
				if(action != null)
					_creationTagActionQueue.Enqueue(new Tuple<int, Action>(id, action));
			}
			else
				_tagChangeActions.FindAction(tag, game, gameState, id, value, prevValue)?.Invoke();
		}
		

		public void InvokeQueuedActions(IGame game)
		{
			while(_creationTagActionQueue.Any())
			{
				var item = _creationTagActionQueue.Dequeue();
				item.Item2?.Invoke();
				Entity entity;
				if(_creationTagActionQueue.All(x => x.Item1 != item.Item1) && game.Entities.TryGetValue(item.Item1, out entity))
					entity.Info.HasOutstandingTagChanges = false;
			}
		}

		public void ClearQueuedActions() => _creationTagActionQueue.Clear();

	}
}