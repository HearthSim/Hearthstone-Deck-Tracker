#region

using System;
using System.Collections.Generic;
using System.Linq;
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
			KnownCardIds = new Dictionary<int, string>();
		}

		public int AddToTurn { get; set; }
		public bool CurrentEntityHasCardId { get; set; }
		public int CurrentEntityId { get; set; }
		public long CurrentOffset { get; set; }
		public bool DoUpdate { get; set; }
		public bool First { get; set; }
		public bool GameEnded { get; set; }
		public IGameHandler GameHandler { get; set; }
		public DateTime LastGameStart { get; set; }
		public int LastId { get; set; }
		public int MaxId { get; set; }
		public bool OpponentUsedHeroPower { get; set; }
		public bool PlayerUsedHeroPower { get; set; }
		public long PreviousSize { get; set; }
		public ReplayKeyPoint ProposedKeyPoint { get; set; }
		public dynamic WaitForController { get; set; }
		public bool FoundSpectatorStart { get; set; }
		public int JoustReveals { get; set; }
		public Dictionary<int, string> KnownCardIds { get; set; }
		public int LastCardPlayed { get; set; }
		public bool WasInProgress { get; set; }
		public bool SetupDone { get; set; }

		public void ProposeKeyPoint(KeyPointType type, int id, ActivePlayer player)
		{
			if(ProposedKeyPoint != null)
				ReplayMaker.Generate(ProposedKeyPoint.Type, ProposedKeyPoint.Id, ProposedKeyPoint.Player, _game);
			ProposedKeyPoint = new ReplayKeyPoint(null, type, id, player);
		}

		public int GetTurnNumber()
		{
			if(!_game.IsMulliganDone)
				return 0;
			if(AddToTurn == -1)
			{
				var firstPlayer = _game.Entities.FirstOrDefault(e => e.Value.HasTag(GAME_TAG.FIRST_PLAYER));
				if(firstPlayer.Value != null)
					AddToTurn = firstPlayer.Value.GetTag(GAME_TAG.CONTROLLER) == _game.Player.Id ? 0 : 1;
			}
			var entity = _game.Entities.FirstOrDefault(e => e.Value != null && e.Value.Name == "GameEntity").Value;
			if(entity != null)
				return (entity.Tags[GAME_TAG.TURN] + (AddToTurn == -1 ? 0 : AddToTurn)) / 2;
			return 0;
		}

		public bool PlayersTurn()
		{
			var firstPlayer = _game.Entities.FirstOrDefault(e => e.Value.HasTag(GAME_TAG.FIRST_PLAYER)).Value;
			if(firstPlayer != null)
			{
				var offset = firstPlayer.IsPlayer ? 0 : 1;
				var gameRoot = _game.Entities.FirstOrDefault(e => e.Value != null && e.Value.Name == "GameEntity").Value;
				if(gameRoot != null)
					return (gameRoot.Tags[GAME_TAG.TURN] + offset) % 2 == 1;
			}
			return false;
		}

		public void GameEndKeyPoint(bool victory, int id)
		{
			if(ProposedKeyPoint != null)
			{
				ReplayMaker.Generate(ProposedKeyPoint.Type, ProposedKeyPoint.Id, ProposedKeyPoint.Player, _game);
				ProposedKeyPoint = null;
			}
			ReplayMaker.Generate(victory ? KeyPointType.Victory : KeyPointType.Defeat, id, ActivePlayer.Player, _game);
		}

		public void Reset()
		{
			First = true;
			AddToTurn = -1;
			GameEnded = false;
			FoundSpectatorStart = false;
			JoustReveals = 0;
			KnownCardIds.Clear();
			LastGameStart = DateTime.Now;
			WaitForController = null;
			MaxId = 0;
			WasInProgress = false;
			SetupDone = false;
		}
	}
}