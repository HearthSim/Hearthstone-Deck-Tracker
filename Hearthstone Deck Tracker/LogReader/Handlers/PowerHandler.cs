#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
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
			var creationTag = false;
			if(GameEntityRegex.IsMatch(logLine))
			{
				var match = GameEntityRegex.Match(logLine);
				var id = int.Parse(match.Groups["id"].Value);
				if(!game.Entities.ContainsKey(id))
					game.Entities.Add(id, new Entity(id) {Name = "GameEntity"});
				gameState.SetCurrentEntity(id);
				if(gameState.DeterminedPlayers)
					_tagChangeHandler.InvokeQueuedActions(game);
				return;
			}
			else if(PlayerEntityRegex.IsMatch(logLine))
			{
				var match = PlayerEntityRegex.Match(logLine);
				var id = int.Parse(match.Groups["id"].Value);
				if(!game.Entities.ContainsKey(id))
					game.Entities.Add(id, new Entity(id));
				if(gameState.WasInProgress)
					game.Entities[id].Name = game.GetStoredPlayerName(id);
				gameState.SetCurrentEntity(id);
				if(gameState.DeterminedPlayers)
					_tagChangeHandler.InvokeQueuedActions(game);
				return;
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
						var players = game.Entities.Where(x => x.Value.HasTag(GameTag.PLAYER_ID)).Take(2).ToList();
						var unnamedPlayers = players.Where(x => string.IsNullOrEmpty(x.Value.Name)).ToList();
						var unknownHumanPlayer = players.FirstOrDefault(x => x.Value.Name == "UNKNOWN HUMAN PLAYER");
						if(unnamedPlayers.Count == 0 && unknownHumanPlayer.Value != null)
						{
							Log.Info("Updating UNKNOWN HUMAN PLAYER");
							entity = unknownHumanPlayer;
						}

						//while the id is unknown, store in tmp entities
						var tmpEntity = _tmpEntities.FirstOrDefault(x => x.Name == rawEntity);
						if(tmpEntity == null)
						{
							tmpEntity = new Entity(_tmpEntities.Count + 1) {Name = rawEntity};
							_tmpEntities.Add(tmpEntity);
						}
						GameTag tag;
						Enum.TryParse(match.Groups["tag"].Value, out tag);
						var value = LogReaderHelper.ParseTag(tag, match.Groups["value"].Value);
						if(unnamedPlayers.Count == 1)
							entity = unnamedPlayers.Single();
						else if(unnamedPlayers.Count == 2 && tag == GameTag.CURRENT_PLAYER && value == 0)
							entity = game.Entities.FirstOrDefault(x => x.Value?.HasTag(GameTag.CURRENT_PLAYER) ?? false);
						if(entity.Value != null)
						{
							entity.Value.Name = tmpEntity.Name;
							foreach(var t in tmpEntity.Tags)
								_tagChangeHandler.TagChange(gameState, t.Key, entity.Key, t.Value, game);
							_tmpEntities.Remove(tmpEntity);
							_tagChangeHandler.TagChange(gameState, match.Groups["tag"].Value, entity.Key, match.Groups["value"].Value, game);
						}
						if(_tmpEntities.Contains(tmpEntity))
						{
							tmpEntity.SetTag(tag, value);
							var player = game.Player.Name == tmpEntity.Name ? game.Player 
										: (game.Opponent.Name == tmpEntity.Name ? game.Opponent : null);
							if(player != null)
							{
								var playerEntity = game.Entities.FirstOrDefault(x => x.Value.GetTag(GameTag.PLAYER_ID) == player.Id).Value;
								if(playerEntity != null)
								{
									playerEntity.Name = tmpEntity.Name;
									foreach(var t in tmpEntity.Tags)
										_tagChangeHandler.TagChange(gameState, t.Key, playerEntity.Id, t.Value, game);
									_tmpEntities.Remove(tmpEntity);
								}
							}
						}
					}
					else
						_tagChangeHandler.TagChange(gameState, match.Groups["tag"].Value, entity.Key, match.Groups["value"].Value, game);
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
				gameState.SetCurrentEntity(id);
				if(gameState.DeterminedPlayers)
					_tagChangeHandler.InvokeQueuedActions(game);
				gameState.CurrentEntityHasCardId = !string.IsNullOrEmpty(cardId);
				gameState.CurrentEntityZone = LogReaderHelper.ParseEnum<Zone>(match.Groups["zone"].Value);
				return;
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
					if(!game.Entities.ContainsKey(entityId))
						game.Entities.Add(entityId, new Entity(entityId));
					game.Entities[entityId].CardId = cardId;
					gameState.SetCurrentEntity(entityId);
					if(gameState.DeterminedPlayers)
						_tagChangeHandler.InvokeQueuedActions(game);
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
				return;
			}
			else if(CreationTagRegex.IsMatch(logLine) && !logLine.Contains("HIDE_ENTITY"))
			{
				var match = CreationTagRegex.Match(logLine);
				_tagChangeHandler.TagChange(gameState, match.Groups["tag"].Value, gameState.CurrentEntityId, match.Groups["value"].Value, game, true);
				creationTag = true;
			}
			if(logLine.Contains("End Spectator"))
				gameState.GameHandler.HandleGameEnd();
			else if(BlockStartRegex.IsMatch(logLine))
			{
				var playerEntity =
					game.Entities.FirstOrDefault(
						e => e.Value.HasTag(GameTag.PLAYER_ID) && e.Value.GetTag(GameTag.PLAYER_ID) == game.Player.Id);
				var opponentEntity =
					game.Entities.FirstOrDefault(
						e => e.Value.HasTag(GameTag.PLAYER_ID) && e.Value.GetTag(GameTag.PLAYER_ID) == game.Opponent.Id);

				var match = BlockStartRegex.Match(logLine);
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
							AddKnownCardId(gameState, game, NonCollectible.Neutral.TradePrinceGallywix_GallywixsCoinToken);
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
							AddKnownCardId(gameState, game, NonCollectible.Rogue.BeneaththeGrounds_AmbushToken, 3);
							break;
						case Collectible.Warrior.IronJuggernaut:
							AddKnownCardId(gameState, game, NonCollectible.Warrior.IronJuggernaut_BurrowingMineToken);
							break;
						case Collectible.Druid.Recycle:
							AddTargetAsKnownCardId(gameState, game, match);
							break;
						case Collectible.Mage.ForgottenTorch:
							AddKnownCardId(gameState, game, NonCollectible.Mage.ForgottenTorch_RoaringTorchToken);
							break;
						case Collectible.Warlock.CurseOfRafaam:
							AddKnownCardId(gameState, game, NonCollectible.Warlock.CurseofRafaam_CursedToken);
							break;
						case Collectible.Neutral.AncientShade:
							AddKnownCardId(gameState, game, NonCollectible.Neutral.AncientShade_AncientCurseToken);
							break;
						case Collectible.Priest.ExcavatedEvil:
							AddKnownCardId(gameState, game, Collectible.Priest.ExcavatedEvil);
							break;
						case Collectible.Neutral.EliseStarseeker:
							AddKnownCardId(gameState, game, NonCollectible.Neutral.EliseStarseeker_MapToTheGoldenMonkeyToken);
							break;
						case NonCollectible.Neutral.EliseStarseeker_MapToTheGoldenMonkeyToken:
							AddKnownCardId(gameState, game, NonCollectible.Neutral.EliseStarseeker_GoldenMonkeyToken);
							break;
						case Collectible.Neutral.Doomcaller:
							AddKnownCardId(gameState, game, NonCollectible.Neutral.Cthun);
							break;
						default:
							if(playerEntity.Value != null && playerEntity.Value.GetTag(GameTag.CURRENT_PLAYER) == 1
								&& !gameState.PlayerUsedHeroPower
								|| opponentEntity.Value != null && opponentEntity.Value.GetTag(GameTag.CURRENT_PLAYER) == 1
								&& !gameState.OpponentUsedHeroPower)
							{
								var card = Database.GetCardFromId(actionStartingCardId);
								if(card.Type == "Hero Power")
								{
									if(playerEntity.Value != null && playerEntity.Value.GetTag(GameTag.CURRENT_PLAYER) == 1)
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
				_tagChangeHandler.ClearQueuedActions();
			else if(gameState.GameTriggerCount == 0 && logLine.Contains("BLOCK_START BlockType=TRIGGER Entity=GameEntity"))
				gameState.GameTriggerCount++;
			else if(gameState.GameTriggerCount == 1 && logLine.Contains("BLOCK_END"))
			{
				gameState.GameTriggerCount++;
				_tagChangeHandler.InvokeQueuedActions(game);
				gameState.SetupDone = true;
			}


			if(game.IsInMenu)
				return;
			if(!creationTag && gameState.DeterminedPlayers)
				_tagChangeHandler.InvokeQueuedActions(game);
			if(!creationTag)
				gameState.ResetCurrentEntity();
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

		internal void Reset()
		{
			_tagChangeHandler.ClearQueuedActions();
		}
	}
}