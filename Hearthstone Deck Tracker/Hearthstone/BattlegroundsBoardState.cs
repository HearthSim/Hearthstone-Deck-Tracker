using Hearthstone_Deck_Tracker.Utility.Logging;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	internal class BattlegroundsBoardState
	{
		Dictionary<int, BoardSnapshot> LastKnownBoardState { get; } = new Dictionary<int, BoardSnapshot>();

		private GameV2 _game;

		public BattlegroundsBoardState(GameV2 game)
		{
			_game = game;
		}

		public void SnapshotCurrentBoard()
		{
			var opponentHero = _game.Entities.Values
				.FirstOrDefault(x => x.IsHero && x.IsInZone(Zone.PLAY) && x.IsControlledBy(_game.Opponent.Id));
			if(opponentHero == null || opponentHero.CardId == null)
				return;
			var leaderboardEntityId = _game.Entities.Values.Where(x => x.IsHero && x.HasTag(GameTag.PLAYER_LEADERBOARD_PLACE))
				.Where(x => x.CardId == opponentHero.CardId)
				.Select(x => x.Id)
				.FirstOrDefault();
			if(leaderboardEntityId == 0)
				return;
			var entities = _game.Entities.Values
				.Where(x => x.IsMinion && x.IsInZone(HearthDb.Enums.Zone.PLAY) && x.IsControlledBy(_game.Opponent.Id))
				.Select(x => x.Clone())
				.ToArray();
			Log.Info($"Snapshotting board state for {opponentHero.Card.Name} with entity id {leaderboardEntityId} ({entities.Length} entities");
			LastKnownBoardState[leaderboardEntityId] = new BoardSnapshot(entities, _game.GetTurnNumber());
		}

		public BoardSnapshot? GetSnapshot(int entityId) => LastKnownBoardState.TryGetValue(entityId, out var state) ? state : null;

		public void Reset()
		{
			LastKnownBoardState.Clear();
		}
	}
}
