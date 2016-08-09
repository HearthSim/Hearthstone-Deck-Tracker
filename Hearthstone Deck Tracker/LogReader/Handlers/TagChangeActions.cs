using System;
using System.Linq;
using System.Threading.Tasks;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static HearthDb.Enums.GameTag;
using static HearthDb.Enums.PlayState;
using static HearthDb.Enums.Zone;
using static Hearthstone_Deck_Tracker.Replay.KeyPointType;

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
					return () => LastCardPlayedChange(gameState, value);
				case DEFENDING:
					return () => DefendingChange(gameState, id, game, value);
				case ATTACKING:
					return () => AttackingChange(gameState, id, game, value);
				case PROPOSED_DEFENDER:
					return () => ProposedDefenderChange(game, value);
				case PROPOSED_ATTACKER:
					return () => ProposedAttackerChange(game, value);
				case NUM_MINIONS_PLAYED_THIS_TURN:
					return () => NumMinionsPlayedThisTurnChange(gameState, game, value);
				case PREDAMAGE:
					return () => PredamageChange(gameState, id, game, value);
				case NUM_TURNS_IN_PLAY:
					return () => NumTurnsInPlayChange(gameState, id, game, value);
				case NUM_ATTACKS_THIS_TURN:
					return () => NumAttacksThisTurnChange(gameState, id, game, value);
				case ZONE_POSITION:
					return () => ZonePositionChange(gameState, id, game);
				case CARD_TARGET:
					return () => CardTargetChange(gameState, id, game, value);
				case WEAPON:
					return () => EquippedWeaponChange(gameState, id, game, value);
				case EXHAUSTED:
					return () => ExhaustedChange(gameState, id, game, value);
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
			}
			return null;
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
			if(!gameState.SetupDone || game.PlayerEntity == null)
				return;
			var activePlayer = game.PlayerEntity.HasTag(CURRENT_PLAYER) ? ActivePlayer.Player : ActivePlayer.Opponent;
			gameState.GameHandler.TurnStart(activePlayer, gameState.GetTurnNumber());
			if(activePlayer == ActivePlayer.Player)
				gameState.PlayerUsedHeroPower = false;
			else
				gameState.OpponentUsedHeroPower = false;
		}

		private void StepChange(IHsGameState gameState, IGame game)
		{
			if(gameState.SetupDone || game.Entities.FirstOrDefault().Value?.Name != "GameEntity")
				return;
			Log.Info("Game was already in progress.");
			gameState.WasInProgress = true;
		}

		private void LastCardPlayedChange(IHsGameState gameState, int value) => gameState.LastCardPlayed = value;

		private void DefendingChange(IHsGameState gameState, int id, IGame game, int value)
		{
			Entity entity;
			if(!game.Entities.TryGetValue(id, out entity))
				return;
			gameState.GameHandler.HandleDefendingEntity(value == 1 ? entity : null);
		}

		private void AttackingChange(IHsGameState gameState, int id, IGame game, int value)
		{
			Entity entity;
			if(!game.Entities.TryGetValue(id, out entity))
				return;
			gameState.GameHandler.HandleAttackingEntity(value == 1 ? entity : null);
		}

		private void ProposedDefenderChange(IGame game, int value) => game.OpponentSecrets.ProposedDefenderEntityId = value;

		private void ProposedAttackerChange(IGame game, int value) => game.OpponentSecrets.ProposedAttackerEntityId = value;

		private void NumMinionsPlayedThisTurnChange(IHsGameState gameState, IGame game, int value)
		{
			if(value <= 0)
				return;
			if(game.PlayerEntity?.IsCurrentPlayer ?? false)
				gameState.GameHandler.HandlePlayerMinionPlayed();
		}

		private void PredamageChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value <= 0)
				return;
			Entity entity;
			if(!game.Entities.TryGetValue(id, out entity))
				return;
			gameState.GameHandler.HandleEntityPredamage(entity, value);
		}

		private void NumTurnsInPlayChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value <= 0)
				return;
			Entity entity;
			if(!game.Entities.TryGetValue(id, out entity))
				return;
			if(game.OpponentEntity?.IsCurrentPlayer ?? false)
				gameState.GameHandler.HandleOpponentTurnStart(entity);
		}

		private void FatigueChange(IHsGameState gameState, int value, IGame game, int id)
		{
			Entity entity;
			if(!game.Entities.TryGetValue(id, out entity))
				return;
			var controller = entity.GetTag(CONTROLLER);
			if(controller == game.Player.Id)
				gameState.GameHandler.HandlePlayerFatigue(value);
			else if(controller == game.Opponent.Id)
				gameState.GameHandler.HandleOpponentFatigue(value);
		}

		private void ControllerChange(IHsGameState gameState, int id, IGame game, int prevValue, int value)
		{
			Entity entity;
			if(!game.Entities.TryGetValue(id, out entity))
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
				{
					gameState.GameHandler.HandleOpponentStolen(entity, entity.CardId, gameState.GetTurnNumber());
					gameState.ProposeKeyPoint(SecretStolen, id, ActivePlayer.Player);
				}
				else if(entity.IsInZone(PLAY))
					gameState.GameHandler.HandleOpponentStolen(entity, entity.CardId, gameState.GetTurnNumber());
			}
			else if(value == game.Opponent.Id)
			{
				if(entity.IsInZone(Zone.SECRET))
				{
					gameState.GameHandler.HandlePlayerStolen(entity, entity.CardId, gameState.GetTurnNumber());
					gameState.ProposeKeyPoint(SecretStolen, id, ActivePlayer.Player);
				}
				else if(entity.IsInZone(PLAY))
					gameState.GameHandler.HandlePlayerStolen(entity, entity.CardId, gameState.GetTurnNumber());
			}
		}

		private void ExhaustedChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value <= 0)
				return;
			Entity entity;
			if(!game.Entities.TryGetValue(id, out entity))
				return;
			var controller = entity.GetTag(CONTROLLER);
			if(entity.GetTag(CARDTYPE) != (int)CardType.HERO_POWER)
				return;
			if(controller == game.Player.Id)
				gameState.ProposeKeyPoint(HeroPower, id, ActivePlayer.Player);
			else if(controller == game.Opponent.Id)
				gameState.ProposeKeyPoint(HeroPower, id, ActivePlayer.Opponent);
		}

		private void EquippedWeaponChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value != 0)
				return;
			Entity entity;
			if(!game.Entities.TryGetValue(id, out entity))
				return;
			var controller = entity.GetTag(CONTROLLER);
			if(controller == game.Player.Id)
				gameState.ProposeKeyPoint(WeaponDestroyed, id, ActivePlayer.Player);
			else if(controller == game.Opponent.Id)
				gameState.ProposeKeyPoint(WeaponDestroyed, id, ActivePlayer.Opponent);
		}

		private void CardTargetChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value <= 0)
				return;
			Entity entity;
			if(!game.Entities.TryGetValue(id, out entity))
				return;
			var controller = entity.GetTag(CONTROLLER);
			if(controller == game.Player.Id)
				gameState.ProposeKeyPoint(PlaySpell, id, ActivePlayer.Player);
			else if(controller == game.Opponent.Id)
				gameState.ProposeKeyPoint(PlaySpell, id, ActivePlayer.Opponent);
		}

		private void ZonePositionChange(IHsGameState gameState, int id, IGame game)
		{
			Entity entity;
			if(!game.Entities.TryGetValue(id, out entity))
				return;
			var zone = entity.GetTag(ZONE);
			var controller = entity.GetTag(CONTROLLER);
			if(zone == (int)HAND)
			{
				if(controller == game.Player.Id)
					ReplayMaker.Generate(HandPos, id, ActivePlayer.Player, game);
				else if(controller == game.Opponent.Id)
					ReplayMaker.Generate(HandPos, id, ActivePlayer.Opponent, game);
			}
			else if(zone == (int)PLAY)
			{
				if(controller == game.Player.Id)
					ReplayMaker.Generate(BoardPos, id, ActivePlayer.Player, game);
				else if(controller == game.Opponent.Id)
					ReplayMaker.Generate(BoardPos, id, ActivePlayer.Opponent, game);
			}
		}

		private void NumAttacksThisTurnChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value <= 0)
				return;
			Entity entity;
			if(!game.Entities.TryGetValue(id, out entity))
				return;
			var controller = entity.GetTag(CONTROLLER);
			if(controller == game.Player.Id)
				gameState.ProposeKeyPoint(Attack, id, ActivePlayer.Player);
			else if(controller == game.Opponent.Id)
				gameState.ProposeKeyPoint(Attack, id, ActivePlayer.Opponent);
		}

		private void CardTypeChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value == (int)CardType.HERO)
				SetHeroAsync(id, game, gameState);
		}

		private void PlaystateChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value == (int)CONCEDED)
				gameState.GameHandler.HandleConcede();
			if(gameState.GameEnded)
				return;
			Entity entity;
			if(!game.Entities.TryGetValue(id, out entity) || !entity.IsPlayer)
				return;
			switch((PlayState)value)
			{
				case WON:
					gameState.GameEndKeyPoint(true, id);
					gameState.GameHandler.HandleWin();
					break;
				case LOST:
					gameState.GameEndKeyPoint(false, id);
					gameState.GameHandler.HandleLoss();
					break;
				case TIED:
					gameState.GameEndKeyPoint(false, id);
					gameState.GameHandler.HandleTied();
					break;
			}
		}

		private void ZoneChange(IHsGameState gameState, int id, IGame game, int value, int prevValue)
		{
			if(id <= 3)
				return;
			Entity entity;
			if(!game.Entities.TryGetValue(id, out entity))
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
					if(!gameState.SetupDone && (id <= maxId || game.GameEntity?.GetTag(STEP) == (int)Step.INVALID && entity.GetTag(ZONE_POSITION) < 5))
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
			Entity entity;
			if(!game.Entities.TryGetValue(id, out entity))
				return;
			if(value == (int)SETASIDE)
			{
				entity.Info.Created = true;
				return;
			}
			if(entity.IsHero || entity.IsHeroPower || entity.HasTag(PLAYER_ID) || entity.GetTag(CARDTYPE) == (int)CardType.GAME
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
			Entity entity;
			if(!game.Entities.TryGetValue(id, out entity))
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
					{
						gameState.GameHandler.HandlePlayerCreateInPlay(entity, cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(Summon, id, ActivePlayer.Player);
					}
					if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentCreateInPlay(entity, cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(Summon, id, ActivePlayer.Opponent);
					}
					break;
				case DECK:
					if(controller == game.Player.Id)
					{
						if(gameState.JoustReveals > 0)
							break;
						gameState.GameHandler.HandlePlayerGetToDeck(entity, cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(CreateToDeck, id, ActivePlayer.Player);
					}
					if(controller == game.Opponent.Id)
					{
						if(gameState.JoustReveals > 0)
							break;
						gameState.GameHandler.HandleOpponentGetToDeck(entity, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(CreateToDeck, id, ActivePlayer.Opponent);
					}
					break;
				case HAND:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerGet(entity, cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(Obtain, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentGet(entity, gameState.GetTurnNumber(), id);
						gameState.ProposeKeyPoint(Obtain, id, ActivePlayer.Opponent);
					}
					break;
				case Zone.SECRET:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerSecretPlayed(entity, cardId, gameState.GetTurnNumber(), (Zone)prevValue);
						gameState.ProposeKeyPoint(SecretPlayed, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentSecretPlayed(entity, cardId, -1, gameState.GetTurnNumber(), (Zone)prevValue, id);
						gameState.ProposeKeyPoint(SecretPlayed, id, ActivePlayer.Opponent);
					}
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
					if(controller == game.Player.Id)
						gameState.ProposeKeyPoint(SecretTriggered, id, ActivePlayer.Player);
					if(controller == game.Opponent.Id)
					{
						Entity entity;
						if(!game.Entities.TryGetValue(id, out entity))
							return;
						gameState.GameHandler.HandleOpponentSecretTrigger(entity, cardId, gameState.GetTurnNumber(), id);
						gameState.ProposeKeyPoint(SecretTriggered, id, ActivePlayer.Opponent);
					}
					break;
				default:
					Log.Warn($"unhandled zone change (id={id}): {prevValue} -> {value}");
					break;
			}
		}

		private void ZoneChangeFromPlay(IHsGameState gameState, int id, IGame game, int value, int prevValue, int controller, string cardId)
		{
			Entity entity;
			if(!game.Entities.TryGetValue(id, out entity))
				return;
			switch((Zone)value)
			{
				case HAND:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerBackToHand(entity, cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(PlayToHand, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentPlayToHand(entity, cardId, gameState.GetTurnNumber(), id);
						gameState.ProposeKeyPoint(PlayToHand, id, ActivePlayer.Opponent);
					}
					break;
				case DECK:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerPlayToDeck(entity, cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(PlayToDeck, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentPlayToDeck(entity, cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(PlayToDeck, id, ActivePlayer.Opponent);
					}
					break;
				case GRAVEYARD:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerPlayToGraveyard(entity, cardId, gameState.GetTurnNumber());
						if(entity.HasTag(HEALTH))
							gameState.ProposeKeyPoint(Death, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentPlayToGraveyard(entity, cardId, gameState.GetTurnNumber(), game.PlayerEntity?.IsCurrentPlayer ?? false);
						if(entity.HasTag(HEALTH))
							gameState.ProposeKeyPoint(Death, id, ActivePlayer.Opponent);
					}
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
			Entity entity;
			if(!game.Entities.TryGetValue(id, out entity))
				return;
			switch((Zone)value)
			{
				case PLAY:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerPlay(entity, cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(Play, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentPlay(entity, cardId, entity.GetTag(ZONE_POSITION),
																 gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(Play, id, ActivePlayer.Opponent);
					}
					break;
				case REMOVEDFROMGAME:
				case SETASIDE:
				case GRAVEYARD:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerHandDiscard(entity, cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(HandDiscard, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentHandDiscard(entity, cardId, entity.GetTag(ZONE_POSITION),
																		gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(HandDiscard, id, ActivePlayer.Opponent);
					}
					break;
				case Zone.SECRET:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerSecretPlayed(entity, cardId, gameState.GetTurnNumber(), (Zone)prevValue);
						gameState.ProposeKeyPoint(SecretPlayed, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentSecretPlayed(entity, cardId, entity.GetTag(ZONE_POSITION),
																		 gameState.GetTurnNumber(), (Zone)prevValue, id);
						gameState.ProposeKeyPoint(SecretPlayed, id, ActivePlayer.Opponent);
					}
					break;
				case DECK:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerMulligan(entity, cardId);
						gameState.ProposeKeyPoint(KeyPointType.Mulligan, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentMulligan(entity, entity.GetTag(ZONE_POSITION));
						gameState.ProposeKeyPoint(KeyPointType.Mulligan, id, ActivePlayer.Opponent);
					}
					break;
				default:
					Log.Warn($"unhandled zone change (id={id}): {prevValue} -> {value}");
					break;
			}
		}

		private void ZoneChangeFromDeck(IHsGameState gameState, int id, IGame game, int value, int prevValue, int controller, string cardId)
		{
			Entity entity;
			if(!game.Entities.TryGetValue(id, out entity))
				return;
			switch((Zone)value)
			{
				case HAND:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerDraw(entity, cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(Draw, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentDraw(entity, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(Draw, id, ActivePlayer.Opponent);
					}
					break;
				case SETASIDE:
				case REMOVEDFROMGAME:
					if(!gameState.SetupDone)
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
					{
						gameState.GameHandler.HandlePlayerDeckDiscard(entity, cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(DeckDiscard, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentDeckDiscard(entity, cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(DeckDiscard, id, ActivePlayer.Opponent);
					}
					break;
				case PLAY:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerDeckToPlay(entity, cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(DeckDiscard, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentDeckToPlay(entity, cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(DeckDiscard, id, ActivePlayer.Opponent);
					}
					break;
				case Zone.SECRET:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerSecretPlayed(entity, cardId, gameState.GetTurnNumber(), (Zone)prevValue);
						gameState.ProposeKeyPoint(SecretPlayed, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentSecretPlayed(entity, cardId, -1, gameState.GetTurnNumber(), (Zone)prevValue, id);
						gameState.ProposeKeyPoint(SecretPlayed, id, ActivePlayer.Player);
					}
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
				Entity entity;
				if(!game.Entities.TryGetValue(id, out entity))
					return;
				gameState.GameHandler.SetPlayerHero(Database.GetHeroNameFromId(entity.CardId));
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
				Entity entity;
				if(!game.Entities.TryGetValue(id, out entity))
					return;
				gameState.GameHandler.SetOpponentHero(Database.GetHeroNameFromId(entity.CardId));
			}
		}
	}
}
