﻿using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Hearthstone.Secrets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HunterSecrets = Hearthstone_Deck_Tracker.Hearthstone.CardIds.Secrets.Hunter;
using MageSecrets = Hearthstone_Deck_Tracker.Hearthstone.CardIds.Secrets.Mage;
using PaladinSecrets = Hearthstone_Deck_Tracker.Hearthstone.CardIds.Secrets.Paladin;
using RogueSecrets = Hearthstone_Deck_Tracker.Hearthstone.CardIds.Secrets.Rogue;

namespace HDTTests.Hearthstone.Secrets
{
	[TestClass]
	public class SecretEventHandlerTest
	{
		private int _entityId;
		private GameV2 _game;
		private GameEventHandler _gameEventHandler;

		private Entity _gameEntity,
			_heroPlayer,
			_heroOpponent,
			_playerSpell1,
			_playerSpell2,
			_playerMinion1,
			_playerMinion2,
			_opponentMinion1,
			_opponentMinion2,
			_opponentDivineShieldMinion,
			_secretHunter1,
			_secretHunter2,
			_secretMage1,
			_secretMage2,
			_secretMage3,
			_secretPaladin1,
			_secretPaladin2,
			_secretRogue1,
			_secretRogue2,
			_opponentEntity,
			_opponentCardInHand1;

		private Entity CreateNewEntity(string cardId)
		{
			return new Entity(_entityId++) { CardId = cardId };
		}

