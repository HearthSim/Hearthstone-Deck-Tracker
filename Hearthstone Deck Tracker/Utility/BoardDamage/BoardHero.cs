using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Utility.BoardDamage
{
	public class BoardHero : BoardEntity
	{
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
		public int Attack
		{
			get { return _attack; }
		}
		public int AttacksThisTurn
		{
			get { return _hero.AttacksThisTurn; }
		}
		public bool Exhausted
		{
			get { return _hero.Exhausted; }
		}
		public bool Include
		{
			get { return _ableToAttack; }
		}
		public bool HasWeapon
		{
			get { return _weapon != null; }
		}
		public string Zone
		{
			get { return _hero.Zone; }
		}

		private BoardCard _hero;
		private BoardCard _weapon;
		private bool _active;
		private bool _ableToAttack;
		private int _attack;
		private int _baseAttack;

		public BoardHero(CardEntity hero, CardEntity weapon, bool activeTurn)
		{
			_active = activeTurn;
			_hero = new BoardCard(hero, activeTurn);
			// hero gains windfury with weapon, doubling attack get base attack
			_baseAttack = hero.Entity.GetTag(Enums.GAME_TAG.ATK);
			if(weapon != null)
				_weapon = new BoardCard(weapon, activeTurn);
			_ableToAttack = activeTurn ? _hero.Include : false;
			_attack = AttackWithWeapon();
		}

		private int AttackWithWeapon()
		{
			if(_ableToAttack)
			{
				// weapon is equipped
				if(_weapon != null)
				{
					// windfury weapon, with more than 2 chages
					// and hero hasn't attacked yet this turn.
					// better to check weapon for durability in 
					// case of windfury, instead of heros exhausted
					if(_weapon.Windfury && _weapon.Health >= 2
						&& _hero.AttacksThisTurn == 0)
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
