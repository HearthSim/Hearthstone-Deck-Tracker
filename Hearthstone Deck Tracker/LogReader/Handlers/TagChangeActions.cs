﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.BobsBuddy;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static HearthDb.CardIds;
using static HearthDb.Enums.GameTag;
using static HearthDb.Enums.PlayState;
using static HearthDb.Enums.Zone;
using CardIds = HearthDb.CardIds;

namespace Hearthstone_Deck_Tracker.LogReader.Handlers
{
	internal class TagChangeActions
	{
		//We have to remove cards moved from deck -> graveyard when this is the parent block due to a data leak introduced by blizzard to the classic format.
		const string ClassicTrackingCardId = HearthDb.CardIds.Collectible.Hunter.TrackingVanilla;

		public Action? FindAction(GameTag tag, IGame game, IHsGameState gameState, int id, int value, int prevValue)
		{
			return () =>
			{
				switch(tag)
				{
					case ZONE:
						ZoneChange(gameState, id, game, value, prevValue);
						break;
					case PLAYSTATE:
						PlaystateChange(gameState, id, game, value);
						break;
					case CARDTYPE:
						CardTypeChange(gameState, id, game, value);
						break;
					case DEFENDING:
						DefendingChange(gameState, id, game, value);
						break;
					case ATTACKING:
						AttackingChange(gameState, id, game, value);
						break;
					case PROPOSED_DEFENDER:
						ProposedDefenderChange(game, value);
						break;
					case PROPOSED_ATTACKER:
						ProposedAttackerChange(gameState, id, game, value);
						break;
					case PREDAMAGE:
						PredamageChange(gameState, id, game, value);
						break;
					case NUM_TURNS_IN_PLAY:
						NumTurnsInPlayChange(gameState, id, game, value);
						break;
					case CONTROLLER:
						ControllerChange(gameState, id, game, prevValue, value);
						break;
					case FATIGUE:
						FatigueChange(gameState, value, game, id);
						break;
					case STEP:
						StepChange(value, prevValue, gameState, game);
						break;
					case TURN:
						TurnChange(gameState, game);
						break;
					case STATE:
						StateChange(value, gameState);
						break;
					case TRANSFORMED_FROM_CARD:
						TransformedFromCardChange(id, value, game);
						break;
					case CREATOR:
					case DISPLAYED_CREATOR:
						CreatorChanged(id, value, game);
						break;
					case WHIZBANG_DECK_ID:
						WhizbangDeckIdChange(id, value, game);
						break;
					case MULLIGAN_STATE:
						MulliganStateChange(id, value, game, gameState);
						break;
					case COPIED_FROM_ENTITY_ID:
						OnCardCopy(id, value, game, gameState);
						break;
					case LINKED_ENTITY:
						OnLinkedEntity(id, value, game, gameState);
						break;
					case TAG_SCRIPT_DATA_NUM_1:
						OnTagScriptDataNum1(id, value, game, gameState);
						break;
					case REBORN:
						OnRebornChange(id, value, game);
						break;
					case DAMAGE:
						DamageChange(gameState, id, game, value, prevValue);
						break;
					case ARMOR:
						ArmorChange(gameState, id, game, value, prevValue);
						break;
					case FORGE_REVEALED:
						OnForgeRevealed(gameState, id, game, value, prevValue);
						break;
					case REVEALED:
						OnRevealed(gameState, id, game, value, prevValue);
						break;
					case PARENT_CARD:
						OnParentCardChange(gameState, id, game, value, prevValue);
						break;
					case HEALTH:
						HealthChange(gameState, id, game, value, prevValue);
						break;
					case MAXRESOURCES:
						MaxResourcesChange(gameState, id, game, value, prevValue);
						break;
					case MAXHANDSIZE:
						MaxHandSizeChange(gameState, id, game, value, prevValue);
						break;
					case CANT_PLAY:
						CantPlayChange(gameState, id, game, value, prevValue);
						break;
					case LETTUCE_ABILITY_TILE_VISUAL_ALL_VISIBLE:
					case LETTUCE_ABILITY_TILE_VISUAL_SELF_ONLY:
					case FAKE_ZONE:
					case FAKE_ZONE_POSITION:
						gameState.GameHandler?.HandleMercenariesStateChange();
						break;
					case PLAYER_TECH_LEVEL:
						gameState.GameHandler?.HandleBattlegroundsPlayerTechLevel(id, value);
						break;
					case PLAYER_TRIPLES:
						gameState.GameHandler?.HandleBattlegroundsPlayerTriples(id, value);
						break;
					case IMMOLATESTAGE:
						OnImmolateStateChange(id, value, game);
						break;
					case RESOURCES_USED:
						OnResourcesUsedChange(id, value, game);
						break;
					case QUEST_REWARD_DATABASE_ID:
						gameState.GameHandler?.HandleQuestRewardDatabaseId(id, value);
						break;
					case (GameTag)2022:
						OnBattlegroundsSetupChange(value, prevValue, game, gameState);
						break;
					case (GameTag)3533:
						OnBattlegroundsCombatSetupChange(value, prevValue, game);
						break;
					case HERO_ENTITY:
						OnHeroEntityChange(id, value, game);
						break;
					case NEXT_OPPONENT_PLAYER_ID:
						OnNextOpponentPlayerId(id, value, game);
						break;
				}
				game.CounterManager.HandleTagChange(tag, gameState, id, value, prevValue);
			};
		}

