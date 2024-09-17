#region

using System;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using static HearthDb.Enums.GameTag;

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
		private readonly bool _dormant;
		private readonly bool _isTitan;
		private readonly int _titanAbilitiesUsed;

		public BoardCard(Entity e, bool active = true)
		{
			var card = Database.GetCardFromId(e.CardId);
			var cardName = card?.Name != null ? card.Name : "";
			var name = string.IsNullOrEmpty(e.Name) ? cardName : e.Name!;

			_stdAttack = e.HasTag(HIDE_STATS) ? 0 : e.GetTag(ATK);
			_health = e.HasTag(HIDE_STATS) ? 0 : e.GetTag(HEALTH);
			_armor = e.GetTag(ARMOR);
			_durability = e.GetTag(DURABILITY);
			_damageTaken = e.GetTag(DAMAGE);
			Exhausted = e.GetTag(EXHAUSTED) == 1 || (e.GetTag(NUM_TURNS_IN_PLAY) == 0 && !e.IsHero);
			_cantAttack = e.GetTag(CANT_ATTACK) == 1;
			_frozen = e.GetTag(FROZEN) == 1;
			Silenced = e.GetTag(SILENCED) == 1;
			Charge = e.GetTag(CHARGE) == 1;
			Windfury = e.GetTag(WINDFURY) == 1;
			MegaWindfury = e.GetTag(MEGA_WINDFURY) == 1 || e.GetTag(WINDFURY) == 3;
			AttacksThisTurn = e.GetTag(NUM_ATTACKS_THIS_TURN);
			_dormant = e.GetTag(DORMANT) == 1;
			_isTitan = e.GetTag(TITAN) == 1;
			if(_isTitan)
			{
				if(e.GetTag(TITAN_ABILITY_USED_1) == 1) _titanAbilitiesUsed += 1;
				if(e.GetTag(TITAN_ABILITY_USED_2) == 1) _titanAbilitiesUsed += 1;
				if(e.GetTag(TITAN_ABILITY_USED_3) == 1) _titanAbilitiesUsed += 1;
			}

			Name = name;
			CardId = e.CardId;
			Taunt = e.GetTag(TAUNT) == 1;
			Zone = Enum.Parse(typeof(Zone), e.GetTag(ZONE).ToString()).ToString();
			CardType = Enum.Parse(typeof(CardType), e.GetTag(CARDTYPE).ToString()).ToString();

			Health = CalculateHealth(e.IsWeapon);
			Attack = CalculateAttack(active, e.IsWeapon);
			Include = IsAbleToAttack(active, e.IsWeapon);
		}

		public string? CardId { get; }
		public bool Silenced { get; private set;  }
		public bool Taunt { get; private set; }
		public bool Charge { get; }
		public bool Windfury { get; }
		public bool MegaWindfury { get; }
		public string CardType { get; private set; }

		public string Name { get; }
		public int Attack { get; }
		public int Health { get; }
		public bool Include { get; }

		public int AttacksThisTurn { get; }
		public int AttacksPerTurn
		{
			get
			{
				if(MegaWindfury && !Silenced)
					return 4;
				if(Windfury)
					return 2;
				return 1;
			}
		}

		public bool Exhausted { get; }

		public string Zone { get; }

		/// <summary>
		/// weapons use durability, instead of health
		/// include armor so heroes are correct
		/// </summary>
		/// <param name="isWeapon"></param>
		/// <returns></returns>
		private int CalculateHealth(bool isWeapon) => isWeapon ? _durability - _damageTaken : _health + _armor - _damageTaken;

		private int CalculateAttack(bool active, bool isWeapon)
		{
			var remainingAttacks = Math.Max(AttacksPerTurn - (active ? AttacksThisTurn : 0), 0);

			if(isWeapon)
				// for weapons, clamp remaining attacks to health
				remainingAttacks = Math.Min(remainingAttacks, Health);

			return remainingAttacks * _stdAttack;
		}

		private bool IsAbleToAttack(bool active, bool isWeapon)
		{
			// TODO: if frozen on turn, may be able to attack next turn
			// don't include weapons if an active turn, count Hero instead
			if(_cantAttack || _frozen || (isWeapon && active) || _dormant || (_isTitan && _titanAbilitiesUsed < 3))
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
			if(AttacksThisTurn == AttacksPerTurn)
			{
				return false;
			}
			// sometimes cards seem to be in wrong zone while in play,
			// these cards don't become exhausted, so check attacks.
			if(Zone.ToLower() == "deck" || Zone.ToLower() == "hand")
				return (!Windfury || AttacksThisTurn < 2) && (Windfury || AttacksThisTurn < 1);
			return true;
		}
	}
}
