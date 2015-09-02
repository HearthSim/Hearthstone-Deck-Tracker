using System;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Replay;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
    class TagChangeHandler
    {
        public void TagChange(IHsGameState gameState, string rawTag, int id, string rawValue, IGame game, bool isRecursive = false)
        {
            if (gameState.LastId != id)
            {
                game.SecondToLastUsedId = gameState.LastId;
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

            if (tag == GAME_TAG.CONTROLLER && gameState.WaitForController != null && game.PlayerId == -1)
            {
                var p1 = game.Entities.FirstOrDefault(e => e.Value.GetTag(GAME_TAG.PLAYER_ID) == 1).Value;
                var p2 = game.Entities.FirstOrDefault(e => e.Value.GetTag(GAME_TAG.PLAYER_ID) == 2).Value;
                if (gameState.CurrentEntityHasCardId)
                {
                    if (p1 != null)
                        p1.IsPlayer = value == 1;
                    if (p2 != null)
                        p2.IsPlayer = value != 1;
                    game.PlayerId = value;
                    game.OpponentId = value == 1 ? 2 : 1;
                }
                else
                {
                    if (p1 != null)
                        p1.IsPlayer = value != 1;
                    if (p2 != null)
                        p2.IsPlayer = value == 1;
                    game.PlayerId = value == 1 ? 2 : 1;
                    game.OpponentId = value;
                }
            }
            var controller = game.Entities[id].GetTag(GAME_TAG.CONTROLLER);
            var player = game.Entities[id].HasTag(GAME_TAG.CONTROLLER) ? (controller == game.PlayerId ? "FRIENDLY" : "OPPOSING") : "";
            var cardId = game.Entities[id].CardId;
            if (tag == GAME_TAG.ZONE)
            {
                //Logger.WriteLine("--------" + player + " " + game.Entities[id].CardId + " " + (TAG_ZONE)prevZone + " -> " +
                //                 (TAG_ZONE)value);

                if (((TAG_ZONE)value == TAG_ZONE.HAND || ((TAG_ZONE)value == TAG_ZONE.PLAY) && game.IsMulliganDone) && gameState.WaitForController == null)
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
                                if (controller == game.PlayerId)
                                {
                                    gameState.GameHandler.HandlePlayerDraw(cardId, gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.Draw, id, ActivePlayer.Player);
                                }
                                else if (controller == game.OpponentId)
                                {
                                    gameState.GameHandler.HandleOpponentDraw(gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.Draw, id, ActivePlayer.Opponent);
                                }
                                break;
                            case TAG_ZONE.REMOVEDFROMGAME:
                            case TAG_ZONE.GRAVEYARD:
                            case TAG_ZONE.SETASIDE:
                            case TAG_ZONE.PLAY:
                                if (controller == game.PlayerId)
                                {
                                    gameState.GameHandler.HandlePlayerDeckDiscard(cardId, gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.DeckDiscard, id, ActivePlayer.Player);
                                }
                                else if (controller == game.OpponentId)
                                {
                                    gameState.GameHandler.HandleOpponentDeckDiscard(cardId, gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.DeckDiscard, id, ActivePlayer.Opponent);
                                }
                                break;
                            case TAG_ZONE.SECRET:
                                if (controller == game.PlayerId)
                                {
                                    gameState.GameHandler.HandlePlayerSecretPlayed(cardId, gameState.GetTurnNumber(), true);
                                    gameState.ProposeKeyPoint(KeyPointType.SecretPlayed, id, ActivePlayer.Player);
                                }
                                else if (controller == game.OpponentId)
                                {
                                    gameState.GameHandler.HandleOpponentSecretPlayed(cardId, -1, gameState.GetTurnNumber(), true, id);
                                    gameState.ProposeKeyPoint(KeyPointType.SecretPlayed, id, ActivePlayer.Player);
                                }
                                break;
                        }
                        break;
                    case TAG_ZONE.HAND:
                        switch ((TAG_ZONE)value)
                        {
                            case TAG_ZONE.PLAY:
                                if (controller == game.PlayerId)
                                {
                                    gameState.GameHandler.HandlePlayerPlay(cardId, gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.Play, id, ActivePlayer.Player);
                                }
                                else if (controller == game.OpponentId)
                                {
                                    gameState.GameHandler.HandleOpponentPlay(cardId, game.Entities[id].GetTag(GAME_TAG.ZONE_POSITION), gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.Play, id, ActivePlayer.Opponent);
                                }
                                break;
                            case TAG_ZONE.REMOVEDFROMGAME:
                            case TAG_ZONE.GRAVEYARD:
                                if (controller == game.PlayerId)
                                {
                                    gameState.GameHandler.HandlePlayerHandDiscard(cardId, gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.HandDiscard, id, ActivePlayer.Player);
                                }
                                else if (controller == game.OpponentId)
                                {
                                    gameState.GameHandler.HandleOpponentHandDiscard(cardId, game.Entities[id].GetTag(GAME_TAG.ZONE_POSITION), gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.HandDiscard, id, ActivePlayer.Opponent);
                                }
                                break;
                            case TAG_ZONE.SECRET:
                                if (controller == game.PlayerId)
                                {
                                    gameState.GameHandler.HandlePlayerSecretPlayed(cardId, gameState.GetTurnNumber(), false);
                                    gameState.ProposeKeyPoint(KeyPointType.SecretPlayed, id, ActivePlayer.Player);
                                }
                                else if (controller == game.OpponentId)
                                {
                                    gameState.GameHandler.HandleOpponentSecretPlayed(cardId, game.Entities[id].GetTag(GAME_TAG.ZONE_POSITION), gameState.GetTurnNumber(), false, id);
                                    gameState.ProposeKeyPoint(KeyPointType.SecretPlayed, id, ActivePlayer.Opponent);
                                }
                                break;
                            case TAG_ZONE.DECK:
                                if (controller == game.PlayerId)
                                {
                                    gameState.GameHandler.HandlePlayerMulligan(cardId);
                                    gameState.ProposeKeyPoint(KeyPointType.Mulligan, id, ActivePlayer.Player);
                                }
                                else if (controller == game.OpponentId)
                                {
                                    gameState.GameHandler.HandleOpponentMulligan(game.Entities[id].GetTag(GAME_TAG.ZONE_POSITION));
                                    gameState.ProposeKeyPoint(KeyPointType.Mulligan, id, ActivePlayer.Opponent);
                                }
                                break;
                        }
                        break;
                    case TAG_ZONE.PLAY:
                        switch ((TAG_ZONE)value)
                        {
                            case TAG_ZONE.HAND:
                                if (controller == game.PlayerId)
                                {
                                    gameState.GameHandler.HandlePlayerBackToHand(cardId, gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.PlayToHand, id, ActivePlayer.Player);
                                }
                                else if (controller == game.OpponentId)
                                {
                                    gameState.GameHandler.HandleOpponentPlayToHand(cardId, gameState.GetTurnNumber(), id);
                                    gameState.ProposeKeyPoint(KeyPointType.PlayToHand, id, ActivePlayer.Opponent);
                                }
                                break;
                            case TAG_ZONE.DECK:
                                if (controller == game.PlayerId)
                                {
                                    gameState.GameHandler.HandlePlayerPlayToDeck(cardId, gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.PlayToDeck, id, ActivePlayer.Player);
                                }
                                else if (controller == game.OpponentId)
                                {
                                    gameState.GameHandler.HandleOpponentPlayToDeck(cardId, gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.PlayToDeck, id, ActivePlayer.Opponent);
                                }
                                break;
                            case TAG_ZONE.GRAVEYARD:
                                if (game.Entities[id].HasTag(GAME_TAG.HEALTH))
                                {
                                    if (controller == game.PlayerId)
                                        gameState.ProposeKeyPoint(KeyPointType.Death, id, ActivePlayer.Player);
                                    else if (controller == game.OpponentId)
                                        gameState.ProposeKeyPoint(KeyPointType.Death, id, ActivePlayer.Opponent);
                                }
                                break;
                        }
                        break;
                    case TAG_ZONE.SECRET:
                        switch ((TAG_ZONE)value)
                        {
                            case TAG_ZONE.SECRET:
                            case TAG_ZONE.GRAVEYARD:
                                if (controller == game.PlayerId)
                                    gameState.ProposeKeyPoint(KeyPointType.SecretTriggered, id, ActivePlayer.Player);
                                if (controller == game.OpponentId)
                                {
                                    gameState.GameHandler.HandleOpponentSecretTrigger(cardId, gameState.GetTurnNumber(), id);
                                    gameState.ProposeKeyPoint(KeyPointType.SecretTriggered, id, ActivePlayer.Opponent);
                                }
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
                                if (controller == game.PlayerId)
                                    gameState.ProposeKeyPoint(KeyPointType.Summon, id, ActivePlayer.Player);
                                if (controller == game.OpponentId)
                                    gameState.ProposeKeyPoint(KeyPointType.Summon, id, ActivePlayer.Opponent);
                                break;
                            case TAG_ZONE.HAND:
                                if (controller == game.PlayerId)
                                {
                                    gameState.GameHandler.HandlePlayerGet(cardId, gameState.GetTurnNumber());
                                    gameState.ProposeKeyPoint(KeyPointType.Obtain, id, ActivePlayer.Player);
                                }
                                else if (controller == game.OpponentId)
                                {
                                    gameState.GameHandler.HandleOpponentGet(gameState.GetTurnNumber(), id);
                                    gameState.ProposeKeyPoint(KeyPointType.Obtain, id, ActivePlayer.Opponent);
                                }
                                break;
                        }
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
            else if (tag == GAME_TAG.NUM_ATTACKS_THIS_TURN && value > 0)
            {
                if (controller == game.PlayerId)
                    gameState.ProposeKeyPoint(KeyPointType.Attack, id, ActivePlayer.Player);
                else if (controller == game.OpponentId)
                    gameState.ProposeKeyPoint(KeyPointType.Attack, id, ActivePlayer.Opponent);
            }
            else if (tag == GAME_TAG.ZONE_POSITION)
            {
                var zone = game.Entities[id].GetTag(GAME_TAG.ZONE);
                if (zone == (int)TAG_ZONE.HAND)
                {
                    if (controller == game.PlayerId)
                        ReplayMaker.Generate(KeyPointType.HandPos, id, ActivePlayer.Player, game);
                    else if (controller == game.OpponentId)
                        ReplayMaker.Generate(KeyPointType.HandPos, id, ActivePlayer.Opponent, game);
                }
                else if (zone == (int)TAG_ZONE.PLAY)
                {
                    if (controller == game.PlayerId)
                        ReplayMaker.Generate(KeyPointType.BoardPos, id, ActivePlayer.Player, game);
                    else if (controller == game.OpponentId)
                        ReplayMaker.Generate(KeyPointType.BoardPos, id, ActivePlayer.Opponent, game);
                }
            }
            else if (tag == GAME_TAG.CARD_TARGET && value > 0)
            {
                if (controller == game.PlayerId)
                    gameState.ProposeKeyPoint(KeyPointType.PlaySpell, id, ActivePlayer.Player);
                else if (controller == game.OpponentId)
                    gameState.ProposeKeyPoint(KeyPointType.PlaySpell, id, ActivePlayer.Opponent);
            }
            else if (tag == GAME_TAG.EQUIPPED_WEAPON && value == 0)
            {
                if (controller == game.PlayerId)
                    gameState.ProposeKeyPoint(KeyPointType.WeaponDestroyed, id, ActivePlayer.Player);
                else if (controller == game.OpponentId)
                    gameState.ProposeKeyPoint(KeyPointType.WeaponDestroyed, id, ActivePlayer.Opponent);
            }
            else if (tag == GAME_TAG.EXHAUSTED && value > 0)
            {
                if (game.Entities[id].GetTag(GAME_TAG.CARDTYPE) == (int)TAG_CARDTYPE.HERO_POWER)
                {
                    if (controller == game.PlayerId)
                        gameState.ProposeKeyPoint(KeyPointType.HeroPower, id, ActivePlayer.Player);
                    else if (controller == game.OpponentId)
                        gameState.ProposeKeyPoint(KeyPointType.HeroPower, id, ActivePlayer.Opponent);
                }
            }
            else if (tag == GAME_TAG.CONTROLLER && game.Entities[id].IsInZone(TAG_ZONE.SECRET))
            {
                if (value == game.PlayerId)
                {
                    gameState.GameHandler.HandleOpponentSecretTrigger(cardId, gameState.GetTurnNumber(), id);
                    gameState.ProposeKeyPoint(KeyPointType.SecretStolen, id, ActivePlayer.Player);
                }
                else if (value == game.OpponentId)
                    gameState.ProposeKeyPoint(KeyPointType.SecretStolen, id, ActivePlayer.Player);
            }
            else if (tag == GAME_TAG.FATIGUE)
            {
                if (controller == game.PlayerId)
                    gameState.GameHandler.HandlePlayerFatigue(Convert.ToInt32(rawValue));
                else if (controller == game.OpponentId)
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