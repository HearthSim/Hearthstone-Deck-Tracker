using System;
using System.Linq;
using System.Collections.Generic;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HunterSecrets = Hearthstone_Deck_Tracker.Hearthstone.CardIds.Secrets.Hunter;
using MageSecrets = Hearthstone_Deck_Tracker.Hearthstone.CardIds.Secrets.Mage;
using PaladinSecrets = Hearthstone_Deck_Tracker.Hearthstone.CardIds.Secrets.Paladin;

namespace HDTTests.Hearthstone
{
    [TestClass]
    public class SecretTests
    {
        private int _entityId;
        private GameV2 _game;
        private GameEventHandler _gameEventHandler;

        private Entity _heroPlayer,
            _heroOpponent,
            _playerSpell1,
            _playerSpell2,
            _playerMinion1,
            _opponentMinion1,
            _opponentMinion2,
            _secretHunter1,
            _secretHunter2,
            _secretMage1,
            _secretMage2,
            _secretPaladin1,
            _secretPaladin2;

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

            _heroPlayer = CreateNewEntity("HERO_01");
            _heroPlayer.SetTag(GameTag.CARDTYPE, (int)CardType.HERO);
            _heroOpponent = CreateNewEntity("HERO_02");
            _heroOpponent.SetTag(GameTag.CARDTYPE, (int) CardType.HERO);
            _heroOpponent.SetTag(GameTag.CONTROLLER, _heroOpponent.Id);

            _game.Entities.Add(0, _heroPlayer);
            _game.Player.Id = _heroPlayer.Id;
            _game.Entities.Add(1, _heroOpponent);
            _game.Opponent.Id = _heroOpponent.Id;

            _playerMinion1 = CreateNewEntity("EX1_010");
            _playerMinion1.SetTag(GameTag.CARDTYPE, (int)CardType.MINION);
            _playerMinion1.SetTag(GameTag.CONTROLLER, _heroPlayer.Id);
            _opponentMinion1 = CreateNewEntity("EX1_020");
            _opponentMinion1.SetTag(GameTag.CARDTYPE, (int)CardType.MINION);
            _opponentMinion1.SetTag(GameTag.CONTROLLER, _heroOpponent.Id);
            _opponentMinion2 = CreateNewEntity("EX1_021");
            _opponentMinion2.SetTag(GameTag.CARDTYPE, (int)CardType.MINION);
            _opponentMinion2.SetTag(GameTag.CONTROLLER, _heroOpponent.Id);
            _playerSpell1 = CreateNewEntity("CS2_029");
            _playerSpell1.SetTag(GameTag.CARDTYPE, (int)CardType.SPELL);
            _playerSpell1.SetTag(GameTag.CARD_TARGET, _opponentMinion1.Id);
            _playerSpell1.SetTag(GameTag.CONTROLLER, _heroPlayer.Id);
            _playerSpell2 = CreateNewEntity("CS2_025");
            _playerSpell2.SetTag(GameTag.CARDTYPE, (int)CardType.SPELL);
            _playerSpell2.SetTag(GameTag.CONTROLLER, _heroPlayer.Id);

            _game.Entities.Add(2, _playerMinion1);
            _game.Entities.Add(3, _opponentMinion1);
            _game.Entities.Add(4, _opponentMinion2);

