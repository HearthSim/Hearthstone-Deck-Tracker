using System.Collections.Generic;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.BoardDamage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.BoardDamage
{
	[TestClass]
	public class BoardStateTest
	{
		private List<Entity> _player;
		private List<Entity> _opponent;
		private Dictionary<int, Entity> _entities;
		private GameV2 _game;

		[TestInitialize]
		public void Setup()
		{
			Core.Game = null;
			_game = new GameV2();
			Core.Game = _game;
			_game.Player.Id = 0;

			_entities = new Dictionary<int, Entity>();
			_entities[0] = new Entity(0);
			_entities[0].SetTag(GameTag.FIRST_PLAYER, 1);
			_entities[1] = new Entity(1);
			_entities[1].Name = "GameEntity";
			_entities[1].SetTag(GameTag.TURN, 11);

			_player = new List<Entity>();
			_player.Add(new EntityBuilder("", 3, 1).InPlay().Charge().ToEntity());
			_player.Add(new EntityBuilder("", 4, 5).InPlay().ToEntity());
			_player.Add(new EntityBuilder("", 3, 2).InPlay().Exhausted().ToEntity());

			_opponent = new List<Entity>();
			_opponent.Add(new EntityBuilder("", 7, 5).InPlay().ToEntity());
			_opponent.Add(new EntityBuilder("", 3, 1).InPlay().Windfury().AttacksThisTurn(1).ToEntity());
			_opponent.Add(new EntityBuilder("", 2, 2).InPlay().Exhausted().ToEntity());
		}

		[TestMethod]
		public void IsDeadToBoard1()
		{
			var playerHero = new EntityBuilder("HERO_01", 0, 30).Hero().Damage(20).ToEntity();
			_player.Add(playerHero);
			var opponentHero = new EntityBuilder("HERO_02", 0, 30).Hero().Damage(10).ToEntity();
			_opponent.Add(opponentHero);

			var board = new BoardState(_player, _opponent, _entities, 1);

			Assert.IsTrue(board.IsPlayerDeadToBoard());
			Assert.IsFalse(board.IsOpponentDeadToBoard());
		}

		[TestMethod]
		public void IsDeadToBoard2()
		{
			var playerHero = new EntityBuilder("HERO_01", 0, 30).Hero().Damage(20).Armor(8).ToEntity();
			_player.Add(playerHero);
			var opponentHero = new EntityBuilder("HERO_02", 0, 30).Hero().Damage(25).ToEntity();
			_opponent.Add(opponentHero);

			var board = new BoardState(_player, _opponent, _entities, 1);

			Assert.IsFalse(board.IsPlayerDeadToBoard());
			Assert.IsTrue(board.IsOpponentDeadToBoard());
		}

		[TestMethod]
		// when hero is dead and removed from play
		public void NullOpponentHero()
		{
			var hero = new EntityBuilder("HERO_01", 0, 30).Hero().Damage(20).ToEntity();
			_player.Add(hero);
			var board = new BoardState(_player, _opponent, _entities, 1);

			Assert.IsTrue(board.IsOpponentDeadToBoard());
		}

		[TestMethod]
		public void NullPlayerHero()
		{
			var hero = new EntityBuilder("HERO_01", 0, 30).Hero().Damage(20).ToEntity();
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
			_player.Add(new EntityBuilder("HERO_01", 5, 30).InPlay().Hero().Damage(20).ToEntity());
			_player.Add(new EntityBuilder("DS1_188", 5, 0).InPlay().Weapon().Windfury().Durability(4).ToEntity());
			_opponent.Add(new EntityBuilder("HERO_02", 0, 30).InPlay().Hero().Damage(10).ToEntity());

			var board = new BoardState(_player, _opponent, _entities, 1);

			Assert.AreEqual(17, board.Player.Damage);
		}
	}
}
