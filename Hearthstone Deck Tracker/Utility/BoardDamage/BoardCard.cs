#region

using System;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.BoardDamage
{
	public class BoardCard : BoardEntity
	{
		private readonly bool _activeTurn;
		private readonly int _armor;
		private readonly bool _cantAttack;
		private readonly int _damageTaken;
		private readonly int _durability;
		private readonly bool _frozen;
		private readonly int _health;
		private bool _justPlayed;
		private readonly int _stdAttack;

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
			Exhausted = e.Entity.GetTag(GAME_TAG.EXHAUSTED) == 1 ? true : false;
			_cantAttack = e.Entity.GetTag(GAME_TAG.CANT_ATTACK) == 1 ? true : false;
			_frozen = e.Entity.GetTag(GAME_TAG.FROZEN) == 1 ? true : false;
			Charge = e.Entity.GetTag(GAME_TAG.CHARGE) == 1 ? true : false;
			Windfury = e.Entity.GetTag(GAME_TAG.WINDFURY) == 1 ? true : false;
			AttacksThisTurn = e.Entity.GetTag(GAME_TAG.NUM_ATTACKS_THIS_TURN);
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

		public string CardId { get; private set; }
		public bool Taunt { get; private set; }
		public bool Charge { get; private set; }
		public bool Windfury { get; private set; }
		public string CardType { get; private set; }

		public string Name { get; private set; }
		public int Attack { get; private set; }
		public int Health { get; private set; }
		public bool Include { get; private set; }

		public int AttacksThisTurn { get; private set; }
		public bool Exhausted { get; private set; }

		public string Zone { get; private set; }

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
				if(Charge && AttacksThisTurn == 0)
					return true;
				return false;
			}
				// sometimes cards seem to be in wrong zone while in play,
				// these cards don't become exhausted, so check attacks.
			if(Zone.ToLower() == "deck" || Zone.ToLower() == "hand")
			{
				if(Windfury && AttacksThisTurn >= 2)
					return false;
				if(!Windfury && AttacksThisTurn >= 1)
					return false;
				return true;
			}
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
				case 3:
				default:
					return _stdAttack;
			}
		}
	}
}