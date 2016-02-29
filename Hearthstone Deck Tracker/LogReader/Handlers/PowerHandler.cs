#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static HearthDb.CardIds;
using static Hearthstone_Deck_Tracker.LogReader.HsLogReaderConstants.PowerTaskList;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	public class PowerHandler
	{
		private readonly TagChangeHandler _tagChangeHandler = new TagChangeHandler();
		private readonly List<Entity> _tmpEntities = new List<Entity>();

		public void Handle(string logLine, IHsGameState gameState, IGame game)
		{
			var setup = false;
			if(GameEntityRegex.IsMatch(logLine))
			{
				var match = GameEntityRegex.Match(logLine);
				var id = int.Parse(match.Groups["id"].Value);
				if(!game.Entities.ContainsKey(id))
					game.Entities.Add(id, new Entity(id) {Name = "GameEntity"});
				gameState.CurrentEntityId = id;
				setup = true;
			}
			else if(PlayerEntityRegex.IsMatch(logLine))
			{
				var match = PlayerEntityRegex.Match(logLine);
				var id = int.Parse(match.Groups["id"].Value);
				if(!game.Entities.ContainsKey(id))
					game.Entities.Add(id, new Entity(id));
				gameState.CurrentEntityId = id;
				setup = true;
			}
			else if(TagChangeRegex.IsMatch(logLine))
			{
				var match = TagChangeRegex.Match(logLine);
				var rawEntity = match.Groups["entity"].Value.Replace("UNKNOWN ENTITY ", "");
				int entityId;
				if(rawEntity.StartsWith("[") && EntityRegex.IsMatch(rawEntity))
				{
					var entity = EntityRegex.Match(rawEntity);
					var id = int.Parse(entity.Groups["id"].Value);
					_tagChangeHandler.TagChange(gameState, match.Groups["tag"].Value, id, match.Groups["value"].Value, game);
				}
				else if(int.TryParse(rawEntity, out entityId))
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

					if(entity.Value == null)
					{
						//while the id is unknown, store in tmp entities
						var tmpEntity = _tmpEntities.FirstOrDefault(x => x.Name == rawEntity);
						if(tmpEntity == null)
						{
							tmpEntity = new Entity(_tmpEntities.Count + 1) {Name = rawEntity};
							_tmpEntities.Add(tmpEntity);
						}
						GAME_TAG tag;
						Enum.TryParse(match.Groups["tag"].Value, out tag);
						var value = LogReaderHelper.ParseTagValue(tag, match.Groups["value"].Value);
						var unnamedPlayers = game.Entities.Where(x => x.Value.HasTag(GAME_TAG.PLAYER_ID) && string.IsNullOrEmpty(x.Value.Name)).ToList();
						if(unnamedPlayers.Count == 1)
							entity = unnamedPlayers.Single();
						else if(unnamedPlayers.Count == 2 && tag == GAME_TAG.CURRENT_PLAYER && value == 0)
							entity = game.Entities.FirstOrDefault(x => x.Value?.HasTag(GAME_TAG.CURRENT_PLAYER) ?? false);
						if(entity.Value != null)
						{
							entity.Value.Name = tmpEntity.Name;
							foreach(var t in tmpEntity.Tags)
								entity.Value.SetTag(t.Key, t.Value);
							SetPlayerName(game, entity.Value.GetTag(GAME_TAG.PLAYER_ID), tmpEntity.Name);
							_tmpEntities.Remove(tmpEntity);
							_tagChangeHandler.TagChange(gameState, match.Groups["tag"].Value, entity.Key, match.Groups["value"].Value, game);
						}
						if(_tmpEntities.Contains(tmpEntity))
						{
							tmpEntity.SetTag(tag, value);
							if(tmpEntity.HasTag(GAME_TAG.ENTITY_ID))
							{
								var id = tmpEntity.GetTag(GAME_TAG.ENTITY_ID);
								if(game.Entities.ContainsKey(id))
								{
									game.Entities[id].Name = tmpEntity.Name;
									foreach(var t in tmpEntity.Tags)
										game.Entities[id].SetTag(t.Key, t.Value);
									_tmpEntities.Remove(tmpEntity);
								}
								else
									Log.Warn("TMP ENTITY (" + rawEntity + ") NOW HAS A KEY, BUT GAME.ENTITIES DOES NOT CONTAIN THIS KEY");
							}
						}
					}
					else
						_tagChangeHandler.TagChange(gameState, match.Groups["tag"].Value, entity.Key, match.Groups["value"].Value, game);
				}

				if(EntityNameRegex.IsMatch(logLine))
				{
					match = EntityNameRegex.Match(logLine);
					var name = match.Groups["name"].Value;
					var player = int.Parse(match.Groups["value"].Value);
					SetPlayerName(game, player, name);
				}
			}
			else if(CreationRegex.IsMatch(logLine))
			{
				var match = CreationRegex.Match(logLine);
				var id = int.Parse(match.Groups["id"].Value);
				var cardId = match.Groups["cardId"].Value;
				if(!game.Entities.ContainsKey(id))
				{
					if(string.IsNullOrEmpty(cardId))
					{
						if(gameState.KnownCardIds.TryGetValue(id, out cardId))
						{
							Log.Info($"Found known cardId for entity {id}: {cardId}");
							gameState.KnownCardIds.Remove(id);
						}
					}
					game.Entities.Add(id, new Entity(id) {CardId = cardId});
				}
				gameState.CurrentEntityId = id;
				gameState.CurrentEntityHasCardId = !string.IsNullOrEmpty(cardId);
				setup = true;
			}
			else if(UpdatingEntityRegex.IsMatch(logLine))
			{
				var match = UpdatingEntityRegex.Match(logLine);
				var cardId = match.Groups["cardId"].Value;
				var rawEntity = match.Groups["entity"].Value;
				int entityId;
				if(rawEntity.StartsWith("[") && EntityRegex.IsMatch(rawEntity))
				{
					var entity = EntityRegex.Match(rawEntity);
					entityId = int.Parse(entity.Groups["id"].Value);
				}
				else if(!int.TryParse(rawEntity, out entityId))
					entityId = -1;
				if(entityId != -1)
				{
					gameState.CurrentEntityId = entityId;
					if(!game.Entities.ContainsKey(entityId))
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
			else if(CreationTagRegex.IsMatch(logLine) && !logLine.Contains("HIDE_ENTITY"))
			{
				var match = CreationTagRegex.Match(logLine);
				_tagChangeHandler.TagChange(gameState, match.Groups["tag"].Value, gameState.CurrentEntityId, match.Groups["value"].Value, game);
				setup = true;
			}
			else if((logLine.Contains("Begin Spectating") || logLine.Contains("Start Spectator")) && game.IsInMenu)
				gameState.GameHandler.SetGameMode(GameMode.Spectator);
			else if(logLine.Contains("End Spectator"))
			{
				gameState.GameHandler.SetGameMode(GameMode.Spectator);
				gameState.GameHandler.HandleGameEnd();
			}
			else if(ActionStartRegex.IsMatch(logLine))
			{
				var playerEntity =
					game.Entities.FirstOrDefault(e => e.Value.HasTag(GAME_TAG.PLAYER_ID) && e.Value.GetTag(GAME_TAG.PLAYER_ID) == game.Player.Id);
				var opponentEntity =
					game.Entities.FirstOrDefault(e => e.Value.HasTag(GAME_TAG.PLAYER_ID) && e.Value.GetTag(GAME_TAG.PLAYER_ID) == game.Opponent.Id);

				var match = ActionStartRegex.Match(logLine);
				var actionStartingCardId = match.Groups["cardId"].Value.Trim();
				var actionStartingEntityId = int.Parse(match.Groups["id"].Value);

				if(string.IsNullOrEmpty(actionStartingCardId))
				{
					Entity actionEntity;
					if(game.Entities.TryGetValue(actionStartingEntityId, out actionEntity))
						actionStartingCardId = actionEntity.CardId;
				}
				if(string.IsNullOrEmpty(actionStartingCardId))
					return;
				if(match.Groups["type"].Value == "TRIGGER")
				{
					switch(actionStartingCardId)
					{
						case Collectible.Rogue.TradePrinceGallywix:
							AddKnownCardId(gameState, game, game.Entities[gameState.LastCardPlayed].CardId);
							AddKnownCardId(gameState, game, NonCollectible.Neutral.GallywixsCoinToken);
							break;
					}
				}
				else //POWER
				{
					switch(actionStartingCardId)
					{
						case Collectible.Rogue.GangUp:
							AddTargetAsKnownCardId(gameState, game, match, 3);
							break;
						case Collectible.Rogue.BeneathTheGrounds:
							AddKnownCardId(gameState, game, NonCollectible.Rogue.AmbushToken, 3);
							break;
						case Collectible.Warrior.IronJuggernaut:
							AddKnownCardId(gameState, game, NonCollectible.Warrior.BurrowingMineToken);
							break;
						case Collectible.Druid.Recycle:
							AddTargetAsKnownCardId(gameState, game, match);
							break;
						case Collectible.Mage.ForgottenTorch:
							AddKnownCardId(gameState, game, NonCollectible.Mage.RoaringTorchToken);
							break;
						case Collectible.Warlock.CurseOfRafaam:
							AddKnownCardId(gameState, game, NonCollectible.Warlock.CursedToken);
							break;
						case Collectible.Neutral.AncientShade:
							AddKnownCardId(gameState, game, NonCollectible.Neutral.AncientCurseToken);
							break;
						case Collectible.Priest.ExcavatedEvil:
							AddKnownCardId(gameState, game, Collectible.Priest.ExcavatedEvil);
							break;
						case Collectible.Neutral.EliseStarseeker:
							AddKnownCardId(gameState, game, NonCollectible.Neutral.MapToTheGoldenMonkeyToken);
							break;
						case NonCollectible.Neutral.MapToTheGoldenMonkeyToken:
							AddKnownCardId(gameState, game, NonCollectible.Neutral.GoldenMonkeyToken);
							break;
						default:
							if(playerEntity.Value != null && playerEntity.Value.GetTag(GAME_TAG.CURRENT_PLAYER) == 1 && !gameState.PlayerUsedHeroPower
							   || opponentEntity.Value != null && opponentEntity.Value.GetTag(GAME_TAG.CURRENT_PLAYER) == 1
							   && !gameState.OpponentUsedHeroPower)
							{
								var card = Database.GetCardFromId(actionStartingCardId);
								if(card.Type == "Hero Power")
								{
									if(playerEntity.Value != null && playerEntity.Value.GetTag(GAME_TAG.CURRENT_PLAYER) == 1)
									{
										gameState.GameHandler.HandlePlayerHeroPower(actionStartingCardId, gameState.GetTurnNumber());
										gameState.PlayerUsedHeroPower = true;
									}
									else if(opponentEntity.Value != null)
									{
										gameState.GameHandler.HandleOpponentHeroPower(actionStartingCardId, gameState.GetTurnNumber());
										gameState.OpponentUsedHeroPower = true;
									}
								}
							}
							break;
					}
				}
			}
			else if(logLine.Contains("BlockType=JOUST"))
				gameState.JoustReveals = 2;
			else if(logLine.Contains("CREATE_GAME"))
				setup = true;

			if(!setup)
				gameState.SetupDone = true;
		}

		private static void SetPlayerName(IGame game, int playerId, string name)
		{
			if(playerId == game.Player.Id)
				game.Player.Name = name;
			else if(playerId == game.Opponent.Id)
				game.Opponent.Name = name;
		}

		private static void AddTargetAsKnownCardId(IHsGameState gameState, IGame game, Match match, int count = 1)
		{
			var target = match.Groups["target"].Value.Trim();
			if(!target.StartsWith("[") || !EntityRegex.IsMatch(target))
				return;
			var cardIdMatch = CardIdRegex.Match(target);
			if(!cardIdMatch.Success)
				return;
			var targetCardId = cardIdMatch.Groups["cardId"].Value.Trim();
			for(var i = 0; i < count; i++)
			{
				var id = GetMaxEntityId(gameState, game) + i + 1;
				if(!gameState.KnownCardIds.ContainsKey(id))
					gameState.KnownCardIds.Add(id, targetCardId);
			}
		}

		private static void AddKnownCardId(IHsGameState gameState, IGame game, string cardId, int count = 1)
		{
			for(var i = 0; i < count; i++)
			{
				var id = GetMaxEntityId(gameState, game) + 1 + i;
				if(!gameState.KnownCardIds.ContainsKey(id))
					gameState.KnownCardIds.Add(id, cardId);
			}
		}

		private static int GetMaxEntityId(IHsGameState gameState, IGame game) => Math.Max(game.Entities.Count, gameState.MaxId);
	}
}