using System;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Utility.BoardDamage
{
	public class BoardCard : BoardEntity
	{
		private int _health;
		private int _armor;
		private int _damageTaken;
		private bool _exhausted;
		private bool _cantAttack;
		private bool _charge;
		private bool _windfury;
		private bool _justPlayed;
		private int _attacksThisTurn;
		private int _stdAttack;
		private int _durability;
		private bool _frozen;
		private bool _activeTurn;

		public string Name { get; private set; }
		public string CardId { get; private set; }
		public int Attack { get; private set; }
		public int Health { get; private set; }
		public bool Include { get; private set; }
		public bool Taunt { get; private set; }

		public int AttacksThisTurn { get { return _attacksThisTurn; } }
		public bool Exhausted { get { return _exhausted; } }
		public bool Charge { get { return _charge; } }
		public bool Windfury { get { return _windfury; } }

		public string Zone { get; private set; }
		public string CardType { get; private set; }

		public BoardCard(CardEntity e, bool active = true)
		{
			var card = Database.GetCardFromId(e.CardId);
			var cardName = card != null ? card.Name : "";
			var name = string.IsNullOrEmpty(e.Entity.Name) ? cardName : e.Entity.Name;

			_activeTurn = active;
			_stdAttack = e.Entity.GetTag(GAME_TAG.ATK);
			_health = e.Entity.GetTag(GAME_TAG.HEALTH);
			_armor = e.Entity.GetTag(GAME_TAG.ARMOR);
			_durability = e.Entity.GetTag(GAME_TAG.DURABILITY);
			_damageTaken = e.Entity.GetTag(GAME_TAG.DAMAGE);
			_exhausted = e.Entity.GetTag(GAME_TAG.EXHAUSTED) == 1 ? true : false;
			_cantAttack = e.Entity.GetTag(GAME_TAG.CANT_ATTACK) == 1 ? true : false;
			_frozen = e.Entity.GetTag(GAME_TAG.FROZEN) == 1 ? true : false;
			_charge = e.Entity.GetTag(GAME_TAG.CHARGE) == 1 ? true : false;
			_windfury = e.Entity.GetTag(GAME_TAG.WINDFURY) == 1 ? true : false;
			_attacksThisTurn = e.Entity.GetTag(GAME_TAG.NUM_ATTACKS_THIS_TURN);
			_justPlayed = e.Entity.GetTag(GAME_TAG.JUST_PLAYED) == 1 ? true : false;

			Name = name;
			CardId = e.CardId;
			Taunt = e.Entity.GetTag(GAME_TAG.TAUNT) == 1 ? true : false;
			Zone = Enum.Parse(typeof(TAG_ZONE), e.Entity.GetTag(GAME_TAG.ZONE).ToString()).ToString();
			CardType = Enum.Parse(typeof(TAG_CARDTYPE), e.Entity.GetTag(GAME_TAG.CARDTYPE).ToString()).ToString();

			Health = CalculateHealth(e.Entity.IsWeapon);
			Attack = CalculateAttack(_activeTurn, e.Entity.IsWeapon);
			Include = IsAbleToAttack(_activeTurn, e.Entity.IsWeapon);
		}

		private int CalculateHealth(bool isWeapon)
		{
			// weapons use durability, instead of health
			if(isWeapon)
				return _durability - _damageTaken;
			// include armor so heros are correct
			return _health + _armor - _damageTaken;
		}

		private int CalculateAttack(bool active, bool isWeapon)
		{
			// for weapons check for windfury and number of hits left
			if(isWeapon)
			{
				if(_windfury && Health >= 2 && _attacksThisTurn == 0)
				{
					return _stdAttack * 2;
				}
			}
			// for minions with windfury that haven't already attacked, double attack
			else if(_windfury && (!active || _attacksThisTurn == 0))
			{
				return _stdAttack * 2;
			}
			return _stdAttack;
		}

		private bool IsAbleToAttack(bool active, bool isWeapon)
		{
			// TODO: if frozen on turn, may be able to attack next turn
			// don't include weapons if an active turn, count Hero instead
			if(_cantAttack || _frozen || (isWeapon && active))
			{
				return false;
			}
			else if(!active)
			{
				// include everything that can attack if not an active turn
				return true;
			}
			else if(_exhausted)
			{
				// newly played card could be given charge
				if(_charge && _attacksThisTurn == 0)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return true;
			}
		}
	}
}
