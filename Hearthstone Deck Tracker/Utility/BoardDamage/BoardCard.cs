#region

using System;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using static Hearthstone_Deck_Tracker.Enums.GAME_TAG;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.BoardDamage
{
	public class BoardCard : IBoardEntity
	{
		private readonly int _armor;
		private readonly bool _cantAttack;
		private readonly int _damageTaken;
		private readonly int _durability;
		private readonly bool _frozen;
		private readonly int _health;
		private readonly int _stdAttack;

		public BoardCard(CardEntity e, bool active = true)
		{
			var card = Database.GetCardFromId(e.CardId);
			var cardName = card != null ? card.Name : "";
			var name = string.IsNullOrEmpty(e.Entity.Name) ? cardName : e.Entity.Name;

			_stdAttack = e.Entity.GetTag(ATK);
			_health = e.Entity.GetTag(HEALTH);
			_armor = e.Entity.GetTag(ARMOR);
			_durability = e.Entity.GetTag(DURABILITY);
			_damageTaken = e.Entity.GetTag(DAMAGE);
			Exhausted = e.Entity.GetTag(EXHAUSTED) == 1;
			_cantAttack = e.Entity.GetTag(CANT_ATTACK) == 1;
			_frozen = e.Entity.GetTag(FROZEN) == 1;
			Charge = e.Entity.GetTag(CHARGE) == 1;
			Windfury = e.Entity.GetTag(WINDFURY) == 1;
			AttacksThisTurn = e.Entity.GetTag(NUM_ATTACKS_THIS_TURN);

			Name = name;
			CardId = e.CardId;
			Taunt = e.Entity.GetTag(TAUNT) == 1;
			Zone = Enum.Parse(typeof(TAG_ZONE), e.Entity.GetTag(ZONE).ToString()).ToString();
			CardType = Enum.Parse(typeof(TAG_CARDTYPE), e.Entity.GetTag(CARDTYPE).ToString()).ToString();

			Health = CalculateHealth(e.Entity.IsWeapon);
			Attack = CalculateAttack(active, e.Entity.IsWeapon);
			Include = IsAbleToAttack(active, e.Entity.IsWeapon);
		}

		public string CardId { get; }
		public bool Taunt { get; private set; }
		public bool Charge { get; }
		public bool Windfury { get; }
		public string CardType { get; private set; }

		public string Name { get; }
		public int Attack { get; }
		public int Health { get; }
		public bool Include { get; }

		public int AttacksThisTurn { get; }
		public bool Exhausted { get; }

		public string Zone { get; }

		/// <summary>
		///  weapons use durability, instead of health
		/// include armor so heros are correct
		/// </summary>
		/// <param name="isWeapon"></param>
		/// <returns></returns>
		private int CalculateHealth(bool isWeapon) => isWeapon ? _durability - _damageTaken : _health + _armor - _damageTaken;

		private int CalculateAttack(bool active, bool isWeapon)
		{
			// V-07-TR-0N is a special case Mega-Windfury
			if(!string.IsNullOrEmpty(CardId) && CardId == "GVG_111t")
				return V07TRONAttack(active);
				// for weapons check for windfury and number of hits left
			if(isWeapon)
			{
				if(Windfury && Health >= 2 && AttacksThisTurn == 0)
					return _stdAttack * 2;
			}
			// for minions with windfury that haven't already attacked, double attack
			else if(Windfury && (!active || AttacksThisTurn == 0))
				return _stdAttack * 2;
			return _stdAttack;
		}

		private bool IsAbleToAttack(bool active, bool isWeapon)
		{
			// TODO: if frozen on turn, may be able to attack next turn
			// don't include weapons if an active turn, count Hero instead
			if(_cantAttack || _frozen || (isWeapon && active))
				return false;
			if(!active)
			{
				// include everything that can attack if not an active turn
				return true;
			}
			if(Exhausted)
			{
				// newly played card could be given charge
				return Charge && AttacksThisTurn == 0;
			}
			// sometimes cards seem to be in wrong zone while in play,
				// these cards don't become exhausted, so check attacks.
			if(Zone.ToLower() == "deck" || Zone.ToLower() == "hand")
				return (!Windfury || AttacksThisTurn < 2) && (Windfury || AttacksThisTurn < 1);
			return true;
		}

		private int V07TRONAttack(bool active)
		{
			if(!active)
				return _stdAttack * 4;

			switch(AttacksThisTurn)
			{
				case 0:
					return _stdAttack * 4;
				case 1:
					return _stdAttack * 3;
				case 2:
					return _stdAttack * 2;
				default:
					return _stdAttack;
			}
		}
	}
}