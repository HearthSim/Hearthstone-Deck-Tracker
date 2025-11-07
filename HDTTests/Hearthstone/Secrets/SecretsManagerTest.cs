using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;
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
				CurrentFormatType = FormatType.FT_WILD
			};

			var secretsManager = new SecretsManager(game, new MockAvailableSecrets(), new RelatedCardsManager());
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
				CurrentFormatType = FormatType.FT_WILD
			};

			var secretsManager = new SecretsManager(game, new MockAvailableSecrets(), new RelatedCardsManager());
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
				CurrentFormatType = FormatType.FT_WILD
			};

			var secretsManager = new SecretsManager(game, new MockAvailableSecrets(), new RelatedCardsManager());

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
				CurrentFormatType = FormatType.FT_WILD
			};

			var secretsManager = new SecretsManager(game, new MockAvailableSecrets(), new RelatedCardsManager());

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
				CurrentFormatType = FormatType.FT_WILD
			};

			var secretsManager = new SecretsManager(game, new MockAvailableSecrets(), new RelatedCardsManager());
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
				CurrentFormatType = FormatType.FT_WILD
			};

			var secretsManager = new SecretsManager(game, new MockAvailableSecrets(), new RelatedCardsManager());

			var entity = new Entity(0);
			entity.SetTag(GameTag.SECRET, 1);
			entity.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);
			secretsManager.NewSecret(entity);
			Assert.AreEqual(1, secretsManager.Secrets.Count);

			var cards = secretsManager.GetSecretList();
			Assert.IsNotNull(cards.SingleOrDefault(x => Paladin.Avenge == x.Id));

			secretsManager.Toggle(Paladin.Avenge);
			cards = secretsManager.GetSecretList();
			Assert.IsNull(cards.SingleOrDefault(x => Paladin.Avenge == x.Id));

			secretsManager.Toggle(Paladin.Avenge);
			cards = secretsManager.GetSecretList();
			Assert.IsNotNull(cards.SingleOrDefault(x => Paladin.Avenge == x.Id));

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
				CurrentFormatType = FormatType.FT_WILD
			};

			var secretsManager = new SecretsManager(game, new MockAvailableSecrets(), new RelatedCardsManager());

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
				CurrentFormatType = FormatType.FT_WILD
			};

			var secretsManager = new SecretsManager(game, new MockAvailableSecrets(), new RelatedCardsManager());

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
				CurrentFormatType = FormatType.FT_WILD
			};

			var callbackCount = 0;
			var secretsManager = new SecretsManager(game, new MockAvailableSecrets(), new RelatedCardsManager());
			secretsManager.OnSecretsChanged += secrets => callbackCount += 1;

			var entity = new Entity(0);
			entity.SetTag(GameTag.SECRET, 1);
			entity.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);
			secretsManager.NewSecret(entity);
			Assert.AreEqual(1, callbackCount);

			secretsManager.Toggle(Paladin.Avenge);
			Assert.AreEqual(2, callbackCount);

			secretsManager.Exclude(new List<MultiIdCard> { Paladin.CompetitiveSpirit, Paladin.GetawayKodo });
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
				CurrentFormatType = FormatType.FT_WILD
			};

			var settings = new MockAvailableSecrets();
			var secretsManager = new SecretsManager(game, settings,  new RelatedCardsManager());

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

			var rogueEntity = new Entity(3);
			rogueEntity.SetTag(GameTag.SECRET, 1);
			rogueEntity.SetTag(GameTag.CLASS, (int)CardClass.ROGUE);
			secretsManager.NewSecret(rogueEntity);
			Assert.AreEqual(4, secretsManager.Secrets.Count);

			var allSecrets = Paladin.All.Concat(Mage.All).Concat(Hunter.All).Concat(Rogue.All).ToList();
			var cards = secretsManager.GetSecretList();
			Assert.AreEqual(allSecrets.Count, cards.Count);
			foreach(var secret in allSecrets)
				Assert.IsNotNull(cards.SingleOrDefault(c => secret == c.Id));

			game.CurrentFormatType = FormatType.FT_STANDARD;
			cards = secretsManager.GetSecretList();
			var standardSecrets = allSecrets.Where(x => x.IsStandard).ToList();
			Assert.AreEqual(standardSecrets.Count(), cards.Count);
			foreach(var secret in standardSecrets)
				Assert.IsNotNull(cards.SingleOrDefault(c => secret == c.Id));

			game.CurrentGameType = GameType.GT_ARENA;
			game.CurrentFormatType = FormatType.FT_WILD; // Arena format is Wild
			cards = secretsManager.GetSecretList();
			Assert.AreEqual(settings.ByType["GT_ARENA"].Count, cards.Count);
		}

		[TestMethod]
		public void CountAdjustment()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_RANKED,
				CurrentFormatType = FormatType.FT_WILD
			};

			Entity RevealedSecret(int id)
			{
				var entity = new Entity(id);
				entity.SetTag(GameTag.SECRET, 1);
				entity.SetTag(GameTag.CLASS, (int)CardClass.ROGUE);
				entity.SetTag(GameTag.CONTROLLER, game.Opponent.Id);
				entity.CardId = Rogue.Plagiarize.Ids[0];
				return entity;
			}

			// SETUP:
			// - One copy of the secret has been revealed
			// - A second copy of the secret has been revealed but was created.
			//   It should not count towards the maximum number of instances of this secret
			//   (In a constructed game only two copies of a secret can be in the deck)
			// - An unknown secret is added
			// - We will create and then reveal a second copy that was NOT created.
			// - In constructed game modes we expect the unknown secret to not be the 2x revealed one

			game.Entities.Add(0, RevealedSecret(0));
			var createdSecret = RevealedSecret(1);
			createdSecret.Info.Created = true;
			game.Entities.Add(1, createdSecret);

			var availableSecrets = new MockAvailableSecrets();
			availableSecrets.ByType["FT_WILD"].Add(Rogue.Plagiarize.Ids[0]);
			availableSecrets.ByType["FT_STANDARD"].Add(Rogue.Plagiarize.Ids[0]);
			availableSecrets.ByType["GT_ARENA"].Add(Rogue.Plagiarize.Ids[0]);

			var secretsManager = new SecretsManager(game, availableSecrets, new RelatedCardsManager());

			var coreSecret = new Entity(2);
			coreSecret.SetTag(GameTag.SECRET, 1);
			coreSecret.SetTag(GameTag.CLASS, (int)CardClass.ROGUE);
			secretsManager.NewSecret(coreSecret);
			Assert.AreEqual(1, secretsManager.Secrets.Count);

			var cards = secretsManager.GetSecretList();
			// unknown secret can be anything
			Assert.IsNotNull(cards.SingleOrDefault(c => Rogue.Plagiarize == c.Id && c.Count == 1));

			game.Entities.Add(3, RevealedSecret(3));
			cards = secretsManager.GetSecretList();
			// unknown secret can not be plagiarize
			Assert.IsNotNull(cards.SingleOrDefault(c => Rogue.Plagiarize == c.Id && c.Count == 0));

			game.CurrentFormatType = FormatType.FT_STANDARD;
			cards = secretsManager.GetSecretList();
			// unknown secret can not be plagiarize
			Assert.IsNotNull(cards.SingleOrDefault(c => Rogue.Plagiarize == c.Id && c.Count == 0));

			game.CurrentGameType = GameType.GT_ARENA;
			game.CurrentFormatType = FormatType.FT_WILD;
			cards = secretsManager.GetSecretList();
			// unknown secret can be plagiarize, no limit in arena
			Assert.IsNotNull(cards.SingleOrDefault(c => Rogue.Plagiarize == c.Id && c.Count == 1));

			coreSecret.Info.Created = true;

			cards = secretsManager.GetSecretList();
			// unknown secret can be plagiarize, no limit in arena
			Assert.IsNotNull(cards.SingleOrDefault(c => Rogue.Plagiarize == c.Id && c.Count == 1));

			game.CurrentGameType = GameType.GT_RANKED;
			cards = secretsManager.GetSecretList();
			// unknown secret is created, can be plagiarize
			Assert.IsNotNull(cards.SingleOrDefault(c => Rogue.Plagiarize == c.Id && c.Count == 1));

			game.CurrentFormatType = FormatType.FT_WILD;
			cards = secretsManager.GetSecretList();
			// unknown secret is created, can be plagiarize
			Assert.IsNotNull(cards.SingleOrDefault(c => Rogue.Plagiarize == c.Id && c.Count == 1));
		}

		[TestMethod]
		public void ArenaSecretFilteredByCreator()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_ARENA,
				CurrentFormatType = FormatType.FT_WILD
			};

			var creator = new Entity(10);
			// use fake tear reality id because it is now also implemented for non-arena
			creator.CardId = "HearthDb.CardIds.Collectible.Mage.TearReality";
			game.Entities.Add(10, creator);

			var createdSecret = new Entity(1);
			createdSecret.SetTag(GameTag.SECRET, 1);
			createdSecret.SetTag(GameTag.CLASS, (int)CardClass.MAGE);
			createdSecret.SetTag(GameTag.CONTROLLER, game.Opponent.Id);
			createdSecret.SetTag(GameTag.CREATOR, 10);
			createdSecret.Info.Created = true;
			game.Entities.Add(1, createdSecret);

			var availableSecrets = new MockAvailableSecrets();
			availableSecrets.ByType["FT_WILD"] = new HashSet<string> { Mage.Counterspell.Ids[0] };
			availableSecrets.CreatedByTypeByCreator["GT_ARENA"]
				.Add(
					"HearthDb.CardIds.Collectible.Mage.TearReality",
					new HashSet<string>
					{
						"AT_002",
						"BT_003",
						"CFM_620",
						"DEEP_000",
						"DMF_107",
						"EX1_294",
						"EX1_295",
						"EX1_594",
						"FP1_018",
						"ICC_082",
						"MAW_006",
						"REV_516",
						"TRL_400",
						"ULD_239",
						"UNG_024",
						"WW_422",
						"tt_010"
					}
				);

			var secretsManager = new SecretsManager(game, availableSecrets, new RelatedCardsManager());


			secretsManager.NewSecret(createdSecret);

			var cards = secretsManager.GetSecretList();

			// Only secrets creatable by TearReality should be included
			Assert.IsNull(cards.SingleOrDefault(c => Mage.Counterspell == c.Id && c.Count == 1));

			Assert.IsNotNull(cards.SingleOrDefault(c => Mage.Vaporize == c.Id && c.Count == 1));
			Assert.IsNotNull(cards.SingleOrDefault(c => Mage.IceBlock == c.Id && c.Count == 1));

			// in not arena only counterspell should be included
			game.CurrentGameType = GameType.GT_RANKED;
			cards = secretsManager.GetSecretList();

			Assert.IsNotNull(cards.SingleOrDefault(c => Mage.Counterspell == c.Id && c.Count == 1));

			Assert.IsNull(cards.SingleOrDefault(c => Mage.Vaporize == c.Id && c.Count == 1));
			Assert.IsNull(cards.SingleOrDefault(c => Mage.IceBlock == c.Id && c.Count == 1));
		}

		[TestMethod]
		public void SecretsFilteredByDrawer()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_RANKED,
				CurrentFormatType = FormatType.FT_WILD
			};

			var watercolorArtist = new Entity(10);
			watercolorArtist.CardId = HearthDb.CardIds.Collectible.Mage.WatercolorArtist;
			game.Entities.Add(10, watercolorArtist);

			var drawnByWatercolorArtistSecret = new Entity(1);
			drawnByWatercolorArtistSecret.SetTag(GameTag.SECRET, 1);
			drawnByWatercolorArtistSecret.SetTag(GameTag.CLASS, (int)CardClass.MAGE);
			drawnByWatercolorArtistSecret.SetTag(GameTag.CONTROLLER, game.Opponent.Id);
			drawnByWatercolorArtistSecret.Info.DrawerId = 10;
			game.Entities.Add(1, drawnByWatercolorArtistSecret);

			var availableSecrets = new MockAvailableSecrets();

			var secretsManager = new SecretsManager(game, availableSecrets, new RelatedCardsManager());

			secretsManager.NewSecret(drawnByWatercolorArtistSecret);

			var cards = secretsManager.GetSecretList();

			// only Frost secrets should be included
			Assert.IsNull(cards.SingleOrDefault(c => Mage.Counterspell == c.Id && c.Count == 1));
			Assert.IsNull(cards.SingleOrDefault(c => Mage.Vaporize == c.Id && c.Count == 1));
			// all Frost secrets should be included
			Assert.IsNotNull(cards.SingleOrDefault(c => Mage.IceBlock == c.Id && c.Count == 1));
			Assert.IsNotNull(cards.SingleOrDefault(c => Mage.IceBarrier == c.Id && c.Count == 1));
			Assert.IsNotNull(cards.SingleOrDefault(c => Mage.OasisAlly == c.Id && c.Count == 1));
			Assert.IsNotNull(cards.SingleOrDefault(c => Mage.FrozenClone == c.Id && c.Count == 1));
			Assert.AreEqual(4, cards.Count);
		}

		[TestMethod]
		public void SecretsNotFilteredByGenericDrawer()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_RANKED,
				CurrentFormatType = FormatType.FT_WILD
			};

			var arcaneIntellect = new Entity(10);
			arcaneIntellect.CardId = HearthDb.CardIds.Collectible.Mage.ArcaneIntellectCore;
			game.Entities.Add(10, arcaneIntellect);

			var drawnByarcaneIntellectSecret = new Entity(1);
			drawnByarcaneIntellectSecret.SetTag(GameTag.SECRET, 1);
			drawnByarcaneIntellectSecret.SetTag(GameTag.CLASS, (int)CardClass.MAGE);
			drawnByarcaneIntellectSecret.SetTag(GameTag.CONTROLLER, game.Opponent.Id);
			drawnByarcaneIntellectSecret.Info.DrawerId = 10;
			game.Entities.Add(1, drawnByarcaneIntellectSecret);

			var availableSecrets = new MockAvailableSecrets();

			var secretsManager = new SecretsManager(game, availableSecrets, new RelatedCardsManager());

			secretsManager.NewSecret(drawnByarcaneIntellectSecret);

			var cards = secretsManager.GetSecretList();

			// all secrets should be available
			Assert.AreEqual(Mage.All.Count, cards.Count);
		}

		[TestMethod]
		public void ArenaSecretFilteredByCreatorAndDrawer()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_ARENA,
				CurrentFormatType = FormatType.FT_WILD
			};

			var creator = new Entity(10)
			{
				CardId = "HearthDb.CardIds.Collectible.Mage.TearReality"
			};
			game.Entities.Add(10, creator);

			var watercolorArtist = new Entity(20)
			{
				CardId = HearthDb.CardIds.Collectible.Mage.WatercolorArtist
			};
			game.Entities.Add(20, watercolorArtist);

			var createdSecret = new Entity(1);
			createdSecret.SetTag(GameTag.SECRET, 1);
			createdSecret.SetTag(GameTag.CLASS, (int)CardClass.MAGE);
			createdSecret.SetTag(GameTag.CONTROLLER, game.Opponent.Id);
			createdSecret.SetTag(GameTag.CREATOR, 10);
			createdSecret.Info.Created = true;
			createdSecret.Info.DrawerId = 20;

			game.Entities.Add(1, createdSecret);

			var availableSecrets = new MockAvailableSecrets();
			availableSecrets.ByType["FT_WILD"] = new HashSet<string> { Mage.Counterspell.Ids[0] };
			availableSecrets.CreatedByTypeByCreator["GT_ARENA"]
				.Add(
					"HearthDb.CardIds.Collectible.Mage.TearReality",
					new HashSet<string>
					{
						"AT_002",
						"BT_003",
						"CFM_620",
						"DEEP_000",
						"DMF_107",
						"EX1_294",
						"EX1_295",
						"EX1_594",
						"FP1_018",
						"ICC_082",
						"MAW_006",
						"REV_516",
						"TRL_400",
						"ULD_239",
						"UNG_024",
						"WW_422",
						"tt_010"
					}
				);

			var secretsManager = new SecretsManager(game, availableSecrets, new RelatedCardsManager());


			secretsManager.NewSecret(createdSecret);

			var cards = secretsManager.GetSecretList();

			// Secrets Not from the past should not be included
			Assert.IsNull(cards.SingleOrDefault(c => Mage.Counterspell == c.Id && c.Count == 1));
			Assert.IsNull(cards.SingleOrDefault(c => Mage.IceBarrier == c.Id && c.Count == 1));

			// Non-Frost secrets should not be included
			Assert.IsNull(cards.SingleOrDefault(c => Mage.Vaporize == c.Id && c.Count == 1));

			Assert.IsNotNull(cards.SingleOrDefault(c => Mage.IceBlock == c.Id && c.Count == 1));
		}

		[TestMethod]
		public void RankedSecretFilteredByCreator()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_RANKED,
				CurrentFormatType = FormatType.FT_STANDARD
			};

			var creator = new Entity(10);
			creator.CardId = HearthDb.CardIds.Collectible.Mage.TearReality;
			game.Entities.Add(10, creator);

			var createdSecret = new Entity(1);
			createdSecret.SetTag(GameTag.SECRET, 1);
			createdSecret.SetTag(GameTag.CLASS, (int)CardClass.MAGE);
			createdSecret.SetTag(GameTag.CONTROLLER, game.Opponent.Id);
			createdSecret.SetTag(GameTag.CREATOR, 10);
			createdSecret.Info.Created = true;
			game.Entities.Add(1, createdSecret);

			var availableSecrets = new MockAvailableSecrets();
			availableSecrets.ByType["FT_STANDARD"] = new HashSet<string> { Mage.Counterspell.Ids[0] };

			var secretsManager = new SecretsManager(game, availableSecrets, new RelatedCardsManager());

			secretsManager.NewSecret(createdSecret);

			var cards = secretsManager.GetSecretList();

			// Only secrets creatable by TearReality should be included
			Assert.IsNull(cards.SingleOrDefault(c => Mage.Counterspell == c.Id && c.Count == 1));

			Assert.IsNotNull(cards.SingleOrDefault(c => Mage.Vaporize == c.Id && c.Count == 1));
			Assert.IsNotNull(cards.SingleOrDefault(c => Mage.IceBlock == c.Id && c.Count == 1));
		}

		[TestMethod]
		public void RankedSecretFilteredByCreatorAndDrawer()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_RANKED,
				CurrentFormatType = FormatType.FT_STANDARD
			};

			var creator = new Entity(10);
			creator.CardId = HearthDb.CardIds.Collectible.Mage.TearReality;
			game.Entities.Add(10, creator);

			var watercolorArtist = new Entity(20)
			{
				CardId = HearthDb.CardIds.Collectible.Mage.WatercolorArtist
			};
			game.Entities.Add(20, watercolorArtist);

			var createdSecret = new Entity(1);
			createdSecret.SetTag(GameTag.SECRET, 1);
			createdSecret.SetTag(GameTag.CLASS, (int)CardClass.MAGE);
			createdSecret.SetTag(GameTag.CONTROLLER, game.Opponent.Id);
			createdSecret.SetTag(GameTag.CREATOR, 10);
			createdSecret.Info.Created = true;
			createdSecret.Info.DrawerId = 20;
			game.Entities.Add(1, createdSecret);

			var availableSecrets = new MockAvailableSecrets();
			availableSecrets.ByType["FT_STANDARD"] = new HashSet<string> { Mage.Counterspell.Ids[0] };

			var secretsManager = new SecretsManager(game, availableSecrets, new RelatedCardsManager());

			secretsManager.NewSecret(createdSecret);

			var cards = secretsManager.GetSecretList();

			// Secrets Not from the past should not be included
			Assert.IsNull(cards.SingleOrDefault(c => Mage.Counterspell == c.Id && c.Count == 1));
			Assert.IsNull(cards.SingleOrDefault(c => Mage.IceBarrier == c.Id && c.Count == 1));

			// Non-Frost secrets should not be included
			Assert.IsNull(cards.SingleOrDefault(c => Mage.Vaporize == c.Id && c.Count == 1));

			Assert.IsNotNull(cards.SingleOrDefault(c => Mage.IceBlock == c.Id && c.Count == 1));
		}

		[TestMethod]
		public void CreatedByFacelessEnigma()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_RANKED,
				CurrentFormatType = FormatType.FT_STANDARD
			};
			game.Player = new Player(game, true) { Id = 1 };
			game.Opponent = new Player(game, false) { Id = 2 };

			var creator = new Entity(10);
			creator.CardId = HearthDb.CardIds.Collectible.Mage.FacelessEnigma;
			creator.SetTag(GameTag.CONTROLLER, game.Player.Id);
			game.Entities.Add(10, creator);

			var option1Secret = new Entity(21);
			option1Secret.SetTag(GameTag.SECRET, 1);
			option1Secret.CardId = HearthDb.CardIds.Collectible.Mage.Counterspell;
			option1Secret.SetTag(GameTag.CONTROLLER, game.Player.Id);
			option1Secret.SetTag(GameTag.ZONE, (int)Zone.GRAVEYARD);
			option1Secret.SetTag(GameTag.CREATOR, 10);
			game.Entities.Add(21, option1Secret);

			var option2Secret = new Entity(22);
			option2Secret.SetTag(GameTag.SECRET, 1);
			option2Secret.CardId = HearthDb.CardIds.Collectible.Mage.IceBarrier;
			option2Secret.SetTag(GameTag.CONTROLLER, game.Player.Id);
			option2Secret.SetTag(GameTag.ZONE, (int)Zone.GRAVEYARD);
			option2Secret.SetTag(GameTag.CREATOR, 10);
			game.Entities.Add(22, option2Secret);

			var playerSecret = new Entity(20);
			playerSecret.SetTag(GameTag.SECRET, 1);
			playerSecret.CardId = HearthDb.CardIds.Collectible.Mage.Counterspell;
			playerSecret.SetTag(GameTag.CONTROLLER, game.Player.Id);
			playerSecret.SetTag(GameTag.ZONE, (int)Zone.SECRET);
			playerSecret.SetTag(GameTag.CREATOR, 10);
			game.Entities.Add(20, playerSecret);

			creator.Info.StoredCardIds = new List<string> { HearthDb.CardIds.Collectible.Mage.IceBarrier };

			var createdSecret = new Entity(1);
			createdSecret.SetTag(GameTag.SECRET, 1);
			createdSecret.SetTag(GameTag.CLASS, (int)CardClass.MAGE);
			createdSecret.SetTag(GameTag.CONTROLLER, game.Opponent.Id);
			createdSecret.SetTag(GameTag.CREATOR, 10);
			createdSecret.Info.Created = true;
			game.Entities.Add(1, createdSecret);

			var availableSecrets = new MockAvailableSecrets();
			availableSecrets.ByType["FT_STANDARD"] = new HashSet<string> { Mage.Counterspell.Ids[0], Mage.ExplosiveRunes.Ids[0], Mage.IceBarrier.Ids[0] };

			var secretsManager = new SecretsManager(game, availableSecrets, new RelatedCardsManager());

			secretsManager.NewSecret(createdSecret);

			var cards = secretsManager.GetSecretList();

			// We should know the secret is Ice Barrier
			Assert.AreEqual(1, cards.Count);
			Assert.IsNull(cards.SingleOrDefault(c => Mage.Counterspell == c.Id && c.Count == 1));
			Assert.IsNull(cards.SingleOrDefault(c => Mage.ExplosiveRunes == c.Id && c.Count == 1));
			Assert.IsNotNull(cards.SingleOrDefault(c => Mage.IceBarrier == c.Id && c.Count == 1));

		}

		[TestMethod]
		public void CreatedByOpponentsFacelessEnigma()
		{
			var game = new MockGame
			{
				CurrentGameType = GameType.GT_RANKED,
				CurrentFormatType = FormatType.FT_STANDARD,
			};
			game.Player = new Player(game, true) { Id = 1 };
			game.Opponent = new Player(game, false) { Id = 2 };

			var creator = new Entity(10);
			creator.CardId = HearthDb.CardIds.Collectible.Mage.FacelessEnigma;
			creator.SetTag(GameTag.CONTROLLER, game.Opponent.Id);
			game.Entities.Add(10, creator);

			var playerSecret = new Entity(20);
			playerSecret.SetTag(GameTag.SECRET, 1);
			playerSecret.CardId = HearthDb.CardIds.Collectible.Mage.Counterspell;
			playerSecret.SetTag(GameTag.CONTROLLER, game.Player.Id);
			playerSecret.SetTag(GameTag.ZONE, (int)Zone.SECRET);
			playerSecret.SetTag(GameTag.CREATOR, 10);
			game.Entities.Add(20, playerSecret);

			creator.Info.StoredCardIds = new List<string> { HearthDb.CardIds.Collectible.Mage.Counterspell };

			var createdSecret = new Entity(1);
			createdSecret.SetTag(GameTag.SECRET, 1);
			createdSecret.SetTag(GameTag.CLASS, (int)CardClass.MAGE);
			createdSecret.SetTag(GameTag.CONTROLLER, game.Opponent.Id);
			createdSecret.SetTag(GameTag.CREATOR, 10);
			createdSecret.Info.Created = true;
			game.Entities.Add(1, createdSecret);

			var availableSecrets = new MockAvailableSecrets();
			availableSecrets.ByType["FT_STANDARD"] = new HashSet<string> { Mage.Counterspell.Ids[0], Mage.ExplosiveRunes.Ids[0], Mage.IceBarrier.Ids[0] };

			var secretsManager = new SecretsManager(game, availableSecrets, new RelatedCardsManager());

			secretsManager.NewSecret(createdSecret);

			var cards = secretsManager.GetSecretList();

			// We should know the secret is NOT Counterspell
			Assert.AreEqual(2, cards.Count);
			Assert.IsNull(cards.SingleOrDefault(c => Mage.Counterspell == c.Id && c.Count == 1));
			Assert.IsNotNull(cards.SingleOrDefault(c => Mage.ExplosiveRunes == c.Id && c.Count == 1));
			Assert.IsNotNull(cards.SingleOrDefault(c => Mage.IceBarrier == c.Id && c.Count == 1));

		}
	}
}
