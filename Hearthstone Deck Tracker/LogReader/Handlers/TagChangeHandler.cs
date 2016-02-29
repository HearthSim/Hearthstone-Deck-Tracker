#region

using System;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static Hearthstone_Deck_Tracker.Enums.GAME_TAG;
using static Hearthstone_Deck_Tracker.Enums.Hearthstone.TAG_PLAYSTATE;
using static Hearthstone_Deck_Tracker.Enums.Hearthstone.TAG_ZONE;
using static Hearthstone_Deck_Tracker.Replay.KeyPointType;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	class TagChangeHandler
	{
		public void TagChange(IHsGameState gameState, string rawTag, int id, string rawValue, IGame game, bool isRecursive = false)
		{
			if(gameState.LastId != id)
			{
				//game.SecondToLastUsedId = gameState.LastId;
				if(gameState.ProposedKeyPoint != null)
				{
					ReplayMaker.Generate(gameState.ProposedKeyPoint.Type, gameState.ProposedKeyPoint.Id, gameState.ProposedKeyPoint.Player, game);
					gameState.ProposedKeyPoint = null;
				}
			}
			gameState.LastId = id;
			if(id > gameState.MaxId)
				gameState.MaxId = id;
			if(!game.Entities.ContainsKey(id))
				game.Entities.Add(id, new Entity(id));
			GAME_TAG tag;
			if(!Enum.TryParse(rawTag, out tag))
			{
				int tmp;
				if(int.TryParse(rawTag, out tmp) && Enum.IsDefined(typeof(GAME_TAG), tmp))
					tag = (GAME_TAG)tmp;
			}
			var value = LogReaderHelper.ParseTagValue(tag, rawValue);
			var prevValue = game.Entities[id].GetTag(tag);
			game.Entities[id].SetTag(tag, value);

			if(tag == CONTROLLER && gameState.WaitForController != null && game.Player.Id == -1)
				DeterminePlayers(gameState, game, value);

			var controller = game.Entities[id].GetTag(CONTROLLER);
			var cardId = game.Entities[id].CardId;
			switch(tag)
			{
				case ZONE:
					if(((TAG_ZONE)value == HAND || ((TAG_ZONE)value == PLAY || (TAG_ZONE)value == DECK) && game.IsMulliganDone) && gameState.WaitForController == null)
					{
						if(!game.IsMulliganDone)
							prevValue = (int)DECK;
						if(controller == 0)
						{
							game.Entities[id].SetTag(ZONE, prevValue);
							gameState.WaitForController = new {Tag = rawTag, Id = id, Value = rawValue};
							return;
						}
					}
					ZoneChange(gameState, id, game, value, prevValue, controller, cardId);
					break;
				case PLAYSTATE:
					PlaystateChange(gameState, id, game, value);
					break;
				case CARDTYPE:
					CardTypeChange(gameState, id, game, value);
					break;
				case CURRENT_PLAYER:
					CurrentPlayerChange(gameState, id, game, value);
					break;
				case LAST_CARD_PLAYED:
					LastCardPlayedChange(gameState, value);
					break;
				case DEFENDING:
					DefendingChange(gameState, id, game, controller, value);
					break;
				case ATTACKING:
					AttackingChange(gameState, id, game, controller, value);
					break;
				case PROPOSED_DEFENDER:
					ProposedDefenderChange(game, value);
					break;
				case PROPOSED_ATTACKER:
					ProposedAttackerChange(game, value);
					break;
				case NUM_MINIONS_PLAYED_THIS_TURN:
					NumMinionsPlayedThisTurnChange(gameState, value);
					break;
				case PREDAMAGE:
					PredamageChange(gameState, id, game, value);
					break;
				case NUM_TURNS_IN_PLAY:
					NumTurnsInPlayChange(gameState, id, game, value);
					break;
				case NUM_ATTACKS_THIS_TURN:
					NumAttacksThisTurnChange(gameState, id, game, value, controller);
					break;
				case ZONE_POSITION:
					ZonePositionChange(gameState, id, game, controller);
					break;
				case CARD_TARGET:
					CardTargetChange(gameState, id, game, value, controller);
					break;
				case EQUIPPED_WEAPON:
					EquippedWeaponChange(gameState, id, game, value, controller);
					break;
				case EXHAUSTED:
					ExhaustedChange(gameState, id, game, value, controller);
					break;
				case CONTROLLER:
					ControllerChange(gameState, id, game, prevValue, value, cardId);
					break;
				case FATIGUE:
					FatigueChange(gameState, rawValue, game, controller);
					break;
				case STEP:
					StepChange(gameState, game);
					break;
			}
			if(gameState.WaitForController != null && !isRecursive)
			{
				TagChange(gameState, (string)gameState.WaitForController.Tag, (int)gameState.WaitForController.Id,
						  (string)gameState.WaitForController.Value, game, true);
				gameState.WaitForController = null;
			}
		}

		private void StepChange(IHsGameState gameState, IGame game)
		{
			if(game.Entities.Count > 1 || game.Entities.FirstOrDefault().Value?.Name != "GameEntity")
				return;
			Log.Info("Game was already in progress.");
			gameState.WasInProgress = true;
		}

		private static void DeterminePlayers(IHsGameState gameState, IGame game, int controller)
		{
			var p1 = game.Entities.FirstOrDefault(e => e.Value.GetTag(PLAYER_ID) == 1).Value;
			var p2 = game.Entities.FirstOrDefault(e => e.Value.GetTag(PLAYER_ID) == 2).Value;
			if(gameState.CurrentEntityHasCardId)
			{
				if(p1 != null)
					p1.IsPlayer = controller == 1;
				if(p2 != null)
					p2.IsPlayer = controller != 1;
				game.Player.Id = controller;
				game.Opponent.Id = controller % 2 + 1;
			}
			else
			{
				if(p1 != null)
					p1.IsPlayer = controller != 1;
				if(p2 != null)
					p2.IsPlayer = controller == 1;
				game.Player.Id = controller % 2 + 1;
				game.Opponent.Id = controller;
			}
		}

		private static void LastCardPlayedChange(IHsGameState gameState, int value) => gameState.LastCardPlayed = value;

		private static void DefendingChange(IHsGameState gameState, int id, IGame game, int controller, int value)
		{
			if(controller == game.Opponent.Id)
				gameState.GameHandler.HandleDefendingEntity(value == 1 ? game.Entities[id] : null);
		}

		private static void AttackingChange(IHsGameState gameState, int id, IGame game, int controller, int value)
		{
			if(controller == game.Player.Id)
				gameState.GameHandler.HandleAttackingEntity(value == 1 ? game.Entities[id] : null);
		}

		private static void ProposedDefenderChange(IGame game, int value) => game.OpponentSecrets.ProposedDefenderEntityId = value;

		private static void ProposedAttackerChange(IGame game, int value) => game.OpponentSecrets.ProposedAttackerEntityId = value;

		private static void NumMinionsPlayedThisTurnChange(IHsGameState gameState, int value)
		{
			if(value <= 0)
				return;
			if(gameState.PlayersTurn())
				gameState.GameHandler.HandlePlayerMinionPlayed();
		}

		private static void PredamageChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value <= 0)
				return;
			if(gameState.PlayersTurn())
				gameState.GameHandler.HandleOpponentDamage(game.Entities[id]);
		}

		private static void NumTurnsInPlayChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value <= 0)
				return;
			if(!gameState.PlayersTurn())
				gameState.GameHandler.HandleOpponentTurnStart(game.Entities[id]);
		}

		private static void FatigueChange(IHsGameState gameState, string rawValue, IGame game, int controller)
		{
			if(controller == game.Player.Id)
				gameState.GameHandler.HandlePlayerFatigue(Convert.ToInt32(rawValue));
			else if(controller == game.Opponent.Id)
				gameState.GameHandler.HandleOpponentFatigue(Convert.ToInt32(rawValue));
		}

		private static void ControllerChange(IHsGameState gameState, int id, IGame game, int prevValue, int value, string cardId)
		{
			if(prevValue <= 0)
				return;
			if(value == game.Player.Id)
			{
				if(game.Entities[id].IsInZone(TAG_ZONE.SECRET))
				{
					gameState.GameHandler.HandleOpponentStolen(game.Entities[id], cardId, gameState.GetTurnNumber());
					gameState.ProposeKeyPoint(SecretStolen, id, ActivePlayer.Player);
				}
				else if(game.Entities[id].IsInZone(PLAY))
					gameState.GameHandler.HandleOpponentStolen(game.Entities[id], cardId, gameState.GetTurnNumber());
			}
			else if(value == game.Opponent.Id)
			{
				if(game.Entities[id].IsInZone(TAG_ZONE.SECRET))
				{
					gameState.GameHandler.HandleOpponentStolen(game.Entities[id], cardId, gameState.GetTurnNumber());
					gameState.ProposeKeyPoint(SecretStolen, id, ActivePlayer.Player);
				}
				else if(game.Entities[id].IsInZone(PLAY))
					gameState.GameHandler.HandlePlayerStolen(game.Entities[id], cardId, gameState.GetTurnNumber());
			}
		}

		private static void ExhaustedChange(IHsGameState gameState, int id, IGame game, int value, int controller)
		{
			if(value <= 0)
				return;
			if(game.Entities[id].GetTag(CARDTYPE) != (int)TAG_CARDTYPE.HERO_POWER)
				return;
			if(controller == game.Player.Id)
				gameState.ProposeKeyPoint(HeroPower, id, ActivePlayer.Player);
			else if(controller == game.Opponent.Id)
				gameState.ProposeKeyPoint(HeroPower, id, ActivePlayer.Opponent);
		}

		private static void EquippedWeaponChange(IHsGameState gameState, int id, IGame game, int value, int controller)
		{
			if(value != 0)
				return;
			if(controller == game.Player.Id)
				gameState.ProposeKeyPoint(WeaponDestroyed, id, ActivePlayer.Player);
			else if(controller == game.Opponent.Id)
				gameState.ProposeKeyPoint(WeaponDestroyed, id, ActivePlayer.Opponent);
		}

		private static void CardTargetChange(IHsGameState gameState, int id, IGame game, int value, int controller)
		{
			if(value <= 0)
				return;
			if(controller == game.Player.Id)
				gameState.ProposeKeyPoint(PlaySpell, id, ActivePlayer.Player);
			else if(controller == game.Opponent.Id)
				gameState.ProposeKeyPoint(PlaySpell, id, ActivePlayer.Opponent);
		}

		private static void ZonePositionChange(IHsGameState gameState, int id, IGame game, int controller)
		{
			var entity = game.Entities[id];
			var zone = entity.GetTag(ZONE);
			if(zone == (int)HAND)
			{
				if(controller == game.Player.Id)
				{
					ReplayMaker.Generate(HandPos, id, ActivePlayer.Player, game);
					gameState.GameHandler.HandleZonePositionUpdate(ActivePlayer.Player, entity, HAND, gameState.GetTurnNumber());
				}
				else if(controller == game.Opponent.Id)
				{
					ReplayMaker.Generate(HandPos, id, ActivePlayer.Opponent, game);
					gameState.GameHandler.HandleZonePositionUpdate(ActivePlayer.Opponent, entity, HAND, gameState.GetTurnNumber());
				}
			}
			else if(zone == (int)PLAY)
			{
				if(controller == game.Player.Id)
				{
					ReplayMaker.Generate(BoardPos, id, ActivePlayer.Player, game);
					gameState.GameHandler.HandleZonePositionUpdate(ActivePlayer.Player, entity, PLAY, gameState.GetTurnNumber());
				}
				else if(controller == game.Opponent.Id)
				{
					ReplayMaker.Generate(BoardPos, id, ActivePlayer.Opponent, game);
					gameState.GameHandler.HandleZonePositionUpdate(ActivePlayer.Opponent, entity, PLAY, gameState.GetTurnNumber());
				}
			}
		}

		private static void NumAttacksThisTurnChange(IHsGameState gameState, int id, IGame game, int value, int controller)
		{
			if(value > 0)
			{
				if(controller == game.Player.Id)
					gameState.ProposeKeyPoint(Attack, id, ActivePlayer.Player);
				else if(controller == game.Opponent.Id)
					gameState.ProposeKeyPoint(Attack, id, ActivePlayer.Opponent);
			}
		}

		private static void CurrentPlayerChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value == 1)
			{
				var activePlayer = game.Entities[id].IsPlayer ? ActivePlayer.Player : ActivePlayer.Opponent;
				gameState.GameHandler.TurnStart(activePlayer, gameState.GetTurnNumber());
				if(activePlayer == ActivePlayer.Player)
					gameState.PlayerUsedHeroPower = false;
				else
					gameState.OpponentUsedHeroPower = false;
			}
		}

		private void CardTypeChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value == (int)TAG_CARDTYPE.HERO)
				SetHeroAsync(id, game, gameState);
		}

		private static void PlaystateChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value == (int)CONCEDED)
				gameState.GameHandler.HandleConcede();
			if(gameState.GameEnded)
				return;
			if(!game.Entities[id].IsPlayer)
				return;
			switch((TAG_PLAYSTATE)value)
			{
				case WON:
					gameState.GameEndKeyPoint(true, id);
					gameState.GameHandler.HandleWin();
					gameState.GameHandler.HandleGameEnd();
					gameState.GameEnded = true;
					break;
				case LOST:
					gameState.GameEndKeyPoint(false, id);
					gameState.GameHandler.HandleLoss();
					gameState.GameHandler.HandleGameEnd();
					gameState.GameEnded = true;
					break;
				case TIED:
					gameState.GameEndKeyPoint(false, id);
					gameState.GameHandler.HandleTied();
					gameState.GameHandler.HandleGameEnd();
					break;
			}
		}

		private static void ZoneChange(IHsGameState gameState, int id, IGame game, int value,
											 int prevValue, int controller, string cardId)
		{
			switch((TAG_ZONE)prevValue)
			{
				case DECK:
					ZoneChangeFromDeck(gameState, id, game, value, prevValue, controller, cardId);
					break;
				case HAND:
					ZoneChangeFromHand(gameState, id, game, value, prevValue, controller, cardId);
					break;
				case PLAY:
					ZoneChangeFromPlay(gameState, id, game, value, prevValue, controller, cardId);
					break;
				case TAG_ZONE.SECRET:
					ZoneChangeFromSecret(gameState, id, game, value, prevValue, controller, cardId);
					break;
				case CREATED:
					if(gameState.WasInProgress && !gameState.SetupDone && id <= 68)
						DelayedZoneChangeFromDeck(gameState, id, game, value, cardId);
					else
						ZoneChangeFromOther(gameState, id, game, value, prevValue, controller, cardId);
					break;
				case GRAVEYARD:
				case SETASIDE:
				case TAG_ZONE.INVALID:
				case REMOVEDFROMGAME:
					ZoneChangeFromOther(gameState, id, game, value, prevValue, controller, cardId);
					break;
				default:
					Log.Warn($"unhandled zone change (id={id}): {prevValue} -> {value}");
					break;
			}
		}

		private static async void DelayedZoneChangeFromDeck(IHsGameState gameState, int id, IGame game, int value, string cardId)
		{
			await game.GameTime.WaitForDuration(50);
			var entity = game.Entities[id];
			if(!entity.IsHero && !entity.IsHeroPower && !entity.HasTag(PLAYER_ID) && entity.GetTag(CARDTYPE) != (int)TAG_CARDTYPE.GAME)
			{
				if(value == (int)DECK)
					return;
				if(id < 68)
					ZoneChangeFromDeck(gameState, id, game, (int)HAND, (int)DECK, entity.GetTag(CONTROLLER), cardId);
				else if(id == 68 && entity.IsSpell)
					ZoneChangeFromOther(gameState, id, game, (int)HAND, (int)CREATED, entity.GetTag(CONTROLLER), cardId);
				if(value == (int)HAND)
					return;
				ZoneChangeFromHand(gameState, id, game, (int)PLAY, (int)HAND, entity.GetTag(CONTROLLER), cardId);
				if(value == (int)PLAY)
					return;
				ZoneChangeFromPlay(gameState, id, game, value, (int)PLAY, entity.GetTag(CONTROLLER), cardId);
			}
		}

		private static void ZoneChangeFromOther(IHsGameState gameState, int id, IGame game, int value, int prevValue, int controller,
												 string cardId)
		{
			switch((TAG_ZONE)value)
			{
				case PLAY:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerCreateInPlay(game.Entities[id], cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(Summon, id, ActivePlayer.Player);
					}
					if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentCreateInPlay(game.Entities[id], cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(Summon, id, ActivePlayer.Opponent);
					}
					break;
				case DECK:
					if(controller == game.Player.Id)
					{
						if(gameState.JoustReveals > 0)
							break;
						gameState.GameHandler.HandlePlayerGetToDeck(game.Entities[id], cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(CreateToDeck, id, ActivePlayer.Player);
					}
					if(controller == game.Opponent.Id)
					{
						if(gameState.JoustReveals > 0)
							break;
						gameState.GameHandler.HandleOpponentGetToDeck(game.Entities[id], gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(CreateToDeck, id, ActivePlayer.Opponent);
					}
					break;
				case HAND:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerGet(game.Entities[id], cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(Obtain, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentGet(game.Entities[id], gameState.GetTurnNumber(), id);
						gameState.ProposeKeyPoint(Obtain, id, ActivePlayer.Opponent);
					}
					break;
				default:
					Log.Warn($"unhandled zone change (id={id}): {prevValue} -> {value}");
					break;
			}
		}

		private static void ZoneChangeFromSecret(IHsGameState gameState, int id, IGame game, int value, int prevValue, int controller,
												  string cardId)
		{
			switch((TAG_ZONE)value)
			{
				case TAG_ZONE.SECRET:
				case GRAVEYARD:
					if(controller == game.Player.Id)
						gameState.ProposeKeyPoint(SecretTriggered, id, ActivePlayer.Player);
					if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentSecretTrigger(game.Entities[id], cardId, gameState.GetTurnNumber(), id);
						gameState.ProposeKeyPoint(SecretTriggered, id, ActivePlayer.Opponent);
					}
					break;
				default:
					Log.Warn($"unhandled zone change (id={id}): {prevValue} -> {value}");
					break;
			}
		}

		private static void ZoneChangeFromPlay(IHsGameState gameState, int id, IGame game, int value, int prevValue, int controller,
												string cardId)
		{
			switch((TAG_ZONE)value)
			{
				case HAND:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerBackToHand(game.Entities[id], cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(PlayToHand, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentPlayToHand(game.Entities[id], cardId, gameState.GetTurnNumber(), id);
						gameState.ProposeKeyPoint(PlayToHand, id, ActivePlayer.Opponent);
					}
					break;
				case DECK:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerPlayToDeck(game.Entities[id], cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(PlayToDeck, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentPlayToDeck(game.Entities[id], cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(PlayToDeck, id, ActivePlayer.Opponent);
					}
					break;
				case REMOVEDFROMGAME:
				case SETASIDE:
				case GRAVEYARD:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerPlayToGraveyard(game.Entities[id], cardId, gameState.GetTurnNumber());
						if(game.Entities[id].HasTag(HEALTH))
							gameState.ProposeKeyPoint(Death, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentPlayToGraveyard(game.Entities[id], cardId, gameState.GetTurnNumber(), gameState.PlayersTurn());
						if(game.Entities[id].HasTag(HEALTH))
							gameState.ProposeKeyPoint(Death, id, ActivePlayer.Opponent);
					}
					break;
				default:
					Log.Warn($"unhandled zone change (id={id}): {prevValue} -> {value}");
					break;
			}
		}

		private static void ZoneChangeFromHand(IHsGameState gameState, int id, IGame game, int value, int prevValue, int controller,
												string cardId)
		{
			switch((TAG_ZONE)value)
			{
				case PLAY:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerPlay(game.Entities[id], cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(Play, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentPlay(game.Entities[id], cardId, game.Entities[id].GetTag(ZONE_POSITION),
																 gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(Play, id, ActivePlayer.Opponent);
					}
					break;
				case REMOVEDFROMGAME:
				case SETASIDE:
				case GRAVEYARD:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerHandDiscard(game.Entities[id], cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(HandDiscard, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentHandDiscard(game.Entities[id], cardId, game.Entities[id].GetTag(ZONE_POSITION),
																		gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(HandDiscard, id, ActivePlayer.Opponent);
					}
					break;
				case TAG_ZONE.SECRET:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerSecretPlayed(game.Entities[id], cardId, gameState.GetTurnNumber(), false);
						gameState.ProposeKeyPoint(SecretPlayed, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentSecretPlayed(game.Entities[id], cardId, game.Entities[id].GetTag(ZONE_POSITION),
																		 gameState.GetTurnNumber(), false, id);
						gameState.ProposeKeyPoint(SecretPlayed, id, ActivePlayer.Opponent);
					}
					break;
				case DECK:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerMulligan(game.Entities[id], cardId);
						gameState.ProposeKeyPoint(Mulligan, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentMulligan(game.Entities[id], game.Entities[id].GetTag(ZONE_POSITION));
						gameState.ProposeKeyPoint(Mulligan, id, ActivePlayer.Opponent);
					}
					break;
				default:
					Log.Warn($"unhandled zone change (id={id}): {prevValue} -> {value}");
					break;
			}
		}

		private static void ZoneChangeFromDeck(IHsGameState gameState, int id, IGame game, int value, int prevValue, int controller,
													  string cardId)
		{
			switch((TAG_ZONE)value)
			{
				case HAND:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerDraw(game.Entities[id], cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(Draw, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						if(!string.IsNullOrEmpty(game.Entities[id].CardId) && gameState.SetupDone)
						{
#if DEBUG
							Log.Debug($"Opponent Draw (EntityID={id}) already has a CardID. Removing. Blizzard Pls.");
#endif
							game.Entities[id].CardId = string.Empty;
						}
						gameState.GameHandler.HandleOpponentDraw(game.Entities[id], gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(Draw, id, ActivePlayer.Opponent);
					}
					break;
				case SETASIDE:
				case REMOVEDFROMGAME:
					if(controller == game.Player.Id)
					{
						if(gameState.JoustReveals > 0)
						{
							gameState.JoustReveals--;
							break;
						}
						gameState.GameHandler.HandlePlayerRemoveFromDeck(game.Entities[id], gameState.GetTurnNumber());
					}
					else if(controller == game.Opponent.Id)
					{
						if(gameState.JoustReveals > 0)
						{
							gameState.JoustReveals--;
							break;
						}
						gameState.GameHandler.HandleOpponentRemoveFromDeck(game.Entities[id], gameState.GetTurnNumber());
					}
					break;
				case GRAVEYARD:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerDeckDiscard(game.Entities[id], cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(DeckDiscard, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentDeckDiscard(game.Entities[id], cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(DeckDiscard, id, ActivePlayer.Opponent);
					}
					break;
				case PLAY:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerDeckToPlay(game.Entities[id], cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(DeckDiscard, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentDeckToPlay(game.Entities[id], cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(DeckDiscard, id, ActivePlayer.Opponent);
					}
					break;
				case TAG_ZONE.SECRET:
					if(controller == game.Player.Id)
					{
						gameState.GameHandler.HandlePlayerSecretPlayed(game.Entities[id], cardId, gameState.GetTurnNumber(), true);
						gameState.ProposeKeyPoint(SecretPlayed, id, ActivePlayer.Player);
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler.HandleOpponentSecretPlayed(game.Entities[id], cardId, -1, gameState.GetTurnNumber(), true, id);
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
				gameState.GameHandler.SetPlayerHero(Database.GetHeroNameFromId(game.Entities[id].CardId));
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
				gameState.GameHandler.SetOpponentHero(Database.GetHeroNameFromId(game.Entities[id].CardId));
		}
	}
}