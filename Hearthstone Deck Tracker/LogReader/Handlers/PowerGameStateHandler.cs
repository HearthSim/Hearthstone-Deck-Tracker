using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Replay;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
    public class PowerGameStateHandler
    {
        private readonly List<Entity> _tmpEntities = new List<Entity>();
        private readonly TagChangeHandler _tagChangeHandler = new TagChangeHandler();

        public void Handle(string logLine, IHsGameState gameState, IGame game)
        {
            if (logLine.Contains("CREATE_GAME"))
            {
                gameState.GameHandler.HandleGameStart();
                gameState.GameEnded = false;
                gameState.AddToTurn = -1;
                gameState.GameLoaded = true;
                gameState.LastGameStart = DateTime.Now;
            }
            else if (HsLogReaderConstants.PowerTaskList.GameEntityRegex.IsMatch(logLine))
            {
                gameState.GameHandler.HandleGameStart();
                gameState.GameEnded = false;
                gameState.AddToTurn = -1;
                var match = HsLogReaderConstants.PowerTaskList.GameEntityRegex.Match(logLine);
                var id = int.Parse(match.Groups["id"].Value);
                if (!game.Entities.ContainsKey(id))
                    game.Entities.Add(id, new Entity(id));
                gameState.CurrentEntityId = id;
            }
            else if (HsLogReaderConstants.PowerTaskList.PlayerEntityRegex.IsMatch(logLine))
            {
                var match = HsLogReaderConstants.PowerTaskList.PlayerEntityRegex.Match(logLine);
                var id = int.Parse(match.Groups["id"].Value);
                if (!game.Entities.ContainsKey(id))
                    game.Entities.Add(id, new Entity(id));
                gameState.CurrentEntityId = id;
            }
            else if (HsLogReaderConstants.PowerTaskList.TagChangeRegex.IsMatch(logLine))
            {
                var match = HsLogReaderConstants.PowerTaskList.TagChangeRegex.Match(logLine);
                var rawEntity = match.Groups["entity"].Value.Replace("UNKNOWN ENTITY ", "");
                int entityId;
                if (rawEntity.StartsWith("[") && HsLogReaderConstants.PowerTaskList.EntityRegex.IsMatch(rawEntity))
                {
                    var entity = HsLogReaderConstants.PowerTaskList.EntityRegex.Match(rawEntity);
                    var id = int.Parse(entity.Groups["id"].Value);
                    _tagChangeHandler.TagChange(gameState, match.Groups["tag"].Value, id, match.Groups["value"].Value, game);
                }
                else if (int.TryParse(rawEntity, out entityId))
                    _tagChangeHandler.TagChange(gameState, match.Groups["tag"].Value, entityId, match.Groups["value"].Value, game);
                else
                {
                    var entity = game.Entities.FirstOrDefault(x => x.Value.Name == rawEntity);

	                if(entity.Value == null)
	                {
		                entity = game.Entities.FirstOrDefault(x => x.Value.Name == "UNKNOWN HUMAN PLAYER");
		                if(entity.Value != null)
			                entity.Value.Name = rawEntity;
	                }

                    if (entity.Value == null)
                    {
                        //while the id is unknown, store in tmp entities
                        var tmpEntity = _tmpEntities.FirstOrDefault(x => x.Name == rawEntity);
                        if (tmpEntity == null)
                        {
                            tmpEntity = new Entity(_tmpEntities.Count + 1);
                            tmpEntity.Name = rawEntity;
                            _tmpEntities.Add(tmpEntity);
                        }
                        GAME_TAG tag;
                        Enum.TryParse(match.Groups["tag"].Value, out tag);
                        var value = HsLogReaderV2.ParseTagValue(tag, match.Groups["value"].Value);
                        tmpEntity.SetTag(tag, value);
                        if (tmpEntity.HasTag(GAME_TAG.ENTITY_ID))
                        {
                            var id = tmpEntity.GetTag(GAME_TAG.ENTITY_ID);
                            if (game.Entities.ContainsKey(id))
                            {
                                game.Entities[id].Name = tmpEntity.Name;
                                foreach (var t in tmpEntity.Tags)
                                    game.Entities[id].SetTag(t.Key, t.Value);
                                _tmpEntities.Remove(tmpEntity);
                                //Logger.WriteLine("COPIED TMP ENTITY (" + rawEntity + ")");
                            }
                            else
                                Logger.WriteLine(
                                    "TMP ENTITY (" + rawEntity + ") NOW HAS A KEY, BUT GAME.ENTITIES DOES NOT CONTAIN THIS KEY",
                                    "LogReader");
                        }
                    }
                    else
                        _tagChangeHandler.TagChange(gameState, match.Groups["tag"].Value, entity.Key, match.Groups["value"].Value, game);
                }

                if (HsLogReaderConstants.PowerTaskList.EntityNameRegex.IsMatch(logLine))
                {
                    match = HsLogReaderConstants.PowerTaskList.EntityNameRegex.Match(logLine);
                    var name = match.Groups["name"].Value;
                    var player = int.Parse(match.Groups["value"].Value);
                    if (player == 1)
                        gameState.GameHandler.HandlePlayerName(name);
                    else if (player == 2)
                        gameState.GameHandler.HandleOpponentName(name);
                }
            }
            else if (HsLogReaderConstants.PowerTaskList.CreationRegex.IsMatch(logLine))
            {
                var match = HsLogReaderConstants.PowerTaskList.CreationRegex.Match(logLine);
                var id = int.Parse(match.Groups["id"].Value);
                var cardId = match.Groups["cardId"].Value;
	            if(!game.Entities.ContainsKey(id))
	            {
		            if(string.IsNullOrEmpty(cardId))
		            {
			            if(gameState.KnownCardIds.TryGetValue(id, out cardId))
			            {
				            Logger.WriteLine(string.Format("Found known cardId for entity {0}: {1}", id, cardId));
				            gameState.KnownCardIds.Remove(id);
			            }
		            }
		            game.Entities.Add(id, new Entity(id) { CardId = cardId });
	            }
                gameState.CurrentEntityId = id;
                gameState.CurrentEntityHasCardId = !string.IsNullOrEmpty(cardId);
            }
            else if (HsLogReaderConstants.PowerTaskList.UpdatingEntityRegex.IsMatch(logLine))
            {
                var match = HsLogReaderConstants.PowerTaskList.UpdatingEntityRegex.Match(logLine);
                var cardId = match.Groups["cardId"].Value;
                var rawEntity = match.Groups["entity"].Value;
                int entityId;
                if (rawEntity.StartsWith("[") && HsLogReaderConstants.PowerTaskList.EntityRegex.IsMatch(rawEntity))
                {
                    var entity = HsLogReaderConstants.PowerTaskList.EntityRegex.Match(rawEntity);
                    entityId = int.Parse(entity.Groups["id"].Value);
                }
                else if (!int.TryParse(rawEntity, out entityId))
                    entityId = -1;
                if (entityId != -1)
                {
                    gameState.CurrentEntityId = entityId;
                    if (!game.Entities.ContainsKey(entityId))
                        game.Entities.Add(entityId, new Entity(entityId));
                    game.Entities[entityId].CardId = cardId;
                }
				if(gameState.JoustReveals > 0)
				{
					Entity currentEntity;
					if(game.Entities.TryGetValue(entityId, out currentEntity))
					{
						if(currentEntity.IsControlledBy(game.Opponent.Id))
							gameState.GameHandler.HandleOpponentJoust(currentEntity, cardId, gameState.GetTurnNumber());
						else if(currentEntity.IsControlledBy(game.Player.Id))
							gameState.GameHandler.HandlePlayerJoust(currentEntity, cardId, gameState.GetTurnNumber());
					}
					//gameState.JoustReveals--;
				}
			}
            else if (HsLogReaderConstants.PowerTaskList.CreationTagRegex.IsMatch(logLine) && !logLine.Contains("HIDE_ENTITY"))
            {
                var match = HsLogReaderConstants.PowerTaskList.CreationTagRegex.Match(logLine);
                _tagChangeHandler.TagChange(gameState, match.Groups["tag"].Value, gameState.CurrentEntityId, match.Groups["value"].Value, game);
            }
            else if ((logLine.Contains("Begin Spectating") || logLine.Contains("Start Spectator")) && game.IsInMenu)
                gameState.GameHandler.SetGameMode(GameMode.Spectator);
            else if (logLine.Contains("End Spectator"))
            {
                gameState.GameHandler.SetGameMode(GameMode.Spectator);
                gameState.GameHandler.HandleGameEnd();
            }
            else if (HsLogReaderConstants.PowerTaskList.ActionStartRegex.IsMatch(logLine))
            {
                Entity actionEntity;
                var playerEntity =
                    game.Entities.FirstOrDefault(
                        e => e.Value.HasTag(GAME_TAG.PLAYER_ID) && e.Value.GetTag(GAME_TAG.PLAYER_ID) == game.Player.Id);
                var opponentEntity =
                    game.Entities.FirstOrDefault(
                        e => e.Value.HasTag(GAME_TAG.PLAYER_ID) && e.Value.GetTag(GAME_TAG.PLAYER_ID) == game.Opponent.Id);

                var match = HsLogReaderConstants.PowerTaskList.ActionStartRegex.Match(logLine);
                var actionStartingCardId = match.Groups["cardId"].Value.Trim();
                var actionStartingEntityId = int.Parse(match.Groups["id"].Value);

                if (string.IsNullOrEmpty(actionStartingCardId))
                {
                    if (game.Entities.TryGetValue(actionStartingEntityId, out actionEntity))
                        actionStartingCardId = actionEntity.CardId;
                }
                if (game.Entities.TryGetValue(actionStartingEntityId, out actionEntity))
                {
                    // spell owned by the player
                    if (actionEntity.HasTag(GAME_TAG.CONTROLLER) && 
                        actionEntity.GetTag(GAME_TAG.CONTROLLER) == game.Player.Id &&
                        actionEntity.GetTag(GAME_TAG.CARDTYPE) == (int)TAG_CARDTYPE.ABILITY)
                    {
                        int targetEntityId = actionEntity.GetTag(GAME_TAG.CARD_TARGET);
                        Entity targetEntity;

                        bool targetsMinion = false;

                        if (game.Entities.TryGetValue(targetEntityId, out targetEntity))
                            targetsMinion = true;

                        gameState.GameHandler.HandlePlayerSpellPlayed(targetsMinion);
                    }
                }
                if (!string.IsNullOrEmpty(actionStartingCardId))
                {
                    if (actionStartingCardId == "BRM_007") //Gang Up
                    {
                        //if (playerEntity.Value != null && playerEntity.Value.GetTag(GAME_TAG.CURRENT_PLAYER) == 1)
                        //{
                            var target = match.Groups["target"].Value.Trim();
                            if (target.StartsWith("[") && HsLogReaderConstants.PowerTaskList.EntityRegex.IsMatch(target))
                            {
                                var cardIdMatch = HsLogReaderConstants.PowerTaskList.CardIdRegex.Match(target);
	                            if(cardIdMatch.Success)
	                            {
		                            var targetCardId = cardIdMatch.Groups["cardId"].Value.Trim();

		                            for(int i = 0; i < 3; i++)
									{
										var id = game.Entities.Count + i + 1;
			                            if(!gameState.KnownCardIds.ContainsKey(id))
				                            gameState.KnownCardIds.Add(id, targetCardId);
		                            }
	                            }
                            }
                        //}
                    }
                    else if (actionStartingCardId == "GVG_056") //Iron Juggernaut
                    {
	                    // burrowing mine will be the entity created next
                        int id = game.Entities.Count + 1;
	                    //if(playerEntity.Value == null || playerEntity.Value.GetTag(GAME_TAG.CURRENT_PLAYER) != 1)
	                    //{
		                    if(!gameState.KnownCardIds.ContainsKey(id))
			                    gameState.KnownCardIds.Add(id, "GVG_056t");
	                    //}
                    }
                    else if (actionStartingCardId == "GVG_031") //Recycle
                    {
	                    // Recycled card will be the entity created next
                        int id = game.Entities.Count + 1;
	                    //if(playerEntity.Value == null || playerEntity.Value.GetTag(GAME_TAG.CURRENT_PLAYER) != 1)
	                    //{
		                    gameState.ProposeKeyPoint(KeyPointType.CreateToDeck, id, ActivePlayer.Player);
		                    var target = match.Groups["target"].Value.Trim();
		                    if(target.StartsWith("[") && HsLogReaderConstants.PowerTaskList.EntityRegex.IsMatch(target))
		                    {
			                    var cardIdMatch = HsLogReaderConstants.PowerTaskList.CardIdRegex.Match(target);
			                    if(cardIdMatch.Success)
			                    {
				                    var targetCardId = cardIdMatch.Groups["cardId"].Value.Trim();
				                    if(!gameState.KnownCardIds.ContainsKey(id))
					                    gameState.KnownCardIds.Add(id, targetCardId);
			                    }
		                    }
	                    //}
                    }
                    else if (actionStartingCardId == "GVG_035") //Malorne
                    {
	                    // Malorne will be the entity created next
                        int id = game.Entities.Count + 1;
	                    //if(playerEntity.Value == null || playerEntity.Value.GetTag(GAME_TAG.CURRENT_PLAYER) != 1)
	                    //{
							if(!gameState.KnownCardIds.ContainsKey(id))
			                    gameState.KnownCardIds.Add(id, "GVG_035");
	                   // }
                    }


                    else
                    {
                        if (playerEntity.Value != null && playerEntity.Value.GetTag(GAME_TAG.CURRENT_PLAYER) == 1
                            && !gameState.PlayerUsedHeroPower
                            || opponentEntity.Value != null && opponentEntity.Value.GetTag(GAME_TAG.CURRENT_PLAYER) == 1
                            && !gameState.OpponentUsedHeroPower)
                        {
                            var card = Database.GetCardFromId(actionStartingCardId);
                            if (card.Type == "Hero Power")
                            {
                                if (playerEntity.Value != null && playerEntity.Value.GetTag(GAME_TAG.CURRENT_PLAYER) == 1)
                                {
                                    gameState.GameHandler.HandlePlayerHeroPower(actionStartingCardId, gameState.GetTurnNumber());
                                    gameState.PlayerUsedHeroPower = true;
                                }
                                else if (opponentEntity.Value != null)
                                {
                                    gameState.GameHandler.HandleOpponentHeroPower(actionStartingCardId, gameState.GetTurnNumber());
                                    gameState.OpponentUsedHeroPower = true;
                                }
                            }
                        }
                    }
                }
            }
            else if (logLine.Contains("BlockType=JOUST"))
            {
                gameState.JoustReveals = 2;
            }
        }

    }
}