#region

using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.BoardDamage
{
	public class BoardHero : IBoardEntity
	{
		private readonly int _baseAttack;
		private readonly BoardCard _hero;
		private readonly BoardCard _weapon;

		public BoardHero(Entity hero, Entity weapon, bool activeTurn)
		{
			_hero = new BoardCard(hero, activeTurn);
			// hero gains windfury with weapon, doubling attack get base attack
			_baseAttack = hero.GetTag(GameTag.ATK);
			if(weapon != null)
				_weapon = new BoardCard(weapon, activeTurn);
			Include = activeTurn && _hero.Include;
			Attack = AttackWithWeapon();
		}

		public bool HasWeapon => _weapon != null;

		public string Name => _hero.Name;

		// total health, including armor
		public int Health => _hero.Health;

		// total attack, weapon plus abilities
		public int Attack { get; }

		public int AttacksThisTurn => _hero.AttacksThisTurn;

		public bool Exhausted => _hero.Exhausted;

		public bool Include { get; }

		public string Zone => _hero.Zone;

		private int AttackWithWeapon()
		{
			if(Include)
			{
				// weapon is equipped
				if(_weapon != null)
				{
					// windfury weapon, with 2 or more charges,
					// and hero hasn't attacked yet this turn.
					if(_weapon.Windfury && _weapon.Health >= 2 && _hero.AttacksThisTurn == 0)
					{
						// double the hero attack value
						return _baseAttack * 2;
					}
				}
			}
			// otherwise normal hero attack is correct
			return _baseAttack;
		}
	}
}
