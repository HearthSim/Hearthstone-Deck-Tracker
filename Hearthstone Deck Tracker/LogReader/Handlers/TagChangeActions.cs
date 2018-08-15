using System;
using System.Linq;
using System.Threading.Tasks;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static HearthDb.Enums.GameTag;
using static HearthDb.Enums.PlayState;
using static HearthDb.Enums.Zone;
using CardIds = HearthDb.CardIds;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	internal class TagChangeActions
	{
		public Action FindAction(GameTag tag, IGame game, IHsGameState gameState, int id, int value, int prevValue)
		{
			switch(tag)
			{
				case ZONE:
					return () => ZoneChange(gameState, id, game, value, prevValue);
				case PLAYSTATE:
					return () => PlaystateChange(gameState, id, game, value);
				case CARDTYPE:
					return () => CardTypeChange(gameState, id, game, value);
				case LAST_CARD_PLAYED:
					return () => LastCardPlayedChange(gameState, game, value);
				case DEFENDING:
					return () => DefendingChange(gameState, id, game, value);
				case ATTACKING:
					return () => AttackingChange(gameState, id, game, value);
				case PROPOSED_DEFENDER:
					return () => ProposedDefenderChange(game, value);
				case PROPOSED_ATTACKER:
					return () => ProposedAttackerChange(game, value);
				case PREDAMAGE:
					return () => PredamageChange(gameState, id, game, value);
				case NUM_TURNS_IN_PLAY:
					return () => NumTurnsInPlayChange(gameState, id, game, value);
				case CONTROLLER:
					return () => ControllerChange(gameState, id, game, prevValue, value);
				case FATIGUE:
					return () => FatigueChange(gameState, value, game, id);
				case STEP:
					return () => StepChange(gameState, game);
				case TURN:
					return () => TurnChange(gameState, game);
				case STATE:
					return () => StateChange(value, gameState);
				case TRANSFORMED_FROM_CARD:
					return () => TransformedFromCardChange(id, value, game);
				case CREATOR:
				case DISPLAYED_CREATOR:
					return () => CreatorChanged(id, value, game);
				case WHIZBANG_DECK_ID:
					return () => WhizbangDeckIdChange(id, value, game);
			}
			return null;
		}

		private void WhizbangDeckIdChange(int id, int value, IGame game)
		{
			if(value == 0)
				return;
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			if(!entity.IsPlayer)
				return;
			DeckManager.AutoSelectDeckById(game, value);
		}

		private void CreatorChanged(int id, int value, IGame game)
		{
			if(value == 0)
				return;
			if(game.Entities.TryGetValue(id, out var entity))
			{
				var displayedCreatorId = entity.GetTag(DISPLAYED_CREATOR);
				if(displayedCreatorId == id)
				{
					// Some cards (e.g. Direhorn Hatchling) wrongfully set DISPLAYED_CREATOR
					// on themselves instead of the created entity.
					return;
				}
				if(game.Entities.TryGetValue(displayedCreatorId, out var displayedCreator))
				{
					// For some reason Far Sight sets DISPLAYED_CREATOR on the entity
					if(displayedCreator.CardId == CardIds.Collectible.Shaman.FarSight)
						return;
				}

				var creatorId = entity.GetTag(CREATOR);
				if(creatorId == id)
				{
					// Same precaution as for Direhorn Hatching above.
					return;
				}
				if(creatorId == game.GameEntity?.Id)
					return;
				// All cards created by Whizbang have a creator tag set
				if(game.Entities.TryGetValue(creatorId, out var creator))
				{
					if(creator.CardId == CardIds.Collectible.Neutral.WhizbangTheWonderful)
						return;
					var controller = creator.GetTag(CONTROLLER);
					var usingWhizbang = controller == game.Player?.Id
										&& (game.PlayerEntity?.HasTag(WHIZBANG_DECK_ID) ?? false)
										|| controller == game.Opponent?.Id
										&& (game.OpponentEntity?.HasTag(WHIZBANG_DECK_ID) ?? false);
					if(usingWhizbang && creator.IsInSetAside && creator.Info.OriginalZone == SETASIDE)
						return;
				}
				entity.Info.Created = true;
			}
		}

		private void TransformedFromCardChange(int id, int value, IGame game)
		{
			if(value == 0)
				return;
			if(game.Entities.TryGetValue(id, out var entity))
				entity.Info.SetOriginalCardId(value);
		}

		private void StateChange(int value, IHsGameState gameState)
		{
			if(value != (int)State.COMPLETE)
				return;
			gameState.GameHandler.HandleGameEnd();
			gameState.GameEnded = true;
		}

		private void TurnChange(IHsGameState gameState, IGame game)
		{
			if(!game.SetupDone || game.PlayerEntity == null)
				return;
			var activePlayer = game.PlayerEntity.HasTag(CURRENT_PLAYER) ? ActivePlayer.Player : ActivePlayer.Opponent;
			if(activePlayer == ActivePlayer.Player)
				gameState.PlayerUsedHeroPower = false;
			else
				gameState.OpponentUsedHeroPower = false;
		}

		private void StepChange(IHsGameState gameState, IGame game)
		{
			if(game.SetupDone || game.Entities.FirstOrDefault().Value?.Name != "GameEntity")
				return;
			Log.Info("Game was already in progress.");
			gameState.WasInProgress = true;
		}

		private void LastCardPlayedChange(IHsGameState gameState, IGame game, int value)
		{
			gameState.LastCardPlayed = value;
			if(!(game.PlayerEntity?.IsCurrentPlayer ?? false))
				return;
			if(!game.Entities.TryGetValue(value, out var entity) || entity == null || !entity.IsMinion)
				return;
			if(entity.HasTag(MODULAR))
			{
				var pos = entity.GetTag(ZONE_POSITION);
				var neighbour = game.Player?.Board.FirstOrDefault(x => x.GetTag(ZONE_POSITION) == pos + 1);
				if(neighbour?.Card?.Race?.Equals(Race.MECHANICAL.ToString(),
						StringComparison.CurrentCultureIgnoreCase) ?? false)
					return;
			}
			gameState.GameHandler.HandlePlayerMinionPlayed();
		}

		private void DefendingChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			gameState.GameHandler.HandleDefendingEntity(value == 1 ? entity : null);
		}

		private void AttackingChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			gameState.GameHandler.HandleAttackingEntity(value == 1 ? entity : null);
		}

		private void ProposedDefenderChange(IGame game, int value) => game.ProposedDefender = value;

		private void ProposedAttackerChange(IGame game, int value) => game.ProposedAttacker = value;

		private void PredamageChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value <= 0)
				return;
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			gameState.GameHandler.HandleEntityPredamage(entity, value);
		}

		private void NumTurnsInPlayChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value <= 0)
				return;
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			gameState.GameHandler.HandleTurnsInPlayChange(entity, gameState.GetTurnNumber());
		}

		private void FatigueChange(IHsGameState gameState, int value, IGame game, int id)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			var controller = entity.GetTag(CONTROLLER);
			if(controller == game.Player.Id)
				gameState.GameHandler.HandlePlayerFatigue(value);
			else if(controller == game.Opponent.Id)
				gameState.GameHandler.HandleOpponentFatigue(value);
		}

		private void ControllerChange(IHsGameState gameState, int id, IGame game, int prevValue, int value)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			if(prevValue <= 0)
			{
				entity.Info.OriginalController = value;
				return;
			}
			if(entity.HasTag(PLAYER_ID))
				return;
			if(value == game.Player.Id)
			{
				if(entity.IsInZone(Zone.SECRET))
					gameState.GameHandler.HandleOpponentStolen(entity, entity.CardId, gameState.GetTurnNumber());
				else if(entity.IsInZone(PLAY))
					gameState.GameHandler.HandleOpponentStolen(entity, entity.CardId, gameState.GetTurnNumber());
			}
			else if(value == game.Opponent.Id)
			{
				if(entity.IsInZone(Zone.SECRET))
					gameState.GameHandler.HandlePlayerStolen(entity, entity.CardId, gameState.GetTurnNumber());
				else if(entity.IsInZone(PLAY))
					gameState.GameHandler.HandlePlayerStolen(entity, entity.CardId, gameState.GetTurnNumber());
			}
		}

		private void CardTypeChange(IHsGameState gameState, int id, IGame game, int value)
		{
			switch (value)
			{
				case (int)CardType.HERO:
					SetHeroAsync(id, game, gameState);
					break;
				case (int)CardType.MINION:
					MinionRevealed(id, game, gameState);
					break;
			}
		}

		private void PlaystateChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value == (int)CONCEDED)
				gameState.GameHandler.HandleConcede();
			if(gameState.GameEnded)
				return;
			if(!game.Entities.TryGetValue(id, out var entity) || !entity.IsPlayer)
				return;
			switch((PlayState)value)
			{
				case WON:
					gameState.GameHandler.HandleWin();
					break;
				case LOST:
					gameState.GameHandler.HandleLoss();
					break;
				case TIED:
					gameState.GameHandler.HandleTied();
					break;
			}
		}

		private void ZoneChange(IHsGameState gameState, int id, IGame game, int value, int prevValue)
		{
			if(id <= 3)
				return;
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			if(!entity.Info.OriginalZone.HasValue)
			{
				if(prevValue != (int)Zone.INVALID && prevValue != (int)SETASIDE)
					entity.Info.OriginalZone = (Zone)prevValue;
				else if(value != (int)Zone.INVALID && value != (int)SETASIDE)
					entity.Info.OriginalZone = (Zone)value;
			}
			var controller = entity.GetTag(CONTROLLER);
			switch((Zone)prevValue)
			{
				case DECK:
					ZoneChangeFromDeck(gameState, id, game, value, prevValue, controller, entity.CardId);
					break;
				case HAND:
					ZoneChangeFromHand(gameState, id, game, value, prevValue, controller, entity.CardId);
					break;
				case PLAY:
					ZoneChangeFromPlay(gameState, id, game, value, prevValue, controller, entity.CardId);
					break;
				case Zone.SECRET:
					ZoneChangeFromSecret(gameState, id, game, value, prevValue, controller, entity.CardId);
					break;
				case Zone.INVALID:
					var maxId = GetMaxHeroPowerId(game);
					if(!game.SetupDone && (id <= maxId || game.GameEntity?.GetTag(STEP) == (int)Step.INVALID && entity.GetTag(ZONE_POSITION) < 5))
					{
						entity.Info.OriginalZone = DECK;
						SimulateZoneChangesFromDeck(gameState, id, game, value, entity.CardId, maxId);
					}
					else
						ZoneChangeFromOther(gameState, id, game, value, prevValue, controller, entity.CardId);
					break;
				case GRAVEYARD:
				case SETASIDE:
				case REMOVEDFROMGAME:
					ZoneChangeFromOther(gameState, id, game, value, prevValue, controller, entity.CardId);
					break;
				default:
					Log.Warn($"unhandled zone change (id={id}): {prevValue} -> {value}");
					break;
			}
		}

		// The last heropower is created after the last hero, therefore +1
		private int GetMaxHeroPowerId(IGame game) => 
			Math.Max(game.PlayerEntity?.GetTag(HERO_ENTITY) ?? 66, game.OpponentEntity?.GetTag(HERO_ENTITY) ?? 66) + 1;

		private void SimulateZoneChangesFromDeck(IHsGameState gameState, int id, IGame game, int value, string cardId, int maxId)
		{
			if(value == (int)DECK)
				return;
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			if(value == (int)SETASIDE)
			{
				entity.Info.Created = true;
				return;
			}
			if(entity.IsHero && !entity.IsPlayableHero || entity.IsHeroPower || entity.HasTag(PLAYER_ID) || entity.GetTag(CARDTYPE) == (int)CardType.GAME
				|| entity.HasTag(CREATOR))
				return;
			ZoneChangeFromDeck(gameState, id, game, (int)HAND, (int)DECK, entity.GetTag(CONTROLLER), cardId);
			if(value == (int)HAND)
				return;
			ZoneChangeFromHand(gameState, id, game, (int)PLAY, (int)HAND, entity.GetTag(CONTROLLER), cardId);
			if(value == (int)PLAY)
				return;
			ZoneChangeFromPlay(gameState, id, game, value, (int)PLAY, entity.GetTag(CONTROLLER), cardId);
		}

		private void ZoneChangeFromOther(IHsGameState gameState, int id, IGame game, int value, int prevValue, int controller, string cardId)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			if(entity.Info.OriginalZone == DECK && value != (int)DECK)
			{
				//This entity was moved from DECK to SETASIDE to HAND, e.g. by Tracking
				entity.Info.Discarded = false;
				ZoneChangeFromDeck(gameState, id, game, value, prevValue, controller, cardId);
				return;
			}
			entity.Info.Created = true;
			switch((Zone)value)
			{
				case PLAY:
					if(controller == game.Player.Id)
						gameState.GameHandler.HandlePlayerCreateInPlay(entity, cardId, gameState.GetTurnNumber());
					if(controller == game.Opponent.Id)
						gameState.GameHandler.HandleOpponentCreateInPlay(entity, cardId, gameState.GetTurnNumber());
					break;
				case DECK:
					if(controller == game.Player.Id)
					{
						if(gameState.JoustReveals > 0)
							break;
						gameState.GameHandler.HandlePlayerGetToDeck(entity, cardId, gameState.GetTurnNumber());
					}
					if(controller == game.Opponent.Id)
					{
						if(gameState.JoustReveals > 0)
							break;
						gameState.GameHandler.HandleOpponentGetToDeck(entity, gameState.GetTurnNumber());
					}
					break;
				case HAND:
					if(controller == game.Player.Id)
						gameState.GameHandler.HandlePlayerGet(entity, cardId, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
						gameState.GameHandler.HandleOpponentGet(entity, gameState.GetTurnNumber(), id);
					break;
				case Zone.SECRET:
					if(controller == game.Player.Id)
						gameState.GameHandler.HandlePlayerSecretPlayed(entity, cardId, gameState.GetTurnNumber(), (Zone)prevValue);
					else if(controller == game.Opponent.Id)
						gameState.GameHandler.HandleOpponentSecretPlayed(entity, cardId, -1, gameState.GetTurnNumber(), (Zone)prevValue, id);
					break;
				case SETASIDE:
					if(controller == game.Player.Id)
						gameState.GameHandler.HandlePlayerCreateInSetAside(entity, gameState.GetTurnNumber());
					if(controller == game.Opponent.Id)
						gameState.GameHandler.HandleOpponentCreateInSetAside(entity, gameState.GetTurnNumber());
					break;
				default:
					Log.Warn($"unhandled zone change (id={id}): {prevValue} -> {value}");
					break;
			}
		}

		private void ZoneChangeFromSecret(IHsGameState gameState, int id, IGame game, int value, int prevValue, int controller, string cardId)
		{
			switch((Zone)value)
			{
				case Zone.SECRET:
				case GRAVEYARD:
					if(controller == game.Opponent.Id)
					{
						if(!game.Entities.TryGetValue(id, out var entity))
							return;
						gameState.GameHandler.HandleOpponentSecretTrigger(entity, cardId, gameState.GetTurnNumber(), id);
					}
					break;
				default:
					Log.Warn($"unhandled zone change (id={id}): {prevValue} -> {value}");
					break;
			}
		}

		private void ZoneChangeFromPlay(IHsGameState gameState, int id, IGame game, int value, int prevValue, int controller, string cardId)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			switch((Zone)value)
			{
				case HAND:
					if(controller == game.Player.Id)
						gameState.GameHandler.HandlePlayerBackToHand(entity, cardId, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
						gameState.GameHandler.HandleOpponentPlayToHand(entity, cardId, gameState.GetTurnNumber(), id);
					break;
				case DECK:
					if(controller == game.Player.Id)
						gameState.GameHandler.HandlePlayerPlayToDeck(entity, cardId, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
						gameState.GameHandler.HandleOpponentPlayToDeck(entity, cardId, gameState.GetTurnNumber());
					break;
				case GRAVEYARD:
					if(controller == game.Player.Id)
						gameState.GameHandler.HandlePlayerPlayToGraveyard(entity, cardId, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
						gameState.GameHandler.HandleOpponentPlayToGraveyard(entity, cardId, gameState.GetTurnNumber(), game.PlayerEntity?.IsCurrentPlayer ?? false);
					break;
				case REMOVEDFROMGAME:
				case SETASIDE:
					if(controller == game.Player.Id)
						gameState.GameHandler.HandlePlayerRemoveFromPlay(entity, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
						gameState.GameHandler.HandleOpponentRemoveFromPlay(entity, gameState.GetTurnNumber());
					break;
				case PLAY:
					break;
				default:
					Log.Warn($"unhandled zone change (id={id}): {prevValue} -> {value}");
					break;
			}
		}

		private void ZoneChangeFromHand(IHsGameState gameState, int id, IGame game, int value, int prevValue, int controller, string cardId)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			switch((Zone)value)
			{
				case PLAY:
					if(controller == game.Player.Id)
						gameState.GameHandler.HandlePlayerPlay(entity, cardId, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentPlay(entity, cardId, entity.GetTag(ZONE_POSITION),
																 gameState.GetTurnNumber());
					}
					break;
				case REMOVEDFROMGAME:
				case SETASIDE:
				case GRAVEYARD:
					if(controller == game.Player.Id)
						gameState.GameHandler.HandlePlayerHandDiscard(entity, cardId, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentHandDiscard(entity, cardId, entity.GetTag(ZONE_POSITION),
																		gameState.GetTurnNumber());
					}
					break;
				case Zone.SECRET:
					if(controller == game.Player.Id)
						gameState.GameHandler.HandlePlayerSecretPlayed(entity, cardId, gameState.GetTurnNumber(), (Zone)prevValue);
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentSecretPlayed(entity, cardId, entity.GetTag(ZONE_POSITION),
																		 gameState.GetTurnNumber(), (Zone)prevValue, id);
					}
					break;
				case DECK:
					if(controller == game.Player.Id)
						gameState.GameHandler.HandlePlayerMulligan(entity, cardId);
					else if(controller == game.Opponent.Id)
						gameState.GameHandler.HandleOpponentMulligan(entity, entity.GetTag(ZONE_POSITION));
					break;
				default:
					Log.Warn($"unhandled zone change (id={id}): {prevValue} -> {value}");
					break;
			}
		}

		private void ZoneChangeFromDeck(IHsGameState gameState, int id, IGame game, int value, int prevValue, int controller, string cardId)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			switch((Zone)value)
			{
				case HAND:
					if(controller == game.Player.Id)
						gameState.GameHandler.HandlePlayerDraw(entity, cardId, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
						gameState.GameHandler.HandleOpponentDraw(entity, gameState.GetTurnNumber());
					break;
				case SETASIDE:
				case REMOVEDFROMGAME:
					if(!game.SetupDone)
					{
						entity.Info.Created = true;
						return;
					}
					if(controller == game.Player.Id)
					{
						if(gameState.JoustReveals > 0)
						{
							gameState.JoustReveals--;
							break;
						}
						gameState.GameHandler.HandlePlayerRemoveFromDeck(entity, gameState.GetTurnNumber());
					}
					else if(controller == game.Opponent.Id)
					{
						if(gameState.JoustReveals > 0)
						{
							gameState.JoustReveals--;
							break;
						}
						gameState.GameHandler.HandleOpponentRemoveFromDeck(entity, gameState.GetTurnNumber());
					}
					break;
				case GRAVEYARD:
					if(controller == game.Player.Id)
						gameState.GameHandler.HandlePlayerDeckDiscard(entity, cardId, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
						gameState.GameHandler.HandleOpponentDeckDiscard(entity, cardId, gameState.GetTurnNumber());
					break;
				case PLAY:
					if(controller == game.Player.Id)
						gameState.GameHandler.HandlePlayerDeckToPlay(entity, cardId, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
						gameState.GameHandler.HandleOpponentDeckToPlay(entity, cardId, gameState.GetTurnNumber());
					break;
				case Zone.SECRET:
					if(controller == game.Player.Id)
						gameState.GameHandler.HandlePlayerSecretPlayed(entity, cardId, gameState.GetTurnNumber(), (Zone)prevValue);
					else if(controller == game.Opponent.Id)
						gameState.GameHandler.HandleOpponentSecretPlayed(entity, cardId, -1, gameState.GetTurnNumber(), (Zone)prevValue, id);
					break;
				default:
					Log.Warn($"unhandled zone change (id={id}): {prevValue} -> {value}");
					break;
			}
		}

		private async void SetHeroAsync(int id, IGame game, IHsGameState gameState)
		{
			Log.Info("Found hero with id=" + id);
			if(game.PlayerEntity == null)
			{
				Log.Info("Waiting for PlayerEntity to exist");
				while(game.PlayerEntity == null)
					await Task.Delay(100);
				Log.Info("Found PlayerEntity");
			}
			if(string.IsNullOrEmpty(game.Player.Class) && id == game.PlayerEntity.GetTag(HERO_ENTITY))
			{
				if(!game.Entities.TryGetValue(id, out var entity))
					return;
				gameState.GameHandler.SetPlayerHero(entity.CardId);
				return;
			}
			if(game.OpponentEntity == null)
			{
				Log.Info("Waiting for OpponentEntity to exist");
				while(game.OpponentEntity == null)
					await Task.Delay(100);
				Log.Info("Found OpponentEntity");
			}
			if(string.IsNullOrEmpty(game.Opponent.Class) && id == game.OpponentEntity.GetTag(HERO_ENTITY))
			{
				if(!game.Entities.TryGetValue(id, out var entity))
					return;
				gameState.GameHandler.SetOpponentHero(entity.CardId);
			}
		}

		private void MinionRevealed(int id, IGame game, IHsGameState gameState)
		{
			if(game.Entities.TryGetValue(id, out var entity))
				game.SecretsManager.OnEntityRevealedAsMinion(entity);
		}
	}
}
