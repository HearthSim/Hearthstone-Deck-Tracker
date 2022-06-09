#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.BobsBuddy;
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
		const string TransferStudentToken = Collectible.Neutral.TransferStudent + "t";

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
				var cardId = EnsureValidCardID(match.Groups["cardId"].Value);
				var zone = GameTagHelper.ParseEnum<Zone>(match.Groups["zone"].Value);
				var guessedCardId = false;
				var guessedLocation = DeckLocation.Unknown;
				if(!game.Entities.ContainsKey(id))
				{
					if(string.IsNullOrEmpty(cardId) && zone != Zone.SETASIDE)
					{
						var blockId = gameState.CurrentBlock?.Id;
						if(blockId.HasValue && gameState.KnownCardIds.ContainsKey(blockId.Value))
						{
							var known = gameState.KnownCardIds[blockId.Value].FirstOrDefault();
							cardId = known.Item1;
							if(!string.IsNullOrEmpty(cardId))
							{
								guessedLocation = known.Item2;
								Log.Info($"Found data for entity={id}: CardId={cardId}, Location={guessedLocation}");
								gameState.KnownCardIds[blockId.Value].Remove(known);
								guessedCardId = true;
							}
						}
					}
					var entity = new Entity(id) { CardId = cardId };
					if(guessedCardId)
						entity.Info.GuessedCardState = GuessedCardState.Guessed;
					if(guessedLocation != DeckLocation.Unknown)
					{
						var newIndex = ++gameState.DredgeCounter;
						var sign = guessedLocation == DeckLocation.Top ? 1 : -1;
						entity.Info.DeckIndex = sign * newIndex;
					}
					game.Entities.Add(id, entity);

					if(gameState.CurrentBlock != null && (entity.CardId?.ToUpper().Contains("HERO") ?? false))
						gameState.CurrentBlock.HasFullEntityHeroPackets = true;
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
					if(string.IsNullOrEmpty(entity.CardId))
						entity.CardId = cardId;
					entity.Info.LatestCardId = cardId;
					if(type == "SHOW_ENTITY")
					{
						if(entity.Info.GuessedCardState != GuessedCardState.None)
							entity.Info.GuessedCardState = GuessedCardState.Revealed;
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
					}
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

			if(logLine.Contains("End Spectator") && !game.IsInMenu)
				gameState.GameHandler?.HandleGameEnd(false);
			else if(logLine.Contains("BLOCK_START"))
			{
				var match = BlockStartRegex.Match(logLine);
				var blockType = match.Success ? match.Groups["type"].Value : null;
				var cardId = match.Success ? match.Groups["Id"].Value : null;
				var target = GetTargetCardId(match);
				var correspondPlayer = match.Success ? int.Parse(match.Groups["player"].Value) : -1;
				gameState.BlockStart(blockType, cardId, target);

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
					if(blockType == "TRIGGER" && actionStartingCardId == Collectible.Neutral.AugmentedElekk)
					{
						if(gameState.CurrentBlock?.Parent != null)
						{
							actionStartingCardId = gameState.CurrentBlock.Parent.CardId;
							blockType = gameState.CurrentBlock.Parent.Type;
							target = gameState.CurrentBlock.Parent.Target;
						}
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
								if(target != null)
									AddKnownCardId(gameState, target, 2);
								break;
							case Collectible.Shaman.Moorabi:
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
							case Collectible.Warrior.Wrenchcalibur:
								AddKnownCardId(gameState, NonCollectible.Neutral.SeaforiumBomber_BombToken);
								break;
							case Collectible.Priest.SpiritOfTheDead:
								if(correspondPlayer == game.Player.Id)
								{
									if(game.Player.LastDiedMinionCardId != null)
										AddKnownCardId(gameState, game.Player.LastDiedMinionCardId);
								}
								else if(correspondPlayer == game.Opponent.Id)
								{
									if(game.Opponent.LastDiedMinionCardId != null)
										AddKnownCardId(gameState, game.Opponent.LastDiedMinionCardId);
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
								if (actionStartingEntity != null)
								{
									var player = actionStartingEntity.IsControlledBy(game.Player.Id) ? game.Opponent : game.Player;
									foreach(var card in player.CardsPlayedThisTurn)
										AddKnownCardId(gameState, card);
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
								AddKnownCardId(gameState, Collectible.Neutral.PiranhaSwarmer); // Is this the correct token? These are 4 different ones
								break;
						}
					}
					else //POWER
					{
						switch(actionStartingCardId)
						{
							case Collectible.Demonhunter.SightlessWatcherCore:
							case Collectible.Demonhunter.SightlessWatcherLegacy:
							case Collectible.Neutral.SirFinleySeaGuide:
							case Collectible.Neutral.AmbassadorFaelin:
								// These are tricky to implement correctly, so
								// until the are, we will just reset the state
								// known about the top/bottom of the deck
								if(actionStartingEntity?.IsControlledBy(game.Player.Id) ?? false)
									gameState.GameHandler?.HandlePlayerUnknownCardAddedToDeck();
								break;

							case Collectible.Rogue.GangUp:
							case Collectible.Hunter.DireFrenzy:
							case Collectible.Rogue.LabRecruiter:
								if(target != null)
									AddKnownCardId(gameState, target, 3);
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
							case Collectible.Neutral.ZolaTheGorgonCore:
							case Collectible.Druid.Splintergraft:
							//case Collectible.Priest.HolyWater: -- TODO
							case Collectible.Neutral.BalefulBanker:
							case Collectible.Neutral.DollmasterDorian:
							case Collectible.Priest.Seance:
								if(target != null)
									AddKnownCardId(gameState, target);
								break;
							case Collectible.Druid.MarkOfTheSpikeshell:
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
							case Collectible.Neutral.EliseStarseekerCore:
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
							//case Collectible.Rogue.Wanted: -- TODO
							//	AddKnownCardId(gameState, NonCollectible.Neutral.TheCoin);
							//	break;
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
							case Collectible.Neutral.DragonBreeder:
								if(target != null)
									AddKnownCardId(gameState, target);
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
					gameState.GameHandler?.HandleChameleosReveal(gameState.ChameleosReveal.Item2);
				}
				gameState.ChameleosReveal = null;

				if(gameState.CurrentBlock?.Type == "TRIGGER"
					&& gameState.CurrentBlock?.CardId == NonCollectible.Neutral.Baconshop8playerenchantTavernBrawl
					&& gameState.CurrentBlock?.HasFullEntityHeroPackets == true
					&& gameState.Turn % 2 == 0)
				{
					game.SnapshotBattlegroundsBoardState();
					if(game.CurrentGameStats != null)
					{
						BobsBuddyInvoker.GetInstance(game.CurrentGameStats.GameId, gameState.GetTurnNumber())?
							.StartCombat();
					}
				}

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

				gameState.BlockEnd();
			}


			if(game.IsInMenu)
				return;
			if(!creationTag && gameState.DeterminedPlayers)
				_tagChangeHandler.InvokeQueuedActions(game);
			if(!creationTag)
				gameState.ResetCurrentEntity();
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

		private static void AddKnownCardId(IHsGameState gameState, string cardId, int count = 1, DeckLocation location = DeckLocation.Unknown)
		{
			if(gameState.CurrentBlock == null)
				return;
			var blockId = gameState.CurrentBlock.Id;
			for(var i = 0; i < count; i++)
			{
				if(!gameState.KnownCardIds.ContainsKey(blockId))
					gameState.KnownCardIds[blockId] = new List<(string, DeckLocation)>();
				gameState.KnownCardIds[blockId].Add((cardId, location));
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