		private void OnBattlegroundsSetupChange(int value, int prevValue, IGame game, IHsGameState gameState)
		{
			if(prevValue == 1 && value == 0)
			{
				if(game.IsBattlegroundsSoloMatch && game.CurrentGameStats != null)
				{
					BobsBuddyInvoker.GetInstance(game.CurrentGameStats.GameId, gameState.GetTurnNumber())?
						.StartCombat();
				}
			}
		}

		private void OnBattlegroundsCombatSetupChange(int value, int prevValue, IGame game)
		{
			if(prevValue == 0 && value == 1)
				game.DuosResetHeroTracking();

			if(prevValue == 1 && value == 0)
			{
				if(!game.IsBattlegroundsDuosMatch || game.DuosWasOpponentHeroModified)
				{
					game.SnapshotBattlegroundsBoardState();
				}
				if(game.IsBattlegroundsDuosMatch && game.CurrentGameStats != null)
				{
					BobsBuddyInvoker.GetInstance(game.CurrentGameStats.GameId, game.GetTurnNumber())?
						.StartCombat();
				}
			}
		}

		private void OnHeroEntityChange(int playerEntityId, int heroEntityId, IGame game)
		{
			if(game.IsBattlegroundsDuosMatch)
			{
				if(playerEntityId == game.PlayerEntity?.Id)
				{
					game.DuosSetHeroModified(true);
				}
				else if(playerEntityId == game.OpponentEntity?.Id)
				{
					game.DuosSetHeroModified(false);
				}
			}
			else if (game.IsTraditionalHearthstoneMatch)
			{
				if(!game.Entities.TryGetValue(heroEntityId, out var entity))
					return;

				var hero = Database.GetCardFromId(entity.CardId);

				var heroName = hero?.GetClasses().FirstOrDefault();

				if(heroName is null) return;

				if(playerEntityId == game.PlayerEntity?.Id)
				{
					game.Player.CurrentClass = heroName;
				}
				else if(playerEntityId == game.OpponentEntity?.Id)
				{
					game.Opponent.CurrentClass = heroName;
				}
			}
		}

		private void OnResourcesUsedChange(int id, int value, IGame game)
		{
			if(game.PlayerEntity == null)
				return;
			if(id != game.PlayerEntity.Id)
				return;
			var available = game.PlayerEntity.GetTag(RESOURCES) + game.PlayerEntity.GetTag(TEMP_RESOURCES);
			game.SecretsManager.HandlePlayerManaRemaining(Math.Max(0, available - value));
		}

		private void OnRebornChange(int id, int value, IGame game)
		{
			if(game.CurrentGameStats == null)
				return;
			if(game.CurrentGameMode != GameMode.Battlegrounds)
				return;
			if(value != 1)
				return;
		}

		private void OnTagScriptDataNum1(int id, int value, IGame game, IHsGameState gameState)
		{
			if(game.CurrentGameMode != GameMode.Battlegrounds)
				return;
			if(game.CurrentGameStats == null)
				return;
			var block = gameState.CurrentBlock;
			if(block == null || block.Type != "TRIGGER" || block.CardId != NonCollectible.Neutral.Baconshop8playerenchantTavernBrawl || value != 1)
				return;
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			if(!entity.IsHeroPower || entity.IsControlledBy(game.Player.Id))
				return;

			if(entity.CardId != entity.Info.LatestCardId) Log.Warn($"CardId Mismatch {entity.CardId} vs {entity.Info.LatestCardId}");
		}

		private void OnCardCopy(int id, int value, IGame game, IHsGameState gameState)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			if(!game.Entities.TryGetValue(value, out var targetEntity))
				return;

			OnDredge(entity, targetEntity, game, gameState);

			if(entity.IsControlledBy(game.Opponent.Id))
				return;

			if(entity.GetTag(CREATOR_DBID) == Hearthstone.CardIds.SuspiciousMysteryDbfId)
			{
				// Card was created by Suspicious Alchemist/Usher/Pirate
				return;
			}


			if(string.IsNullOrEmpty(targetEntity.CardId))
			{
				targetEntity.CardId = entity.Info.LatestCardId;
				targetEntity.Info.GuessedCardState = GuessedCardState.Guessed;

				if(entity.GetTag(CREATOR_DBID) == Hearthstone.CardIds.KeyMasterAlabasterDbfId)
					targetEntity.Info.Hidden = false;

				gameState.GameHandler?.HandleCardCopy();
			}
		}

		private void OnLinkedEntity(int id, int value, IGame game, IHsGameState gameState)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			if(!game.Entities.TryGetValue(value, out var targetEntity))
				return;

			// Eyes in the Sky
			var sourceEntityId = gameState.CurrentBlock?.SourceEntityId;
			if(
				sourceEntityId.HasValue &&
				gameState.CurrentBlock?.Type == "POWER" &&
				game.Entities.TryGetValue(sourceEntityId.Value, out var actionStartingEntity) &&
				actionStartingEntity.CardId == Collectible.Rogue.EyesInTheSky &&
				game.Entities.TryGetValue(id, out var linkingEntity) &&
				game.Entities.TryGetValue(value, out var linkedEntity) &&
				linkingEntity.CardId != "" &&
				linkedEntity.CardId == ""
			)
			{
				linkedEntity.CardId = linkingEntity.CardId;
				linkedEntity.Info.GuessedCardState = GuessedCardState.Guessed;
				Core.UpdateOpponentCards();
				return;
			}

