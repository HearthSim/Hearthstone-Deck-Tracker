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
	}
}
