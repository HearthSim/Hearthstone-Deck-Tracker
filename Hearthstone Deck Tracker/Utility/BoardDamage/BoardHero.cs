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
		private readonly BoardCard? _weapon;

		public BoardHero(Entity hero, Entity? weapon, bool activeTurn)
		{
			_hero = new BoardCard(hero, activeTurn);
			// hero gains windfury with weapon, doubling attack get base attack
			_baseAttack = hero.GetTag(GameTag.ATK);
			if(_baseAttack == 2147483647)
			{
				_baseAttack = 0;
				HasInfiniteAttack = true;
			}
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
		public bool HasInfiniteAttack { get; }

		public int AttacksThisTurn => _hero.AttacksThisTurn;

		public bool Exhausted => _hero.Exhausted;

		public bool Include { get; }

		public string Zone => _hero.Zone;

		private int AttackWithWeapon()
		{
			if(Include)
			{
				if(_weapon != null)
				{
					if((_hero.Windfury || _weapon.Windfury) && _weapon.Health >= 2 && _hero.AttacksThisTurn == 0)
					{
						return _baseAttack * 2;
					}

					if(_hero.Windfury && !_weapon.Windfury && _weapon.Health == 1)
					{
						return _baseAttack * 2 - _weapon.Attack;
					}

				}
				// Hero got windfury from other means (Inara, Sand Art Elemental)
				else if(_hero.Windfury && _hero.AttacksThisTurn == 0)
				{
					return _baseAttack * 2;
				}
			}
			// otherwise normal hero attack is correct
			return _baseAttack;
		}
	}
}