		[TestInitialize]
		public void Setup()
		{
			Core.Game = null;
			_game = new GameV2();
			Core.Game = _game;
			_gameEventHandler = new GameEventHandler(_game);

			//Player_IDs are currently not quite representative of an actual game
			_gameEntity = CreateNewEntity(null);
			_gameEntity.Name = "GameEntity";
			_heroPlayer = CreateNewEntity("HERO_01");
			_heroPlayer.SetTag(GameTag.CARDTYPE, (int)CardType.HERO);
			_heroPlayer.SetTag(GameTag.CONTROLLER, _heroPlayer.Id);
			_heroPlayer.SetTag(GameTag.MULLIGAN_STATE, (int)Mulligan.DONE);
			_heroPlayer.SetTag(GameTag.PLAYER_ID, _heroPlayer.Id);
			_heroOpponent = CreateNewEntity("HERO_02");
			_heroOpponent.SetTag(GameTag.CARDTYPE, (int)CardType.HERO);
			_heroOpponent.SetTag(GameTag.CONTROLLER, _heroOpponent.Id);
			_opponentEntity = CreateNewEntity("");
			_opponentEntity.SetTag(GameTag.PLAYER_ID, _heroOpponent.Id);
			_opponentEntity.SetTag(GameTag.MULLIGAN_STATE, (int)Mulligan.DONE);

			_game.Entities.Add(0, _gameEntity);
			_game.Entities.Add(1, _heroPlayer);
			_game.Player.Id = _heroPlayer.Id;
			_game.Entities.Add(2, _heroOpponent);
			_game.Opponent.Id = _heroOpponent.Id;
			_game.Entities.Add(3, _opponentEntity);

			_playerMinion1 = CreateNewEntity("EX1_010");
			_playerMinion1.SetTag(GameTag.CARDTYPE, (int)CardType.MINION);
			_playerMinion1.SetTag(GameTag.CONTROLLER, _heroPlayer.Id);
			_playerMinion2 = CreateNewEntity("EX1_011");
			_playerMinion2.SetTag(GameTag.CARDTYPE, (int)CardType.MINION);
			_playerMinion2.SetTag(GameTag.CONTROLLER, _heroPlayer.Id);
			_opponentMinion1 = CreateNewEntity("EX1_020");
			_opponentMinion1.SetTag(GameTag.CARDTYPE, (int)CardType.MINION);
			_opponentMinion1.SetTag(GameTag.CONTROLLER, _heroOpponent.Id);
			_opponentMinion2 = CreateNewEntity("EX1_021");
			_opponentMinion2.SetTag(GameTag.CARDTYPE, (int)CardType.MINION);
			_opponentMinion2.SetTag(GameTag.CONTROLLER, _heroOpponent.Id);
			_opponentDivineShieldMinion = CreateNewEntity("EX1_008");
			_opponentDivineShieldMinion.SetTag(GameTag.CARDTYPE, (int)CardType.MINION);
			_opponentDivineShieldMinion.SetTag(GameTag.CONTROLLER, _heroOpponent.Id);
			_opponentDivineShieldMinion.SetTag(GameTag.DIVINE_SHIELD, 1);
			_playerSpell1 = CreateNewEntity("CS2_029");
			_playerSpell1.SetTag(GameTag.CARDTYPE, (int)CardType.SPELL);
			_playerSpell1.SetTag(GameTag.CARD_TARGET, _opponentMinion1.Id);
			_playerSpell1.SetTag(GameTag.CONTROLLER, _heroPlayer.Id);
			_playerSpell2 = CreateNewEntity("CS2_025");
			_playerSpell2.SetTag(GameTag.CARDTYPE, (int)CardType.SPELL);
			_playerSpell2.SetTag(GameTag.CONTROLLER, _heroPlayer.Id);

			_game.Entities.Add(4, _playerMinion1);
			_game.Entities.Add(5, _playerMinion2);
			_game.Entities.Add(6, _opponentMinion1);
			_game.Entities.Add(7, _opponentMinion2);

			_opponentCardInHand1 = CreateNewEntity("");
			_opponentCardInHand1.SetTag(GameTag.CONTROLLER, _heroOpponent.Id);
			_opponentCardInHand1.SetTag(GameTag.ZONE, (int)Zone.HAND);
			_game.Entities.Add(_opponentCardInHand1.Id, _opponentCardInHand1);

			_secretHunter1 = CreateNewEntity("");
			_secretHunter1.SetTag(GameTag.CLASS, (int)CardClass.HUNTER);
			_secretHunter1.SetTag(GameTag.SECRET, 1);
			_secretHunter1.SetTag(GameTag.ZONE, (int)Zone.SECRET);
			_secretHunter2 = CreateNewEntity("");
			_secretHunter2.SetTag(GameTag.CLASS, (int)CardClass.HUNTER);
			_secretHunter2.SetTag(GameTag.SECRET, 1);
			_secretMage1 = CreateNewEntity("");
			_secretMage1.SetTag(GameTag.CLASS, (int)CardClass.MAGE);
			_secretMage1.SetTag(GameTag.SECRET, 1);
			_secretMage1.SetTag(GameTag.ZONE, (int)Zone.SECRET);
			_secretMage2 = CreateNewEntity("");
			_secretMage2.SetTag(GameTag.CLASS, (int)CardClass.MAGE);
			_secretMage2.SetTag(GameTag.SECRET, 1);
			_secretMage3 = CreateNewEntity("");
			_secretMage3.SetTag(GameTag.CLASS, (int)CardClass.MAGE);
			_secretMage3.SetTag(GameTag.SECRET, 1);
			_secretPaladin1 = CreateNewEntity("");
			_secretPaladin1.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);
			_secretPaladin1.SetTag(GameTag.SECRET, 1);
			_secretPaladin1.SetTag(GameTag.ZONE, (int)Zone.SECRET);
			_secretPaladin2 = CreateNewEntity("");
			_secretPaladin2.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);
			_secretPaladin2.SetTag(GameTag.SECRET, 1);
			_secretRogue1 = CreateNewEntity("");
			_secretRogue1.SetTag(GameTag.CLASS, (int)CardClass.ROGUE);
			_secretRogue1.SetTag(GameTag.SECRET, 1);
			_secretRogue1.SetTag(GameTag.ZONE, (int)Zone.SECRET);
			_secretRogue2 = CreateNewEntity("");
			_secretRogue2.SetTag(GameTag.CLASS, (int)CardClass.ROGUE);
			_secretRogue2.SetTag(GameTag.SECRET, 1);

			_gameEventHandler.HandleOpponentSecretPlayed(_secretHunter1, "", 0, 0, Zone.HAND, _secretHunter1.Id);
			_game.Entities[_secretHunter1.Id] = _secretHunter1;

			_gameEventHandler.HandleOpponentSecretPlayed(_secretMage1, "", 0, 0, Zone.HAND, _secretMage1.Id);
			_game.Entities[_secretMage1.Id] = _secretMage1;

			_gameEventHandler.HandleOpponentSecretPlayed(_secretPaladin1, "", 0, 0, Zone.HAND, _secretPaladin1.Id);
			_game.Entities[_secretPaladin1.Id] = _secretPaladin1;

