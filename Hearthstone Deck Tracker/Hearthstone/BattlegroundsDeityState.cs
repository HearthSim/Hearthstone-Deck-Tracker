using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	/// <summary>
	/// Tracks the last known Deity of every hero. A Deity has no entity of its own until it awakens,
	/// so until then it only exists as tags on its sigil: the card it is going to be, plus the stats
	/// it has accumulated. An opponent's sigil is only readable while we are fighting them.
	/// </summary>
	internal class BattlegroundsDeityState
	{
		Dictionary<int, DeitySnapshot> LastKnownDeity { get; } = new Dictionary<int, DeitySnapshot>();

		private readonly GameV2 _game;

		public BattlegroundsDeityState(GameV2 game)
		{
			_game = game;
		}

		public void SnapshotCurrentDeity()
		{
			var opponentHero = _game.Entities.Values
				.FirstOrDefault(x => x.IsHero && x.IsInZone(Zone.PLAY) && x.IsControlledBy(_game.Opponent.Id));
			if(opponentHero?.CardId == null)
				return;
			var playerId = opponentHero.GetTag(GameTag.PLAYER_ID);
			if(playerId == 0)
				return;

			// the sigil is copied into the combat and dropped again right after, so take the newest one
			var sigil = _game.Entities.Values
				.Where(x => x.CardId == HearthDb.CardIds.NonCollectible.Neutral.SecretDeityDnt && x.IsControlledBy(_game.Opponent.Id))
				.OrderByDescending(x => x.Id)
				.FirstOrDefault();
			if(sigil == null)
				return;

			var card = Database.GetCardFromDbfId(sigil.GetTag(GameTag.BACON_EVOLUTION_CARD_ID), false);
			if(card == null)
				return;

			// the stats are the Deity's current total, not a bonus on top of the printed ones
			var attack = sigil.GetTag(GameTag.BACON_EVOLUTION_CARD_OVERWRITE_ATK);
			var health = sigil.GetTag(GameTag.BACON_EVOLUTION_CARD_OVERWRITE_HEALTH);

			// the sigil itself stays non-golden: Mask of Ancient Ones only turns the Deity golden as
			// it awakens, so the trinket is the one thing that tells us ahead of time
			var isGolden = _game.Opponent.Trinkets
				.Any(x => x.CardId == HearthDb.CardIds.NonCollectible.Neutral.MaskOfAncientOnes);

			Log.Info($"Snapshotting {card.Name} ({attack}/{health}{(isGolden ? ", golden" : "")}) as the Deity of {opponentHero.Card.Name} with player id {playerId}");
			LastKnownDeity[playerId] = new DeitySnapshot(card, attack, health, isGolden, _game.GetTurnNumber());
		}

		public DeitySnapshot? GetSnapshot(int entityId)
		{
			if(!_game.Entities.TryGetValue(entityId, out var entity))
				return null;

			return LastKnownDeity.TryGetValue(entity.GetTag(GameTag.PLAYER_ID), out var state) ? state : null;
		}

		public void Reset()
		{
			LastKnownDeity.Clear();
		}
	}
}
