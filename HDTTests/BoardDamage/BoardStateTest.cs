using System.Collections.Generic;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.BoardDamage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.BoardDamage
{
	[TestClass]
	public class BoardStateTest
	{
		private List<CardEntity> _player;
		private List<CardEntity> _opponent;
		private Dictionary<int, Entity> _entities;

		[TestInitialize]
		public void Setup()
		{
			_entities = new Dictionary<int, Entity>();
			_entities[0] = new Entity(0);
			_entities[0].SetTag(GAME_TAG.FIRST_PLAYER, 1);
			_entities[0].IsPlayer = true;
			_entities[1] = new Entity(1);
			_entities[1].Name = "GameEntity";
			_entities[1].SetTag(GAME_TAG.TURN, 11);

			_player = new List<CardEntity>();
			_player.Add(new EntityBuilder("", 3, 1).InPlay().Charge().ToCardEntity());
			_player.Add(new EntityBuilder("", 4, 5).InPlay().ToCardEntity());
			_player.Add(new EntityBuilder("", 3, 2).InPlay().Exhausted().ToCardEntity());

			_opponent = new List<CardEntity>();
			_opponent.Add(new EntityBuilder("", 7, 5).InPlay().ToCardEntity());
			_opponent.Add(new EntityBuilder("", 3, 1).InPlay().Windfury().AttacksThisTurn(1).ToCardEntity());
			_opponent.Add(new EntityBuilder("", 2, 2).InPlay().Exhausted().ToCardEntity());
		}

		[TestMethod]
		public void IsDeadToBoard1()
		{
			var playerHero = new EntityBuilder("HERO_01", 0, 30).Damage(20).ToCardEntity();
			_player.Add(playerHero);
			var opponentHero = new EntityBuilder("HERO_02", 0, 30).Damage(10).ToCardEntity();
			_opponent.Add(opponentHero);

			var board = new BoardState(_player, _opponent, _entities, 1);

			Assert.IsTrue(board.IsPlayerDeadToBoard());
			Assert.IsFalse(board.IsOpponentDeadToBoard());
		}

		[TestMethod]
		public void IsDeadToBoard2()
		{
			var playerHero = new EntityBuilder("HERO_01", 0, 30).Damage(20).Armor(8).ToCardEntity();
			_player.Add(playerHero);
			var opponentHero = new EntityBuilder("HERO_02", 0, 30).Damage(25).ToCardEntity();
			_opponent.Add(opponentHero);

			var board = new BoardState(_player, _opponent, _entities, 1);

			Assert.IsFalse(board.IsPlayerDeadToBoard());
			Assert.IsTrue(board.IsOpponentDeadToBoard());
		}

		[TestMethod]
		// when hero is dead and removed from play
		public void NullOpponentHero()
		{
			var hero = new EntityBuilder("HERO_01", 0, 30).Damage(20).ToCardEntity();
			_player.Add(hero);
			var board = new BoardState(_player, _opponent, _entities, 1);

			Assert.IsTrue(board.IsOpponentDeadToBoard());
		}

		[TestMethod]
		public void NullPlayerHero()
		{
			var hero = new EntityBuilder("HERO_01", 0, 30).Damage(20).ToCardEntity();
			_opponent.Add(hero);
			var board = new BoardState(_player, _opponent, _entities, 1);

			Assert.IsTrue(board.IsPlayerDeadToBoard());
		}

		[TestMethod]
		public void NoHeros()
		{
			var board = new BoardState(_player, _opponent, _entities, 1);
			Assert.IsTrue(board.IsPlayerDeadToBoard());
		}

		[TestMethod]
		public void WindfuryWeaponEquipped()
		{
			_player.Add(new EntityBuilder("HERO_01", 5, 30).InPlay().Hero().Damage(20).ToCardEntity());
			_player.Add(new EntityBuilder("DS1_188", 5, 0).InPlay().Weapon().Windfury().Durability(4).ToCardEntity());
			_opponent.Add(new EntityBuilder("HERO_02", 0, 30).InPlay().Hero().Damage(10).ToCardEntity());

			var board = new BoardState(_player, _opponent, _entities, 1);

			Assert.AreEqual(17, board.Player.Damage);
		}
	}
}