			OnDredge(entity, targetEntity, game, gameState);
		}

		private readonly HashSet<string> _canCastDredge = new()
		{
			Collectible.Druid.ConvokeTheSpirits,
			Collectible.Mage.PuzzleBoxOfYoggSaron,
		};

		private void OnDredge(Hearthstone.Entities.Entity entity, Hearthstone.Entities.Entity target, IGame game, IHsGameState gameState)
		{
			if(entity.GetTag(LINKED_ENTITY) != target.Id)
				return;
			if(entity.GetTag(COPIED_FROM_ENTITY_ID) != target.Id)
				return;
			if(!entity.IsControlledBy(game.Player.Id))
				return;
			if(!entity.IsInZone(SETASIDE) || !target.IsInZone(DECK))
				return;

			var source = entity.GetTag(CREATOR);
			if(source == 0 || !game.Entities.TryGetValue(source, out var sourceEntity) || !sourceEntity.HasDredge())
				return;

			if(gameState.CurrentBlock == null)
				return;

			if(_canCastDredge.Contains(gameState.CurrentBlock.Parent?.CardId ?? ""))
			{
				// Dredge effect was automatically cast by another card. Not revealed to the player.
				return;
			}

			if (gameState.CurrentBlock.DredgeCounter == 0)
			{
				gameState.DredgeCounter += 3;
			}

			var index = gameState.DredgeCounter - (gameState.CurrentBlock.DredgeCounter++);
			target.Info.DeckIndex = -index;

			Log.Info($"Dredge Bottom: {target}");

			gameState.GameHandler?.HandlePlayerDredge();
		}

		private void MulliganStateChange(int id, int value, IGame game, IHsGameState gameState)
		{
			if(value == 0)
				return;
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			if(entity.IsPlayer && (Mulligan)value == Mulligan.DONE)
				gameState.GameHandler?.HandlePlayerMulliganDone();
		}

		private void WhizbangDeckIdChange(int id, int value, IGame game)
		{
			if(value == 0)
				return;
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			if(entity.IsControlledBy(game.Player.Id))
				game.Player.IsPlayingWhizbang = true;
			else if(entity.IsControlledBy(game.Opponent.Id))
				game.Opponent.IsPlayingWhizbang = true;
			if(!entity.IsPlayer)
				return;
			if(Config.Instance.AutoDeckDetection)
				DeckManager.AutoSelectTemplateDeckByDeckId(game, value);
		}

		private void CreatorChanged(int id, int value, IGame game)
		{
			if(value == 0)
				return;
			if(game.Entities.TryGetValue(id, out var entity))
			{
				var displayedCreatorId = entity.GetTag(DISPLAYED_CREATOR);
				if(displayedCreatorId == id)
				{
					// Some cards (e.g. Direhorn Hatchling) wrongfully set DISPLAYED_CREATOR
					// on themselves instead of the created entity.
					return;
				}
				if(game.Entities.TryGetValue(displayedCreatorId, out var displayedCreator))
				{
					// For some reason Far Sight sets DISPLAYED_CREATOR on the entity
					if(displayedCreator.CardId == Collectible.Shaman.FarSight || displayedCreator.CardId == Collectible.Shaman.FarSightCore || displayedCreator.CardId == Collectible.Shaman.FarSightVanilla)
						return;
				}

				var creatorId = entity.GetTag(CREATOR);
				if(creatorId == id)
				{
					// Same precaution as for Direhorn Hatching above.
					return;
				}
				if(creatorId == game.GameEntity?.Id)
					return;
				// All cards created by Whizbang have a creator tag set
				if(game.Entities.TryGetValue(creatorId, out var creator))
				{
					if(creator.CardId == CardIds.Collectible.Neutral.WhizbangTheWonderful)
						return;
					var controller = creator.GetTag(CONTROLLER);
					var usingWhizbang = controller == game.Player?.Id && game.Player.IsPlayingWhizbang
										|| controller == game.Opponent?.Id && game.Opponent.IsPlayingWhizbang;
					if(usingWhizbang && creator.IsInSetAside)
						return;
				}
				entity.Info.Created = true;
			}
		}

		private void TransformedFromCardChange(int id, int value, IGame game)
		{
			if(value == 0)
				return;
			if(game.Entities.TryGetValue(id, out var entity))
				entity.Info.SetOriginalCardId(value);
		}

		private void StateChange(int value, IHsGameState gameState)
		{
			if(value != (int)State.COMPLETE)
				return;
			gameState.GameHandler?.HandleGameEnd(true);
			gameState.GameEnded = true;
		}

		private void TurnChange(IHsGameState gameState, IGame game)
		{
			if(!game.SetupDone || game.PlayerEntity == null)
				return;
			if(game.PlayerEntity.HasTag(CURRENT_PLAYER))
				gameState.PlayerUsedHeroPower = false;
			else
				gameState.OpponentUsedHeroPower = false;
		}

		private void StepChange(int value, int prevValue, IHsGameState gameState, IGame game)
		{
			if((Step)value == Step.BEGIN_MULLIGAN)
				gameState.GameHandler?.HandleBeginMulligan();
			gameState.GameHandler?.HandleMercenariesStateChange();
			if (game.PlayerEntity != null && game.PlayerEntity.HasTag(CURRENT_PLAYER) && (Step)value == Step.MAIN_CLEANUP) {
				var remainingMana = game.PlayerEntity.GetTag(RESOURCES) + game.PlayerEntity.GetTag(TEMP_RESOURCES) - game.PlayerEntity.GetTag(RESOURCES_USED);
				game.SecretsManager.HandlePlayerTurnEnd(remainingMana);
			}
			if(game.SetupDone || game.Entities.FirstOrDefault().Value?.Name != "GameEntity")
				return;
			Log.Info("Game was already in progress.");
			gameState.WasInProgress = true;
		}

		private void DefendingChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			gameState.GameHandler?.HandleDefendingEntity(value == 1 ? entity : null);
		}

		private void AttackingChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			gameState.GameHandler?.HandleAttackingEntity(value == 1 ? entity : null);
		}

		private void ProposedDefenderChange(IGame game, int value) => game.ProposedDefender = value;

		private void ProposedAttackerChange(IHsGameState gameState, int id, IGame game, int value) {
			game.ProposedAttacker = value;
			if(value <= 0)
				return;
			if(!game.Entities.TryGetValue(value, out var entity))
				return;
			if(entity.IsHero)
			{
				Log.Debug($"Saw hero attack from {entity.CardId}");

				if(game.IsBattlegroundsDuosMatch && game.CurrentGameStats != null)
				{
					BobsBuddyInvoker.GetInstance(game.CurrentGameStats.GameId, gameState.GetTurnNumber())?
						.MaybeRunDuosPartialCombat();
				}
			}
			gameState.GameHandler?.HandleProposedAttackerChange(entity);
		}


		private void PredamageChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value <= 0)
				return;
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			gameState.GameHandler?.HandleEntityPredamage(entity, value);
		}

		private void ArmorChange(IHsGameState gameState, int id, IGame game, int value, int prevValue)
		{
			if(value <= 0)
				return;
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			//We do prevValue - value because armor gets smaller as you lose it and damage gets bigger as you lose life.
			gameState.GameHandler?.HandleEntityLostArmor(entity, prevValue - value);
		}

		// The HEALTH tag is the total/max Health, the displayed health is HEALTH - DAMAGE
		private void HealthChange(IHsGameState gameState, int id, IGame game, int value, int prevValue)
		{
			if(value <= 0)
				return;
			if(!game.Entities.TryGetValue(id, out var entity))
				return;

			if(!game.IsTraditionalHearthstoneMatch)
				return;

			if(!entity.IsHero || !entity.IsInPlay) return;

			if(entity.IsControlledBy(game.Player.Id))
			{
				gameState.GameHandler?.HandlePlayerMaxHealthChange(value);
			}
			else if(entity.IsControlledBy(game.Opponent.Id))
			{
				gameState.GameHandler?.HandleOpponentMaxHealthChange(value);
			}

		}

		// In Traditional Hearthstone, Resources is Mana
		private void MaxResourcesChange(IHsGameState gameState, int id, IGame game, int value, int prevValue)
		{
			if(value <= 0)
				return;
			if(!game.Entities.TryGetValue(id, out var entity))
				return;

			if(!game.IsTraditionalHearthstoneMatch)
				return;

			if(entity.IsControlledBy(game.Player.Id))
			{
				gameState.GameHandler?.HandlePlayerMaxManaChange(value);
			}
			else if(entity.IsControlledBy(game.Opponent.Id))
			{
				gameState.GameHandler?.HandleOpponentMaxManaChange(value);
			}
		}

		private void MaxHandSizeChange(IHsGameState gameState, int id, IGame game, int value, int prevValue)
		{
			if(value <= 0)
				return;
			if(!game.Entities.TryGetValue(id, out var entity))
				return;

			if(!game.IsTraditionalHearthstoneMatch)
				return;

			// Dark Gift blocks temporarily override the hand size when a card with "Sweet Dreams" is picked
			// We don't care about these
			var isDarkGiftBlock = (
				gameState.CurrentBlock?.CardId == NonCollectible.Neutral.TreacherousTormentor_DarkGiftToken &&
				gameState.CurrentBlock?.Type == "POWER"
			);
			if(isDarkGiftBlock)
				return;

			if(entity.IsControlledBy(game.Player.Id))
			{
				gameState.GameHandler?.HandlePlayerMaxHandSizeChange(value);
			}
			else if(entity.IsControlledBy(game.Opponent.Id))
			{
				gameState.GameHandler?.HandleOpponentMaxHandSizeChange(value);
			}
		}

		private void OnForgeRevealed(IHsGameState gameState, int id, IGame game, int value, int prevValue)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;

			if(entity.IsControlledBy(game.Opponent.Id) && entity.IsInZone(HAND))
			{
				entity.Info.Forged = true;
				entity.Info.Hidden = false;
			}

		}

		private void OnRevealed(IHsGameState gameState, int id, IGame game, int value, int prevValue)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;

			var isStartOfTheGameEffect = gameState.CurrentBlock?.TriggerKeyword == "START_OF_GAME_KEYWORD";
			entity.Info.Hidden = isStartOfTheGameEffect && entity.IsControlledBy(game.Opponent.Id);

			if(isStartOfTheGameEffect && entity.IsControlledBy(game.Opponent.Id))
			{
				entity.Info.GuessedCardState = GuessedCardState.Revealed;
				Core.UpdateOpponentCards();
			}

		}

		private void CantPlayChange(IHsGameState gameState, int id, IGame game, int value, int prevValue)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;

			var player = entity.IsControlledBy(game.Player.Id) ? game.Player : game.Opponent;
			player.CardsPlayedThisMatch.Remove(entity);
			player.CardsPlayedThisTurn.Remove(entity);
			player.SpellsPlayedCards.Remove(entity);
			player.SpellsPlayedInFriendlyCharacters.Remove(entity);
			player.SpellsPlayedInOpponentCharacters.Remove(entity);
		}

		private void OnParentCardChange(IHsGameState gameState, int id, IGame game, int value, int prevValue)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;

			// when a starship is launched it sets the value to 0
			// otherwise it sets to the parent starship id
			if(!game.Entities.TryGetValue(value, out var parentEntity))
			{

				// every piece sets the parent to 0, we only need to get 1 of them
				if(gameState.StarshipLauchBlockIds.Contains(gameState.CurrentBlock?.Id))
					return;

				if(gameState.CurrentBlock?.Type != "POWER" ||
				   !(CardUtils.IsStarship(gameState.CurrentBlock?.CardId) || gameState.CurrentBlock?.CardId == Collectible.Neutral.TheExodar))
					return;

				if(!game.Entities.TryGetValue(prevValue, out var starshipToken))
					return;

				var player = entity.IsControlledBy(game.Player.Id) ? game.Player : game.Opponent;
				gameState.StarshipLauchBlockIds.Add(gameState.CurrentBlock?.Id);
				player.LaunchedStarships.Add(starshipToken.CardId);

				var excludedPieces = new List<string>
				{
					NonCollectible.Neutral.LaunchStarship,
					NonCollectible.Neutral.AbortLaunch,
				};

				var starshipPieces = starshipToken.Info.StoredCardIds.Where(cardId => !excludedPieces.Contains(cardId));
				player.LaunchedStarships.AddRange(starshipPieces);
				return;
			}

			if(entity.CardId != null)
				parentEntity.Info.StoredCardIds.Add(entity.CardId);
		}

		private void DamageChange(IHsGameState gameState, int id, IGame game, int value, int prevValue)
		{
			if(value <= 0)
				return;
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			game.Entities.TryGetValue(entity.GetTag(GameTag.LAST_AFFECTED_BY), out var dealer);
			gameState.GameHandler?.HandleEntityDamage(dealer, entity, value - prevValue);
		}

		private void NumTurnsInPlayChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value <= 0)
				return;
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			gameState.GameHandler?.HandleTurnsInPlayChange(entity, gameState.GetTurnNumber());
		}

		private void FatigueChange(IHsGameState gameState, int value, IGame game, int id)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			var controller = entity.GetTag(CONTROLLER);
			if(controller == game.Player.Id)
				gameState.GameHandler?.HandlePlayerFatigue(value);
			else if(controller == game.Opponent.Id)
				gameState.GameHandler?.HandleOpponentFatigue(value);
		}

		private void ControllerChange(IHsGameState gameState, int id, IGame game, int prevValue, int value)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
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
					gameState.GameHandler?.HandleOpponentStolen(entity, entity.Info.LatestCardId, gameState.GetTurnNumber());
				else if(entity.IsInZone(PLAY))
					gameState.GameHandler?.HandleOpponentStolen(entity, entity.Info.LatestCardId, gameState.GetTurnNumber());
			}
			else if(value == game.Opponent.Id)
			{
				if(entity.IsInZone(Zone.SECRET))
					gameState.GameHandler?.HandlePlayerStolen(entity, entity.Info.LatestCardId, gameState.GetTurnNumber());
				else if(entity.IsInZone(PLAY))
					gameState.GameHandler?.HandlePlayerStolen(entity, entity.Info.LatestCardId, gameState.GetTurnNumber());
			}
		}

		private void CardTypeChange(IHsGameState gameState, int id, IGame game, int value)
		{
			switch (value)
			{
				case (int)CardType.HERO:
					SetHeroAsync(id, game, gameState);
					break;
				case (int)CardType.MINION:
					MinionRevealed(id, game, gameState);
					break;
			}
		}

		private void PlaystateChange(IHsGameState gameState, int id, IGame game, int value)
		{
			if(value == (int)CONCEDED)
				gameState.GameHandler?.HandleConcede();
			if(gameState.GameEnded)
				return;
			if(!game.Entities.TryGetValue(id, out var entity) || !entity.IsPlayer)
				return;
			switch((PlayState)value)
			{
				case WON:
					gameState.GameHandler?.HandleWin();
					break;
				case LOST:
					gameState.GameHandler?.HandleLoss();
					break;
				case TIED:
					gameState.GameHandler?.HandleTied();
					break;
			}
		}

		private void ZoneChange(IHsGameState gameState, int id, IGame game, int value, int prevValue)
		{
			if(id <= 3)
				return;
			if(!game.Entities.TryGetValue(id, out var entity))
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
					ZoneChangeFromDeck(gameState, id, game, value, prevValue, controller, entity.Info.LatestCardId);
					break;
				case HAND:
					ZoneChangeFromHand(gameState, id, game, value, prevValue, controller, entity.Info.LatestCardId);
					break;
				case PLAY:
					ZoneChangeFromPlay(gameState, id, game, value, prevValue, controller, entity.Info.LatestCardId);
					break;
				case Zone.SECRET:
					ZoneChangeFromSecret(gameState, id, game, value, prevValue, controller, entity.Info.LatestCardId);
					break;
				case Zone.INVALID:
					if(!game.SetupDone && (Zone)value == GRAVEYARD)
					{
						// Souleater's Scythe causes entites to be created in the graveyard.
						// We need to not reveal this card for the opponent and only reveal
						// it for the player after mulligan.
						entity.Info.InGraveardAtStartOfGame = true;
					}

					var maxId = GetMaxHeroPowerId(game);
					if(!game.SetupDone && (id <= maxId || game.GameEntity?.GetTag(STEP) == (int)Step.INVALID && entity.GetTag(ZONE_POSITION) < 5))
					{
						entity.Info.OriginalZone = DECK;
						SimulateZoneChangesFromDeck(gameState, id, game, value, entity.Info.LatestCardId, maxId);
					}
					else
						ZoneChangeFromOther(gameState, id, game, value, prevValue, controller, entity.Info.LatestCardId);
					break;
				case SETASIDE:
					if((Zone)value == PLAY && controller == game.Opponent.Id && game.CurrentGameMode == GameMode.Battlegrounds)
					{
						var copiedFrom = entity.GetTag(COPIED_FROM_ENTITY_ID);
						if(copiedFrom > 0 && game.Entities.TryGetValue(copiedFrom, out var source) && source.IsInHand && !source.HasCardId)
						{
							if(game.CurrentGameStats != null)
								BobsBuddyInvoker.GetInstance(game.CurrentGameStats.GameId, game.GetTurnNumber())?.UpdateOpponentHand(source, entity);
						}
					}
					ZoneChangeFromOther(gameState, id, game, value, prevValue, controller, entity.Info.LatestCardId);
					break;
				case GRAVEYARD:
				case REMOVEDFROMGAME:
					ZoneChangeFromOther(gameState, id, game, value, prevValue, controller, entity.Info.LatestCardId);
					break;
			}


			if((Zone)value == PLAY)
			{
				if(game.Entities.TryGetValue(id, out var e) && (e?.IsMinion ?? false))
				{
					gameState.MinionsInPlay.Add(e.CardId ?? "");
				}
			}
		}

		// The last heropower is created after the last hero, therefore +1
		private int GetMaxHeroPowerId(IGame game) =>
			Math.Max(game.PlayerEntity?.GetTag(HERO_ENTITY) ?? 66, game.OpponentEntity?.GetTag(HERO_ENTITY) ?? 66) + 1;

		private void SimulateZoneChangesFromDeck(IHsGameState gameState, int id, IGame game, int value, string? cardId, int maxId)
		{
			if(value == (int)DECK)
				return;
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			if(value == (int)SETASIDE)
			{
				entity.Info.Created = true;
				return;
			}
			if(entity.IsHero && !entity.IsPlayableHero || entity.IsHeroPower || entity.HasTag(PLAYER_ID) || entity.GetTag(CARDTYPE) == (int)CardType.GAME
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

		private void ZoneChangeFromOther(IHsGameState gameState, int id, IGame game, int value, int prevValue, int controller, string? cardId)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			var currentBlockCardId = gameState.CurrentBlock?.CardId ?? "";
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
					if(controller == game.Player.Id && cardId != null)
						gameState.GameHandler?.HandlePlayerCreateInPlay(entity, cardId, gameState.GetTurnNumber());
					if(controller == game.Opponent.Id)
						gameState.GameHandler?.HandleOpponentCreateInPlay(entity, cardId, gameState.GetTurnNumber());
					break;
				case DECK:
					if(controller == game.Player.Id && cardId != null)
					{
						if(gameState.CurrentBlock?.CardId == Collectible.Neutral.Overplanner)
						{
							var newIndex = ++gameState.DredgeCounter;
							entity.Info.DeckIndex = newIndex;
						}

						if(gameState.JoustReveals > 0)
							break;
						gameState.GameHandler?.HandlePlayerGetToDeck(entity, cardId, gameState.GetTurnNumber());
					}
					if(controller == game.Opponent.Id)
					{
						if(gameState.JoustReveals > 0)
							break;
						gameState.GameHandler?.HandleOpponentGetToDeck(entity, gameState.GetTurnNumber());
					}
					break;
				case HAND:
					if(controller == game.Player.Id && cardId != null)
						gameState.GameHandler?.HandlePlayerGet(entity, cardId, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
						gameState.GameHandler?.HandleOpponentGet(entity, gameState.GetTurnNumber(), id);
					break;
				case Zone.SECRET:
					if(controller == game.Player.Id && cardId != null)
						gameState.GameHandler?.HandlePlayerSecretPlayed(entity, cardId, gameState.GetTurnNumber(), (Zone)prevValue, currentBlockCardId);
					else if(controller == game.Opponent.Id)
						gameState.GameHandler?.HandleOpponentSecretPlayed(entity, cardId, -1, gameState.GetTurnNumber(), (Zone)prevValue, id);
					break;
				case SETASIDE:
					if(controller == game.Player.Id)
						gameState.GameHandler?.HandlePlayerCreateInSetAside(entity, gameState.GetTurnNumber());
					if(controller == game.Opponent.Id)
					{
						gameState.GameHandler?.HandleOpponentCreateInSetAside(entity, gameState.GetTurnNumber());
						if(gameState.CurrentBlock?.CardId == Collectible.Neutral.GrandArchivist
							&& gameState.CurrentBlock.EntityDiscardedByArchivist != null)
							gameState.CurrentBlock.EntityDiscardedByArchivist.CardId = entity.Info.LatestCardId;
					}
					break;
			}
		}

		private void ZoneChangeFromSecret(IHsGameState gameState, int id, IGame game, int value, int prevValue, int controller, string? cardId)
		{
			switch((Zone)value)
			{
				case Zone.SECRET:
				case GRAVEYARD:
					if(controller == game.Opponent.Id)
					{
						if(!game.Entities.TryGetValue(id, out var entity))
							return;

						game.SecretsManager.RemoveSecret(entity);
						Core.UpdateOpponentCards();
					}
					break;
				case Zone.SETASIDE:
					if(controller == game.Opponent.Id)
					{
						if(!game.Entities.TryGetValue(id, out var entity))
							return;
						gameState.GameHandler?.HandleOpponentSecretRemove(entity, cardId, gameState.GetTurnNumber());
					}
					break;
			}
		}

		private void ZoneChangeFromPlay(IHsGameState gameState, int id, IGame game, int value, int prevValue, int controller, string? cardId)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			switch((Zone)value)
			{
				case HAND:
					if(controller == game.Player.Id && cardId != null)
						gameState.GameHandler?.HandlePlayerBackToHand(entity, cardId, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
						gameState.GameHandler?.HandleOpponentPlayToHand(entity, cardId, gameState.GetTurnNumber(), id);
					break;
				case DECK:
					if(controller == game.Player.Id && cardId != null)
						gameState.GameHandler?.HandlePlayerPlayToDeck(entity, cardId, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
						gameState.GameHandler?.HandleOpponentPlayToDeck(entity, cardId, gameState.GetTurnNumber());
					break;
				case GRAVEYARD:
					if(controller == game.Player.Id && cardId != null)
						gameState.GameHandler?.HandlePlayerPlayToGraveyard(entity, cardId, gameState.GetTurnNumber(), game.PlayerEntity?.IsCurrentPlayer ?? false);
					else if(controller == game.Opponent.Id)
						gameState.GameHandler?.HandleOpponentPlayToGraveyard(entity, cardId, gameState.GetTurnNumber(), game.PlayerEntity?.IsCurrentPlayer ?? false);
					break;
				case REMOVEDFROMGAME:
				case SETASIDE:
					if(controller == game.Player.Id)
						gameState.GameHandler?.HandlePlayerRemoveFromPlay(entity, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
						gameState.GameHandler?.HandleOpponentRemoveFromPlay(entity, gameState.GetTurnNumber());
					break;
				case PLAY:
					break;
			}

			if((Zone)value != PLAY)
			{
				if(game.Entities.TryGetValue(id, out var e) && (e?.IsMinion ?? false))
				{
					gameState.MinionsInPlay.Remove(e.CardId ?? "");
				}
			}
		}

		private void ZoneChangeFromHand(IHsGameState gameState, int id, IGame game, int value, int prevValue, int controller, string? cardId)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;
			var currentBlockCardId = gameState.CurrentBlock?.CardId ?? "";
			// When a card is moved from hand it is not relevant if it was mulliganed.
			// If not cleared, we may display mulliganed mark to cards if they return to hand.
			entity.Info.Mulliganed = false;
			switch((Zone)value)
			{
				// cards can go from hand to play zone for reasons that are not playing them
				// e.g. Dirty Rat, Summon when Drawn
				case PLAY when gameState.CurrentBlock?.Type == "PLAY":
					gameState.LastCardPlayed = id;
					if(controller == game.Player.Id)
					{
						if(cardId != null)
							gameState.GameHandler?.HandlePlayerPlay(entity, cardId, gameState.GetTurnNumber(), currentBlockCardId);
						var magnetic = false;
						if(entity.IsMinion)
						{
							if(entity.HasTag(MODULAR) && (game.PlayerEntity?.IsCurrentPlayer ?? false))
							{
								var pos = entity.GetTag(ZONE_POSITION);
								var neighbour = game.Player?.Board.FirstOrDefault(x => x.GetTag(ZONE_POSITION) == pos + 1);
								magnetic = neighbour?.Card?.RaceEnum == Race.MECHANICAL;
							}
							if(!magnetic)
								gameState.GameHandler?.HandlePlayerMinionPlayed(entity);
						}
					}
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler?.HandleOpponentPlay(entity, cardId, entity.GetTag(ZONE_POSITION),
																 gameState.GetTurnNumber());
					}
					break;
				case PLAY when gameState.CurrentBlock?.Type != "PLAY":
					if(controller == game.Player.Id)
						gameState.GameHandler?.HandlePlayerHandToPlay(entity, cardId, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
						gameState.GameHandler?.HandleOpponentHandToPlay(entity, cardId, gameState.GetTurnNumber());
					break;
				case REMOVEDFROMGAME:
				case SETASIDE:
				case GRAVEYARD:
					if(controller == game.Player.Id && cardId != null)
						gameState.GameHandler?.HandlePlayerHandDiscard(entity, cardId, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler?.HandleOpponentHandDiscard(entity, cardId, entity.GetTag(ZONE_POSITION),
																		gameState.GetTurnNumber());
					}
					break;
				case Zone.SECRET:
					if(controller == game.Player.Id && cardId != null)
							gameState.GameHandler?.HandlePlayerSecretPlayed(entity, cardId, gameState.GetTurnNumber(), (Zone)prevValue, currentBlockCardId);
					else if(controller == game.Opponent.Id)
					{
						gameState.GameHandler?.HandleOpponentSecretPlayed(entity, cardId, entity.GetTag(ZONE_POSITION),
																		 gameState.GetTurnNumber(), (Zone)prevValue, id);
					}
					break;
				case DECK:
					if(controller == game.Player.Id && cardId != null)
					{
						if(game.PlayerEntity != null && game.PlayerEntity.GetTag(MULLIGAN_STATE) == (int)Mulligan.DONE)
						{
							gameState.GameHandler?.HandlePlayerHandToDeck(entity, cardId, gameState);
						}
						else
						{
							gameState.GameHandler?.HandlePlayerMulligan(entity, cardId);
						}
					}
					else if(controller == game.Opponent.Id)
					{
						if(!string.IsNullOrEmpty(cardId))
							gameState.GameHandler?.HandleOpponentHandToDeck(entity, cardId, gameState);
						if(game.OpponentEntity != null && game.OpponentEntity.GetTag(MULLIGAN_STATE) == (int)Mulligan.DEALING)
							gameState.GameHandler?.HandleOpponentMulligan(entity, entity.GetTag(ZONE_POSITION));
					}
					break;
			}
		}

		private void ZoneChangeFromDeck(IHsGameState gameState, int id, IGame game, int value, int prevValue, int controller, string? cardId)
		{
			if(!game.Entities.TryGetValue(id, out var entity))
				return;

			entity.Info.DeckIndex = 0;

			var currentBlockCardId = gameState.CurrentBlock?.CardId ?? "";
			switch((Zone)value)
			{
				case HAND:
					if(cardId == NonCollectible.Deathknight.DistressedKvaldir_FrostPlagueToken ||
						cardId == NonCollectible.Deathknight.DistressedKvaldir_BloodPlagueToken ||
						cardId == NonCollectible.Deathknight.DistressedKvaldir_UnholyPlagueToken)
					{
						gameState.LastPlagueDrawn.Push(cardId!);
					}
					if(controller == game.Player.Id && cardId != null)
						gameState.GameHandler?.HandlePlayerDraw(entity, cardId, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
					{
						var drawerCardId = gameState.CurrentBlock?.CardId ?? "";
						int? drawerId = null;
						if(drawerCardId != "" && (gameState.CurrentBlock?.Parent == null || !gameState.CurrentBlock.Parent.IsTradeableAction))
						{
							drawerId = game.Entities.FirstOrDefault(x => x.Value.CardId == drawerCardId).Value?.Id;
						}
						gameState.GameHandler?.HandleOpponentDraw(entity, gameState.GetTurnNumber(), cardId, drawerId);
					}
					break;
				case SETASIDE:
				case REMOVEDFROMGAME:
					if(!game.SetupDone)
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
						gameState.GameHandler?.HandlePlayerRemoveFromDeck(entity, gameState.GetTurnNumber());
					}
					else if(controller == game.Opponent.Id)
					{
						if(gameState.JoustReveals > 0)
						{
							gameState.JoustReveals--;
							break;
						}
						if (gameState.CurrentBlock?.CardId == Collectible.Neutral.GrandArchivist)
						{
							gameState.CurrentBlock.EntityDiscardedByArchivist = entity;
						}
						gameState.GameHandler?.HandleOpponentRemoveFromDeck(entity, gameState.GetTurnNumber());
					}
					break;
				case GRAVEYARD:
					var parentId = gameState.CurrentBlock?.CardId;

					if(parentId != null)
					{
						if(parentId == ClassicTrackingCardId)
						{
							entity.Info.Hidden = true;
							entity.SetTag(GameTag.ZONE, (int)Zone.DECK);
						}
					}

					if(controller == game.Player.Id && cardId != null)
						gameState.GameHandler?.HandlePlayerDeckDiscard(entity, cardId, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
						gameState.GameHandler?.HandleOpponentDeckDiscard(entity, cardId, gameState.GetTurnNumber());
					break;
				case PLAY:
					if(controller == game.Player.Id)
						gameState.GameHandler?.HandlePlayerDeckToPlay(entity, cardId, gameState.GetTurnNumber());
					else if(controller == game.Opponent.Id)
						gameState.GameHandler?.HandleOpponentDeckToPlay(entity, cardId, gameState.GetTurnNumber());
					break;
				case Zone.SECRET:
					if(controller == game.Player.Id && cardId != null)
						gameState.GameHandler?.HandlePlayerSecretPlayed(entity, cardId, gameState.GetTurnNumber(), (Zone)prevValue, currentBlockCardId);
					else if(controller == game.Opponent.Id)
						gameState.GameHandler?.HandleOpponentSecretPlayed(entity, cardId, -1, gameState.GetTurnNumber(), (Zone)prevValue, id);
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
			if(string.IsNullOrEmpty(game.Player.OriginalClass) && id == game.PlayerEntity.GetTag(HERO_ENTITY))
			{
				if(!game.Entities.TryGetValue(id, out var entity))
					return;
				if(entity.CardId != entity.Info.LatestCardId) Log.Warn($"CardId Mismatch {entity.CardId} vs {entity.Info.LatestCardId}");
				gameState.GameHandler?.SetPlayerHero(entity.CardId);
				return;
			}
			if(game.OpponentEntity == null)
			{
				Log.Info("Waiting for OpponentEntity to exist");
				while(game.OpponentEntity == null)
					await Task.Delay(100);
				Log.Info("Found OpponentEntity");
			}
			if(string.IsNullOrEmpty(game.Opponent.OriginalClass) && id == game.OpponentEntity.GetTag(HERO_ENTITY))
			{
				if(!game.Entities.TryGetValue(id, out var entity))
					return;
				if(entity.CardId != entity.Info.LatestCardId) Log.Warn($"CardId Mismatch {entity.CardId} vs {entity.Info.LatestCardId}");
				gameState.GameHandler?.SetOpponentHero(entity.CardId);
			}
		}

		private void MinionRevealed(int id, IGame game, IHsGameState gameState)
		{
			if(game.Entities.TryGetValue(id, out var entity))
				game.SecretsManager.OnEntityRevealedAsMinion(entity);
		}

		private void OnImmolateStateChange(int id, int value, IGame game)
		{
			if(value == 4 && game.Entities.TryGetValue(id, out var entity) && entity.CardId != NonCollectible.Neutral.TheCoinCore)
				entity.ClearCardId();
		}

		private void OnNextOpponentPlayerId(int entityId, int playerId, IGame game)
		{
			if(entityId != game.PlayerEntity?.Id)
				return;
			OpponentDeadForTracker.SetNextOpponentPlayerId(playerId, game);
		}
	}
}