			_gameEventHandler.HandleOpponentSecretPlayed(_secretRogue1, "", 0, 0, Zone.HAND, _secretRogue1.Id);
			_game.Entities[_secretRogue1.Id] = _secretRogue1;

		}


		[TestMethod]
		public void SingleSecret_HeroToHero_PlayerAttackTest()
		{
			_playerMinion1.SetTag(GameTag.ZONE, (int)Zone.HAND);
			_heroPlayer.SetTag(GameTag.HEALTH, Database.GetCardFromId(_heroPlayer.CardId).Health);
			_game.SecretsManager.HandleAttack(_heroPlayer, _heroOpponent);
			VerifySecrets(0, HunterSecrets.All, HunterSecrets.BearTrap, HunterSecrets.ExplosiveTrap, HunterSecrets.WanderingMonster);
			VerifySecrets(1, MageSecrets.All, MageSecrets.IceBarrier);
			VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.NobleSacrifice);
			VerifySecrets(3, RogueSecrets.All);

			_playerMinion1.SetTag(GameTag.ZONE, (int)Zone.PLAY);
			_playerMinion2.SetTag(GameTag.ZONE, (int)Zone.PLAY);
			_game.SecretsManager.HandleAttack(_heroPlayer, _heroOpponent);
			VerifySecrets(0, HunterSecrets.All, HunterSecrets.BearTrap, HunterSecrets.ExplosiveTrap,
				HunterSecrets.Misdirection, HunterSecrets.WanderingMonster);
			VerifySecrets(1, MageSecrets.All, MageSecrets.IceBarrier);
			VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.NobleSacrifice);
			VerifySecrets(3, RogueSecrets.All);
		}

		[TestMethod]
		public void SingleSecret_MinionToHero_PlayerAttackTest()
		{
			_playerMinion1.SetTag(GameTag.ZONE, (int)Zone.PLAY);
			_playerMinion1.SetTag(GameTag.HEALTH, Database.GetCardFromId(_playerMinion1.CardId).Health);
			_game.SecretsManager.HandleAttack(_playerMinion1, _heroOpponent);
			VerifySecrets(0, HunterSecrets.All, HunterSecrets.BearTrap, HunterSecrets.ExplosiveTrap,
				HunterSecrets.FreezingTrap, HunterSecrets.WanderingMonster);
			VerifySecrets(1, MageSecrets.All, MageSecrets.IceBarrier, MageSecrets.Vaporize, MageSecrets.FlameWard);
			VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.NobleSacrifice);
			VerifySecrets(3, RogueSecrets.All, RogueSecrets.ShadowClone);

			_playerMinion2.SetTag(GameTag.ZONE, (int)Zone.PLAY);
			_game.SecretsManager.HandleAttack(_playerMinion1, _heroOpponent);
			VerifySecrets(0, HunterSecrets.All, HunterSecrets.BearTrap, HunterSecrets.ExplosiveTrap,
				HunterSecrets.FreezingTrap, HunterSecrets.Misdirection, HunterSecrets.WanderingMonster);
			VerifySecrets(1, MageSecrets.All, MageSecrets.IceBarrier, MageSecrets.Vaporize, MageSecrets.FlameWard);
			VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.NobleSacrifice);
			VerifySecrets(3, RogueSecrets.All, RogueSecrets.SuddenBetrayal, RogueSecrets.ShadowClone);
		}

		[TestMethod]
		public void SingleSecret_HeroToMinion_PlayerAttackTest()
		{
			_game.SecretsManager.HandleAttack(_heroPlayer, _opponentMinion1);
			VerifySecrets(0, HunterSecrets.All, HunterSecrets.SnakeTrap, HunterSecrets.VenomstrikeTrap, HunterSecrets.PackTactics);
			VerifySecrets(1, MageSecrets.All, MageSecrets.SplittingImage);
			VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.NobleSacrifice, PaladinSecrets.AutodefenseMatrix);
			VerifySecrets(3, RogueSecrets.All, RogueSecrets.Bamboozle);
		}

		[TestMethod]
		public void SingleSecret_MinionToMinion_PlayerAttackTest()
		{
			_game.SecretsManager.HandleAttack(_playerMinion1, _opponentMinion1);
			VerifySecrets(0, HunterSecrets.All, HunterSecrets.FreezingTrap, HunterSecrets.SnakeTrap,
				HunterSecrets.VenomstrikeTrap, HunterSecrets.PackTactics);
			VerifySecrets(1, MageSecrets.All, MageSecrets.SplittingImage);
			VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.NobleSacrifice, PaladinSecrets.AutodefenseMatrix);
			VerifySecrets(3, RogueSecrets.All, RogueSecrets.Bamboozle);
		}

		[TestMethod]
		public void SingleSecret_MinionToDivineShieldMinion_PlayerAttackTest()
		{
			_game.SecretsManager.HandleAttack(_playerMinion1, _opponentDivineShieldMinion);
			VerifySecrets(0, HunterSecrets.All, HunterSecrets.FreezingTrap, HunterSecrets.SnakeTrap,
				HunterSecrets.VenomstrikeTrap, HunterSecrets.PackTactics);
			VerifySecrets(1, MageSecrets.All, MageSecrets.SplittingImage);
			VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.NobleSacrifice);
			VerifySecrets(3, RogueSecrets.All, RogueSecrets.Bamboozle);
		}

		[TestMethod]
		public void SingleSecret_OnlyMinionDied()
		{
			_opponentMinion2.SetTag(GameTag.ZONE, (int)Zone.HAND);
			_gameEventHandler.HandleOpponentMinionDeath(_opponentMinion1, 2);
			VerifySecrets(0, HunterSecrets.All);
			VerifySecrets(1, MageSecrets.All, MageSecrets.Duplicate, MageSecrets.Effigy);
			VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.Redemption, PaladinSecrets.GetawayKodo);
			VerifySecrets(3, RogueSecrets.All, RogueSecrets.CheatDeath);
		}

		[TestMethod]
		public void SingleSecret_OneMinionDied()
		{
			_opponentMinion2.SetTag(GameTag.ZONE, (int)Zone.PLAY);
			_gameEventHandler.HandleOpponentMinionDeath(_opponentMinion1, 2);
			_game.GameTime.Time += TimeSpan.FromSeconds(1);
			VerifySecrets(0, HunterSecrets.All);
			VerifySecrets(1, MageSecrets.All, MageSecrets.Duplicate, MageSecrets.Effigy);
			VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.Avenge, PaladinSecrets.Redemption, PaladinSecrets.GetawayKodo);
			VerifySecrets(3, RogueSecrets.All, RogueSecrets.CheatDeath);
		}

		[TestMethod]
		public void SingleSecret_MinionPlayed()
		{
			_gameEventHandler.HandlePlayerMinionPlayed(_playerMinion1);
			VerifySecrets(0, HunterSecrets.All, HunterSecrets.Snipe);
			VerifySecrets(1, MageSecrets.All, MageSecrets.ExplosiveRunes, MageSecrets.MirrorEntity, MageSecrets.PotionOfPolymorph, MageSecrets.FrozenClone);
			VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.Repentance);
			VerifySecrets(3, RogueSecrets.All, RogueSecrets.Ambush);
		}

		[TestMethod]
		public void SingleSecret_DormantMinionPlayed()
		{
			_playerMinion1.SetTag(GameTag.DORMANT, 1);
			_gameEventHandler.HandlePlayerMinionPlayed(_playerMinion1);
			VerifySecrets(0, HunterSecrets.All);
			VerifySecrets(1, MageSecrets.All, MageSecrets.MirrorEntity, MageSecrets.FrozenClone);
			VerifySecrets(2, PaladinSecrets.All);
			VerifySecrets(3, RogueSecrets.All, RogueSecrets.Ambush);
		}

		[TestMethod]
		public void SingleSecret_OpponentDamage()
		{
			_gameEventHandler.HandleOpponentDamage(_heroOpponent);
			VerifySecrets(0, HunterSecrets.All);
			VerifySecrets(1, MageSecrets.All);
			VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.EyeForAnEye);
			VerifySecrets(3, RogueSecrets.All, RogueSecrets.Evasion);
		}

		[TestMethod]
		public void SingleSecret_MinionTarget_SpellPlayed()
		{
			_game.SecretsManager.HandleCardPlayed(_playerSpell1);
			_game.GameTime.Time += TimeSpan.FromSeconds(1);
			VerifySecrets(0, HunterSecrets.All, HunterSecrets.CatTrick);
			VerifySecrets(1, MageSecrets.All, MageSecrets.Counterspell, MageSecrets.Spellbender, MageSecrets.ManaBind, MageSecrets.NetherwindPortal);
			VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.OhMyYogg);
			VerifySecrets(3, RogueSecrets.All, RogueSecrets.DirtyTricks);
		}

		[TestMethod]
		public void SingleSecret_NoMinionTarget_SpellPlayed()
		{
			_game.SecretsManager.HandleCardPlayed(_playerSpell2);
			_game.GameTime.Time += TimeSpan.FromSeconds(1);
			VerifySecrets(0, HunterSecrets.All, HunterSecrets.CatTrick);
			VerifySecrets(1, MageSecrets.All, MageSecrets.Counterspell, MageSecrets.ManaBind, MageSecrets.NetherwindPortal);
			VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.OhMyYogg);
			VerifySecrets(3, RogueSecrets.All, RogueSecrets.DirtyTricks);
		}

		[TestMethod]
		public void SingleSecret_MinionOnBoard_NoMinionTarget_SpellPlayed()
		{
			_opponentMinion1.SetTag(GameTag.ZONE, (int)Zone.PLAY);
			_game.SecretsManager.HandleCardPlayed(_playerSpell2);
			_game.GameTime.Time += TimeSpan.FromSeconds(1);
			VerifySecrets(0, HunterSecrets.All, HunterSecrets.CatTrick);
			VerifySecrets(1, MageSecrets.All, MageSecrets.Counterspell, MageSecrets.ManaBind, MageSecrets.NetherwindPortal);
			VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.NeverSurrender, PaladinSecrets.OhMyYogg);
			VerifySecrets(3, RogueSecrets.All, RogueSecrets.DirtyTricks);
		}

		//[TestMethod]
		//public void SingleSecret_MinionInPlay_OpponentTurnStart()
		//{
		//	_opponentEntity.SetTag(GameTag.CURRENT_PLAYER, 1);
		//	_gameEventHandler.HandleTurnsInPlayChange(_opponentMinion1, 1);
		//	VerifySecrets(0, HunterSecrets.All);
		//	VerifySecrets(1, MageSecrets.All);
		//	VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.CompetitiveSpirit);
		//	VerifySecrets(3, RogueSecrets.All);
		//}

		//[TestMethod]
		//public void SingleSecret_NoMinionInPlay_OpponentTurnStart()
		//{
		//	_gameEventHandler.HandleTurnsInPlayChange(_heroOpponent, 1);
		//	VerifySecrets(0, HunterSecrets.All);
		//	VerifySecrets(1, MageSecrets.All);
		//	VerifySecrets(2, PaladinSecrets.All);
		//	VerifySecrets(3, RogueSecrets.All);
		//}

		[TestMethod]
		public void SingleSecret_Retarget_FriendlyHitsFriendly()
		{
			_game.SecretsManager.HandleAttack(_playerMinion1, _heroPlayer);
			VerifySecrets(0, HunterSecrets.All);
			VerifySecrets(1, MageSecrets.All);
			VerifySecrets(2, PaladinSecrets.All);
			VerifySecrets(3, RogueSecrets.All);

			// minions can't actually hit themselves, but this works for the
			// purposes of this test.
			_game.SecretsManager.HandleAttack(_playerMinion1, _playerMinion1);
			VerifySecrets(0, HunterSecrets.All);
			VerifySecrets(1, MageSecrets.All);
			VerifySecrets(2, PaladinSecrets.All);
			VerifySecrets(3, RogueSecrets.All);
		}

		[TestMethod]
		public void SingleSecret_OpponentAttack_Retarget_OpponentHitsOpponent()
		{
			_game.SecretsManager.HandleAttack(_opponentMinion1, _heroOpponent);
			VerifySecrets(0, HunterSecrets.All);
			VerifySecrets(1, MageSecrets.All);
			VerifySecrets(2, PaladinSecrets.All);
			VerifySecrets(3, RogueSecrets.All);

			_game.SecretsManager.HandleAttack(_opponentMinion1, _opponentMinion2);
			VerifySecrets(0, HunterSecrets.All);
			VerifySecrets(1, MageSecrets.All);
			VerifySecrets(2, PaladinSecrets.All);
			VerifySecrets(3, RogueSecrets.All);
		}

		[TestMethod]
		public void SingleSecret_PlayerPlaysMinion_OpponentPlaysMinion()
		{
			_game.SecretsManager.HandleMinionPlayed(_playerMinion1);
			_opponentCardInHand1.SetTag(GameTag.CARDTYPE, (int)CardType.MINION);
			_game.SecretsManager.OnEntityRevealedAsMinion(_opponentCardInHand1);

			VerifySecrets(0, HunterSecrets.All, HunterSecrets.HiddenCache, HunterSecrets.Snipe);
		}

		[TestMethod]
		public void SingleSecret_MinionToHero_PlayerImmune_PlayerAttackTest()
		{
			_playerMinion1.SetTag(GameTag.ZONE, (int)Zone.PLAY);
			_heroPlayer.SetTag(GameTag.IMMUNE, 1);
			_game.SecretsManager.HandleAttack(_playerMinion1, _heroOpponent);
			VerifySecrets(0, HunterSecrets.All, HunterSecrets.ExplosiveTrap, HunterSecrets.WanderingMonster);
		}

		[TestMethod]
		public void SingleSecret_HeroToHero_MinionImmune_PlayerAttackTest()
		{
			_playerMinion1.SetTag(GameTag.ZONE, (int)Zone.PLAY);
			_playerMinion1.SetTag(GameTag.IMMUNE, 1);
			_game.SecretsManager.HandleAttack(_heroPlayer, _heroOpponent);
			VerifySecrets(0, HunterSecrets.All, HunterSecrets.ExplosiveTrap, HunterSecrets.WanderingMonster);
		}

		[TestMethod]
		public void SingleSecret_PlayerTurnStart_OpponentPlayedCards_PlagerizeTriggered()
		{
			_game.Player.Play(_opponentMinion1, 1);
			_game.SecretsManager.HandleOpponentTurnStart();
			VerifySecrets(0, HunterSecrets.All);
			VerifySecrets(1, MageSecrets.All);
			VerifySecrets(2, PaladinSecrets.All);
			VerifySecrets(3, RogueSecrets.All, RogueSecrets.Plagiarize);
		}

		[TestMethod]
		public void SingleSecret_PlayerTurnStart_OpponentPlayedNoCards_PlagerizeNotTriggered()
		{
			_game.Player.Play(_opponentMinion1, 1);
			_game.Player.OnTurnStart();
			_game.SecretsManager.HandleOpponentTurnStart();
			VerifySecrets(0, HunterSecrets.All);
			VerifySecrets(1, MageSecrets.All);
			VerifySecrets(2, PaladinSecrets.All);
			VerifySecrets(3, RogueSecrets.All);
		}

		//[TestMethod]
		//public void SingleSecret_OpponentTurnStart_OpponentTookNoDamage_RiggedFaireGameTriggered()
		//{
		//	_game.Opponent.OnTurnStart();
		//	_game.SecretsManager.HandleOpponentTurnStart();
		//	VerifySecrets(0, HunterSecrets.All);
		//	VerifySecrets(1, MageSecrets.All, MageSecrets.RiggedFaireGame);
		//	VerifySecrets(2, PaladinSecrets.All);
		//	VerifySecrets(3, RogueSecrets.All);
		//}

		[TestMethod]
		public void MultipleSecrets_MinionToHero_ExplosiveTrapTriggered_MinionDied_PlayerAttackTest()
		{
			_playerMinion1.SetTag(GameTag.ZONE, (int)Zone.PLAY);
			_playerMinion1.SetTag(GameTag.HEALTH, -1);
			_game.SecretsManager.HandleAttack(_playerMinion1, _heroOpponent);
			VerifySecrets(0, HunterSecrets.All, HunterSecrets.ExplosiveTrap,
				HunterSecrets.WanderingMonster);
			VerifySecrets(1, MageSecrets.All, MageSecrets.IceBarrier);
			VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.NobleSacrifice);
			VerifySecrets(3, RogueSecrets.All);
		}

		[TestMethod]
		public void MultipleSecrets_MinionPlayed_MinionDied()
		{
			_gameEventHandler.HandlePlayerMinionPlayed(_playerMinion1);
			_gameEventHandler.HandlePlayerMinionDeath(_playerMinion1);
			VerifySecrets(0, HunterSecrets.All);
			VerifySecrets(1, MageSecrets.All, MageSecrets.FrozenClone);
			VerifySecrets(2, PaladinSecrets.All);
			VerifySecrets(3, RogueSecrets.All);
		}

		[TestMethod]
		public void MultipleSecrets_SpellPlayedCounterspellTriggered_SpellTriggeredSecretsNotExcludedAsync()
		{
			var mockegame = new MockGame();
			mockegame.GameTime = new GameTime();
			mockegame.OpponentSecretCount = 2;
			mockegame.SecretsManager = new SecretsManager(mockegame, null);
			mockegame.SecretsManager.Secrets.Add(new Secret(_secretHunter1));
			mockegame.SecretsManager.Secrets.Add(new Secret(_secretMage1));
			mockegame.SecretsManager.HandleCardPlayed(_playerSpell2);
			mockegame.SecretsManager.SecretTriggered(CreateNewEntity(MageSecrets.Counterspell));
			mockegame.GameTime.Time += TimeSpan.FromSeconds(1);
			Assert.IsTrue(mockegame.SecretsManager.Secrets[1].IsExcluded(MageSecrets.Counterspell));
			Assert.IsFalse(mockegame.SecretsManager.Secrets[1].IsExcluded(MageSecrets.NetherwindPortal));
		}

		//[TestMethod]
		//public void MultipleSecrets_MinionPlayed_SecretTriggered_MinionDied()
		//{
		//	_gameEventHandler.HandleOpponentSecretPlayed(_secretMage2, "", 0, 0, Zone.HAND, _secretMage2.Id);
		//	_secretMage2.CardId = MageSecrets.ExplosiveRunes;
		//	_gameEventHandler.HandlePlayerMinionPlayed(_playerMinion1);
		//	_gameEventHandler.HandleOpponentSecretTrigger(_secretMage2, "", 2, _secretMage1.Id);
		//	_gameEventHandler.HandlePlayerMinionDeath(_playerMinion1);
		//	VerifySecrets(0, HunterSecrets.All);
		//	VerifySecrets(1, MageSecrets.All, MageSecrets.ExplosiveRunes, MageSecrets.FrozenClone);
		//	VerifySecrets(2, PaladinSecrets.All);
		//	VerifySecrets(3, RogueSecrets.All);
		//}

		//[TestMethod]
		//public void MultipleSecrets_MinionPlayed_MultipleSecretTriggered_MinionDied()
		//{
		//	_gameEventHandler.HandleOpponentSecretPlayed(_secretMage2, "", 0, 0, Zone.HAND, _secretMage2.Id);
		//	_gameEventHandler.HandleOpponentSecretPlayed(_secretMage3, "", 0, 0, Zone.HAND, _secretMage3.Id);
		//	_secretMage2.CardId = MageSecrets.PotionOfPolymorph;
		//	_secretMage3.CardId = MageSecrets.ExplosiveRunes;
		//	_gameEventHandler.HandlePlayerMinionPlayed(_playerMinion1);
		//	_gameEventHandler.HandleOpponentSecretTrigger(_secretMage2, "", 2, _secretMage1.Id);
		//	_gameEventHandler.HandleOpponentSecretTrigger(_secretMage3, "", 2, _secretMage2.Id);
		//	_gameEventHandler.HandlePlayerMinionDeath(_playerMinion1);
		//	VerifySecrets(0, HunterSecrets.All);
		//	VerifySecrets(1, MageSecrets.All, MageSecrets.ExplosiveRunes, MageSecrets.FrozenClone, MageSecrets.PotionOfPolymorph);
		//	VerifySecrets(2, PaladinSecrets.All);
		//	VerifySecrets(3, RogueSecrets.All);
		//}

		//[TestMethod]
		//public void MultipleSecrets_MinionPlayed_MinionDiedNextTurn()
		//{
		//	_gameEventHandler.HandlePlayerMinionPlayed(_playerMinion1);
		//	_gameEventHandler.TurnStart(Hearthstone_Deck_Tracker.Enums.ActivePlayer.Player, 2);
		//	_gameEventHandler.HandlePlayerMinionDeath(_playerMinion1);
		//	VerifySecrets(0, HunterSecrets.All, HunterSecrets.Snipe);
		//	VerifySecrets(1, MageSecrets.All, MageSecrets.ExplosiveRunes, MageSecrets.MirrorEntity, MageSecrets.PotionOfPolymorph, MageSecrets.FrozenClone);
		//	VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.Repentance);
		//	VerifySecrets(3, RogueSecrets.All, RogueSecrets.Ambush);
		//}

		[TestMethod]
		public void MultipleSecrets_MinionPlayed_AnotherMinionDied()
		{
			_gameEventHandler.HandlePlayerMinionPlayed(_playerMinion1);
			_gameEventHandler.HandlePlayerMinionDeath(_playerMinion2);
			VerifySecrets(0, HunterSecrets.All, HunterSecrets.Snipe);
			VerifySecrets(1, MageSecrets.All, MageSecrets.ExplosiveRunes, MageSecrets.MirrorEntity, MageSecrets.PotionOfPolymorph, MageSecrets.FrozenClone);
			VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.Repentance);
			VerifySecrets(3, RogueSecrets.All, RogueSecrets.Ambush);
		}


		//[TestMethod]
		//public void MultipleSecrets_MinionToHero_VaporizeTriggered_PlayerAttackTest()
		//{
		//	_gameEventHandler.HandleOpponentSecretPlayed(_secretMage2, "", 0, 0, Zone.HAND, _secretMage2.Id);
		//	_secretMage2.CardId = MageSecrets.Vaporize;

		//	_playerMinion1.SetTag(GameTag.ZONE, (int)Zone.PLAY);
		//	_playerMinion1.SetTag(GameTag.HEALTH, Database.GetCardFromId(_playerMinion1.CardId).Health);
		//	_game.ProposedAttacker = _playerMinion1.Id;
		//	_game.ProposedDefender = _heroOpponent.Id;
		//	_gameEventHandler.HandleOpponentSecretTrigger(_secretMage2, "", 2, _secretMage2.Id);

		//	VerifySecrets(0, HunterSecrets.All, HunterSecrets.ExplosiveTrap, HunterSecrets.WanderingMonster);
		//	VerifySecrets(1, MageSecrets.All, MageSecrets.Vaporize, MageSecrets.IceBarrier);
		//	VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.NobleSacrifice);
		//	VerifySecrets(3, RogueSecrets.All);
		//}

		//[TestMethod]
		//public void MultipleSecrets_MinionToMinion_FreezingTrapTriggered_PlayerAttackTest()
		//{
		//	_gameEventHandler.HandleOpponentSecretPlayed(_secretHunter2, "", 0, 0, Zone.HAND, _secretHunter2.Id);
		//	_secretHunter2.CardId = HunterSecrets.FreezingTrap;

		//	_playerMinion1.SetTag(GameTag.ZONE, (int)Zone.PLAY);
		//	_playerMinion1.SetTag(GameTag.HEALTH, Database.GetCardFromId(_playerMinion1.CardId).Health);
		//	_opponentMinion1.SetTag(GameTag.ZONE, (int)Zone.PLAY);
		//	_opponentMinion1.SetTag(GameTag.HEALTH, Database.GetCardFromId(_opponentMinion1.CardId).Health);
		//	_game.ProposedAttacker = _playerMinion1.Id;
		//	_game.ProposedDefender = _opponentMinion1.Id;
		//	_gameEventHandler.HandleOpponentSecretTrigger(_secretHunter2, "", 2, _secretHunter2.Id);

		//	VerifySecrets(0, HunterSecrets.All, HunterSecrets.FreezingTrap, HunterSecrets.PackTactics, HunterSecrets.SnakeTrap, HunterSecrets.VenomstrikeTrap);
		//	VerifySecrets(1, MageSecrets.All, MageSecrets.SplittingImage);
		//	VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.AutodefenseMatrix, PaladinSecrets.NobleSacrifice);
		//	VerifySecrets(3, RogueSecrets.All, RogueSecrets.Bamboozle);
		//}

		private void VerifySecrets(int index, List<string> allSecrets, params string[] triggered)
		{
			var secrets = _game.SecretsManager.Secrets[index];
			foreach(var secret in allSecrets)
				Assert.AreEqual(triggered.Contains(secret), secrets.IsExcluded(secret), Database.GetCardFromId(secret).Name);
		}
	}
}
