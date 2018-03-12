using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Controls.Stats;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Hearthstone.Secrets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Hearthstone_Deck_Tracker.Hearthstone.CardIds.Secrets;

namespace HDTTests.Hearthstone.Secrets
{
	[TestClass]
	public class SecretsManagerTest
	{
		[TestMethod]
		public void NewSecret_InvalidEntity()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_RANKED,
				CurrentFormat = Format.Wild
			};

			var secretsManager = new SecretsManager(game);
			Assert.AreEqual(0, secretsManager.GetSecretList().Count);

			var added = secretsManager.NewSecret(null);
			Assert.IsFalse(added);

			var invalidEntity = new Entity(0);
			Assert.AreEqual(0, secretsManager.GetSecretList().Count);

			added = secretsManager.NewSecret(invalidEntity);
			Assert.IsFalse(added);
			Assert.AreEqual(0, secretsManager.GetSecretList().Count);

			invalidEntity.SetTag(GameTag.SECRET, 1);
			added = secretsManager.NewSecret(invalidEntity);
			Assert.IsFalse(added);
			Assert.AreEqual(0, secretsManager.GetSecretList().Count);

			invalidEntity.SetTag(GameTag.SECRET, 0);
			invalidEntity.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);
			added = secretsManager.NewSecret(invalidEntity);
			Assert.IsFalse(added);
			Assert.AreEqual(0, secretsManager.GetSecretList().Count);
		}

		[TestMethod]
		public void NewSecret_ValidEntity()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_RANKED,
				CurrentFormat = Format.Wild
			};

			var secretsManager = new SecretsManager(game);
			Assert.AreEqual(0, secretsManager.GetSecretList().Count);

			var validEntity = new Entity(0);
			validEntity.SetTag(GameTag.SECRET, 1);
			validEntity.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);

			var added = secretsManager.NewSecret(validEntity);
			Assert.IsTrue(added);
			Assert.AreEqual(1, secretsManager.Secrets.Count);

			var validEntity2 = new Entity(1);
			validEntity2.SetTag(GameTag.SECRET, 1);
			validEntity2.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);
			added = secretsManager.NewSecret(validEntity);
			Assert.IsTrue(added);
			Assert.AreEqual(2, secretsManager.Secrets.Count);

			var validEntity3 = new Entity(2);
			validEntity3.SetTag(GameTag.SECRET, 1);
			validEntity3.SetTag(GameTag.CLASS, (int)CardClass.MAGE);
			added = secretsManager.NewSecret(validEntity);
			Assert.IsTrue(added);
			Assert.AreEqual(3, secretsManager.Secrets.Count);
		}

		[TestMethod]
		public void RemoveSecret_InvalidEntity()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_RANKED,
				CurrentFormat = Format.Wild
			};

			var secretsManager = new SecretsManager(game);

			var removed = secretsManager.RemoveSecret(null);
			Assert.IsFalse(removed);

			var entity = new Entity(0);
			removed = secretsManager.RemoveSecret(entity);
			Assert.IsFalse(removed);

			entity.SetTag(GameTag.SECRET, 1);
			entity.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);
			secretsManager.NewSecret(entity);
			Assert.AreEqual(1, secretsManager.Secrets.Count);

			var entity2 = new Entity(1);
			removed = secretsManager.RemoveSecret(entity2);
			Assert.IsFalse(removed);
			Assert.AreEqual(1, secretsManager.Secrets.Count);
		}

		[TestMethod]
		public void RemoveSecret_ValidEntity()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_RANKED,
				CurrentFormat = Format.Wild
			};

			var secretsManager = new SecretsManager(game);

			var entity = new Entity(0);
			entity.SetTag(GameTag.SECRET, 1);
			entity.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);
			secretsManager.NewSecret(entity);
			Assert.AreEqual(1, secretsManager.Secrets.Count);

			var entity2 = new Entity(0);
			entity2.SetTag(GameTag.SECRET, 1);
			entity2.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);
			secretsManager.NewSecret(entity2);
			Assert.AreEqual(2, secretsManager.Secrets.Count);

			var removed = secretsManager.RemoveSecret(entity);
			Assert.IsTrue(removed);
			Assert.AreEqual(1, secretsManager.Secrets.Count);

			removed = secretsManager.RemoveSecret(entity2);
			Assert.IsTrue(removed);
			Assert.AreEqual(0, secretsManager.Secrets.Count);
		}

		[TestMethod]
		public void InvalidClass()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_RANKED,
				CurrentFormat = Format.Wild
			};

			var secretsManager = new SecretsManager(game);
			var entity = new Entity(0);
			entity.SetTag(GameTag.SECRET, 1);
			entity.SetTag(GameTag.CLASS, (int)CardClass.PRIEST);
			secretsManager.NewSecret(entity);
			Assert.AreEqual(1, secretsManager.Secrets.Count);

			Assert.AreEqual(0, secretsManager.GetSecretList().Count);
			secretsManager.Toggle(Paladin.Avenge);
			Assert.AreEqual(0, secretsManager.GetSecretList().Count);
		}

		[TestMethod]
		public void Toggle_SingleClass()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_RANKED,
				CurrentFormat = Format.Wild
			};

			var secretsManager = new SecretsManager(game);

			var entity = new Entity(0);
			entity.SetTag(GameTag.SECRET, 1);
			entity.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);
			secretsManager.NewSecret(entity);
			Assert.AreEqual(1, secretsManager.Secrets.Count);

			var cards = secretsManager.GetSecretList();
			Assert.AreEqual(1, cards.Single(x => x.Id == Paladin.Avenge).Count);

			secretsManager.Toggle(Paladin.Avenge);
			cards = secretsManager.GetSecretList();
			Assert.AreEqual(0, cards.Single(x => x.Id == Paladin.Avenge).Count);

			secretsManager.Toggle(Paladin.Avenge);
			cards = secretsManager.GetSecretList();
			Assert.AreEqual(1, cards.Single(x => x.Id == Paladin.Avenge).Count);

			foreach(var id in Paladin.All)
				secretsManager.Toggle(id);

			cards = secretsManager.GetSecretList();
			foreach(var card in cards)
				Assert.AreEqual(0, card.Count);

			foreach(var id in Paladin.All)
				secretsManager.Toggle(id);

			cards = secretsManager.GetSecretList();
			foreach(var card in cards)
				Assert.AreEqual(1, card.Count);
		}

		[TestMethod]
		public void Toggle_MultiClass()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_RANKED,
				CurrentFormat = Format.Wild
			};

			var secretsManager = new SecretsManager(game);

			var paladinEntity = new Entity(0);
			paladinEntity.SetTag(GameTag.SECRET, 1);
			paladinEntity.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);
			secretsManager.NewSecret(paladinEntity);
			Assert.AreEqual(1, secretsManager.Secrets.Count);

			var mageEntity = new Entity(1);
			mageEntity.SetTag(GameTag.SECRET, 1);
			mageEntity.SetTag(GameTag.CLASS, (int)CardClass.MAGE);
			secretsManager.NewSecret(mageEntity);
			Assert.AreEqual(2, secretsManager.Secrets.Count);

			var hunterEntity = new Entity(2);
			hunterEntity.SetTag(GameTag.SECRET, 1);
			hunterEntity.SetTag(GameTag.CLASS, (int)CardClass.HUNTER);
			secretsManager.NewSecret(hunterEntity);
			Assert.AreEqual(3, secretsManager.Secrets.Count);

			var allSecrets = Paladin.All.Concat(Mage.All).Concat(Hunter.All).ToList();
			
			var cards = secretsManager.GetSecretList();
			foreach(var card in cards)
				Assert.AreEqual(1, card.Count);

			foreach(var id in allSecrets)
				secretsManager.Toggle(id);

			cards = secretsManager.GetSecretList();
			foreach(var card in cards)
				Assert.AreEqual(0, card.Count);

			foreach(var id in allSecrets)
				secretsManager.Toggle(id);

			cards = secretsManager.GetSecretList();
			foreach(var card in cards)
				Assert.AreEqual(1, card.Count);
		}

		[TestMethod]
		public void Reset()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_RANKED,
				CurrentFormat = Format.Wild
			};

			var secretsManager = new SecretsManager(game);

			var entity = new Entity(0);
			entity.SetTag(GameTag.SECRET, 1);
			entity.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);
			secretsManager.NewSecret(entity);
			Assert.AreEqual(1, secretsManager.Secrets.Count);

			secretsManager.Reset();
			Assert.AreEqual(0, secretsManager.Secrets.Count);
		}

		[TestMethod]
		public void OnSecretsChangedEvent()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_RANKED,
				CurrentFormat = Format.Wild
			};

			var callbackCount = 0;
			var secretsManager = new SecretsManager(game);
			secretsManager.OnSecretsChanged += secrets => callbackCount += 1;

			var entity = new Entity(0);
			entity.SetTag(GameTag.SECRET, 1);
			entity.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);
			secretsManager.NewSecret(entity);
			Assert.AreEqual(1, callbackCount);

			secretsManager.Toggle(Paladin.Avenge);
			Assert.AreEqual(2, callbackCount);

			secretsManager.Exclude(new List<string> { Paladin.CompetitiveSpirit, Paladin.GetawayKodo });
			Assert.AreEqual(3, callbackCount);

			secretsManager.Toggle(Paladin.Avenge);
			Assert.AreEqual(4, callbackCount);

			secretsManager.RemoveSecret(entity);
			Assert.AreEqual(5, callbackCount);

			secretsManager.Reset();
			Assert.AreEqual(6, callbackCount);
		}

		[TestMethod]
		public void GameType_Format()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_RANKED,
				CurrentFormat = Format.Wild
			};

			var secretsManager = new SecretsManager(game);

			var paladinEntity = new Entity(0);
			paladinEntity.SetTag(GameTag.SECRET, 1);
			paladinEntity.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);
			secretsManager.NewSecret(paladinEntity);
			Assert.AreEqual(1, secretsManager.Secrets.Count);

			var mageEntity = new Entity(1);
			mageEntity.SetTag(GameTag.SECRET, 1);
			mageEntity.SetTag(GameTag.CLASS, (int)CardClass.MAGE);
			secretsManager.NewSecret(mageEntity);
			Assert.AreEqual(2, secretsManager.Secrets.Count);

			var hunterEntity = new Entity(2);
			hunterEntity.SetTag(GameTag.SECRET, 1);
			hunterEntity.SetTag(GameTag.CLASS, (int)CardClass.HUNTER);
			secretsManager.NewSecret(hunterEntity);
			Assert.AreEqual(3, secretsManager.Secrets.Count);

			var wildSecrets = Paladin.All.Where(x => x != Paladin.HandOfSalvation)
				.Concat(Mage.All).Concat(Hunter.All).ToList();
			var cards = secretsManager.GetSecretList();
			Assert.AreEqual(wildSecrets.Count, cards.Count);
			foreach(var secret in wildSecrets)
				Assert.IsNotNull(cards.SingleOrDefault(c => c.Id == secret));

			game.CurrentFormat = Format.Standard;
			cards = secretsManager.GetSecretList();
			var wildSets = Helper.WildOnlySets;
			var standardSecrets = wildSecrets.Where(x => !wildSets.Contains(Database.GetCardFromId(x).Set)).ToList();
			Assert.AreEqual(standardSecrets.Count, cards.Count);
			foreach(var secret in standardSecrets)
				Assert.IsNotNull(cards.SingleOrDefault(c => c.Id == secret));

			game.CurrentGameType = GameType.GT_ARENA;
			game.CurrentFormat = Format.Wild; // Arena format is Wild
			cards = secretsManager.GetSecretList();
			var arenaSecrets = standardSecrets.Where(x => !ArenaExcludes.Contains(x)).Concat(ArenaOnly).ToList();
			Assert.AreEqual(arenaSecrets.Count, cards.Count);
			foreach(var secret in arenaSecrets)
				Assert.IsNotNull(cards.SingleOrDefault(c => c.Id == secret));
		}

		[TestMethod]
		public void CountAdjustment()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_RANKED,
				CurrentFormat = Format.Wild
			};

			Entity RevealedSecret(int id)
			{
				var entity = new Entity(id);
				entity.SetTag(GameTag.SECRET, 1);
				entity.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);
				entity.SetTag(GameTag.CONTROLLER, game.Opponent.Id);
				entity.CardId = Paladin.Repentance;
				return entity;
			}
			
			game.Entities.Add(0, RevealedSecret(0));
			var createdSecret = RevealedSecret(1);
			createdSecret.Info.Created = true;
			game.Entities.Add(1, createdSecret);

			var secretsManager = new SecretsManager(game);

			var paladinEntity = new Entity(2);
			paladinEntity.SetTag(GameTag.SECRET, 1);
			paladinEntity.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);
			secretsManager.NewSecret(paladinEntity);
			Assert.AreEqual(1, secretsManager.Secrets.Count);

			var cards = secretsManager.GetSecretList();
			Assert.IsNotNull(cards.SingleOrDefault(c => c.Id == Paladin.Repentance && c.Count == 1));

			game.Entities.Add(3, RevealedSecret(3));
			cards = secretsManager.GetSecretList();
			Assert.IsNotNull(cards.SingleOrDefault(c => c.Id == Paladin.Repentance && c.Count == 0));

			game.CurrentFormat = Format.Standard;
			cards = secretsManager.GetSecretList();
			Assert.IsNotNull(cards.SingleOrDefault(c => c.Id == Paladin.Repentance && c.Count == 0));

			game.CurrentGameType = GameType.GT_ARENA;
			cards = secretsManager.GetSecretList();
			Assert.IsNotNull(cards.SingleOrDefault(c => c.Id == Paladin.Repentance && c.Count == 1));

			paladinEntity.Info.Created = true;

			cards = secretsManager.GetSecretList();
			Assert.IsNotNull(cards.SingleOrDefault(c => c.Id == Paladin.Repentance && c.Count == 1));

			game.CurrentGameType = GameType.GT_RANKED;

			cards = secretsManager.GetSecretList();
			Assert.IsNotNull(cards.SingleOrDefault(c => c.Id == Paladin.Repentance && c.Count == 1));

			game.CurrentFormat = Format.Wild;
			cards = secretsManager.GetSecretList();
			Assert.IsNotNull(cards.SingleOrDefault(c => c.Id == Paladin.Repentance && c.Count == 1));
		}
	}
}
