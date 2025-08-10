using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Neutral;
using static Hearthstone_Deck_Tracker.LogReader.LogConstants.Choices;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers;

internal class ChoicesHandler
{
	private IChoiceWithId? _tmpChoice { get; set; } = null;

	public void Handle(string logLine, IHsGameState gameState, IGame game)
	{
		if(ChoicesHeaderRegex.IsMatch(logLine))
		{
			if(_tmpChoice != null)
				Flush(gameState, game);

			var match = ChoicesHeaderRegex.Match(logLine);
			var choiceId = int.Parse(match.Groups["id"].Value);
			int? taskList = match.Groups["taskList"].Success ? int.Parse(match.Groups["taskList"].Value) : null;
			var playerName = match.Groups["player"].Value;
			var playerId = gameState.PlayerIdsByPlayerName.TryGetValue(playerName, out var pi) ? (int?)pi : null;
			var choiceType = Enum.TryParse(match.Groups["choiceType"].Value, out ChoiceType ct) ? ct : ChoiceType.INVALID;

			if(playerId.HasValue)
			{
				_tmpChoice = new ChoiceBuilder(choiceId, taskList, playerId.Value, choiceType);
			}
		}
		else if(ChoicesSourceRegex.IsMatch(logLine))
		{
			if(_tmpChoice == null || _tmpChoice is not ChoiceBuilder cb)
				return;

			var match = ChoicesSourceRegex.Match(logLine);
			cb.SourceEntityId = int.Parse(match.Groups["id"].Value);
		}
		else if(ChosenHeaderRegex.IsMatch(logLine))
		{
			var match = ChosenHeaderRegex.Match(logLine);
			var id = int.Parse(match.Groups["id"].Value);
			if(_tmpChoice != null)
				Flush(gameState, game);

			if(!gameState.ChoicesById.TryGetValue(id, out var c) || c is not OfferedChoice offeredChoice)
				return;

			var playerName = match.Groups["player"].Value;
			var playerId = gameState.PlayerIdsByPlayerName.TryGetValue(playerName, out var pi) ? (int?)pi : null;
			if(offeredChoice.PlayerId != playerId)
				// the choice refers to a different player than originally - probably something is corrupt.
				return;

			_tmpChoice = offeredChoice;
		}
		else if(ChoicesEntityRegex.IsMatch(logLine))
		{
			if(_tmpChoice == null)
				return;

			var match = ChoicesEntityRegex.Match(logLine);
			var id = int.Parse(match.Groups["id"].Value);

			if(_tmpChoice is ChoiceBuilder cb)
			{
				cb.AttachOfferedEntity(id);
			}
			else if(_tmpChoice is OfferedChoice tc)
			{
				tc.AttachChosenEntity(id);
			}
		}
		else if(EndTaskListRegex.IsMatch(logLine))
		{
			if(_tmpChoice != null)
				Flush(gameState, game);

			var match = EndTaskListRegex.Match(logLine);
			var taskList = int.Parse(match.Groups["taskList"].Value);
			if(gameState.ChoicesByTaskList.TryGetValue(taskList, out var choices))
			{
				foreach(var choice in choices)
				{
					if(choice is OfferedChoice tc)
					{
						if(tc.PlayerId == game.Player.Id)
							gameState.GameHandler?.HandlePlayerEntityChoices(tc);
					}
				}
				gameState.ChoicesByTaskList.Remove(taskList);
			}
		}
	}

	public void Flush(IHsGameState gameState, IGame game)
	{
		if(_tmpChoice == null)
			return;

		if(_tmpChoice is ChoiceBuilder cb)
		{
			var choice = cb.BuildOfferedChoice();
			gameState.ChoicesById[cb.Id] = choice;
			var taskList = cb.TaskList;
			if(taskList is not int tl)
			{
				// without a task list the can emit the choice immediately
				if(choice.PlayerId == game.Player.Id)
					gameState.GameHandler?.HandlePlayerEntityChoices(choice);
			}
			else {
				// if the choice has a task list, we need to queue it up to show later when the task list ends
				if(!gameState.ChoicesByTaskList.ContainsKey(tl))
				{
					gameState.ChoicesByTaskList[tl] = new List<IHsChoice>();
				}

				gameState.ChoicesByTaskList[tl].Add(choice);
			}
		}
		else if(_tmpChoice is OfferedChoice tc)
		{
			var choice = tc.BuildCompletedChoice();
			gameState.ChoicesById[tc.Id] = choice;
			if(choice.PlayerId == game.Player.Id)
				gameState.GameHandler?.HandlePlayerEntitiesChosen(choice);
			if(choice.PlayerId == game.Opponent.Id)
				gameState.GameHandler?.HandleOpponentEntitiesChosen(choice, gameState);
		}
		_tmpChoice = null;
	}


	private interface IChoiceWithId
	{
		public int Id { get; }
	}

	private class ChoiceBuilder : IChoiceWithId
	{
		public int Id { get; }
		public int? TaskList { get; }
		public int PlayerId { get; set; }
		public ChoiceType ChoiceType { get; set;  }
		public int? SourceEntityId { get; set; }

		public ChoiceBuilder(int id, int? taskList, int playerId, ChoiceType choiceType)
		{
			Id = id;
			TaskList = taskList;
			PlayerId = playerId;
			ChoiceType = choiceType;
		}

		private List<int> OfferedEntityIds { get; } = new();
		public void AttachOfferedEntity(int entityId)
		{
			OfferedEntityIds.Add(entityId);
		}

		public OfferedChoice BuildOfferedChoice()
		{
			return new OfferedChoice(
				id: Id,
				taskList: TaskList,
				playerId: PlayerId,
				choiceType: ChoiceType,
				sourceEntityId: SourceEntityId ?? 1,
				offeredEntityIds: OfferedEntityIds
			);
		}
	}

	private class OfferedChoice : IHsChoice, IChoiceWithId
	{
		public int Id { get; }
		public int? TaskList { get; }
		public int PlayerId { get; set; }
		public ChoiceType ChoiceType { get;  }
		public int SourceEntityId { get; }
		public IEnumerable<int> OfferedEntityIds { get; }

		public OfferedChoice(int id, int? taskList, int playerId, ChoiceType choiceType, int sourceEntityId, IEnumerable<int> offeredEntityIds)
		{
			Id = id;
			TaskList = taskList;
			PlayerId = playerId;
			ChoiceType = choiceType;
			SourceEntityId = sourceEntityId;
			OfferedEntityIds = offeredEntityIds;
		}

		private List<int> ChosenEntityIds { get; } = new();
		public void AttachChosenEntity(int entityId)
		{
			ChosenEntityIds.Add(entityId);
		}

		public CompletedChoice BuildCompletedChoice()
		{
			return new CompletedChoice(
				id: Id,
				taskList: TaskList,
				playerId: PlayerId,
				choiceType: ChoiceType,
				sourceEntityId: SourceEntityId,
				offeredEntityIds: OfferedEntityIds,
				chosenEntityIds: ChosenEntityIds
			);
		}
	}

	private class CompletedChoice : OfferedChoice, IHsCompletedChoice
	{
		public IEnumerable<int> ChosenEntityIds { get; }

		public CompletedChoice(int id, int? taskList, int playerId, ChoiceType choiceType, int sourceEntityId, IEnumerable<int> offeredEntityIds, IEnumerable<int> chosenEntityIds) :
			base(id: id, taskList: taskList, playerId: playerId, choiceType: choiceType, sourceEntityId: sourceEntityId, offeredEntityIds: offeredEntityIds)
		{
			ChosenEntityIds = chosenEntityIds;
		}
	}

}
