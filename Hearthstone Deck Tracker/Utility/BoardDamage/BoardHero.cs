#region

using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.BoardDamage
{
	public class BoardHero : BoardEntity
	{
		private bool _active;
		private readonly int _baseAttack;

		private readonly BoardCard _hero;
		private readonly BoardCard _weapon;

		public BoardHero(CardEntity hero, CardEntity weapon, bool activeTurn)
		{
			_active = activeTurn;
			_hero = new BoardCard(hero, activeTurn);
			// hero gains windfury with weapon, doubling attack get base attack
			_baseAttack = hero.Entity.GetTag(GAME_TAG.ATK);
			if(weapon != null)
				_weapon = new BoardCard(weapon, activeTurn);
			Include = activeTurn ? _hero.Include : false;
			Attack = AttackWithWeapon();
		}

		public bool HasWeapon
		{
			get { return _weapon != null; }
		}

		public string Name
		{
			get { return _hero.Name; }
		}

		// total health, including armor
		public int Health
		{
			get { return _hero.Health; }
		}

		// total attack, weapon plus abilities
		public int Attack { get; private set; }

		public int AttacksThisTurn
		{
			get { return _hero.AttacksThisTurn; }
		}

		public bool Exhausted
		{
			get { return _hero.Exhausted; }
		}

		public bool Include { get; private set; }

		public string Zone
		{
			get { return _hero.Zone; }
		}

		private int AttackWithWeapon()
		{
			if(Include)
			{
				// weapon is equipped
				if(_weapon != null)
				{
					// windfury weapon, with more than 2 chages
					// and hero hasn't attacked yet this turn.
					// better to check weapon for durability in 
					// case of windfury, instead of heros exhausted
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