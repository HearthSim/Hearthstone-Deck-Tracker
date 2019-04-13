#region

using System;
using System.Collections.Generic;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Replay;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader
{
	public class HsGameState : IHsGameState
	{
		private readonly GameV2 _game;

		public HsGameState(GameV2 game)
		{
			_game = game;
			KnownCardIds = new Dictionary<int, IList<string>>();
		}
		public bool CurrentEntityHasCardId { get; set; }
		public int CurrentEntityId { get; private set; }
		public bool GameEnded { get; set; }
		public IGameHandler GameHandler { get; set; }
		public DateTime LastGameStart { get; set; }
		public int LastId { get; set; }
		public bool OpponentUsedHeroPower { get; set; }
		public bool PlayerUsedHeroPower { get; set; }
		public bool FoundSpectatorStart { get; set; }
		public int JoustReveals { get; set; }
		public Dictionary<int, IList<string>> KnownCardIds { get; set; }
		public int LastCardPlayed { get; set; }
		public bool WasInProgress { get; set; }
		public int GameTriggerCount { get; set; }
		public Zone CurrentEntityZone { get; set; }
		public bool DeterminedPlayers => _game.Player.Id > 0 && _game.Opponent.Id > 0;
		public Tuple<int, string> ChameleosReveal { get; set; }

		public int GetTurnNumber()
		{
			return _game.GetTurnNumber();
		}

		public void Reset()
		{
			GameEnded = false;
			JoustReveals = 0;
			KnownCardIds.Clear();
			LastGameStart = DateTime.Now;
			WasInProgress = false;
			CurrentEntityId = 0;
			GameTriggerCount = 0;
			CurrentBlock = null;
			_maxBlockId = 0;
		}

		public void SetCurrentEntity(int id)
		{
			CurrentEntityId = id;
		}

		public void ResetCurrentEntity() => CurrentEntityId = 0;

		private int _maxBlockId;
		public Block CurrentBlock { get; private set; }

		public void BlockStart(string type, string cardId, string target)
		{
			var blockId = _maxBlockId++;
			CurrentBlock = CurrentBlock?.CreateChild(blockId, type, cardId, target) ?? new Block(null, blockId, type, cardId, target);
		}

		public void BlockEnd()
		{
			CurrentBlock = CurrentBlock?.Parent;
			if(_game.Entities.TryGetValue(CurrentEntityId, out var entity))
				entity.Info.HasOutstandingTagChanges = false;
		}
	}

	public class Block
	{
		public Block Parent { get; }
		public IList<Block> Children { get; }
		public int Id { get; }
		public string Type { get; }
		public string CardId { get; }
		public string Target { get; }

		public Entity EntityDiscardedByArchivist { get; set; }

		public Block(Block parent, int blockId, string type, string cardId, string target)
		{
			Parent = parent;
			Children = new List<Block>();
			Id = blockId;
			Type = type;
			CardId = cardId;
			Target = target;
		}

		public Block CreateChild(int blockId, string type, string cardId, string target) => new Block(this, blockId, type, cardId, target);
	}
}
