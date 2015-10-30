using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
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
			_instance.SetTag(GAME_TAG.ATK, attack);
			_instance.SetTag(GAME_TAG.HEALTH, health);
			_cardId = cardid;
		}

		public EntityBuilder Attack(int value)
		{
			_instance.SetTag(GAME_TAG.ATK, value);
			return this;
		}

		public EntityBuilder Health(int value)
		{
			_instance.SetTag(GAME_TAG.HEALTH, value);
			return this;
		}

		public EntityBuilder Armor(int value)
		{
			_instance.SetTag(GAME_TAG.ARMOR, value);
			return this;
		}

		public EntityBuilder Damage(int value)
		{
			_instance.SetTag(GAME_TAG.DAMAGE, value);
			return this;
		}

		public EntityBuilder Exhausted()
		{
			_instance.SetTag(GAME_TAG.EXHAUSTED, 1);
			return this;
		}

		public EntityBuilder AttacksThisTurn(int value)
		{
			_instance.SetTag(GAME_TAG.NUM_ATTACKS_THIS_TURN, value);
			return this;
		}
		
		public EntityBuilder Durability(int value)
		{
			_instance.SetTag(GAME_TAG.DURABILITY, value);
			return this;
		}

		public EntityBuilder Frozen()
		{
			_instance.SetTag(GAME_TAG.FROZEN, 1);
			return this;
		}

		public EntityBuilder Taunt()
		{
			_instance.SetTag(GAME_TAG.TAUNT, 1);
			return this;
		}		

		public EntityBuilder Charge()
		{
			_instance.SetTag(GAME_TAG.CHARGE, 1);
			return this;
		}

		public EntityBuilder Windfury()
		{
			_instance.SetTag(GAME_TAG.WINDFURY, 1);
			return this;
		}

		public EntityBuilder InPlay()
		{
			_instance.SetTag(GAME_TAG.ZONE, (int)TAG_ZONE.PLAY);
			return this;
		}

		public EntityBuilder Setaside()
		{
			_instance.SetTag(GAME_TAG.ZONE, (int)TAG_ZONE.SETASIDE);
			return this;
		}

		public EntityBuilder Graveyard()
		{
			_instance.SetTag(GAME_TAG.ZONE, (int)TAG_ZONE.GRAVEYARD);
			return this;
		}

		public EntityBuilder Deck()
		{
			_instance.SetTag(GAME_TAG.ZONE, (int)TAG_ZONE.DECK);
			return this;
		}

		public EntityBuilder Hand()
		{
			_instance.SetTag(GAME_TAG.ZONE, (int)TAG_ZONE.HAND);
			return this;
		}

		public EntityBuilder Invalid()
		{
			_instance.SetTag(GAME_TAG.ZONE, (int)TAG_ZONE.INVALID);
			return this;
		}

		public EntityBuilder CantAttack()
		{
			_instance.SetTag(GAME_TAG.CANT_ATTACK, 1);
			return this;
		}

		public EntityBuilder JustPlayed()
		{
			_instance.SetTag(GAME_TAG.JUST_PLAYED, 1);
			return this;
		}

		public EntityBuilder Weapon()
		{
			_instance.SetTag(GAME_TAG.CARDTYPE, (int)TAG_CARDTYPE.WEAPON);
			return this;
		}

		public EntityBuilder Hero()
		{
			_instance.SetTag(GAME_TAG.CARDTYPE, (int)TAG_CARDTYPE.HERO);
			return this;
		}

		public EntityBuilder Minion()
		{
			_instance.SetTag(GAME_TAG.CARDTYPE, (int)TAG_CARDTYPE.MINION);
			return this;
		}

		public Entity ToEntity()
		{
			if(string.IsNullOrWhiteSpace(_cardId))
			{
				return _instance;
			}
			else
			{
				_instance.CardId = _cardId;
				return _instance;
			}				
		}

		public CardEntity ToCardEntity()
		{
			return new CardEntity(ToEntity());			
		}

		public BoardCard ToBoardCard(bool active = true)
		{
			return new BoardCard(ToCardEntity(), active);
		}
	}
}
