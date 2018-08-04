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
using static Hearthstone_Deck_Tracker.LogReader.LogConstants.PowerTaskList;

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
			if(PlayerEntityRegex.IsMatch(logLine))
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
			if(TagChangeRegex.IsMatch(logLine))
			{
				var match = TagChangeRegex.Match(logLine);
				var rawEntity = match.Groups["entity"].Value.Replace("UNKNOWN ENTITY ", "");
				if(rawEntity.StartsWith("[") && EntityRegex.IsMatch(rawEntity))
				{
					var entity = EntityRegex.Match(rawEntity);
					var id = int.Parse(entity.Groups["id"].Value);
					_tagChangeHandler.TagChange(gameState, match.Groups["tag"].Value, id, match.Groups["value"].Value, game);
				}
				else if(int.TryParse(rawEntity, out int entityId))
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
							tmpEntity = new Entity(_tmpEntities.Count + 1) { Name = rawEntity };
							_tmpEntities.Add(tmpEntity);
						}
						Enum.TryParse(match.Groups["tag"].Value, out GameTag tag);
						var value = GameTagHelper.ParseTag(tag, match.Groups["value"].Value);
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
				var zone = GameTagHelper.ParseEnum<Zone>(match.Groups["zone"].Value);
				if(!game.Entities.ContainsKey(id))
				{
					if(string.IsNullOrEmpty(cardId) && zone != Zone.SETASIDE)
					{
						var blockId = gameState.CurrentBlock?.Id;
						if(blockId.HasValue && gameState.KnownCardIds.ContainsKey(blockId.Value))
						{
							cardId = gameState.KnownCardIds[blockId.Value].FirstOrDefault();
							if(!string.IsNullOrEmpty(cardId))
							{
								Log.Info($"Found known cardId for entity {id}: {cardId}");
								gameState.KnownCardIds[blockId.Value].Remove(cardId);
							}
						}
					}
					game.Entities.Add(id, new Entity(id) {CardId = cardId});
				}
				gameState.SetCurrentEntity(id);
				if(gameState.DeterminedPlayers)
					_tagChangeHandler.InvokeQueuedActions(game);
				gameState.CurrentEntityHasCardId = !string.IsNullOrEmpty(cardId);
				gameState.CurrentEntityZone = zone;
				return;
			}
			else if(UpdatingEntityRegex.IsMatch(logLine))
			{
				var match = UpdatingEntityRegex.Match(logLine);
				var cardId = match.Groups["cardId"].Value;
				var rawEntity = match.Groups["entity"].Value;
				var type = match.Groups["type"].Value;
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
					var entity = game.Entities[entityId];
					if(type != "CHANGE_ENTITY" || string.IsNullOrEmpty(entity.CardId))
						entity.CardId = cardId;
					if(type == "CHANGE_ENTITY")
					{
						if(!entity.Info.OriginalEntityWasCreated.HasValue)
							entity.Info.OriginalEntityWasCreated = entity.Info.Created;
						if(entity.GetTag(GameTag.TRANSFORMED_FROM_CARD) == 46706)
							gameState.ChameleosReveal = new Tuple<int, string>(entityId, cardId);
					}
					gameState.SetCurrentEntity(entityId);
					if(gameState.DeterminedPlayers)
						_tagChangeHandler.InvokeQueuedActions(game);
				}
				if(gameState.JoustReveals > 0)
				{
					if(game.Entities.TryGetValue(entityId, out Entity currentEntity))
					{
						if(currentEntity.IsControlledBy(game.Opponent.Id))
							gameState.GameHandler.HandleOpponentJoust(currentEntity, cardId, gameState.GetTurnNumber());
						else if(currentEntity.IsControlledBy(game.Player.Id))
							gameState.GameHandler.HandlePlayerJoust(currentEntity, cardId, gameState.GetTurnNumber());
					}
				}
				return;
			}
			else if(CreationTagRegex.IsMatch(logLine) && !logLine.Contains("HIDE_ENTITY"))
			{
				var match = CreationTagRegex.Match(logLine);
				_tagChangeHandler.TagChange(gameState, match.Groups["tag"].Value, gameState.CurrentEntityId, match.Groups["value"].Value, game, true);
				creationTag = true;
			}
			else if(logLine.Contains("HIDE_ENTITY"))
			{
				if(gameState.CurrentBlock?.CardId == Collectible.Neutral.KingTogwaggle
					|| gameState.CurrentBlock?.CardId == NonCollectible.Neutral.KingTogwaggle_KingsRansomToken)
				{
					var match = HideEntityRegex.Match(logLine);
					if(match.Success)
					{
						var id = int.Parse(match.Groups["id"].Value);
						if(game.Entities.TryGetValue(id, out var entity))
							entity.Info.Hidden = true;
					}
				}
			}
			if(logLine.Contains("End Spectator") && !game.IsInMenu)
				gameState.GameHandler.HandleGameEnd();
			else if(logLine.Contains("BLOCK_START"))
			{
				var match = BlockStartRegex.Match(logLine);
				var blockType = match.Success ? match.Groups["type"].Value : null;
				var cardId = match.Success ? match.Groups["Id"].Value : null;
				gameState.BlockStart(blockType, cardId);

				if(match.Success && (blockType == "TRIGGER" || blockType == "POWER"))
				{
					var playerEntity =
						game.Entities.FirstOrDefault(
							e => e.Value.HasTag(GameTag.PLAYER_ID) && e.Value.GetTag(GameTag.PLAYER_ID) == game.Player.Id);
					var opponentEntity =
						game.Entities.FirstOrDefault(
							e => e.Value.HasTag(GameTag.PLAYER_ID) && e.Value.GetTag(GameTag.PLAYER_ID) == game.Opponent.Id);

					var actionStartingCardId = match.Groups["cardId"].Value.Trim();
					var actionStartingEntityId = int.Parse(match.Groups["id"].Value);

					if(string.IsNullOrEmpty(actionStartingCardId))
					{
						if(game.Entities.TryGetValue(actionStartingEntityId, out Entity actionEntity))
							actionStartingCardId = actionEntity.CardId;
					}
					if(string.IsNullOrEmpty(actionStartingCardId))
						return;
					if(actionStartingCardId == Collectible.Shaman.Shudderwock)
					{
						var effectCardId = match.Groups["effectCardId"].Value;
						if (!string.IsNullOrEmpty(effectCardId))
							actionStartingCardId = effectCardId;
					}
					if(actionStartingCardId == NonCollectible.Rogue.ValeeratheHollow_ShadowReflectionToken)
					{
						actionStartingCardId = cardId;
					}
					if(blockType == "TRIGGER")
					{
						switch(actionStartingCardId)
						{
							case Collectible.Rogue.TradePrinceGallywix:
								if(!game.Entities.TryGetValue(gameState.LastCardPlayed, out var lastPlayed))
									break;
								AddKnownCardId(gameState, lastPlayed.CardId);
								AddKnownCardId(gameState, NonCollectible.Neutral.TradePrinceGallywix_GallywixsCoinToken);
								break;
							case Collectible.Shaman.WhiteEyes:
								AddKnownCardId(gameState, NonCollectible.Shaman.WhiteEyes_TheStormGuardianToken);
								break;
							case Collectible.Hunter.RaptorHatchling:
								AddKnownCardId(gameState, NonCollectible.Hunter.RaptorHatchling_RaptorPatriarchToken);
								break;
							case Collectible.Warrior.DirehornHatchling:
								AddKnownCardId(gameState, NonCollectible.Warrior.DirehornHatchling_DirehornMatriarchToken);
								break;
							case Collectible.Mage.FrozenClone:
								AddKnownCardId(gameState, GetTargetCardId(match), 2);
								break;
							case Collectible.Shaman.Moorabi:
							case Collectible.Rogue.SonyaShadowdancer:
								AddKnownCardId(gameState, GetTargetCardId(match));
								break;
							case Collectible.Neutral.HoardingDragon:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoin, 2);
								break;
							case Collectible.Priest.GildedGargoyle:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoin);
								break;
							case Collectible.Druid.AstralTiger:
								AddKnownCardId(gameState, Collectible.Druid.AstralTiger);
								break;
							case Collectible.Rogue.Kingsbane:
								AddKnownCardId(gameState, Collectible.Rogue.Kingsbane);
								break;
							case Collectible.Neutral.WeaselTunneler:
								AddKnownCardId(gameState, Collectible.Neutral.WeaselTunneler);
								break;
							case Collectible.Neutral.SparkDrill:
								AddKnownCardId(gameState, NonCollectible.Neutral.SparkDrill_SparkToken, 2);
								break;
							//TODO: Augmented Elekk
						}
					}
					else //POWER
					{
						switch(actionStartingCardId)
						{
							case Collectible.Rogue.GangUp:
							case Collectible.Hunter.DireFrenzy:
							case Collectible.Rogue.LabRecruiter:
								AddKnownCardId(gameState, GetTargetCardId(match), 3);
								break;
							case Collectible.Rogue.BeneathTheGrounds:
								AddKnownCardId(gameState, NonCollectible.Rogue.BeneaththeGrounds_NerubianAmbushToken, 3);
								break;
							case Collectible.Warrior.IronJuggernaut:
								AddKnownCardId(gameState, NonCollectible.Warrior.IronJuggernaut_BurrowingMineToken);
								break;
							case Collectible.Druid.Recycle:
							case Collectible.Mage.ManicSoulcaster:
							case Collectible.Neutral.ZolaTheGorgon:
							case Collectible.Druid.Splintergraft:
							//case Collectible.Priest.HolyWater: -- TODO
							case Collectible.Neutral.BalefulBanker:
							case Collectible.Neutral.DollmasterDorian:
								AddKnownCardId(gameState, GetTargetCardId(match));
								break;
							case Collectible.Mage.ForgottenTorch:
								AddKnownCardId(gameState, NonCollectible.Mage.ForgottenTorch_RoaringTorchToken);
								break;
							case Collectible.Warlock.CurseOfRafaam:
								AddKnownCardId(gameState, NonCollectible.Warlock.CurseofRafaam_CursedToken);
								break;
							case Collectible.Neutral.AncientShade:
								AddKnownCardId(gameState, NonCollectible.Neutral.AncientShade_AncientCurseToken);
								break;
							case Collectible.Priest.ExcavatedEvil:
								AddKnownCardId(gameState, Collectible.Priest.ExcavatedEvil);
								break;
							case Collectible.Neutral.EliseStarseeker:
								AddKnownCardId(gameState, NonCollectible.Neutral.EliseStarseeker_MapToTheGoldenMonkeyToken);
								break;
							case NonCollectible.Neutral.EliseStarseeker_MapToTheGoldenMonkeyToken:
								AddKnownCardId(gameState, NonCollectible.Neutral.EliseStarseeker_GoldenMonkeyToken);
								break;
							case Collectible.Neutral.Doomcaller:
								AddKnownCardId(gameState, NonCollectible.Neutral.Cthun);
								break;
							case Collectible.Druid.JadeIdol:
								AddKnownCardId(gameState, Collectible.Druid.JadeIdol, 3);
								break;
							case NonCollectible.Hunter.TheMarshQueen_QueenCarnassaToken:
								AddKnownCardId(gameState, NonCollectible.Hunter.TheMarshQueen_CarnassasBroodToken, 15);
								break;
							case Collectible.Neutral.EliseTheTrailblazer:
								AddKnownCardId(gameState, NonCollectible.Neutral.ElisetheTrailblazer_UngoroPackToken);
								break;
							case Collectible.Mage.GhastlyConjurer:
								AddKnownCardId(gameState, Collectible.Mage.MirrorImage);
								break;
							case Collectible.Mage.DeckOfWonders:
								AddKnownCardId(gameState, NonCollectible.Mage.DeckofWonders_ScrollOfWonderToken, 5);
								break;
							case Collectible.Neutral.TheDarkness:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheDarkness_DarknessCandleToken, 3);
								break;
							case Collectible.Rogue.FaldoreiStrider:
								AddKnownCardId(gameState, NonCollectible.Rogue.FaldoreiStrider_SpiderAmbushEnchantment, 3);
								break;
							case Collectible.Neutral.KingTogwaggle:
								AddKnownCardId(gameState, NonCollectible.Neutral.KingTogwaggle_KingsRansomToken);
								break;
							case NonCollectible.Neutral.TheCandle:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCandle);
								break;
							case NonCollectible.Neutral.CoinPouch:
								AddKnownCardId(gameState, NonCollectible.Neutral.SackOfCoins);
								break;
							case NonCollectible.Neutral.SackOfCoins:
								AddKnownCardId(gameState, NonCollectible.Neutral.HeftySackOfCoins);
								break;
							case NonCollectible.Neutral.CreepyCurio:
								AddKnownCardId(gameState, NonCollectible.Neutral.HauntedCurio);
								break;
							case NonCollectible.Neutral.HauntedCurio:
								AddKnownCardId(gameState, NonCollectible.Neutral.CursedCurio);
								break;
							case NonCollectible.Neutral.OldMilitiaHorn:
								AddKnownCardId(gameState, NonCollectible.Neutral.MilitiaHorn);
								break;
							case NonCollectible.Neutral.MilitiaHorn:
								AddKnownCardId(gameState, NonCollectible.Neutral.VeteransMilitiaHorn);
								break;
							case NonCollectible.Neutral.SurlyMob:
								AddKnownCardId(gameState, NonCollectible.Neutral.AngryMob);
								break;
							case NonCollectible.Neutral.AngryMob:
								AddKnownCardId(gameState, NonCollectible.Neutral.CrazedMob);
								break;
							case Collectible.Neutral.SparkEngine:
								AddKnownCardId(gameState, NonCollectible.Neutral.SparkDrill_SparkToken);
								break;
							case Collectible.Priest.ExtraArms:
								AddKnownCardId(gameState, NonCollectible.Priest.ExtraArms_MoreArmsToken);
								break;
							case Collectible.Neutral.SeaforiumBomber:
								AddKnownCardId(gameState, NonCollectible.Neutral.SeaforiumBomber_BombToken);
								break;
							//case Collectible.Rogue.Wanted: -- TODO
							//	AddKnownCardId(gameState, NonCollectible.Neutral.TheCoin);
							//	break;
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
				else if(logLine.Contains("BlockType=REVEAL_CARD"))
					gameState.JoustReveals = 1;
				else if(gameState.GameTriggerCount == 0 && logLine.Contains("BLOCK_START BlockType=TRIGGER Entity=GameEntity"))
					gameState.GameTriggerCount++;
			}
			else if(logLine.Contains("CREATE_GAME"))
				_tagChangeHandler.ClearQueuedActions();
			else if(logLine.Contains("BLOCK_END"))
			{
				if(gameState.GameTriggerCount < 10 && (game.GameEntity?.HasTag(GameTag.TURN) ?? false))
				{
					gameState.GameTriggerCount += 10;
					_tagChangeHandler.InvokeQueuedActions(game);
					game.SetupDone = true;
				}
				if(gameState.CurrentBlock?.Type == "JOUST" || gameState.CurrentBlock?.Type == "REVEAL_CARD")
				{
					//make sure there are no more queued actions that might depend on JoustReveals
					_tagChangeHandler.InvokeQueuedActions(game);
					gameState.JoustReveals = 0;
				}

				if(gameState.CurrentBlock?.Type == "TRIGGER"
					&& (gameState.CurrentBlock?.CardId == NonCollectible.Neutral.Chameleos_ShiftingEnchantment
						|| gameState.CurrentBlock?.CardId == Collectible.Priest.Chameleos)
					&& gameState.ChameleosReveal != null
					&& game.Entities.TryGetValue(gameState.ChameleosReveal.Item1, out var chameleos)
					&& chameleos.HasTag(GameTag.SHIFTING))
				{
					gameState.GameHandler.HandleChameleosReveal(gameState.ChameleosReveal.Item2);
				}

				gameState.ChameleosReveal = null;

				gameState.BlockEnd();
			}


			if(game.IsInMenu)
				return;
			if(!creationTag && gameState.DeterminedPlayers)
				_tagChangeHandler.InvokeQueuedActions(game);
			if(!creationTag)
				gameState.ResetCurrentEntity();
		}

		private static string GetTargetCardId(Match match)
		{
			var target = match.Groups["target"].Value.Trim();
			if(!target.StartsWith("[") || !EntityRegex.IsMatch(target))
				return null;
			var cardIdMatch = CardIdRegex.Match(target);
			return !cardIdMatch.Success ? null : cardIdMatch.Groups["cardId"].Value.Trim();
		}

		private static void AddKnownCardId(IHsGameState gameState, string cardId, int count = 1)
		{
			var blockId = gameState.CurrentBlock.Id;
			for(var i = 0; i < count; i++)
			{
				if(!gameState.KnownCardIds.ContainsKey(blockId))
					gameState.KnownCardIds[blockId] = new List<string>();
				gameState.KnownCardIds[blockId].Add(cardId);
			}
		}

		internal void Reset() => _tagChangeHandler.ClearQueuedActions();
	}
}
