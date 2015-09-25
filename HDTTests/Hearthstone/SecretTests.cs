using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
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

        private Entity _hero1,
            _hero2,
            _minion1,
            _minion2,
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
            _game = new GameV2();
            _gameEventHandler = new GameEventHandler(_game);
            _hero1 = CreateNewEntity("HERO_01");
            _hero2 = CreateNewEntity("HERO_02");
            _minion1 = CreateNewEntity("EX1_010");
            _minion2 = CreateNewEntity("EX1_020");
            _secretHunter1 = CreateNewEntity("");
            _secretHunter1.SetTag(GAME_TAG.CLASS, (int) TAG_CLASS.HUNTER);
            _secretHunter2 = CreateNewEntity("");
            _secretHunter2.SetTag(GAME_TAG.CLASS, (int) TAG_CLASS.HUNTER);
            _secretMage1 = CreateNewEntity("");
            _secretMage1.SetTag(GAME_TAG.CLASS, (int) TAG_CLASS.MAGE);
            _secretMage2 = CreateNewEntity("");
            _secretMage2.SetTag(GAME_TAG.CLASS, (int) TAG_CLASS.MAGE);
            _secretPaladin1 = CreateNewEntity("");
            _secretPaladin1.SetTag(GAME_TAG.CLASS, (int) TAG_CLASS.PALADIN);
            _secretPaladin2 = CreateNewEntity("");
            _secretPaladin2.SetTag(GAME_TAG.CLASS, (int) TAG_CLASS.PALADIN);
            _gameEventHandler.HandleOpponentSecretPlayed(_secretHunter1, "", 0, 0, false, _secretHunter1.Id);
            //_gameEventHandler.HandleOpponentSecretPlayed(_secretHunter2, "", 0, 0, false, _secretHunter2.Id);
            _gameEventHandler.HandleOpponentSecretPlayed(_secretMage1, "", 0, 0, false, _secretMage1.Id);
            //_gameEventHandler.HandleOpponentSecretPlayed(_secretMage2, "", 0, 0, false, _secretMage2.Id);
            _gameEventHandler.HandleOpponentSecretPlayed(_secretPaladin1, "", 0, 0, false, _secretPaladin1.Id);
            //_gameEventHandler.HandleOpponentSecretPlayed(_secretPaladin2, "", 0, 0, false, _secretPaladin2.Id);
        }

        [TestMethod]
        public void SingleSecret_HeroToHero_PlayerAttackTest()
        {
            _gameEventHandler.HandlePlayerAttack(_hero1, _hero2);
            var hunterSecrets = _game.OpponentSecrets.Secrets[0].PossibleSecrets;
            Assert.IsFalse(hunterSecrets[HunterSecrets.BearTrap]);
            Assert.IsFalse(hunterSecrets[HunterSecrets.ExplosiveTrap]);
            Assert.IsTrue(hunterSecrets[HunterSecrets.FreezingTrap]);
            Assert.IsFalse(hunterSecrets[HunterSecrets.Misdirection]);
            Assert.IsTrue(hunterSecrets[HunterSecrets.SnakeTrap]);
            Assert.IsTrue(hunterSecrets[HunterSecrets.Snipe]);

            var mageSecrets = _game.OpponentSecrets.Secrets[1].PossibleSecrets;
            Assert.IsTrue(mageSecrets[MageSecrets.Counterspell]);
            Assert.IsTrue(mageSecrets[MageSecrets.Duplicate]);
            Assert.IsTrue(mageSecrets[MageSecrets.Effigy]);
            Assert.IsFalse(mageSecrets[MageSecrets.IceBarrier]);
            Assert.IsTrue(mageSecrets[MageSecrets.IceBlock]);
            Assert.IsTrue(mageSecrets[MageSecrets.MirrorEntity]);
            Assert.IsTrue(mageSecrets[MageSecrets.Spellbender]);
            Assert.IsTrue(mageSecrets[MageSecrets.Vaporize]);


            var paladinSecrets = _game.OpponentSecrets.Secrets[2].PossibleSecrets;
            Assert.IsTrue(paladinSecrets[PaladinSecrets.Avenge]);
            Assert.IsTrue(paladinSecrets[PaladinSecrets.CompetitiveSpirit]);
            Assert.IsTrue(paladinSecrets[PaladinSecrets.EyeForAnEye]);
            Assert.IsFalse(paladinSecrets[PaladinSecrets.NobleSacrifice]);
            Assert.IsTrue(paladinSecrets[PaladinSecrets.Redemption]);
            Assert.IsTrue(paladinSecrets[PaladinSecrets.Repentance]);
        }

        [TestMethod]
        public void SingleSecret_MinionToHero_PlayerAttackTest()
        {
            _gameEventHandler.HandlePlayerAttack(_minion1, _hero2);
            var hunterSecrets = _game.OpponentSecrets.Secrets[0].PossibleSecrets;
            Assert.IsFalse(hunterSecrets[HunterSecrets.BearTrap]);
            Assert.IsFalse(hunterSecrets[HunterSecrets.ExplosiveTrap]);
            Assert.IsFalse(hunterSecrets[HunterSecrets.FreezingTrap]);
            Assert.IsFalse(hunterSecrets[HunterSecrets.Misdirection]);
            Assert.IsTrue(hunterSecrets[HunterSecrets.SnakeTrap]);
            Assert.IsTrue(hunterSecrets[HunterSecrets.Snipe]);

            var mageSecrets = _game.OpponentSecrets.Secrets[1].PossibleSecrets;
            Assert.IsTrue(mageSecrets[MageSecrets.Counterspell]);
            Assert.IsTrue(mageSecrets[MageSecrets.Duplicate]);
            Assert.IsTrue(mageSecrets[MageSecrets.Effigy]);
            Assert.IsFalse(mageSecrets[MageSecrets.IceBarrier]);
            Assert.IsTrue(mageSecrets[MageSecrets.IceBlock]);
            Assert.IsTrue(mageSecrets[MageSecrets.MirrorEntity]);
            Assert.IsTrue(mageSecrets[MageSecrets.Spellbender]);
            Assert.IsFalse(mageSecrets[MageSecrets.Vaporize]);


            var paladinSecrets = _game.OpponentSecrets.Secrets[2].PossibleSecrets;
            Assert.IsTrue(paladinSecrets[PaladinSecrets.Avenge]);
            Assert.IsTrue(paladinSecrets[PaladinSecrets.CompetitiveSpirit]);
            Assert.IsTrue(paladinSecrets[PaladinSecrets.EyeForAnEye]);
            Assert.IsFalse(paladinSecrets[PaladinSecrets.NobleSacrifice]);
            Assert.IsTrue(paladinSecrets[PaladinSecrets.Redemption]);
            Assert.IsTrue(paladinSecrets[PaladinSecrets.Repentance]);
        }

        [TestMethod]
        public void SingleSecret_HeroToMinion_PlayerAttackTest()
        {
            _gameEventHandler.HandlePlayerAttack(_hero1, _minion2);
            var hunterSecrets = _game.OpponentSecrets.Secrets[0].PossibleSecrets;
            Assert.IsTrue(hunterSecrets[HunterSecrets.BearTrap]);
            Assert.IsTrue(hunterSecrets[HunterSecrets.ExplosiveTrap]);
            Assert.IsTrue(hunterSecrets[HunterSecrets.FreezingTrap]);
            Assert.IsTrue(hunterSecrets[HunterSecrets.Misdirection]);
            Assert.IsFalse(hunterSecrets[HunterSecrets.SnakeTrap]);
            Assert.IsTrue(hunterSecrets[HunterSecrets.Snipe]);

            var mageSecrets = _game.OpponentSecrets.Secrets[1].PossibleSecrets;
            Assert.IsTrue(mageSecrets[MageSecrets.Counterspell]);
            Assert.IsTrue(mageSecrets[MageSecrets.Duplicate]);
            Assert.IsTrue(mageSecrets[MageSecrets.Effigy]);
            Assert.IsTrue(mageSecrets[MageSecrets.IceBarrier]);
            Assert.IsTrue(mageSecrets[MageSecrets.IceBlock]);
            Assert.IsTrue(mageSecrets[MageSecrets.MirrorEntity]);
            Assert.IsTrue(mageSecrets[MageSecrets.Spellbender]);
            Assert.IsTrue(mageSecrets[MageSecrets.Vaporize]);


            var paladinSecrets = _game.OpponentSecrets.Secrets[2].PossibleSecrets;
            Assert.IsTrue(paladinSecrets[PaladinSecrets.Avenge]);
            Assert.IsTrue(paladinSecrets[PaladinSecrets.CompetitiveSpirit]);
            Assert.IsTrue(paladinSecrets[PaladinSecrets.EyeForAnEye]);
            Assert.IsFalse(paladinSecrets[PaladinSecrets.NobleSacrifice]);
            Assert.IsTrue(paladinSecrets[PaladinSecrets.Redemption]);
            Assert.IsTrue(paladinSecrets[PaladinSecrets.Repentance]);
        }

        [TestMethod]
        public void SingleSecret_MinionToMinion_PlayerAttackTest()
        {
            _gameEventHandler.HandlePlayerAttack(_minion1, _minion2);
            var hunterSecrets = _game.OpponentSecrets.Secrets[0].PossibleSecrets;
            Assert.IsTrue(hunterSecrets[HunterSecrets.BearTrap]);
            Assert.IsTrue(hunterSecrets[HunterSecrets.ExplosiveTrap]);
            Assert.IsFalse(hunterSecrets[HunterSecrets.FreezingTrap]);
            Assert.IsTrue(hunterSecrets[HunterSecrets.Misdirection]);
            Assert.IsFalse(hunterSecrets[HunterSecrets.SnakeTrap]);
            Assert.IsTrue(hunterSecrets[HunterSecrets.Snipe]);

            var mageSecrets = _game.OpponentSecrets.Secrets[1].PossibleSecrets;
            Assert.IsTrue(mageSecrets[MageSecrets.Counterspell]);
            Assert.IsTrue(mageSecrets[MageSecrets.Duplicate]);
            Assert.IsTrue(mageSecrets[MageSecrets.Effigy]);
            Assert.IsTrue(mageSecrets[MageSecrets.IceBarrier]);
            Assert.IsTrue(mageSecrets[MageSecrets.IceBlock]);
            Assert.IsTrue(mageSecrets[MageSecrets.MirrorEntity]);
            Assert.IsTrue(mageSecrets[MageSecrets.Spellbender]);
            Assert.IsTrue(mageSecrets[MageSecrets.Vaporize]);

            var paladinSecrets = _game.OpponentSecrets.Secrets[2].PossibleSecrets;
            Assert.IsTrue(paladinSecrets[PaladinSecrets.Avenge]);
            Assert.IsTrue(paladinSecrets[PaladinSecrets.CompetitiveSpirit]);
            Assert.IsTrue(paladinSecrets[PaladinSecrets.EyeForAnEye]);
            Assert.IsFalse(paladinSecrets[PaladinSecrets.NobleSacrifice]);
            Assert.IsTrue(paladinSecrets[PaladinSecrets.Redemption]);
            Assert.IsTrue(paladinSecrets[PaladinSecrets.Repentance]);
        }
    }
}