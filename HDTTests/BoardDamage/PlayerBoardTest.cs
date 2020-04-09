using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.BoardDamage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.BoardDamage
{
	[TestClass]
	public class PlayerBoardTest
	{
		private List<Entity> _cards;

		[TestInitialize]
		public void Setup()
		{
			var hero = new EntityBuilder("HERO_01", 5, 30).Hero().ToEntity();
			var weapon = new EntityBuilder("DS1_188", 5, 0)
				.Weapon().Durability(2).ToEntity();
			_cards = new List<Entity>();
			_cards.Add(hero);
			_cards.Add(weapon);
			_cards.Add(new EntityBuilder("", 4, 4).Setaside().ToEntity());
			_cards.Add(new EntityBuilder("", 6, 4).InPlay().Taunt().ToEntity());
			_cards.Add(new EntityBuilder("", 3, 1).InPlay().Charge().ToEntity());
			_cards.Add(new EntityBuilder("", 3, 1).InPlay().Windfury().ToEntity());
			_cards.Add(new EntityBuilder("", 2, 2).InPlay().Exhausted().ToEntity());
			_cards.Add(new EntityBuilder("", 2, 2).InPlay().ZeroTurnsInPlay().ToEntity());
			_cards.Add(new EntityBuilder("", 10, 10).InPlay().Dormant().ToEntity());
		}

		[TestMethod]
		public void Damage_Active()
		{
			var board = new PlayerBoard(_cards, true);
			Assert.AreEqual(20, board.Damage);
		}

		[TestMethod]
		public void Damage_NotActive()
		{
			var board = new PlayerBoard(_cards, false);
			Assert.AreEqual(24, board.Damage);
		}

		[TestMethod]
		// Handles polymorphed and druid choice cards
		public void IgnoreSetaside()
		{
			var board = new PlayerBoard(_cards, true);
			Assert.AreEqual(8, board.Cards.Count);
		}

		[TestMethod]
		public void IgnoreGraveyard()
		{
			_cards.Add(new EntityBuilder("", 3, 3).Graveyard().ToEntity());
			var board = new PlayerBoard(_cards, true);
			Assert.AreEqual(8, board.Cards.Count);
		}

		[TestMethod]
		// happens when equip one on top of other
		public void AllowMultipleWeaponsOnBoard()
		{
			_cards.Add(new EntityBuilder("", 3, 0)
				.Weapon().JustPlayed().Durability(2).ToEntity());
			var board = new PlayerBoard(_cards, true);
			Assert.IsTrue(board.Hero.HasWeapon);
		}
	}
}
