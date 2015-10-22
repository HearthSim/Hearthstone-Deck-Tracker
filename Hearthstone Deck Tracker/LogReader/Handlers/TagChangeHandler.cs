using System;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Replay;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
    class TagChangeHandler
    {
        public void TagChange(IHsGameState gameState, string rawTag, int id, string rawValue, IGame game, bool isRecursive = false)
        {
            if (gameState.LastId != id)
            {
                //game.SecondToLastUsedId = gameState.LastId;
                if (gameState.ProposedKeyPoint != null)
                {
                    ReplayMaker.Generate(gameState.ProposedKeyPoint.Type, gameState.ProposedKeyPoint.Id, gameState.ProposedKeyPoint.Player, game);
                    gameState.ProposedKeyPoint = null;
                }
            }
            gameState.LastId = id;
            if (!game.Entities.ContainsKey(id))
                game.Entities.Add(id, new Entity(id));
            GAME_TAG tag;
            if (!Enum.TryParse(rawTag, out tag))
            {
                int tmp;
                if (int.TryParse(rawTag, out tmp) && Enum.IsDefined(typeof(GAME_TAG), tmp))
                    tag = (GAME_TAG)tmp;
            }
            var value = HsLogReaderV2.ParseTagValue(tag, rawValue);
            var prevZone = game.Entities[id].GetTag(GAME_TAG.ZONE);
            game.Entities[id].SetTag(tag, value);

            if (tag == GAME_TAG.CONTROLLER && gameState.WaitForController != null && game.Player.Id == -1)
            {
                var p1 = game.Entities.FirstOrDefault(e => e.Value.GetTag(GAME_TAG.PLAYER_ID) == 1).Value;
                var p2 = game.Entities.FirstOrDefault(e => e.Value.GetTag(GAME_TAG.PLAYER_ID) == 2).Value;
                if (gameState.CurrentEntityHasCardId)
                {
                    if (p1 != null)
                        p1.IsPlayer = value == 1;
                    if (p2 != null)
                        p2.IsPlayer = value != 1;
                    game.Player.Id = value;
                    game.Opponent.Id = value == 1 ? 2 : 1;
                }
                else
                {
                    if (p1 != null)
                        p1.IsPlayer = value != 1;
                    if (p2 != null)
                        p2.IsPlayer = value == 1;
                    game.Player.Id = value == 1 ? 2 : 1;
                    game.Opponent.Id = value;
                }
            }
            var controller = game.Entities[id].GetTag(GAME_TAG.CONTROLLER);
            var player = game.Entities[id].HasTag(GAME_TAG.CONTROLLER) ? (controller == game.Player.Id ? "FRIENDLY" : "OPPOSING") : "";
            var cardId = game.Entities[id].CardId;
            if (tag == GAME_TAG.ZONE)
            {
                //Logger.WriteLine("--------" + player + " " + game.Entities[id].CardId + " " + (TAG_ZONE)prevZone + " -> " +
                //                 (TAG_ZONE)value);

                if (((TAG_ZONE)value == TAG_ZONE.HAND || ((TAG_ZONE)value == TAG_ZONE.PLAY || (TAG_ZONE)value == TAG_ZONE.DECK) && game.IsMulliganDone) && gameState.WaitForController == null)
                {
                    if (!game.IsMulliganDone)
                        prevZone = (int)TAG_ZONE.DECK;
                    if (controller == 0)
                    {
                        game.Entities[id].SetTag(GAME_TAG.ZONE, prevZone);
                        gameState.WaitForController = new { Tag = rawTag, Id = id, Value = rawValue };
                        //Logger.WriteLine("CURRENTLY NO CONTROLLER SET FOR CARD, WAITING...");
                        return;
                    }
                }
                switch ((TAG_ZONE)prevZone)
                {
                    case TAG_ZONE.DECK:
                        switch ((TAG_ZONE)value)
                        {
                            case TAG_ZONE.HAND:
                                if (controller == game.Player.Id)
                                {
                                    gameState.GameHandler.HandlePlayerDraw(game.Entities[id], cardId, gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.Draw, id, ActivePlayer.Player);
                                }
                                else if (controller == game.Opponent.Id)
                                {
	                                if(!string.IsNullOrEmpty(game.Entities[id].CardId))
	                                {
#if DEBUG
										Logger.WriteLine(string.Format("Opponent Draw (EntityID={0}) already has a CardID. Removing. Blizzard Pls.", id), "TagChange");
#endif
										game.Entities[id].CardId = string.Empty;
	                                }
                                    gameState.GameHandler.HandleOpponentDraw(game.Entities[id], gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.Draw, id, ActivePlayer.Opponent);
                                }
                                break;
							case TAG_ZONE.SETASIDE:
							case TAG_ZONE.REMOVEDFROMGAME:
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
							case TAG_ZONE.GRAVEYARD:
								if(controller == game.Player.Id)
								{
									gameState.GameHandler.HandlePlayerDeckDiscard(game.Entities[id], cardId, gameState.GetTurnNumber());
									gameState.ProposeKeyPoint(KeyPointType.DeckDiscard, id, ActivePlayer.Player);
								}
								else if(controller == game.Opponent.Id)
								{
									gameState.GameHandler.HandleOpponentDeckDiscard(game.Entities[id], cardId, gameState.GetTurnNumber());
									gameState.ProposeKeyPoint(KeyPointType.DeckDiscard, id, ActivePlayer.Opponent);
								}
								break;
							case TAG_ZONE.PLAY:
                                if (controller == game.Player.Id)
                                {
                                    gameState.GameHandler.HandlePlayerDeckToPlay(game.Entities[id], cardId, gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.DeckDiscard, id, ActivePlayer.Player);
                                }
                                else if (controller == game.Opponent.Id)
                                {
                                    gameState.GameHandler.HandleOpponentDeckToPlay(game.Entities[id], cardId, gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.DeckDiscard, id, ActivePlayer.Opponent);
                                }
                                break;
                            case TAG_ZONE.SECRET:
                                if (controller == game.Player.Id)
                                {
                                    gameState.GameHandler.HandlePlayerSecretPlayed(game.Entities[id], cardId, gameState.GetTurnNumber(), true);
                                    gameState.ProposeKeyPoint(KeyPointType.SecretPlayed, id, ActivePlayer.Player);
                                }
                                else if (controller == game.Opponent.Id)
                                {
                                    gameState.GameHandler.HandleOpponentSecretPlayed(game.Entities[id], cardId, -1, gameState.GetTurnNumber(), true, id);
                                    gameState.ProposeKeyPoint(KeyPointType.SecretPlayed, id, ActivePlayer.Player);
                                }
                                break;
							default:
								Logger.WriteLine(string.Format("WARNING - unhandled zone change (id={0}): {1} -> {2}", id, (TAG_ZONE)prevZone, (TAG_ZONE)value), "TagChange");
		                        break;


                        }
						break;
                    case TAG_ZONE.HAND:
                        switch ((TAG_ZONE)value)
                        {
                            case TAG_ZONE.PLAY:
                                if (controller == game.Player.Id)
                                {
                                    gameState.GameHandler.HandlePlayerPlay(game.Entities[id], cardId, gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.Play, id, ActivePlayer.Player);
                                }
                                else if (controller == game.Opponent.Id)
                                {
                                    gameState.GameHandler.HandleOpponentPlay(game.Entities[id], cardId, game.Entities[id].GetTag(GAME_TAG.ZONE_POSITION), gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.Play, id, ActivePlayer.Opponent);
                                }
                                break;
                            case TAG_ZONE.REMOVEDFROMGAME:
                            case TAG_ZONE.GRAVEYARD:
                                if (controller == game.Player.Id)
                                {
                                    gameState.GameHandler.HandlePlayerHandDiscard(game.Entities[id], cardId, gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.HandDiscard, id, ActivePlayer.Player);
                                }
                                else if (controller == game.Opponent.Id)
                                {
                                    gameState.GameHandler.HandleOpponentHandDiscard(game.Entities[id], cardId, game.Entities[id].GetTag(GAME_TAG.ZONE_POSITION), gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.HandDiscard, id, ActivePlayer.Opponent);
                                }
                                break;
                            case TAG_ZONE.SECRET:
                                if (controller == game.Player.Id)
                                {
                                    gameState.GameHandler.HandlePlayerSecretPlayed(game.Entities[id], cardId, gameState.GetTurnNumber(), false);
                                    gameState.ProposeKeyPoint(KeyPointType.SecretPlayed, id, ActivePlayer.Player);
                                }
                                else if (controller == game.Opponent.Id)
                                {
                                    gameState.GameHandler.HandleOpponentSecretPlayed(game.Entities[id], cardId, game.Entities[id].GetTag(GAME_TAG.ZONE_POSITION), gameState.GetTurnNumber(), false, id);
                                    gameState.ProposeKeyPoint(KeyPointType.SecretPlayed, id, ActivePlayer.Opponent);
                                }
                                break;
                            case TAG_ZONE.DECK:
                                if (controller == game.Player.Id)
                                {
                                    gameState.GameHandler.HandlePlayerMulligan(game.Entities[id], cardId);
                                    gameState.ProposeKeyPoint(KeyPointType.Mulligan, id, ActivePlayer.Player);
                                }
                                else if (controller == game.Opponent.Id)
                                {
                                    gameState.GameHandler.HandleOpponentMulligan(game.Entities[id], game.Entities[id].GetTag(GAME_TAG.ZONE_POSITION));
                                    gameState.ProposeKeyPoint(KeyPointType.Mulligan, id, ActivePlayer.Opponent);
                                }
                                break;
							default:
								Logger.WriteLine(string.Format("WARNING - unhandled zone change (id={0}): {1} -> {2}", id, (TAG_ZONE)prevZone, (TAG_ZONE)value), "TagChange");
								break;
						}
                        break;
                    case TAG_ZONE.PLAY:
                        switch ((TAG_ZONE)value)
                        {
                            case TAG_ZONE.HAND:
                                if (controller == game.Player.Id)
                                {
                                    gameState.GameHandler.HandlePlayerBackToHand(game.Entities[id], cardId, gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.PlayToHand, id, ActivePlayer.Player);
                                }
                                else if (controller == game.Opponent.Id)
                                {
                                    gameState.GameHandler.HandleOpponentPlayToHand(game.Entities[id], cardId, gameState.GetTurnNumber(), id);
                                    gameState.ProposeKeyPoint(KeyPointType.PlayToHand, id, ActivePlayer.Opponent);
                                }
                                break;
                            case TAG_ZONE.DECK:
                                if (controller == game.Player.Id)
                                {
                                    gameState.GameHandler.HandlePlayerPlayToDeck(game.Entities[id], cardId, gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.PlayToDeck, id, ActivePlayer.Player);
                                }
                                else if (controller == game.Opponent.Id)
                                {
                                    gameState.GameHandler.HandleOpponentPlayToDeck(game.Entities[id], cardId, gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.PlayToDeck, id, ActivePlayer.Opponent);
                                }
                                break;
                            case TAG_ZONE.GRAVEYARD:
	                                if(controller == game.Player.Id)
	                                {
		                                gameState.GameHandler.HandlePlayerPlayToGraveyard(game.Entities[id], cardId,
		                                                                                   gameState.GetTurnNumber());
										if(game.Entities[id].HasTag(GAME_TAG.HEALTH))
											gameState.ProposeKeyPoint(KeyPointType.Death, id, ActivePlayer.Player);
	                                }
                                    else if(controller == game.Opponent.Id)
									{
                                        gameState.GameHandler.HandleOpponentPlayToGraveyard(game.Entities[id], cardId,
                                                                                           gameState.GetTurnNumber(), gameState.PlayersTurn());
                                        if (game.Entities[id].HasTag(GAME_TAG.HEALTH))
											gameState.ProposeKeyPoint(KeyPointType.Death, id, ActivePlayer.Opponent);
                                    }
		                        break;
							default:
								Logger.WriteLine(string.Format("WARNING - unhandled zone change (id={0}): {1} -> {2}", id, (TAG_ZONE)prevZone, (TAG_ZONE)value), "TagChange");
								break;
						}
                        break;
                    case TAG_ZONE.SECRET:
                        switch ((TAG_ZONE)value)
                        {
                            case TAG_ZONE.SECRET:
                            case TAG_ZONE.GRAVEYARD:
                                if (controller == game.Player.Id)
                                    gameState.ProposeKeyPoint(KeyPointType.SecretTriggered, id, ActivePlayer.Player);
                                if (controller == game.Opponent.Id)
                                {
                                    gameState.GameHandler.HandleOpponentSecretTrigger(game.Entities[id], cardId, gameState.GetTurnNumber(), id);
                                    gameState.ProposeKeyPoint(KeyPointType.SecretTriggered, id, ActivePlayer.Opponent);
                                }
                                break;
							default:
								Logger.WriteLine(string.Format("WARNING - unhandled zone change (id={0}): {1} -> {2}", id, (TAG_ZONE)prevZone, (TAG_ZONE)value), "TagChange");
								break;
						}
                        break;
                    case TAG_ZONE.GRAVEYARD:
                    case TAG_ZONE.SETASIDE:
                    case TAG_ZONE.CREATED:
                    case TAG_ZONE.INVALID:
                    case TAG_ZONE.REMOVEDFROMGAME:
                        switch ((TAG_ZONE)value)
						{
							case TAG_ZONE.PLAY:
								if(controller == game.Player.Id)
								{
									gameState.GameHandler.HandlePlayerCreateInPlay(game.Entities[id], cardId, gameState.GetTurnNumber());
									gameState.ProposeKeyPoint(KeyPointType.Summon, id, ActivePlayer.Player);
								}
								if(controller == game.Opponent.Id)
								{
									gameState.GameHandler.HandleOpponentCreateInPlay(game.Entities[id], cardId, gameState.GetTurnNumber());
									gameState.ProposeKeyPoint(KeyPointType.Summon, id, ActivePlayer.Opponent);
								}
								break;
							case TAG_ZONE.DECK:
								if(controller == game.Player.Id)
								{
									if(gameState.JoustReveals > 0)
										break;
									gameState.GameHandler.HandlePlayerGetToDeck(game.Entities[id], cardId, gameState.GetTurnNumber());
									gameState.ProposeKeyPoint(KeyPointType.CreateToDeck, id, ActivePlayer.Player);
								}
								if(controller == game.Opponent.Id)
								{
									if(gameState.JoustReveals > 0)
										break;
									gameState.GameHandler.HandleOpponentGetToDeck(game.Entities[id], gameState.GetTurnNumber());
									gameState.ProposeKeyPoint(KeyPointType.CreateToDeck, id, ActivePlayer.Opponent);
								}
								break;
							case TAG_ZONE.HAND:
                                if (controller == game.Player.Id)
                                {
                                    gameState.GameHandler.HandlePlayerGet(game.Entities[id], cardId, gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.Obtain, id, ActivePlayer.Player);
                                }
                                else if (controller == game.Opponent.Id)
                                {
                                    gameState.GameHandler.HandleOpponentGet(game.Entities[id], gameState.GetTurnNumber(), id);
                                    gameState.ProposeKeyPoint(KeyPointType.Obtain, id, ActivePlayer.Opponent);
                                }
                                break;
							default:
								Logger.WriteLine(string.Format("WARNING - unhandled zone change (id={0}): {1} -> {2}", id, (TAG_ZONE)prevZone, (TAG_ZONE)value), "TagChange");
								break;
						}
                        break;
					default:
						Logger.WriteLine(string.Format("WARNING - unhandled zone change (id={0}): {1} -> {2}", id, (TAG_ZONE)prevZone, (TAG_ZONE)value), "TagChange");
		                break;
                }
            }
            else if (tag == GAME_TAG.PLAYSTATE)
            {
                if (value == (int)TAG_PLAYSTATE.QUIT)
                    gameState.GameHandler.HandleConcede();
                if (!gameState.GameEnded)
                {
                    if (game.Entities[id].IsPlayer)
                    {
                        if (value == (int)TAG_PLAYSTATE.WON)
                        {
                            gameState.GameEndKeyPoint(true, id);
                            gameState.GameHandler.HandleWin();
                            gameState.GameHandler.HandleGameEnd();
                            gameState.GameEnded = true;
                        }
                        else if (value == (int)TAG_PLAYSTATE.LOST)
                        {
                            gameState.GameEndKeyPoint(false, id);
                            gameState.GameHandler.HandleLoss();
                            gameState.GameHandler.HandleGameEnd();
                            gameState.GameEnded = true;
                        }
                        else if (value == (int)TAG_PLAYSTATE.TIED)
                        {
                            gameState.GameEndKeyPoint(false, id);
                            gameState.GameHandler.HandleTied();
                            gameState.GameHandler.HandleGameEnd();
                        }
                    }
                }
            }
            else if (tag == GAME_TAG.CURRENT_PLAYER && value == 1)
            {
                var activePlayer = game.Entities[id].IsPlayer ? ActivePlayer.Player : ActivePlayer.Opponent;
                gameState.GameHandler.TurnStart(activePlayer, gameState.GetTurnNumber());
                if (activePlayer == ActivePlayer.Player)
                    gameState.PlayerUsedHeroPower = false;
                else
                    gameState.OpponentUsedHeroPower = false;
            }
            else if (tag == GAME_TAG.DEFENDING)
            {
                if (player == "OPPOSING")
                    gameState.GameHandler.HandleDefendingEntity(value == 1 ? game.Entities[id] : null);
            }
            else if (tag == GAME_TAG.ATTACKING)
            {
                if (player == "FRIENDLY")
                    gameState.GameHandler.HandleAttackingEntity(value == 1 ? game.Entities[id] : null);
            }
            else if (tag == GAME_TAG.PROPOSED_DEFENDER)
            {
                game.OpponentSecrets.proposedDefenderEntityId = value;
            }
            else if (tag == GAME_TAG.PROPOSED_ATTACKER)
            {
                game.OpponentSecrets.proposedAttackerEntityId = value;
            }
            else if (tag == GAME_TAG.NUM_MINIONS_PLAYED_THIS_TURN && value > 0)
            {
                if (gameState.PlayersTurn())
                {
                    gameState.GameHandler.HandlePlayerMinionPlayed();
                }
            }
            else if (tag == GAME_TAG.PREDAMAGE && value > 0)
            {
                if (gameState.PlayersTurn())
                {
                    gameState.GameHandler.HandleOpponentDamage(game.Entities[id]);
                }
            }
            else if (tag == GAME_TAG.NUM_TURNS_IN_PLAY && value > 0)
            {
                if (!gameState.PlayersTurn())
                {
                    gameState.GameHandler.HandleOpponentTurnStart(game.Entities[id]);
                }
            }
            else if (tag == GAME_TAG.NUM_ATTACKS_THIS_TURN && value > 0)
            {
                if (controller == game.Player.Id)
                    gameState.ProposeKeyPoint(KeyPointType.Attack, id, ActivePlayer.Player);
                else if (controller == game.Opponent.Id)
                    gameState.ProposeKeyPoint(KeyPointType.Attack, id, ActivePlayer.Opponent);
            }
            else if (tag == GAME_TAG.ZONE_POSITION)
            {
                var zone = game.Entities[id].GetTag(GAME_TAG.ZONE);
                if (zone == (int)TAG_ZONE.HAND)
                {
	                if(controller == game.Player.Id)
	                {
		                ReplayMaker.Generate(KeyPointType.HandPos, id, ActivePlayer.Player, game);
						gameState.GameHandler.HandleZonePositionUpdate(ActivePlayer.Player, TAG_ZONE.HAND, gameState.GetTurnNumber());
					}
                    else if(controller == game.Opponent.Id)
                    {
	                    ReplayMaker.Generate(KeyPointType.HandPos, id, ActivePlayer.Opponent, game);
						gameState.GameHandler.HandleZonePositionUpdate(ActivePlayer.Opponent, TAG_ZONE.HAND, gameState.GetTurnNumber());
					}
                }
                else if (zone == (int)TAG_ZONE.PLAY)
                {
	                if(controller == game.Player.Id)
	                {
		                ReplayMaker.Generate(KeyPointType.BoardPos, id, ActivePlayer.Player, game);
						gameState.GameHandler.HandleZonePositionUpdate(ActivePlayer.Player, TAG_ZONE.PLAY, gameState.GetTurnNumber());
					}
                    else if(controller == game.Opponent.Id)
                    {
	                    ReplayMaker.Generate(KeyPointType.BoardPos, id, ActivePlayer.Opponent, game);
						gameState.GameHandler.HandleZonePositionUpdate(ActivePlayer.Opponent, TAG_ZONE.PLAY, gameState.GetTurnNumber());
					}
                }
            }
            else if (tag == GAME_TAG.CARD_TARGET && value > 0)
            {
                if (controller == game.Player.Id)
                    gameState.ProposeKeyPoint(KeyPointType.PlaySpell, id, ActivePlayer.Player);
                else if (controller == game.Opponent.Id)
                    gameState.ProposeKeyPoint(KeyPointType.PlaySpell, id, ActivePlayer.Opponent);
            }
            else if (tag == GAME_TAG.EQUIPPED_WEAPON && value == 0)
            {
                if (controller == game.Player.Id)
                    gameState.ProposeKeyPoint(KeyPointType.WeaponDestroyed, id, ActivePlayer.Player);
                else if (controller == game.Opponent.Id)
                    gameState.ProposeKeyPoint(KeyPointType.WeaponDestroyed, id, ActivePlayer.Opponent);
            }
            else if (tag == GAME_TAG.EXHAUSTED && value > 0)
            {
                if (game.Entities[id].GetTag(GAME_TAG.CARDTYPE) == (int)TAG_CARDTYPE.HERO_POWER)
                {
                    if (controller == game.Player.Id)
                        gameState.ProposeKeyPoint(KeyPointType.HeroPower, id, ActivePlayer.Player);
                    else if (controller == game.Opponent.Id)
                        gameState.ProposeKeyPoint(KeyPointType.HeroPower, id, ActivePlayer.Opponent);
                }
            }
            else if (tag == GAME_TAG.CONTROLLER)
            {
	            if (value == game.Player.Id)
	            {
		            if (game.Entities[id].IsInZone(TAG_ZONE.SECRET))
					{
						gameState.GameHandler.HandleOpponentStolen(game.Entities[id], cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(KeyPointType.SecretStolen, id, ActivePlayer.Player);
		            }
		            else if (game.Entities[id].IsInZone(TAG_ZONE.PLAY))
						gameState.GameHandler.HandleOpponentStolen(game.Entities[id], cardId, gameState.GetTurnNumber());
				}
	            else if (value == game.Opponent.Id)
	            {
		            if(game.Entities[id].IsInZone(TAG_ZONE.SECRET))
					{
						gameState.GameHandler.HandleOpponentStolen(game.Entities[id], cardId, gameState.GetTurnNumber());
						gameState.ProposeKeyPoint(KeyPointType.SecretStolen, id, ActivePlayer.Player);
		            }
					else if (game.Entities[id].IsInZone(TAG_ZONE.PLAY))
						gameState.GameHandler.HandlePlayerStolen(game.Entities[id], cardId, gameState.GetTurnNumber());
	            }
            }
            else if (tag == GAME_TAG.FATIGUE)
            {
                if (controller == game.Player.Id)
                    gameState.GameHandler.HandlePlayerFatigue(Convert.ToInt32(rawValue));
                else if (controller == game.Opponent.Id)
                    gameState.GameHandler.HandleOpponentFatigue(Convert.ToInt32(rawValue));
            }
            if (gameState.WaitForController != null)
            {
                if (!isRecursive)
                {
                    TagChange(gameState, (string)gameState.WaitForController.Tag, (int)gameState.WaitForController.Id, (string)gameState.WaitForController.Value, game, true);
                    gameState.WaitForController = null;
                }
            }
        }

    }
}