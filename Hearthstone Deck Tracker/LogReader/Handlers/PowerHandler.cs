#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.BobsBuddy;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility.Logging;
using NuGet;
using static HearthDb.CardIds;
using static Hearthstone_Deck_Tracker.LogReader.LogConstants.PowerTaskList;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	public class PowerHandler
	{
		private readonly TagChangeHandler _tagChangeHandler = new TagChangeHandler();
		private readonly List<Entity> _tmpEntities = new List<Entity>();
		const string TransferStudentToken = Collectible.Neutral.TransferStudent + "t";

		public void Handle(string logLine, DateTime logLineTime, IHsGameState gameState, IGame game)
		{
			var isInsideMetaDataHistoryTarget = false;
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
						else if(tag == GameTag.HERO_ENTITY)
						{
							var bob = players.FirstOrDefault(x => x.Value.HasTag(GameTag.BACON_DUMMY_PLAYER));
							if(bob.Value != null)
							{
								entity = bob;
							}
						}
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
				var cardId = EnsureValidCardID(match.Groups["cardId"].Value);
				var zone = GameTagHelper.ParseEnum<Zone>(match.Groups["zone"].Value);
				var guessedCardId = false;
				var guessedLocation = DeckLocation.Unknown;
				string? copyOfCardId = null;
				EntityInfo? entityInfo = null;
				if(!game.Entities.ContainsKey(id))
				{
					if(string.IsNullOrEmpty(cardId) && zone != Zone.SETASIDE)
					{
						var blockId = gameState.CurrentBlock?.Id;
						if(blockId.HasValue && gameState.KnownCardIds.ContainsKey(blockId.Value))
						{
							var known = gameState.KnownCardIds[blockId.Value].FirstOrDefault();
							cardId = known.Item1;
							copyOfCardId = known.Item3;
							entityInfo = known.Item4;
							if(!string.IsNullOrEmpty(cardId))
							{
								guessedLocation = known.Item2;
								Log.Info($"Found data for entity={id}: CardId={cardId}, Location={guessedLocation}");
								guessedCardId = true;
							}
							gameState.KnownCardIds[blockId.Value].Remove(known);
						}
						else if(gameState.CurrentBlock is {
							        CardId: NonCollectible.Neutral.MarintheManager_TolinsGobletToken or
							        NonCollectible.Neutral.TolinsGobletHeroic
						        })
						{
							cardId = "";
							var lastCardDrawnId = game.Opponent.Hand.OrderByDescending(e => e.ZonePosition).FirstOrDefault()?.Id;
							var lastCardDrawnEntity = game.Entities.TryGetValue(lastCardDrawnId ?? -1, out var e) ? e : null;
							copyOfCardId = lastCardDrawnEntity?.Info.CopyOfCardId ?? lastCardDrawnId.ToString();
						}
					}
					var entity = new Entity(id) { CardId = cardId};
					if(entityInfo != null)
					{
						entity.Info.Forged = entityInfo.Forged;
						entity.Info.CostReduction = entityInfo.CostReduction;
						entity.Info.ExtraInfo = entityInfo.ExtraInfo;
						entity.Info.StoredCardIds = entityInfo.StoredCardIds;
					}

					if(guessedCardId)
						entity.Info.GuessedCardState = GuessedCardState.Guessed;
					if(guessedLocation != DeckLocation.Unknown)
					{
						var newIndex = ++gameState.DredgeCounter;
						var sign = guessedLocation == DeckLocation.Top ? 1 : -1;
						entity.Info.DeckIndex = sign * newIndex;
					}
					entity.Info.CopyOfCardId = copyOfCardId;

					game.Entities.Add(id, entity);

					if(gameState.CurrentBlock != null && zone == Zone.DECK)
					{
						gameState.CurrentBlock.EntitiesCreatedInDeck.Add((entity, new HashSet<int>()));
					}

					if(gameState.CurrentBlock != null && (entity.CardId?.ToUpper().Contains("HERO") ?? false))
						gameState.CurrentBlock.HasFullEntityHeroPackets = true;
				}
				gameState.SetCurrentEntity(id);
				if(gameState.DeterminedPlayers)
					_tagChangeHandler.InvokeQueuedActions(game);
				gameState.CurrentEntityHasCardId = !string.IsNullOrEmpty(cardId);
				gameState.CurrentEntityZone = zone;

				// For tourists, a different entity of the Tourist card is created by the TouristEnchantment, and that entity is REMOVEDFROMGAME.
				// we can predict, then, that there is a real entity of that cardId on the opponents deck.
				if(zone == Zone.REMOVEDFROMGAME && gameState.CurrentBlock != null)
				{
					if(game.Entities.TryGetValue(gameState.CurrentBlock.SourceEntityId, out Entity actionStartingEntity))
					{
						if(
							actionStartingEntity.CardId == NonCollectible.Neutral.TouristVfxEnchantmentEnchantment
							&& actionStartingEntity.IsControlledBy(game.Opponent.Id)
							&& game.Opponent.RevealedCards.All(c => c.Id != cardId)
						)
						{
							game.Opponent.PredictUniqueCardInDeck(cardId, false);
							Core.UpdateOpponentCards();
							if(cardId == Collectible.Warlock.SummonerDarkmarrow)
							{
								game.Opponent.HasDeathKnightTourist = true;
								Core.UpdateOpponentResourcesWidget();
							}
						}
					}

				}

				return;
			}
			else if(UpdatingEntityRegex.IsMatch(logLine))
			{
				var match = UpdatingEntityRegex.Match(logLine);
				var cardId = EnsureValidCardID(match.Groups["cardId"].Value);
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
					var oldCardId = entity.CardId;
					if(
						string.IsNullOrEmpty(entity.CardId) ||
						// placeholders and Fantastic Treasure (Marin's hero power)
						entity.HasTag(GameTag.BACON_IS_MAGIC_ITEM_DISCOVER) ||
						// Souvenir Stand
						entity.HasTag(GameTag.BACON_TRINKET) ||
						// Heroes during Battlegrounds reroll
						entity.HasTag(GameTag.BACON_HERO_CAN_BE_DRAFTED) ||
						entity.HasTag(GameTag.BACON_SKIN) ||
						(entity.CardId?.StartsWith("CREATED_BY_") ?? false)
					)
						entity.CardId = cardId;
					entity.Info.LatestCardId = cardId;
					if(type == "SHOW_ENTITY")
					{
						if(entity.Info.GuessedCardState != GuessedCardState.None)
							entity.Info.GuessedCardState = GuessedCardState.Revealed;
						if((gameState.CurrentBlock is { HideShowEntities: true }
						    && !entity.Info.RevealedOnHistory
						    && !entity.HasTag(GameTag.DISPLAYED_CREATOR)) || entity.CardId == NonCollectible.Rogue.GaronaHalforcen_KingLlaneToken)
						{
							entity.Info.Hidden = true;
						}
						else
						{
							entity.Info.Hidden = false;
						}

						if(entity.Info.DeckIndex < 0 && gameState.CurrentBlock != null && gameState.CurrentBlock.SourceEntityId != 0)
						{
							if(game.Entities.TryGetValue(gameState.CurrentBlock.SourceEntityId, out var source) && source.HasDredge())
							{
								var newIndex = ++gameState.DredgeCounter;
								entity.Info.DeckIndex = newIndex;
								Log.Info($"Dredge Top: {entity}");
								gameState.GameHandler?.HandlePlayerDredge();
							}
						}

						if(entity.CardId == NonCollectible.Neutral.PhotographerFizzle_FizzlesSnapshotToken
						   && gameState.CurrentBlock is { CardId: Collectible.Neutral.PhotographerFizzle })
						{
							if(entity.IsControlledBy(game.Player.Id))
								entity.Info.StoredCardIds.AddRange(game.Player.Hand.OrderBy(e => e.ZonePosition).Select(e => e.Card.Id));
							else if(entity.IsControlledBy(game.Opponent.Id))
							{
								entity.Info.StoredCardIds.AddRange(game.Opponent.Hand.OrderBy(e => e.ZonePosition)
									.Select(e => {
										if(e.HasCardId && !e.Info.Hidden)
										{
											return e.Card.Id;
										}
										return e.Id.ToString();
									}));
								entity.Info.GuessedCardState = GuessedCardState.Guessed;
							}
						}

						if(gameState.CurrentBlock is { CardId: Collectible.Shaman.Triangulate } && entity is { CardId: not null })
						{
							if(entity.IsControlledBy(game.Player.Id))
							{
								AddKnownCardId(gameState, entity.CardId, count: 3, info: entity.Info);
							}
							else if(entity.IsControlledBy(game.Opponent.Id))
							{
								gameState.TriangulatePlayed = true;
							}
						}

						if(gameState.CurrentBlock is { CardId: Collectible.Priest.Repackage } &&
						   entity.CardId == NonCollectible.Priest.Repackage_RepackagedBoxToken)
						{
							entity.Info.StoredCardIds.AddRange(gameState.MinionsInPlay);
							if(entity.IsControlledBy(game.Opponent.Id))
							{
								entity.Info.GuessedCardState = GuessedCardState.Guessed;
							}
						}

						if(entity.IsControlledBy(game.Opponent.Id) &&
						   entity is { CardId: Collectible.Warlock.SummonerDarkmarrow, Info.Created: false }
						   )
						{
							game.Opponent.HasDeathKnightTourist = true;
							Core.UpdateOpponentResourcesWidget();
						}

					}

					var fizzleSnapshots = game.Opponent.PlayerEntities
						.Where(e => e.CardId == NonCollectible.Neutral.PhotographerFizzle_FizzlesSnapshotToken);

					foreach(var fizzle in fizzleSnapshots)
					{
						if(fizzle.Info.StoredCardIds.Contains(entity.Id.ToString()))
						{
							var index = fizzle.Info.StoredCardIds.FindIndex(e => e == entity.Id.ToString());
							if(index != -1)
							{
								fizzle.Info.StoredCardIds[index] = entity.Card.Id;
							}
						}
					}

					HandleCopiedCard(game, entity);

					if(type == "CHANGE_ENTITY")
					{
						if(!entity.Info.OriginalEntityWasCreated.HasValue)
							entity.Info.OriginalEntityWasCreated = entity.Info.Created;
						if(entity.GetTag(GameTag.TRANSFORMED_FROM_CARD) == 46706)
							gameState.ChameleosReveal = new Tuple<int, string>(entityId, cardId);
						// Battlegrounds hero reroll
						if(entity.IsHero && entity.IsControlledBy(Core.Game.Player.Id) && (game.GameEntity?.GetTag(GameTag.STEP) ?? (int)Step.INVALID) <= (int)Step.BEGIN_MULLIGAN)
							gameState.GameHandler?.HandleBattlegroundsHeroReroll(entity, oldCardId);
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
							gameState.GameHandler?.HandleOpponentJoust(currentEntity, cardId, gameState.GetTurnNumber());
						else if(currentEntity.IsControlledBy(game.Player.Id))
							gameState.GameHandler?.HandlePlayerJoust(currentEntity, cardId, gameState.GetTurnNumber());
					}
				}
				return;
			}
			else if(CreationTagRegex.IsMatch(logLine) && !logLine.Contains("HIDE_ENTITY"))
			{
				var match = CreationTagRegex.Match(logLine);
				_tagChangeHandler.TagChange(gameState, match.Groups["tag"].Value, gameState.CurrentEntityId, match.Groups["value"].Value, game, true);
				creationTag = true;
				if(gameState.TriangulatePlayed)
				{
					var tag = GameTagHelper.ParseEnum<GameTag>(match.Groups["tag"].Value);
					var value = GameTagHelper.ParseTag(tag, match.Groups["value"].Value);
					if(tag == GameTag.LINKED_ENTITY)
					{
						AddKnownCardId(gameState, "", count: 3, copyOfCardId: value.ToString());
						gameState.TriangulatePlayed = false;
					}
					else if(tag == GameTag.CASTS_WHEN_DRAWN && value == 1 &&
							game.Entities.TryGetValue(gameState.CurrentEntityId, out var gameEntity))
					{
						var cardId = gameEntity.CardId;
						if(cardId != null)
						{
							RemoveKnownCardId(gameState, 3);
							AddKnownCardId(gameState, cardId, count: 3, info: gameEntity.Info);
						}
						gameState.TriangulatePlayed = false;
					}
				}
			}
			else if(logLine.Contains("HIDE_ENTITY"))
			{
				var match = HideEntityRegex.Match(logLine);
				if(match.Success)
				{
					var id = int.Parse(match.Groups["id"].Value);
					if(game.Entities.TryGetValue(id, out var entity))
					{
						if(entity.Info.GuessedCardState == GuessedCardState.Revealed)
							entity.Info.GuessedCardState = GuessedCardState.Guessed;
						if(gameState.CurrentBlock?.CardId == Collectible.Neutral.KingTogwaggle
							|| gameState.CurrentBlock?.CardId == NonCollectible.Neutral.KingTogwaggle_KingsRansomToken)
						{
							entity.Info.Hidden = true;
						}

						// Plagues are flagged here due to the following info leak:
						// 1. Plagues are created in the opponent's deck
						// 2. SHOW_ENTITY followed by HIDE_ENTITY
						// 3. Later on the card may enter hand in a way where it doesn't trigger (e.g. due to Sir Finley)
						// 4. When the hand updates, we exclude the card because the entity is now in the hand (this is the info leak).
						// By setting a GuessedCardState here we prevent the card from appearing as drawn.
						if(
							entity.CardId == NonCollectible.Deathknight.DistressedKvaldir_FrostPlagueToken
							|| entity.CardId == NonCollectible.Deathknight.DistressedKvaldir_BloodPlagueToken
							|| entity.CardId == NonCollectible.Deathknight.DistressedKvaldir_UnholyPlagueToken
						)
						{
							entity.Info.GuessedCardState = GuessedCardState.Guessed;
						}

						var blockId = gameState.CurrentBlock?.Id;
						if(blockId.HasValue && gameState.KnownCardIds.ContainsKey(blockId.Value))
						{
							var known = gameState.KnownCardIds[blockId.Value].FirstOrDefault();
							if(entity.CardId == known.Item1 && known.Item2 != DeckLocation.Unknown)
							{
								Log.Info($"Setting DeckLocation={known.Item1} for {entity}");
								var newIndex = ++gameState.DredgeCounter;
								var sign = known.Item2 == DeckLocation.Top ? 1 : -1;
								entity.Info.DeckIndex = sign * newIndex;
							}
						}
					}
				}
			}
			else if(ShuffleRegex.IsMatch(logLine))
			{
				var match = ShuffleRegex.Match(logLine);
				var playerId = int.Parse(match.Groups["id"].Value);
				if(playerId == game.Player.Id)
				{
					game.Player.ShuffleDeck();
					gameState.GameHandler?.HandlePlayerDredge();
				}
			}
			else if(logLine.Contains("META_DATA - Meta=OVERRIDE_HISTORY"))
			{
				if(gameState.CurrentBlock != null)
					gameState.CurrentBlock.HideShowEntities = true;
			}
			else if(logLine.Contains("META_DATA - Meta=HISTORY_TARGET"))
			{
				gameState.IsInsideMetaDataHistoryTarget = true;
				isInsideMetaDataHistoryTarget = true;
			}
			else if(MetaInfoRegex.IsMatch(logLine))
			{
				if(gameState.IsInsideMetaDataHistoryTarget)
				{
					var match = MetaInfoRegex.Match(logLine);
					try
					{
						var entityId = int.Parse(match.Groups["id"].Value);
						if(game.Entities.TryGetValue(entityId, out var entity))
						{
							entity.Info.RevealedOnHistory = true;
							entity.Info.Hidden = entity.CardId == NonCollectible.Rogue.GaronaHalforcen_KingLlaneToken;
						}
					}
					catch(FormatException e)
					{
						Log.Info(e.Message);
					}

					isInsideMetaDataHistoryTarget = true;
				}

			}
			else if(SubSpellStartRegex.IsMatch(logLine))
			{
				var match = SubSpellStartRegex.Match(logLine);
				try
				{
					var sourceId = int.Parse(match.Groups["source"].Value);
					if(game.Entities.TryGetValue(sourceId, out var entity))
					{
						if(entity.CardId == Collectible.Druid.BottomlessToyChest)
						{
							var lastCardDrawnId = game.Opponent.Hand.OrderByDescending(e => e.ZonePosition).FirstOrDefault()?.Id;
							var lastCardDrawnEntity = game.Entities.TryGetValue(lastCardDrawnId ?? -1, out var e) ? e : null;
							var copyOfCardId = lastCardDrawnEntity?.Info.CopyOfCardId ?? lastCardDrawnId.ToString();
							AddKnownCardId(gameState, "", copyOfCardId: copyOfCardId);
						}
					}
				}
				catch(FormatException e)
				{
					Log.Info(e.Message);
				}
			}

			gameState.IsInsideMetaDataHistoryTarget = isInsideMetaDataHistoryTarget;

			if(logLine.Contains("End Spectator") && !game.IsInMenu)
				gameState.GameHandler?.HandleGameEnd(false);
			else if(logLine.Contains("BLOCK_START"))
			{
				var match = BlockStartRegex.Match(logLine);
				var blockType = match.Success ? match.Groups["type"].Value : null;
				var cardId = match.Success ? match.Groups["Id"].Value : null;
				var target = GetTargetCardId(match);
				var correspondPlayer = match.Success ? int.Parse(match.Groups["player"].Value) : -1;
				var triggerKeyword = match.Success ? match.Groups["triggerKeyword"].Value : null;
				gameState.BlockStart(blockType, cardId, target, triggerKeyword);

				if(match.Success && blockType == "PLAY")
				{
					gameState.LastPlayBlockTime = logLineTime;
				}
				else if(match.Success && (blockType == "TRIGGER" || blockType == "POWER"))
				{
					var playerEntity =
						game.Entities.FirstOrDefault(
							e => e.Value.HasTag(GameTag.PLAYER_ID) && e.Value.GetTag(GameTag.PLAYER_ID) == game.Player.Id);
					var opponentEntity =
						game.Entities.FirstOrDefault(
							e => e.Value.HasTag(GameTag.PLAYER_ID) && e.Value.GetTag(GameTag.PLAYER_ID) == game.Opponent.Id);

					var actionStartingCardId = match.Groups["cardId"].Value.Trim();
					var actionStartingEntityId = int.Parse(match.Groups["id"].Value);
					if(gameState.CurrentBlock != null)
						gameState.CurrentBlock.SourceEntityId = actionStartingEntityId;

					Entity? actionStartingEntity = null;

					if(string.IsNullOrEmpty(actionStartingCardId))
					{
						if(game.Entities.TryGetValue(actionStartingEntityId, out actionStartingEntity))
							actionStartingCardId = actionStartingEntity.CardId;
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
							case Collectible.Neutral.SphereOfSapience:
								// These are tricky to implement correctly, so
								// until the are, we will just reset the state
								// known about the top/bottom of the deck
								if(actionStartingEntity?.IsControlledBy(game.Player.Id) ?? false)
									gameState.GameHandler?.HandlePlayerUnknownCardAddedToDeck();
								break;

							case Collectible.Rogue.TradePrinceGallywix:
								if(!game.Entities.TryGetValue(gameState.LastCardPlayed, out var lastPlayed) || lastPlayed.CardId == null)
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
							case Collectible.Mage.FrozenCloneCorePlaceholder:
								if(target != null)
									AddKnownCardId(gameState, target, 2);
								break;
							case Collectible.Shaman.Moorabi:
							case Collectible.Shaman.MoorabiCorePlaceholder:
							case Collectible.Rogue.SonyaShadowdancer:
								if(target != null)
									AddKnownCardId(gameState, target);
								break;
							case Collectible.Neutral.HoardingDragon:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoinCore, 2);
								break;
							case Collectible.Priest.GildedGargoyle:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoinCore);
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
							case NonCollectible.Neutral.HakkartheSoulflayer_CorruptedBloodToken:
								AddKnownCardId(gameState, NonCollectible.Neutral.HakkartheSoulflayer_CorruptedBloodToken, 2);
								break;
							//TODO: Gral, the Shark?
							case Collectible.Paladin.ImmortalPrelate:
								AddKnownCardId(gameState, Collectible.Paladin.ImmortalPrelate);
								break;
							case Collectible.Neutral.Explodineer:
							case Collectible.Warrior.Wrenchcalibur:
								AddKnownCardId(gameState, NonCollectible.Neutral.SeaforiumBomber_BombToken);
								break;
							case Collectible.Priest.SpiritOfTheDead:
								if(correspondPlayer == game.Player.Id)
								{
									if(game.Player.LastDiedMinionCard?.CardId != null)
										AddKnownCardId(gameState, game.Player.LastDiedMinionCard.CardId);
								}
								else if(correspondPlayer == game.Opponent.Id)
								{
									if(game.Opponent.LastDiedMinionCard?.CardId != null)
										AddKnownCardId(gameState, game.Opponent.LastDiedMinionCard.CardId);
								}
								break;
							case Collectible.Druid.SecureTheDeck:
								AddKnownCardId(gameState, Collectible.Druid.ClawLegacy, 3);
								break;
							case Collectible.Rogue.Waxadred:
								AddKnownCardId(gameState, NonCollectible.Rogue.Waxadred_WaxadredsCandleToken);
								break;
							case Collectible.Neutral.BadLuckAlbatross:
								AddKnownCardId(gameState, NonCollectible.Neutral.BadLuckAlbatross_AlbatrossToken, 2);
								break;
							case Collectible.Priest.ReliquaryOfSouls:
								AddKnownCardId(gameState, NonCollectible.Priest.ReliquaryofSouls_ReliquaryPrimeToken);
								break;
							case Collectible.Mage.AstromancerSolarian:
								AddKnownCardId(gameState, NonCollectible.Mage.AstromancerSolarian_SolarianPrimeToken);
								break;
							case Collectible.Warlock.KanrethadEbonlocke:
								AddKnownCardId(gameState, NonCollectible.Warlock.KanrethadEbonlocke_KanrethadPrimeToken);
								break;
							case Collectible.Paladin.MurgurMurgurgle:
								AddKnownCardId(gameState, NonCollectible.Paladin.MurgurMurgurgle_MurgurglePrimeToken);
								break;
							case Collectible.Rogue.Akama:
								AddKnownCardId(gameState, NonCollectible.Rogue.Akama_AkamaPrimeToken);
								break;
							case Collectible.Druid.ArchsporeMsshifn:
								AddKnownCardId(gameState, NonCollectible.Druid.ArchsporeMsshifn_MsshifnPrimeToken);
								break;
							case Collectible.Shaman.LadyVashj:
								AddKnownCardId(gameState, NonCollectible.Shaman.LadyVashj_VashjPrimeToken);
								break;
							case Collectible.Hunter.ZixorApexPredator:
								AddKnownCardId(gameState, NonCollectible.Hunter.ZixorApexPredator_ZixorPrimeToken);
								break;
							case Collectible.Warrior.KargathBladefist:
								AddKnownCardId(gameState, NonCollectible.Warrior.KargathBladefist_KargathPrimeToken);
								break;
							case Collectible.Neutral.SneakyDelinquent:
								AddKnownCardId(gameState, NonCollectible.Neutral.SneakyDelinquent_SpectralDelinquentToken);
								break;
							case Collectible.Neutral.FishyFlyer:
								AddKnownCardId(gameState, NonCollectible.Neutral.FishyFlyer_SpectralFlyerToken);
								break;
							case Collectible.Neutral.SmugSenior:
								AddKnownCardId(gameState, NonCollectible.Neutral.SmugSenior_SpectralSeniorToken);
								break;
							case Collectible.Rogue.Plagiarize:
							case Collectible.Rogue.PlagiarizeCorePlaceholder:
								if (actionStartingEntity != null)
								{
									var player = actionStartingEntity.IsControlledBy(game.Player.Id) ? game.Opponent : game.Player;
									foreach(var entity in player.CardsPlayedThisTurn)
									{
										if(entity.CardId != null)
											AddKnownCardId(gameState, entity.CardId);
									}
								}
								break;
							case Collectible.Rogue.EfficientOctoBot:
								if(actionStartingEntity != null)
									if(actionStartingEntity.IsControlledBy(game.Opponent.Id))
										gameState.GameHandler?.HandleOpponentHandCostReduction(1);
								break;
							case Collectible.Neutral.KeymasterAlabaster:
								// The player controlled side of this is handled by TagChangeActions.OnCardCopy
								if(actionStartingEntity != null && actionStartingEntity.IsControlledBy(game.Opponent.Id) && game.Player.LastDrawnCardId != null)
									AddKnownCardId(gameState, game.Player.LastDrawnCardId);
								break;
							case Collectible.Neutral.EducatedElekk:
								if(actionStartingEntity != null)
								{
									if(actionStartingEntity.IsInGraveyard)
									{
										foreach(var card in actionStartingEntity.Info.StoredCardIds)
											AddKnownCardId(gameState, card);
									}
									else if(game.Entities.TryGetValue(gameState.LastCardPlayed, out var lastPlayedEntity) && lastPlayedEntity.CardId != null)
										actionStartingEntity.Info.StoredCardIds.Add(lastPlayedEntity.CardId);
								}
								break;
							case Collectible.Shaman.DiligentNotetaker:
								if(game.Entities.TryGetValue(gameState.LastCardPlayed, out var lastPlayedEntity1) && lastPlayedEntity1.CardId != null)
									AddKnownCardId(gameState, lastPlayedEntity1.CardId);
								break;
							case Collectible.Neutral.CthunTheShattered:
								// The pieces are created in random order. So we can not assign predicted ids to entities the way we usually do.
								if (actionStartingEntity != null)
								{
									var player = actionStartingEntity.IsControlledBy(game.Player.Id) ? game.Player : game.Opponent;
									player.PredictUniqueCardInDeck(NonCollectible.Neutral.CThuntheShattered_EyeOfCthunToken, true);
									player.PredictUniqueCardInDeck(NonCollectible.Neutral.CThuntheShattered_BodyOfCthunToken, true);
									player.PredictUniqueCardInDeck(NonCollectible.Neutral.CThuntheShattered_MawOfCthunToken, true);
									player.PredictUniqueCardInDeck(NonCollectible.Neutral.CThuntheShattered_HeartOfCthunToken, true);
								}
								break;
							case Collectible.Priest.MidaPureLight:
								AddKnownCardId(gameState, NonCollectible.Priest.MidaPureLight_FragmentOfMidaToken);
								break;
							case Collectible.Warlock.CurseOfAgony:
								AddKnownCardId(gameState, NonCollectible.Warlock.CurseofAgony_AgonyToken, 3);
								break;
							case Collectible.Neutral.AzsharanSentinel:
								AddKnownCardId(gameState, NonCollectible.Neutral.AzsharanSentinel_SunkenSentinelToken, 1, DeckLocation.Bottom);
								break;
							case Collectible.Warrior.AzsharanTrident:
								AddKnownCardId(gameState, NonCollectible.Warrior.AzsharanTrident_SunkenTridentToken, 1, DeckLocation.Bottom);
								break;
							case Collectible.Hunter.AzsharanSaber:
								AddKnownCardId(gameState, NonCollectible.Hunter.AzsharanSaber_SunkenSaberToken, 1, DeckLocation.Bottom);
								break;
							case Collectible.Demonhunter.AzsharanDefector:
								AddKnownCardId(gameState, NonCollectible.Demonhunter.AzsharanDefector_SunkenDefectorToken, 1, DeckLocation.Bottom);
								break;
							case Collectible.Druid.Bottomfeeder:
								AddKnownCardId(gameState, Collectible.Druid.Bottomfeeder, 1, DeckLocation.Bottom);
								break;
							case Collectible.Shaman.PiranhaPoacher:
								AddKnownCardId(gameState, Collectible.Neutral.PiranhaSwarmer);
								break;
							case Collectible.Paladin.SinfulSousChef:
								AddKnownCardId(gameState, NonCollectible.Paladin.SilverHandRecruitLegacyToken1, 2);
								break;
							case Collectible.Neutral.RivendareWarrider:
								AddKnownCardId(gameState, NonCollectible.Neutral.RivendareWarrider_BlaumeuxFamineriderToken);
								AddKnownCardId(gameState, NonCollectible.Neutral.RivendareWarrider_KorthazzDeathriderToken);
								AddKnownCardId(gameState, NonCollectible.Neutral.RivendareWarrider_ZeliekConquestriderToken);
								break;
							case NonCollectible.Deathknight.Helya_HelyaEnchantment:
								if (!gameState.LastPlagueDrawn.IsEmpty())
								{
									AddKnownCardId(gameState, gameState.LastPlagueDrawn.Pop());
								}
								break;
							case Collectible.Rogue.TombPillagerLOE:
							case Collectible.Rogue.TombPillagerWONDERS:
							case Collectible.Rogue.TombPillagerCorePlaceholder:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoinCore, 2);
								break;
							case Collectible.Rogue.LoanShark:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoinCore, 2);
								break;
							case Collectible.Rogue.CoppertailSnoop:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoinCore);
								break;
							case Collectible.Warlock.DisposalAssistant:
								AddKnownCardId(gameState, NonCollectible.Neutral.TramMechanic_BarrelOfSludgeToken, 1, DeckLocation.Bottom);
								break;
							case Collectible.Warlock.SludgeOnWheels:
								AddKnownCardId(gameState, NonCollectible.Neutral.TramMechanic_BarrelOfSludgeToken, 1, DeckLocation.Bottom);
								break;
							case Collectible.Neutral.AdaptiveAmalgam:
								AddKnownCardId(gameState, Collectible.Neutral.AdaptiveAmalgam);
								break;
							case Collectible.Demonhunter.PatchesThePilot:
								AddKnownCardId(gameState, NonCollectible.Demonhunter.PatchesthePilot_ParachuteToken, 6);
								break;
							case Collectible.Neutral.WhelpWrangler:
								AddKnownCardId(gameState, NonCollectible.Neutral.TaketotheSkies_HappyWhelpToken);
								break;
							case Collectible.Hunter.RangerGilly:
								AddKnownCardId(gameState, NonCollectible.Hunter.RangerGilly_IslandCrocoliskToken);
								break;
							case Collectible.Neutral.MiracleSalesman:
								AddKnownCardId(gameState, NonCollectible.Neutral.MiracleSalesman_SnakeOilToken);
								break;
							case Collectible.Hunter.Starshooter:
								AddKnownCardId(gameState, Collectible.Hunter.ArcaneShotCore);
								break;
							case Collectible.Priest.PuppetTheatre:
								if(target != null)
									AddKnownCardId(gameState, target);
								break;
							case Collectible.Paladin.LifesavingAura:
								AddKnownCardId(gameState, NonCollectible.Paladin.Grillmaster_SunscreenToken);
								break;
							case Collectible.Rogue.MetalDetector:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoinCore);
								break;
							case NonCollectible.Rogue.GaronaHalforcen_KingLlaneToken:
								AddKnownCardId(gameState, NonCollectible.Rogue.GaronaHalforcen_KingLlaneToken);
								break;
							case NonCollectible.Paladin.LibramofDivinity_LibramOfDivinityEnchantment:
								AddKnownCardId(gameState, Collectible.Paladin.LibramOfDivinity);
								break;
							case NonCollectible.Neutral.Corpsicle_CorpsicleEnchantment:
								AddKnownCardId(gameState, Collectible.Deathknight.Corpsicle);
								break;
							case Collectible.Mage.CommanderSivara:
							case Collectible.Neutral.TidepoolPupil:
								if(
									gameState.CurrentBlock?.Parent?.CardId != null
									&& Database.GetCardFromId(gameState.CurrentBlock.Parent.CardId)?.Type == "Spell"
									&& actionStartingEntity != null
									)
								{
									var maxCards = 3;
									if(actionStartingEntity.Info.StoredCardIds.Count() < maxCards)
										actionStartingEntity.Info.StoredCardIds.Add(gameState.CurrentBlock.Parent.CardId);
								}
								break;
							case Collectible.Neutral.AugmentedElekk:
								if (gameState.CurrentBlock?.Parent != null)
								{
									var (entity, ids) = gameState.CurrentBlock.Parent.EntitiesCreatedInDeck
										.LastOrDefault(
											x => !x.ids.Contains(gameState.CurrentBlock.SourceEntityId)
										);
									if(entity?.CardId != null)
									{
										ids.Add(gameState.CurrentBlock.SourceEntityId);
										AddKnownCardId(gameState, entity.CardId);
									}
								}
								break;
							case Collectible.Neutral.Meadowstrider:
								AddKnownCardId(gameState, Collectible.Neutral.Meadowstrider, 1, DeckLocation.Bottom);
								break;
							case Collectible.Paladin.IdoOfTheThreshfleet:
								AddKnownCardId(gameState, NonCollectible.Paladin.IdooftheThreshfleet_CallTheThreshfleetToken);
								break;
							case Collectible.Hunter.RangariScout:
								// discover options often are copies of other entities
								// when they are discovered, they are still not created on game.Entities
								// here we check if they are a copy of other entity, if they are we use the original entity id
								var chosenId = gameState.LastEntityChosenOnDiscover;
								var chosenEntity = game.Entities.TryGetValue(chosenId, out var e) ? e : null;
								var isCopiedEntity = chosenEntity?.GetTag(GameTag.COPIED_FROM_ENTITY_ID) > 0;
								gameState.LastEntityChosenOnDiscover = isCopiedEntity ? chosenEntity?.GetTag(GameTag.COPIED_FROM_ENTITY_ID) ?? chosenId : chosenId;

								AddKnownCardId(gameState, "", copyOfCardId: gameState.LastEntityChosenOnDiscover.ToString());
								break;
						}

						if(triggerKeyword == "SECRET")
						{
							if(actionStartingEntity != null)
							{
								if(actionStartingEntity.IsControlledBy(game.Player.Id))
								{
									gameState.GameHandler?.HandlePlayerSecretTrigger(actionStartingEntity, cardId,
										gameState.GetTurnNumber(), actionStartingEntityId);
								}
								else
								{
									gameState.GameHandler?.HandleOpponentSecretTrigger(actionStartingEntity, cardId,
										gameState.GetTurnNumber(), actionStartingEntityId);
								}
							}
						}
					}
					else //POWER
					{
						switch(actionStartingCardId)
						{
							case Collectible.Demonhunter.SightlessWatcherCorePlaceholder:
							case Collectible.Demonhunter.SightlessWatcherLegacy:
							case Collectible.Neutral.AmbassadorFaelin:
								// These are tricky to implement correctly, so
								// until the are, we will just reset the state
								// known about the top/bottom of the deck
								if(actionStartingEntity?.IsControlledBy(game.Player.Id) ?? false)
									gameState.GameHandler?.HandlePlayerUnknownCardAddedToDeck();
								break;

							case Collectible.Rogue.GangUp:
							case Collectible.Hunter.DireFrenzy:
							case Collectible.Hunter.DireFrenzyCorePlaceholder:
							case Collectible.Rogue.LabRecruiter:
								if(target != null)
									AddKnownCardId(gameState, target, 3);
								break;
							case Collectible.Rogue.BeneathTheGrounds:
								AddKnownCardId(gameState, NonCollectible.Rogue.BeneaththeGrounds_NerubianAmbushToken, 3);
								break;
							case Collectible.Warrior.IronJuggernautGVG:
								AddKnownCardId(gameState, NonCollectible.Warrior.IronJuggernaut_BurrowingMineToken);
								break;
							case Collectible.Druid.Recycle:
							case Collectible.Mage.ManicSoulcaster:
							case Collectible.Neutral.ZolaTheGorgon:
							case Collectible.Neutral.ZolaTheGorgonCorePlaceholder:
							case Collectible.Druid.Splintergraft:
							//case Collectible.Priest.HolyWater: -- TODO
							case Collectible.Neutral.BalefulBanker:
							case Collectible.Neutral.DollmasterDorian:
							case Collectible.Priest.Seance:
							case Collectible.Druid.MarkOfTheSpikeshell:
							case Collectible.Neutral.DragonBreeder:
							case Collectible.Shaman.ColdStorage:
							case Collectible.Priest.PowerChordSynchronize:
							case Collectible.Rogue.Shadowcaster:
								if(target != null)
									AddKnownCardId(gameState, target);
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
							case Collectible.Neutral.EliseStarseekerCorePlaceholder:
								AddKnownCardId(gameState, NonCollectible.Neutral.EliseStarseeker_MapToTheGoldenMonkeyToken);
								break;
							case NonCollectible.Neutral.EliseStarseeker_MapToTheGoldenMonkeyToken:
								AddKnownCardId(gameState, NonCollectible.Neutral.EliseStarseeker_GoldenMonkeyToken);
								break;
							case Collectible.Neutral.Doomcaller:
								AddKnownCardId(gameState, NonCollectible.Neutral.CthunOG);
								break;
							case Collectible.Druid.JadeIdol:
							case NonCollectible.Druid.JadeIdol_JadeStash:
								AddKnownCardId(gameState, Collectible.Druid.JadeIdol, 3);
								break;
							case NonCollectible.Hunter.TheMarshQueen_QueenCarnassaToken:
								AddKnownCardId(gameState, NonCollectible.Hunter.TheMarshQueen_CarnassasBroodToken, 20);
								break;
							case Collectible.Neutral.EliseTheTrailblazer:
								AddKnownCardId(gameState, NonCollectible.Neutral.ElisetheTrailblazer_UngoroPackToken);
								break;
							case Collectible.Mage.GhastlyConjurer:
							case Collectible.Mage.GhastlyConjurerCorePlaceholder:
								AddKnownCardId(gameState, Collectible.Mage.MirrorImageLegacy);
								break;
							case Collectible.Druid.ThorngrowthSentries:
								AddKnownCardId(gameState, NonCollectible.Druid.ThorngrowthSentries_ThornguardTurtleToken, 2);
								break;
							case Collectible.Mage.DeckOfWonders:
								AddKnownCardId(gameState, NonCollectible.Mage.DeckofWonders_ScrollOfWonderToken, 5);
								break;
							case Collectible.Neutral.TheDarkness:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheDarkness_DarknessCandleToken, 3);
								break;
							case Collectible.Rogue.FaldoreiStrider:
							case Collectible.Rogue.FaldoreiStriderCorePlaceholder:
								AddKnownCardId(gameState, NonCollectible.Rogue.FaldoreiStrider_SpiderAmbushEnchantment, 3);
								break;
							case Collectible.Neutral.KingTogwaggle:
								AddKnownCardId(gameState, NonCollectible.Neutral.KingTogwaggle_KingsRansomToken);
								break;
							case NonCollectible.Neutral.TheCandle:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCandle);
								break;
							case NonCollectible.Neutral.CoinPouchGILNEAS:
								AddKnownCardId(gameState, NonCollectible.Neutral.SackOfCoinsGILNEAS);
								break;
							case NonCollectible.Neutral.SackOfCoinsGILNEAS:
								AddKnownCardId(gameState, NonCollectible.Neutral.HeftySackOfCoinsGILNEAS);
								break;
							case NonCollectible.Neutral.CreepyCurioGILNEAS:
								AddKnownCardId(gameState, NonCollectible.Neutral.HauntedCurioGILNEAS);
								break;
							case NonCollectible.Neutral.HauntedCurioGILNEAS:
								AddKnownCardId(gameState, NonCollectible.Neutral.CursedCurioGILNEAS);
								break;
							case NonCollectible.Neutral.OldMilitiaHornGILNEAS:
								AddKnownCardId(gameState, NonCollectible.Neutral.MilitiaHornGILNEAS);
								break;
							case NonCollectible.Neutral.MilitiaHornGILNEAS:
								AddKnownCardId(gameState, NonCollectible.Neutral.VeteransMilitiaHornGILNEAS);
								break;
							case NonCollectible.Neutral.SurlyMobGILNEAS:
								AddKnownCardId(gameState, NonCollectible.Neutral.AngryMobGILNEAS);
								break;
							case NonCollectible.Neutral.AngryMobGILNEAS:
								AddKnownCardId(gameState, NonCollectible.Neutral.CrazedMobGILNEAS);
								break;
							case Collectible.Neutral.SparkEngine:
								AddKnownCardId(gameState, NonCollectible.Neutral.SparkDrill_SparkToken);
								break;
							case Collectible.Priest.ExtraArms:
								AddKnownCardId(gameState, NonCollectible.Priest.ExtraArms_MoreArmsToken);
								break;
							case Collectible.Neutral.SeaforiumBomber:
							case Collectible.Warrior.ClockworkGoblin:
								AddKnownCardId(gameState, NonCollectible.Neutral.SeaforiumBomber_BombToken);
								break;
							case Collectible.Rogue.Wanted:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoinCore);
								break;
							//TODO: Hex Lord Malacrass
							//TODO: Krag'wa, the Frog
							case Collectible.Hunter.HalazziTheLynx:
								AddKnownCardId(gameState, NonCollectible.Hunter.Springpaw_LynxToken, 10);
								break;
							case Collectible.Neutral.BananaVendor:
								AddKnownCardId(gameState, NonCollectible.Neutral.BananaBuffoon_BananasToken, 4);
								break;
							case Collectible.Neutral.BananaBuffoon:
								AddKnownCardId(gameState, NonCollectible.Neutral.BananaBuffoon_BananasToken, 2);
								break;
							case Collectible.Neutral.BootyBayBookie:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoinCore);
								break;
							case Collectible.Neutral.PortalKeeper:
							case Collectible.Neutral.PortalOverfiend:
								AddKnownCardId(gameState, NonCollectible.Neutral.PortalKeeper_FelhoundPortalToken);
								break;
							case Collectible.Rogue.TogwagglesScheme:
								if(target != null)
									AddKnownCardId(gameState, target);
								break;
							case Collectible.Paladin.SandwaspQueen:
								AddKnownCardId(gameState, NonCollectible.Paladin.SandwaspQueen_SandwaspToken, 2);
								break;
							case Collectible.Rogue.ShadowOfDeath:
								AddKnownCardId(gameState, NonCollectible.Rogue.ShadowofDeath_ShadowToken, 3);
								break;
							case Collectible.Warlock.Impbalming:
								AddKnownCardId(gameState, NonCollectible.Warlock.Impbalming_WorthlessImpToken, 3);
								break;
							case Collectible.Druid.YseraUnleashed:
								AddKnownCardId(gameState, NonCollectible.Druid.YseraUnleashed_DreamPortalToken, 7);
								break;
							case Collectible.Rogue.BloodsailFlybooter:
								AddKnownCardId(gameState, NonCollectible.Rogue.BloodsailFlybooter_SkyPirateToken, 2);
								break;
							case Collectible.Rogue.UmbralSkulker:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoinCore, 3);
								break;
							case Collectible.Neutral.Sathrovarr:
								if(target != null)
									AddKnownCardId(gameState, target, 3);
								break;
							case Collectible.Warlock.SchoolSpirits:
							case Collectible.Warlock.SoulShear:
							case Collectible.Warlock.SpiritJailer:
							case Collectible.Demonhunter.Marrowslicer:
								AddKnownCardId(gameState, NonCollectible.Warlock.SchoolSpirits_SoulFragmentToken, 2);
								break;
							case Collectible.Mage.ConfectionCyclone:
								AddKnownCardId(gameState, NonCollectible.Mage.ConfectionCyclone_SugarElementalToken, 2);
								break;
							case Collectible.Druid.KiriChosenOfElune:
								AddKnownCardId(gameState, Collectible.Druid.LunarEclipse);
								AddKnownCardId(gameState, Collectible.Druid.SolarEclipse);
								break;
							case Collectible.Druid.KiriChosenOfEluneCorePlaceholder:
								AddKnownCardId(gameState, Collectible.Druid.LunarEclipseCorePlaceholder);
								AddKnownCardId(gameState, Collectible.Druid.SolarEclipseCorePlaceholder);
								break;
							case NonCollectible.Neutral.CThuntheShattered_EyeOfCthunToken:
							case NonCollectible.Neutral.CThuntheShattered_HeartOfCthunToken:
							case NonCollectible.Neutral.CThuntheShattered_BodyOfCthunToken:
							case NonCollectible.Neutral.CThuntheShattered_MawOfCthunToken:
								// A new copy of C'Thun is created in the last of these POWER blocks.
								// This currently leads to a duplicate copy of C'Thun showing up in the
								// opponents deck list, but it will have to do for now.
								AddKnownCardId(gameState, Collectible.Neutral.CthunTheShattered);
								break;
							case Collectible.Hunter.SunscaleRaptor:
								AddKnownCardId(gameState, Collectible.Hunter.SunscaleRaptor);
								break;
							case Collectible.Neutral.Mankrik:
								AddKnownCardId(gameState, NonCollectible.Neutral.Mankrik_OlgraMankriksWifeToken);
								break;
							case Collectible.Neutral.ShadowHunterVoljin:
								if(target != null)
									AddKnownCardId(gameState, target);
								break;
							case Collectible.Paladin.AldorAttendant:
								if(actionStartingEntity != null)
								{
									if(actionStartingEntity.IsControlledBy(game.Player.Id))
										gameState.GameHandler?.HandlePlayerLibramReduction(1);
									else
										gameState.GameHandler?.HandleOpponentLibramReduction(1);
								}
								break;
							case Collectible.Paladin.AldorTruthseeker:
								if(actionStartingEntity != null)
								{
									if(actionStartingEntity.IsControlledBy(game.Player.Id))
										gameState.GameHandler?.HandlePlayerLibramReduction(2);
									else
										gameState.GameHandler?.HandleOpponentLibramReduction(2);
								}
								break;
							case Collectible.Druid.VibrantSquirrel:
								AddKnownCardId(gameState, NonCollectible.Druid.VibrantSquirrel_AcornToken, 4);
								break;
							case Collectible.Mage.FirstFlame:
								AddKnownCardId(gameState, NonCollectible.Mage.FirstFlame_SecondFlameToken);
								break;
							case Collectible.Rogue.Garrote:
								AddKnownCardId(gameState, NonCollectible.Rogue.Garrote_BleedToken, 3);
								break;
							case Collectible.Neutral.MailboxDancer:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoinCore);
								break;
							case Collectible.Neutral.NorthshireFarmer:
								if(target != null)
									AddKnownCardId(gameState, target, 3);
								break;
							case Collectible.Rogue.LoanShark:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoinCore);
								break;
							case Collectible.Warlock.SeedsOfDestruction:
								AddKnownCardId(gameState, NonCollectible.Warlock.DreadlichTamsin_FelRiftToken, 3);
								break;
							case Collectible.Mage.BuildASnowman:
								AddKnownCardId(gameState, NonCollectible.Mage.BuildaSnowman_BuildASnowbruteToken);
								break;
							case Collectible.Warrior.Scrapsmith:
								AddKnownCardId(gameState, NonCollectible.Warrior.Scrapsmith_ScrappyGruntToken);
								break;
							case Collectible.Neutral.RamCommander:
								AddKnownCardId(gameState, NonCollectible.Neutral.RamCommander_BattleRamToken);
								break;
							case Collectible.Warlock.DraggedBelow:
							case Collectible.Warlock.SirakessCultist:
							case Collectible.Warlock.AbyssalWave:
							case Collectible.Warlock.Zaqul:
								AddKnownCardId(gameState, NonCollectible.Warlock.SirakessCultist_AbyssalCurseToken);
								break;
							case Collectible.Neutral.SchoolTeacher:
								AddKnownCardId(gameState, NonCollectible.Neutral.SchoolTeacher_NagalingToken);
								break;
							case Collectible.Warlock.AzsharanScavenger:
								AddKnownCardId(gameState, NonCollectible.Warlock.AzsharanScavenger_SunkenScavengerToken, 1, DeckLocation.Bottom);
								break;
							case Collectible.Priest.AzsharanRitual:
								AddKnownCardId(gameState, NonCollectible.Priest.AzsharanRitual_SunkenRitualToken, 1, DeckLocation.Bottom);
								break;
							case Collectible.Shaman.AzsharanScroll:
								AddKnownCardId(gameState, NonCollectible.Shaman.AzsharanScroll_SunkenScrollToken, 1, DeckLocation.Bottom);
								break;
							case Collectible.Paladin.AzsharanMooncatcher:
								AddKnownCardId(gameState, NonCollectible.Paladin.AzsharanMooncatcher_SunkenMooncatcherToken, 1, DeckLocation.Bottom);
								break;
							case Collectible.Rogue.AzsharanVessel:
								AddKnownCardId(gameState, NonCollectible.Rogue.AzsharanVessel_SunkenVesselToken, 1, DeckLocation.Bottom);
								break;
							case Collectible.Shaman.Schooling:
								AddKnownCardId(gameState, Collectible.Neutral.PiranhaSwarmer, 3); // Is this the correct token? These are 4 different ones
								break;
							case Collectible.Druid.AzsharanGardens:
								AddKnownCardId(gameState, NonCollectible.Druid.AzsharanGardens_SunkenGardensToken, 1, DeckLocation.Bottom);
								break;
							case Collectible.Mage.AzsharanSweeper:
								AddKnownCardId(gameState, NonCollectible.Mage.AzsharanSweeper_SunkenSweeperToken, 1, DeckLocation.Bottom);
								break;
							case Collectible.Rogue.BootstrapSunkeneer:
								if(target != null)
									AddKnownCardId(gameState, target, 1, DeckLocation.Bottom);
								break;
							case Collectible.Mage.FrozenTouch:
								AddKnownCardId(gameState, NonCollectible.Mage.FrozenTouch_FrozenTouchToken);
								break;
							case Collectible.Mage.ArcaneWyrm:
								AddKnownCardId(gameState, Collectible.Mage.ArcaneBolt);
								break;
							case Collectible.Priest.SisterSvalna:
								AddKnownCardId(gameState, NonCollectible.Priest.SisterSvalna_VisionOfDarknessToken);
								break;
							case Collectible.Neutral.PozzikAudioEngineer:
								AddKnownCardId(gameState, NonCollectible.Neutral.PozzikAudioEngineer_AudioBotToken, 2);
								break;
							case Collectible.Hunter.MisterMukla:
								AddKnownCardId(gameState, NonCollectible.Neutral.KingMukla_BananasToken, 10);
								break;
							case Collectible.Shaman.SaxophoneSoloist:
								AddKnownCardId(gameState, Collectible.Shaman.SaxophoneSoloist);
								break;
							case Collectible.Paladin.TheCountess:
								AddKnownCardId(gameState, NonCollectible.Paladin.TheCountess_LegendaryInvitationToken);
								break;
							case Collectible.Neutral.LicensedAdventurer:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoinCore);
								break;
							case Collectible.Mage.SteamSurger:
								AddKnownCardId(gameState, Collectible.Mage.FlameGeyser);
								break;
							case Collectible.Warrior.BoombossThogrun:
								AddKnownCardId(gameState, NonCollectible.Warrior.BoombossThogrun_TNTToken, 3);
								break;
							case NonCollectible.Neutral.KoboldMiner_PouchOfCoinsToken:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoinCore, 2);
								break;
							case Collectible.Rogue.DartThrow:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoinCore);
								break;
							case Collectible.Rogue.BountyWrangler:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoinCore);
								break;
							case Collectible.Neutral.GreedyPartner:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoinCore);
								break;
							case Collectible.Neutral.SnakeOilSeller:
								AddKnownCardId(gameState, NonCollectible.Neutral.MiracleSalesman_SnakeOilToken, 2);
								break;
							case Collectible.Warlock.DisposalAssistant:
								AddKnownCardId(gameState, NonCollectible.Neutral.TramMechanic_BarrelOfSludgeToken, 1, DeckLocation.Bottom);
								break;
							case Collectible.Warlock.MassProduction:
								AddKnownCardId(gameState, Collectible.Warlock.MassProduction, 2);
								break;
							case Collectible.Warrior.SafetyExpert:
								AddKnownCardId(gameState, NonCollectible.Neutral.SeaforiumBomber_BombToken, 3);
								break;
							case Collectible.Neutral.Incindius:
								AddKnownCardId(gameState, NonCollectible.Neutral.Incindius_EruptionToken, 5);
								break;
							case Collectible.Neutral.Mixologist:
								AddKnownCardId(gameState, NonCollectible.Neutral.Mixologist_MixologistsSpecialToken);
								break;
							case Collectible.Neutral.CelestialProjectionist:
								if(target != null)
									AddKnownCardId(gameState, target);
								break;
							case Collectible.Neutral.Gorgonzormu:
								AddKnownCardId(gameState, NonCollectible.Neutral.Gorgonzormu_DeliciousCheeseToken);
								break;
							case Collectible.Rogue.TentacleGrip:
								AddKnownCardId(gameState, Collectible.Neutral.ChaoticTendril);
								break;
							case Collectible.Rogue.DigForTreasure:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoinCore);
								break;
							case Collectible.Rogue.OhManager:
								AddKnownCardId(gameState, NonCollectible.Neutral.TheCoinCore);
								break;
							case Collectible.Neutral.CarryOnGrub:
								// TODO Token2 if only one card is left
								AddKnownCardId(gameState, NonCollectible.Neutral.CarryOnGrub_CarryOnSuitcaseToken1);
								break;
							case Collectible.Warrior.TheRyecleaver:
								AddKnownCardId(gameState, NonCollectible.Warrior.TheRyecleaver_SliceOfBreadToken);
								break;
							case NonCollectible.Neutral.PhotographerFizzle_FizzlesSnapshotToken:
								foreach(var card in actionStartingEntity?.Info.StoredCardIds ?? new List<string>())
								{
									// When the opponent plays the "Fizzle" card, a snapshot of the game state is captured.
									// Some cards are revealed, providing their exact cardId, while others we only know the entityId.
									// We handle these cases differently based on the information available:
									//
									// 1. If the revealed identifier is a number, it represents an entityId
									//    In this case, we link the created card to the existing entity.
									//
									// 2. If the revealed identifier is not a number, it represents a cardId
									//    Here, we create a new card using the known cardId.
									if(int.TryParse(card, out _))
									{
										AddKnownCardId(gameState, "", copyOfCardId: card);
									}
									else
									{
										AddKnownCardId(gameState, card);
									}
								}
								break;
							case NonCollectible.Priest.Repackage_RepackagedBoxToken:
								foreach(var card in actionStartingEntity?.Info.StoredCardIds ?? new List<string>())
								{
									AddKnownCardId(gameState, card);
								}
								break;
							case Collectible.Neutral.MarinTheManager:
								if(actionStartingEntity?.IsControlledBy(game.Opponent.Id) == true)
								{
									foreach(var id in new List<string> {
								        NonCollectible.Neutral.MarintheManager_TolinsGobletToken,
								        NonCollectible.Neutral.MarintheManager_GoldenKoboldToken,
								        NonCollectible.Neutral.MarintheManager_WondrousWandToken,
								        NonCollectible.Neutral.MarintheManager_ZarogsCrownToken,
									})
									{
										if(id != null)
										{
											game.Opponent.PredictUniqueCardInDeck(id, true);
										}
									}
								}
								break;
							case Collectible.Demonhunter.XortothBreakerOfStars:
								AddKnownCardId(gameState, NonCollectible.Demonhunter.XortothBreakerofStars_StarOfOriginationToken);
								AddKnownCardId(gameState, NonCollectible.Demonhunter.XortothBreakerofStars_StarOfConclusionToken);
								break;
							case Collectible.Rogue.Talgath:
								AddKnownCardId(gameState, Collectible.Rogue.BackstabCore);
								break;
							case Collectible.Neutral.AstralVigilant:
								AddKnownCardId(gameState, game.Opponent.CardsPlayedThisMatch
									.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, game.Opponent))
									.Where(card => card is { Mechanics: not null } && card.isDraenei())
									.Select(card => card!.Id)
									.LastOrDefault()!);
								break;
							case Collectible.Mage.StellarBalance:
								AddKnownCardId(gameState, Collectible.Druid.MoonfireCorePlaceholder);
								AddKnownCardId(gameState, Collectible.Druid.StarfireLegacy);
								break;
							case Collectible.Mage.SpiritGatherer:
								AddKnownCardId(gameState, NonCollectible.Mage.WispTokenEMERALD_DREAM);
								break;
							case NonCollectible.Warrior.EntertheLostCity_LatorviusGazeOfTheCityToken:
								if(actionStartingEntity?.IsControlledBy(game.Opponent.Id) == true)
								{
									foreach(var id in new List<string> {
								        NonCollectible.Druid.JungleGiants_BarnabusTheStomperToken,
								        NonCollectible.Hunter.TheMarshQueen_QueenCarnassaToken,
								        NonCollectible.Mage.OpentheWaygate_TimeWarpToken,
								        NonCollectible.Paladin.TheLastKaleidosaur_GalvadonToken,
								        NonCollectible.Priest.AwakentheMakers_AmaraWardenOfHopeToken,
								        NonCollectible.Rogue.TheCavernsBelow_CrystalCoreTokenUNGORO,
								        NonCollectible.Shaman.UnitetheMurlocs_MegafinToken,
								        NonCollectible.Warlock.LakkariSacrifice_NetherPortalToken1,
								        NonCollectible.Warrior.FirePlumesHeart_SulfurasToken,
							        }) {
										if(id != null)
										{
											game.Opponent.PredictUniqueCardInDeck(id, true);
										}
									}
								}
								break;
							case NonCollectible.Neutral.SemiStablePortal_RewindTimelineToken:
								if(gameState.LastPlayBlockTime is not null)
								{
									Core.HandleRewind(gameState.LastPlayBlockTime.Value, logLineTime);
								}
								break;
							case Collectible.Druid.SkyMotherAviana:
								var createdByAviana = new FakeCard(Collectible.Druid.SkyMotherAviana)
								{
									Cost = 1,
									Type = CardType.MINION,
									Rarity = Rarity.LEGENDARY,
									Tags = new Dictionary<string, int>
									{
										{ GameTag.ELITE.ToString(), 1 }
									}
								};
								AddKnownCardId(gameState, createdByAviana.Serialize(), count: 10);
								break;
							default:
								if(playerEntity.Value != null && playerEntity.Value.GetTag(GameTag.CURRENT_PLAYER) == 1
									&& !gameState.PlayerUsedHeroPower
									|| opponentEntity.Value != null && opponentEntity.Value.GetTag(GameTag.CURRENT_PLAYER) == 1
									&& !gameState.OpponentUsedHeroPower)
								{
									var card = Database.GetCardFromId(actionStartingCardId!);
									if(card?.Type == "Hero Power")
									{
										if(playerEntity.Value != null && playerEntity.Value.GetTag(GameTag.CURRENT_PLAYER) == 1)
										{
											gameState.GameHandler?.HandlePlayerHeroPower(actionStartingCardId!, gameState.GetTurnNumber());
											gameState.PlayerUsedHeroPower = true;
										}
										else if(opponentEntity.Value != null)
										{
											gameState.GameHandler?.HandleOpponentHeroPower(actionStartingCardId!, gameState.GetTurnNumber());
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
					gameState.GameHandler?.HandleSetupDone();
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
					gameState.GameHandler?.HandleChameleosReveal(gameState.ChameleosReveal.Item2);
				}
				gameState.ChameleosReveal = null;

				var abyssalCurseCreators = new string[] {
					Collectible.Warlock.DraggedBelow,
					Collectible.Warlock.SirakessCultist,
					Collectible.Warlock.AbyssalWave,
					Collectible.Warlock.Zaqul
				};
				if(gameState.CurrentBlock?.Type == "POWER"
					&& abyssalCurseCreators.Contains(gameState.CurrentBlock?.CardId))
				{
					var sourceEntity = game.Entities.FirstOrDefault(e => e.Key == gameState.CurrentBlock!.SourceEntityId).Value;
					var abyssalCurse = game.Entities.LastOrDefault(k => k.Value.GetTag(GameTag.CREATOR) == sourceEntity.Id).Value;
					var nextDamage = abyssalCurse?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1) ?? 0;

					if(sourceEntity.IsControlledBy(game.Player.Id))
						gameState.GameHandler?.HandleOpponentAbyssalCurse(nextDamage);
					else
						gameState.GameHandler?.HandlePlayerAbyssalCurse(nextDamage);
				}

				// Handle Choral Mrrrglr enchantment in Battlegrounds
				// Check at BLOCK_END because the enchantment is updated DURING the block, not at BLOCK_START
				if(game.CurrentGameMode == GameMode.Battlegrounds && game.CurrentGameStats != null &&
				   gameState.CurrentBlock?.Type == "TRIGGER")
				{
					if(gameState.CurrentBlock?.CardId == NonCollectible.Neutral.ChoralMrrrglr)
					{
						var choralEntity = game.Entities.TryGetValue(gameState.CurrentBlock.SourceEntityId, out var entity) ? entity : null;
						if(choralEntity != null && choralEntity.IsControlledBy(game.Opponent.Id))
						{
							// Find the Chorus enchantment that was CHANGED in this block and attached to Choral
							// The enchantment is created inside the TRIGGER block, so it exists in game.Entities by BLOCK_END
							var chorusEnchantment = game.Entities.Values
								.FirstOrDefault(e => e.CardId == NonCollectible.Neutral.ChoralMrrrglr_ChorusEnchantment &&
								                     e.GetTag(GameTag.ATTACHED) == choralEntity.Id &&
								                     e.GetTag(GameTag.CREATOR) == choralEntity.Id);

							if(chorusEnchantment != null)
								BobsBuddyInvoker.GetInstance(game.CurrentGameStats.GameId, game.GetTurnNumber()).UpdateMinionEnchantment(chorusEnchantment, choralEntity.Id, false);
						}
					}
					if(gameState.CurrentBlock is { CardId: NonCollectible.Neutral.TimewarpedNelliesShipToken1, TriggerKeyword: "DEATHRATTLE" })
					{
						var nelliesEntity = game.Entities.TryGetValue(gameState.CurrentBlock.SourceEntityId, out var entity) ? entity : null;
						if(nelliesEntity != null)
						{
							var summonedEntities = game.Entities.Values
								.Where(e =>
									e.GetTag(GameTag.CARDTYPE) == (int)CardType.MINION &&
						            e.GetTag(GameTag.CREATOR) == nelliesEntity.GetTag(GameTag.CREATOR) &&
									e.GetTag(GameTag.ZONE) == (int)Zone.PLAY
								).Select(x => x.Card.DbfId).ToArray();

							if(summonedEntities.Any())
								BobsBuddyInvoker.GetInstance(game.CurrentGameStats.GameId, game.GetTurnNumber())
									.UpdateNelliesShipEnchantment(summonedEntities, nelliesEntity.Id, nelliesEntity.IsControlledBy(game.Player.Id));
						}
					}

				}

				gameState.BlockEnd();
			}

			if(game.IsInMenu)
				return;
			if(!creationTag && gameState.DeterminedPlayers)
				_tagChangeHandler.InvokeQueuedActions(game);
			if(!creationTag)
				gameState.ResetCurrentEntity();
		}

		private void HandleCopiedCard(IGame game, Entity entity)
		{
			var copiesOfCard = game.Opponent.PlayerEntities
				.Where(e => e.Info.CopyOfCardId == entity.Id.ToString());

			foreach (var copy in copiesOfCard)
			{
				copy.CardId = entity.CardId;
				copy.Info.GuessedCardState = GuessedCardState.Guessed;
			}

			if(entity.Info.CopyOfCardId != null)
			{
				var matchingEntities = game.Opponent.PlayerEntities
					.Where(e =>
						e.Id.ToString() == entity.Info.CopyOfCardId ||
						e.Info.CopyOfCardId == entity.Info.CopyOfCardId);

				foreach (var matchingEntity in matchingEntities)
				{
					if(matchingEntity.Id == entity.Id) continue;
					matchingEntity.CardId = entity.CardId;
					matchingEntity.Info.Hidden = false;
					matchingEntity.Info.CopyOfCardId = entity.Id.ToString();
					matchingEntity.Info.GuessedCardState = GuessedCardState.Guessed;
				}
			}
		}

		private static string EnsureValidCardID(string cardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return cardId;
			if(cardId.StartsWith(TransferStudentToken) && !cardId.EndsWith("e"))
				return Collectible.Neutral.TransferStudent;
			if(CardIds.UpgradeOverrides.TryGetValue(cardId, out var overrideId))
				return overrideId;
			return cardId;
		}

		private static string? GetTargetCardId(Match match)
		{
			var target = match.Groups["target"].Value.Trim();
			if(!target.StartsWith("[") || !EntityRegex.IsMatch(target))
				return null;
			var cardIdMatch = CardIdRegex.Match(target);
			return !cardIdMatch.Success ? null : cardIdMatch.Groups["cardId"].Value.Trim();
		}

		private static void AddKnownCardId(IHsGameState gameState, string cardId, int count = 1, DeckLocation location = DeckLocation.Unknown, string? copyOfCardId = null, EntityInfo? info = null)
		{
			if(gameState.CurrentBlock == null)
				return;
			var blockId = gameState.CurrentBlock.Id;
			for(var i = 0; i < count; i++)
			{
				if(!gameState.KnownCardIds.ContainsKey(blockId))
					gameState.KnownCardIds[blockId] = new List<(string, DeckLocation, string?, EntityInfo?)>();
				gameState.KnownCardIds[blockId].Add((cardId, location, copyOfCardId, info));
			}
		}

		private static void RemoveKnownCardId(IHsGameState gameState, int count = 1)
		{
			if(gameState.CurrentBlock == null)
				return;
			var blockId = gameState.CurrentBlock.Id;
			for(var i = 0; i < count; i++)
			{
				if(gameState.KnownCardIds.TryGetValue(blockId, out var blockCardIds) && blockCardIds.Count > 0)
					blockCardIds.RemoveAt(blockCardIds.Count - 1);
			}
		}

		internal void Reset() => _tagChangeHandler.ClearQueuedActions();
	}

	public enum DeckLocation
	{
		Unknown,
		Top,
		Bottom
	}
}
