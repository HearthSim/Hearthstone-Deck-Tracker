using Hearthstone_Deck_Tracker.Utility.Logging;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	internal class BattlegroundsBoardState
	{
		Dictionary<int, BoardSnapshot> LastKnownBoardState { get; } = new Dictionary<int, BoardSnapshot>();

		Dictionary<int, BoardSnapshot> LastKnownBoardStateByHeroEntityId { get; } = new Dictionary<int, BoardSnapshot>();

		private GameV2 _game;

		public BattlegroundsBoardState(GameV2 game)
		{
			_game = game;
		}

		public void SnapshotCurrentBoard()
		{
			var opponentHero = _game.Entities.Values
				.FirstOrDefault(x => x.IsHero && x.IsInZone(Zone.PLAY) && x.IsControlledBy(_game.Opponent.Id));
			if(opponentHero?.CardId == null)
				return;
			var heroEntityId = opponentHero.Id;
			var playerId = opponentHero.GetTag(GameTag.PLAYER_ID);
			if(playerId == 0)
				return;
			var entities = _game.Entities.Values
				.Where(x => x.IsMinion && x.IsInZone(HearthDb.Enums.Zone.PLAY) && x.IsControlledBy(_game.Opponent.Id))
				.Select(x => x.Clone())
				.ToArray();
			var snapshot = new BoardSnapshot(entities, _game.GetTurnNumber());
			Log.Info($"Snapshotting board state for {opponentHero.Card.Name} with player id {playerId} hero entity {heroEntityId} ({entities.Length} entities)");
			LastKnownBoardState[playerId] = snapshot;
			LastKnownBoardStateByHeroEntityId[heroEntityId] = snapshot;
		}

		public BoardSnapshot? GetSnapshot(int entityId)
		{
			if(!_game.Entities.TryGetValue(entityId, out var entity))
			{
				Log.Info($"Entity {entityId} not found in game entities, trying direct lookup");
				return LastKnownBoardStateByHeroEntityId.TryGetValue(entityId, out var directState) ? directState : null;
			}

			return LastKnownBoardState.TryGetValue(entity.GetTag(GameTag.PLAYER_ID), out var state) ? state : null;
		}

		public void Reset()
		{
			LastKnownBoardState.Clear();
			LastKnownBoardStateByHeroEntityId.Clear();
		}
	}
}