            _secretHunter1 = CreateNewEntity("");
            _secretHunter1.SetTag(GameTag.CLASS, (int)CardClass.HUNTER);
            _secretHunter2 = CreateNewEntity("");
            _secretHunter2.SetTag(GameTag.CLASS, (int)CardClass.HUNTER);
            _secretMage1 = CreateNewEntity("");
            _secretMage1.SetTag(GameTag.CLASS, (int)CardClass.MAGE);
            _secretMage2 = CreateNewEntity("");
            _secretMage2.SetTag(GameTag.CLASS, (int)CardClass.MAGE);
            _secretPaladin1 = CreateNewEntity("");
            _secretPaladin1.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);
            _secretPaladin2 = CreateNewEntity("");
            _secretPaladin2.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);

            _gameEventHandler.HandleOpponentSecretPlayed(_secretHunter1, "", 0, 0, Zone.HAND, _secretHunter1.Id);
            _gameEventHandler.HandleOpponentSecretPlayed(_secretMage1, "", 0, 0, Zone.HAND, _secretMage1.Id);
            _gameEventHandler.HandleOpponentSecretPlayed(_secretPaladin1, "", 0, 0, Zone.HAND, _secretPaladin1.Id);
        }

        [TestMethod]
        public void SingleSecret_HeroToHero_PlayerAttackTest()
        {
            _playerMinion1.SetTag(GameTag.ZONE, (int)Zone.HAND);
            _game.OpponentSecrets.ZeroFromAttack(_heroPlayer, _heroOpponent);
            VerifySecrets(0, HunterSecrets.All, HunterSecrets.BearTrap, HunterSecrets.ExplosiveTrap);
            VerifySecrets(1, MageSecrets.All, MageSecrets.IceBarrier);
            VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.NobleSacrifice);

            _playerMinion1.SetTag(GameTag.ZONE, (int)Zone.PLAY);
            _game.OpponentSecrets.ZeroFromAttack(_heroPlayer, _heroOpponent);
            VerifySecrets(0, HunterSecrets.All, HunterSecrets.BearTrap, HunterSecrets.ExplosiveTrap,
                             HunterSecrets.Misdirection);
            VerifySecrets(1, MageSecrets.All, MageSecrets.IceBarrier);
            VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.NobleSacrifice);
        }

        [TestMethod]
        public void SingleSecret_MinionToHero_PlayerAttackTest()
        {
            _playerMinion1.SetTag(GameTag.ZONE, (int)Zone.PLAY);
            _game.OpponentSecrets.ZeroFromAttack(_playerMinion1, _heroOpponent);
            VerifySecrets(0, HunterSecrets.All, HunterSecrets.BearTrap, HunterSecrets.ExplosiveTrap,
                HunterSecrets.FreezingTrap, HunterSecrets.Misdirection);
            VerifySecrets(1, MageSecrets.All, MageSecrets.IceBarrier, MageSecrets.Vaporize);
            VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.NobleSacrifice);
        }

        [TestMethod]
        public void SingleSecret_HeroToMinion_PlayerAttackTest()
        {
            _game.OpponentSecrets.ZeroFromAttack(_heroPlayer, _opponentMinion1);
            VerifySecrets(0, HunterSecrets.All, HunterSecrets.SnakeTrap);
            VerifySecrets(1, MageSecrets.All);
            VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.NobleSacrifice);
        }

        [TestMethod]
        public void SingleSecret_MinionToMinion_PlayerAttackTest()
        {
            _game.OpponentSecrets.ZeroFromAttack(_playerMinion1, _opponentMinion1);
            VerifySecrets(0, HunterSecrets.All, HunterSecrets.FreezingTrap, HunterSecrets.SnakeTrap);
            VerifySecrets(1, MageSecrets.All);
            VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.NobleSacrifice);
        }

        [TestMethod]
        public void SingleSecret_OnlyMinionDied()
        {
            _opponentMinion2.SetTag(GameTag.ZONE, (int)Zone.HAND);
            _gameEventHandler.HandleOpponentMinionDeath(_opponentMinion1, 2);
            VerifySecrets(0, HunterSecrets.All);
            VerifySecrets(1, MageSecrets.All, MageSecrets.Duplicate, MageSecrets.Effigy);
            VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.Redemption);
        }

        [TestMethod]
        public void SingleSecret_OneMinionDied()
        {
            _opponentMinion2.SetTag(GameTag.ZONE, (int)Zone.PLAY);
            _gameEventHandler.HandleOpponentMinionDeath(_opponentMinion1, 2);
            _game.GameTime.Time += TimeSpan.FromSeconds(1);
            VerifySecrets(0, HunterSecrets.All);
            VerifySecrets(1, MageSecrets.All, MageSecrets.Duplicate, MageSecrets.Effigy);
            VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.Avenge, PaladinSecrets.Redemption);
        }

        [TestMethod]
        public void SingleSecret_MinionPlayed()
        {
            _gameEventHandler.HandlePlayerMinionPlayed();
            VerifySecrets(0, HunterSecrets.All, HunterSecrets.Snipe);
            VerifySecrets(1, MageSecrets.All, MageSecrets.MirrorEntity);
            VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.Repentance);
        }

        [TestMethod]
        public void SingleSecret_OpponentDamage()
        {
            _gameEventHandler.HandleOpponentDamage(_heroOpponent);
            VerifySecrets(0, HunterSecrets.All);
            VerifySecrets(1, MageSecrets.All);
            VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.EyeForAnEye);
        }

        [TestMethod]
        public void SingleSecret_MinionTarget_SpellPlayed()
        {
            _gameEventHandler.HandleSecretsOnPlay(_playerSpell1);
            _game.GameTime.Time += TimeSpan.FromSeconds(1);
            VerifySecrets(0, HunterSecrets.All);
            VerifySecrets(1, MageSecrets.All, MageSecrets.Counterspell, MageSecrets.Spellbender);
            VerifySecrets(2, PaladinSecrets.All);
        }

        [TestMethod]
        public void SingleSecret_NoMinionTarget_SpellPlayed()
        {
            _gameEventHandler.HandleSecretsOnPlay(_playerSpell2);
            _game.GameTime.Time += TimeSpan.FromSeconds(1);
            VerifySecrets(0, HunterSecrets.All);
            VerifySecrets(1, MageSecrets.All, MageSecrets.Counterspell);
            VerifySecrets(2, PaladinSecrets.All);
        }

        [TestMethod]
        public void SingleSecret_MinionInPlay_OpponentTurnStart()
        {
            _gameEventHandler.HandleOpponentTurnStart(_opponentMinion1);
            VerifySecrets(0, HunterSecrets.All);
            VerifySecrets(1, MageSecrets.All);
            VerifySecrets(2, PaladinSecrets.All, PaladinSecrets.CompetitiveSpirit);
        }

        [TestMethod]
        public void SingleSecret_NoMinionInPlay_OpponentTurnStart()
        {
            _gameEventHandler.HandleOpponentTurnStart(_heroOpponent);
            VerifySecrets(0, HunterSecrets.All);
            VerifySecrets(1, MageSecrets.All);
            VerifySecrets(2, PaladinSecrets.All);
        }

        private void VerifySecrets(int secretIndex, List<string> allSecrets, params string[] triggered)
        {
            var secrets = _game.OpponentSecrets.Secrets[secretIndex];
            foreach (var secret in allSecrets)
                Assert.AreEqual(secrets.PossibleSecrets[secret], !triggered.Contains(secret),
                    Database.GetCardFromId(secret).Name);
        }
    }
}