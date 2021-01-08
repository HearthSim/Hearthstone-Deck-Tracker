﻿using Hearthstone_Deck_Tracker.Utility.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	internal class BattlegroundsBoardState
	{
		Dictionary<string, BoardSnapshot> LastKnownBoardState { get; } = new Dictionary<string, BoardSnapshot>();

		const string UntransformedArannaCardid = HearthDb.CardIds.NonCollectible.Neutral.ArannaStarseekerTavernBrawl1;
		const string TransformedArannaCardid = HearthDb.CardIds.NonCollectible.Neutral.ArannaStarseeker_ArannaUnleashedTokenTavernBrawl;

		private static readonly Dictionary<string, string> TransformableHeroCardidTable = new Dictionary<string, string>()
		{
			{ TransformedArannaCardid, UntransformedArannaCardid}
		};

		private GameV2 _game;

		public BattlegroundsBoardState(GameV2 game)
		{
			_game = game;
		}

		public void SnapshotCurrentBoard()
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
			var correctedHero = opponentHero.CardId != null ? GetCorrectBoardstateHeroId(opponentHero.CardId) : opponentHero.CardId;
			Log.Info($"Snapshotting board state for {opponentHero.Card.Name} with cardid {opponentHero.CardId} (corrected={correctedHero}) with {entities.Length} entities");
			LastKnownBoardState[correctedHero] = new BoardSnapshot(entities, _game.GetTurnNumber());
		}

		public BoardSnapshot GetSnapshot(string opponentHeroCardId) => opponentHeroCardId != null && LastKnownBoardState.TryGetValue(GetCorrectBoardstateHeroId(opponentHeroCardId), out var state) ? state : null;

		public void Reset()
		{
			LastKnownBoardState.Clear();
		}

		public static string GetCorrectBoardstateHeroId(string heroId) => TransformableHeroCardidTable.TryGetValue(heroId, out var mapped) ? mapped : heroId;
	}
}
