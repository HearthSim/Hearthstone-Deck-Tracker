using HearthDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.BoardDamage
{
	[TestClass]
	public class BoardCardTest
	{
		private EntityBuilder _minion;
		private EntityBuilder _weapon;

		[TestInitialize]
		public void Setup()
		{
			_minion = new EntityBuilder("CS1_069", 3, 6);
			_weapon = new EntityBuilder("DS1_188", 5, 0);
			_weapon.Weapon().Durability(2);
		}

		[TestMethod]
		public void PropertyAssignment()
		{
			var card = _minion.Charge().InPlay().ToBoardCard();
			Assert.AreEqual("Fen Creeper", card.Name);
			Assert.AreEqual(3, card.Attack);
			Assert.AreEqual(6, card.Health);
			Assert.AreEqual("PLAY", card.Zone);
			Assert.IsTrue(card.Charge);
		}

		[TestMethod]
		public void HealthMinion()
		{
			var card = _minion.Damage(2).ToBoardCard();
			Assert.AreEqual(4, card.Health);
		}

		[TestMethod]
		public void HealthWeapon()
		{
			var card = _weapon.Damage(1).ToBoardCard();
			Assert.AreEqual(1, card.Health);
		}

		[TestMethod]
		public void DontInclude_IfCantAttack()
		{
			var card = _minion.CantAttack().ToBoardCard();
			Assert.IsFalse(card.Include);
		}

		[TestMethod]
		public void DontInclude_IfFrozen()
		{
			var card = _minion.Frozen().ToBoardCard();
			Assert.IsFalse(card.Include);
		}

		[TestMethod]
		public void DontInclude_IfExhausted()
		{
			var card = _minion.Exhausted().ToBoardCard();
			Assert.IsFalse(card.Include);
		}

		[TestMethod]
		public void DontInclude_IfInDeckZoneAndAttacked()
		{
			var card = _minion.Deck().AttacksThisTurn(1).ToBoardCard();
			Assert.IsFalse(card.Include);	
		}

		[TestMethod]
		public void DontInclude_IfInHandZoneAndWindfuryAttackedTwice()
		{
			var card = _minion.Hand().Windfury().AttacksThisTurn(2).ToBoardCard();
			Assert.IsFalse(card.Include);
		}

		[TestMethod]
		public void Include_IfInDeckHandAndNotAttacked()
		{
			var card = _minion.Hand().AttacksThisTurn(0).ToBoardCard();
			Assert.IsTrue(card.Include);
		}

		[TestMethod]
		public void Include_IfInDeckZoneAndWindfuryAttackedOnce()
		{
			var card = _minion.Deck().Windfury().AttacksThisTurn(1).ToBoardCard();
			Assert.IsTrue(card.Include);
		}

		[TestMethod]
		public void Include_IfExhaustedAndCharged()
		{
			var card = _minion.Exhausted().Charge().AttacksThisTurn(0).ToBoardCard();
			Assert.IsTrue(card.Include);
		}

		[TestMethod]
		public void DontInclude_IfExhaustedAndCharged()
		{
			var card = _minion.Exhausted().Charge().AttacksThisTurn(1).ToBoardCard();
			Assert.IsFalse(card.Include);
		}

		[TestMethod]
		public void Include_IfNotActive()
		{
			var card = _minion.Exhausted().ToBoardCard(false);
			Assert.IsTrue(card.Include);
		}

		[TestMethod]
		public void DontInclude_WeaponWhenActive()
		{
			var card = _weapon.ToBoardCard();
			Assert.IsFalse(card.Include);
		}

		[TestMethod]
		public void Include_WeaponWhenPassive()
		{
			var card = _weapon.ToBoardCard(false);
			Assert.IsTrue(card.Include);
		}

		[TestMethod]
		public void Attack_WithWindfury()
		{
			var card = _minion.Windfury().ToBoardCard();
			Assert.AreEqual(6, card.Attack);
		}

		[TestMethod]
		public void Attack_WithWindfuryAlreadyAttacked()
		{
			var card = _minion.Windfury().AttacksThisTurn(1).ToBoardCard();
			Assert.AreEqual(3, card.Attack);
		}

		[TestMethod]
		public void Attack_Weapon()
		{
			var card = _weapon.ToBoardCard();
			Assert.AreEqual(5, card.Attack);
		}

		[TestMethod]
		public void Attack_WeaponWithWindfury()
		{
			var card = _weapon.Windfury().ToBoardCard();
			Assert.AreEqual(10, card.Attack);
		}

		[TestMethod]
		public void Attack_WeaponWithWindfuryAttackedOnce()
		{
			var card = _weapon.Windfury().AttacksThisTurn(1).ToBoardCard();
			Assert.AreEqual(5, card.Attack);
		}

		[TestMethod]
		public void Attack_WeaponWithWindfuryOneHitLeft()
		{
			var card = _weapon.Windfury().Durability(2).Damage(1).ToBoardCard();
			Assert.AreEqual(5, card.Attack);
		}

		[TestMethod]
		public void Attack_MegaWindfury_V07TR0N()
		{
			var eb = new EntityBuilder("GVG_111t", 4, 8).Windfury().Charge().InPlay();

			Assert.AreEqual(16, eb.ToBoardCard().Attack);
			Assert.AreEqual(16, eb.Exhausted().ToBoardCard(false).Attack);
			Assert.AreEqual(12, eb.AttacksThisTurn(1).ToBoardCard().Attack);
			Assert.AreEqual(8, eb.AttacksThisTurn(2).ToBoardCard().Attack);
			Assert.AreEqual(4, eb.AttacksThisTurn(3).ToBoardCard().Attack);
			Assert.AreEqual(4, eb.AttacksThisTurn(4).ToBoardCard().Attack);
		}

		[TestMethod]
		public void AttackHealth_EntityHasHideStats()
		{
			var eb = new EntityBuilder(CardIds.NonCollectible.Neutral.TheDarkness_TheDarkness, 20, 20).InPlay();

			var card = eb.ToBoardCard();
			Assert.AreEqual(20, card.Attack);
			Assert.AreEqual(20, card.Health);

			var hidden = eb.HideStats().ToBoardCard();
			Assert.AreEqual(0, hidden.Attack);
			Assert.AreEqual(0, hidden.Health);
		}

		[TestMethod]
		public void Include_MegaWindfury_V07TR0N()
		{
			var eb = new EntityBuilder("GVG_111t", 4, 8).Windfury().Charge().InPlay();

			Assert.IsTrue(eb.ToBoardCard().Include);
			Assert.IsTrue(eb.AttacksThisTurn(1).ToBoardCard().Include);
			Assert.IsTrue(eb.AttacksThisTurn(3).ToBoardCard().Include);
			Assert.IsFalse(eb.AttacksThisTurn(4).Exhausted().ToBoardCard().Include);
		}
	}
}
