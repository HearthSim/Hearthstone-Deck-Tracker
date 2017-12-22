using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.BoardDamage;

namespace HDTTests.BoardDamage
{
	public class EntityBuilder
	{
		private Entity _instance;
		private string _cardId;

		public EntityBuilder()
		{
			_instance = new Entity();
		}

		public EntityBuilder(string cardid, int attack, int health)
		{
			_instance = new Entity();
			_instance.SetTag(GameTag.ATK, attack);
			_instance.SetTag(GameTag.HEALTH, health);
			_cardId = cardid;
		}

		public EntityBuilder Attack(int value)
		{
			_instance.SetTag(GameTag.ATK, value);
			return this;
		}

		public EntityBuilder Health(int value)
		{
			_instance.SetTag(GameTag.HEALTH, value);
			return this;
		}

		public EntityBuilder Armor(int value)
		{
			_instance.SetTag(GameTag.ARMOR, value);
			return this;
		}

		public EntityBuilder Damage(int value)
		{
			_instance.SetTag(GameTag.DAMAGE, value);
			return this;
		}

		public EntityBuilder Exhausted()
		{
			_instance.SetTag(GameTag.EXHAUSTED, 1);
			return this;
		}

		public EntityBuilder AttacksThisTurn(int value)
		{
			_instance.SetTag(GameTag.NUM_ATTACKS_THIS_TURN, value);
			return this;
		}
		
		public EntityBuilder Durability(int value)
		{
			_instance.SetTag(GameTag.DURABILITY, value);
			return this;
		}

		public EntityBuilder Frozen()
		{
			_instance.SetTag(GameTag.FROZEN, 1);
			return this;
		}

		public EntityBuilder Taunt()
		{
			_instance.SetTag(GameTag.TAUNT, 1);
			return this;
		}		

		public EntityBuilder Charge()
		{
			_instance.SetTag(GameTag.CHARGE, 1);
			return this;
		}

		public EntityBuilder Windfury()
		{
			_instance.SetTag(GameTag.WINDFURY, 1);
			return this;
		}

		public EntityBuilder InPlay()
		{
			_instance.SetTag(GameTag.ZONE, (int)Zone.PLAY);
			return this;
		}

		public EntityBuilder Setaside()
		{
			_instance.SetTag(GameTag.ZONE, (int)Zone.SETASIDE);
			return this;
		}

		public EntityBuilder Graveyard()
		{
			_instance.SetTag(GameTag.ZONE, (int)Zone.GRAVEYARD);
			return this;
		}

		public EntityBuilder Deck()
		{
			_instance.SetTag(GameTag.ZONE, (int)Zone.DECK);
			return this;
		}

		public EntityBuilder Hand()
		{
			_instance.SetTag(GameTag.ZONE, (int)Zone.HAND);
			return this;
		}

		public EntityBuilder Invalid()
		{
			_instance.SetTag(GameTag.ZONE, (int)Zone.INVALID);
			return this;
		}

		public EntityBuilder CantAttack()
		{
			_instance.SetTag(GameTag.CANT_ATTACK, 1);
			return this;
		}

		public EntityBuilder JustPlayed()
		{
			_instance.SetTag(GameTag.JUST_PLAYED, 1);
			return this;
		}

		public EntityBuilder Weapon()
		{
			_instance.SetTag(GameTag.CARDTYPE, (int)CardType.WEAPON);
			return this;
		}

		public EntityBuilder Hero()
		{
			_instance.SetTag(GameTag.CARDTYPE, (int)CardType.HERO);
			return this;
		}

		public EntityBuilder Minion()
		{
			_instance.SetTag(GameTag.CARDTYPE, (int)CardType.MINION);
			return this;
		}

		public EntityBuilder HideStats()
		{
			_instance.SetTag(GameTag.HIDE_STATS, 1);
			return this;
		}

		public Entity ToEntity()
		{
			if(string.IsNullOrWhiteSpace(_cardId))
				return _instance;
			_instance.CardId = _cardId;
			return _instance;
		}

		public BoardCard ToBoardCard(bool active = true)
		{
			return new BoardCard(ToEntity(), active);
		}
	}
}
