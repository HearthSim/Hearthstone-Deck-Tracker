using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.BoardDamage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.BoardDamage
{
	[TestClass]
	public class PlayerBoardTest
	{
		private List<CardEntity> _cards;

		[TestInitialize]
		public void Setup()
		{
			var hero = new EntityBuilder("HERO_01", 5, 30).ToCardEntity();
			var weapon = new EntityBuilder("DS1_188", 5, 0)
				.Weapon().Durability(2).ToCardEntity();
			_cards = new List<CardEntity>();
			_cards.Add(hero);
			_cards.Add(weapon);
			_cards.Add(new EntityBuilder("", 4, 4).Setaside().ToCardEntity());
			_cards.Add(new EntityBuilder("", 6, 4).InPlay().Taunt().ToCardEntity());
			_cards.Add(new EntityBuilder("", 3, 1).InPlay().Charge().ToCardEntity());
			_cards.Add(new EntityBuilder("", 3, 1).InPlay().Windfury().ToCardEntity());
			_cards.Add(new EntityBuilder("", 2, 2).InPlay().Exhausted().ToCardEntity());
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
			Assert.AreEqual(22, board.Damage);
		}

		[TestMethod]
		// Handles polymorphed and druid choice cards
		public void IgnoreSetaside()
		{
			var board = new PlayerBoard(_cards, true);
			Assert.AreEqual(6, board.Cards.Count);
		}

		[TestMethod]
		public void IgnoreGraveyard()
		{
			_cards.Add(new EntityBuilder("", 3, 3).Graveyard().ToCardEntity());
			var board = new PlayerBoard(_cards, true);
			Assert.AreEqual(6, board.Cards.Count);
		}

		[TestMethod]
		// happens when equip one on top of other
		public void AllowMultipleWeaponsOnBoard()
		{
			_cards.Add(new EntityBuilder("", 3, 0)
				.Weapon().JustPlayed().Durability(2).ToCardEntity());
			var board = new PlayerBoard(_cards, true);
			Assert.IsTrue(board.Hero.HasWeapon);
		}
	}
}
