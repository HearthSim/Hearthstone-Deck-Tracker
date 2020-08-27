using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class BattlegroundsBoardState
	{
		Dictionary<string, BoardSnapshot> LastKnownBoardState { get; } = new Dictionary<string, BoardSnapshot>();

		const string UntransformedArannaCardid = HearthDb.CardIds.NonCollectible.Neutral.ArannaStarseekerTavernBrawl1;
		const string TransformedArannaCardid = HearthDb.CardIds.NonCollectible.Neutral.ArannaStarseeker_ArannaUnleashedTokenTavernBrawl;

		private static readonly Dictionary<string, string> _lastKnownBoardStateLookup = new Dictionary<string, string>()
		{
			{ TransformedArannaCardid, UntransformedArannaCardid}
		};

		private GameV2 _game;

		public BattlegroundsBoardState(GameV2 game)
		{
			_game = game;
		}

		public void UpdateSnapshot()
		{
			var opponentHero = _game.Entities.Values
				.Where(x => x.IsHero && x.IsInZone(HearthDb.Enums.Zone.PLAY) && x.IsControlledBy(_game.Opponent.Id))
				.FirstOrDefault();
			if(opponentHero == null)
				return;
			var entities = _game.Entities.Values
				.Where(x => x.IsMinion && x.IsInZone(HearthDb.Enums.Zone.PLAY) && x.IsControlledBy(_game.Opponent.Id))
				.Select(x => x.Clone())
				.ToArray();
			Log.Info($"Snapshotting board state for {opponentHero.Card.Name} with {entities.Length} entities");
			LastKnownBoardState[GetCorrectLastKnownBoardStateCardId(opponentHero.CardId)] = new BoardSnapshot(entities, _game.GetTurnNumber());
		}

		public BoardSnapshot GetSnapshot(string opponentHeroCardId) => LastKnownBoardState.TryGetValue(opponentHeroCardId, out var state) ? state : null;

		public void Reset()
		{
			LastKnownBoardState.Clear();
		}

		internal string GetCorrectLastKnownBoardStateCardId(string cardId) => cardId != null && _lastKnownBoardStateLookup.TryGetValue(cardId, out var mapped) ? mapped : cardId;

	}
}
