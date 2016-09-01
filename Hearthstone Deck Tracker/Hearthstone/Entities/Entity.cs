#region

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone.Entities
{
	[Serializable]
	public class Entity
	{
		[NonSerialized]
		private Card _cachedCard;

		public Entity()
		{
			Tags = new Dictionary<GameTag, int>();
			_info = new EntityInfo(this);
		}

		public Entity(int id) : this()
		{
			Id = id;
		}

		[NonSerialized]
		private readonly EntityInfo _info;
		public EntityInfo Info => _info;
		public Dictionary<GameTag, int> Tags { get; set; }
		public string Name { get; set; }
		public int Id { get; set; }
		public string CardId { get; set; }

		/// <Summary>
		/// This is player entity, NOT the player hero.
		/// </Summary>
		public bool IsPlayer => GetTag(GameTag.PLAYER_ID) == Core.Game.Player.Id;

		[JsonIgnore]
		public bool IsHero => GetTag(GameTag.CARDTYPE) == (int)CardType.HERO;

		[JsonIgnore]
		public bool IsActiveDeathrattle => HasTag(GameTag.DEATHRATTLE) && GetTag(GameTag.DEATHRATTLE) == 1;

		/// <Summary>
		/// This is opponent entity, NOT the opponent hero.
		/// </Summary>
		[JsonIgnore]
		public bool IsOpponent => !IsPlayer && HasTag(GameTag.PLAYER_ID);

		[JsonIgnore]
		public bool IsMinion => HasTag(GameTag.CARDTYPE) && GetTag(GameTag.CARDTYPE) == (int)CardType.MINION;

		[JsonIgnore]
		public bool IsWeapon => HasTag(GameTag.CARDTYPE) && GetTag(GameTag.CARDTYPE) == (int)CardType.WEAPON;

		[JsonIgnore]
		public bool IsInHand => IsInZone(Zone.HAND);

		[JsonIgnore]
		public bool IsInPlay => IsInZone(Zone.PLAY);

		[JsonIgnore]
		public bool IsInDeck => IsInZone(Zone.DECK);

		[JsonIgnore]
		public bool IsInGraveyard => IsInZone(Zone.GRAVEYARD);

		[JsonIgnore]
		public bool IsInSetAside => IsInZone(Zone.SETASIDE);

		[JsonIgnore]
		public bool IsInSecret => IsInZone(Zone.SECRET);

		[JsonIgnore]
		public Card Card
			=>
				_cachedCard
				?? (_cachedCard =
					(Database.GetCardFromId(CardId)
					 ?? new Card(string.Empty, null, Rarity.FREE, "unknown", "unknown", 0, "unknown", 0, 1, "", "", 0, 0, "unknown", null, 0, "", "")))
			;

		[JsonIgnore]
		public int Attack => GetTag(GameTag.ATK);

		[JsonIgnore]
		public SolidColorBrush AttackTextColor
		{
			get
			{
				var color = Colors.White;
				if(!string.IsNullOrEmpty(CardId) && Attack > Card.Attack)
					color = Colors.LawnGreen;
				return new SolidColorBrush(color);
			}
		}

		[JsonIgnore]
		public int Health => GetTag(GameTag.HEALTH) - GetTag(GameTag.DAMAGE);

		[JsonIgnore]
		public SolidColorBrush HealthTextColor
		{
			get
			{
				var color = Colors.White;
				if(GetTag(GameTag.DAMAGE) > 0)
					color = Colors.Red;
				else if(!string.IsNullOrEmpty(CardId) && Health > Card.Health)
					color = Colors.LawnGreen;

				return new SolidColorBrush(color);
			}
		}

		[JsonIgnore]
		public int Cost => HasTag(GameTag.COST) ? GetTag(GameTag.COST) : Card.Cost;

		[JsonIgnore]
		public SolidColorBrush CostTextColor
		{
			get
			{
				var color = Colors.White;
				if(!string.IsNullOrEmpty(CardId))
				{
					if(Cost < Card.Cost)
						color = Colors.LawnGreen;
					else if(Cost > Card.Cost)
						color = Colors.Red;
				}
				return new SolidColorBrush(color);
			}
		}

		[JsonIgnore]
		public DrawingBrush Background => Card.Background;

		[JsonIgnore]
		public FontFamily Font
		{
			get
			{
				var lang = Config.Instance.SelectedLanguage;
				var font = new FontFamily();
				// if the language uses a Latin script use Belwe font
				if(Helper.LatinLanguages.Contains(lang) || Config.Instance.NonLatinUseDefaultFont == false)
					font = new FontFamily(new Uri("pack://application:,,,/"), "./resources/#Belwe Bd BT");
				return font;
			}
		}

		[JsonIgnore]
		public string LocalizedName => Card.LocalizedName;

		[JsonIgnore]
		public string Effects
		{
			get
			{
				var effects = string.Empty;
				if(HasTag(GameTag.DIVINE_SHIELD))
					effects += "Divine Shield";
				if(HasTag(GameTag.TAUNT))
					effects += (string.IsNullOrEmpty(effects) ? string.Empty : Environment.NewLine) + "Taunt";
				if(HasTag(GameTag.STEALTH))
					effects += (string.IsNullOrEmpty(effects) ? string.Empty : Environment.NewLine) + "Stealth";
				if(HasTag(GameTag.SILENCED))
					effects += (string.IsNullOrEmpty(effects) ? string.Empty : Environment.NewLine) + "Silenced";
				if(HasTag(GameTag.FROZEN))
					effects += (string.IsNullOrEmpty(effects) ? string.Empty : Environment.NewLine) + "Frozen";
				if(HasTag(GameTag.ENRAGED))
					effects += (string.IsNullOrEmpty(effects) ? string.Empty : Environment.NewLine) + "Enraged";
				if(HasTag(GameTag.WINDFURY))
					effects += (string.IsNullOrEmpty(effects) ? string.Empty : Environment.NewLine) + "Windfury";
				return effects;
			}
		}

		[JsonIgnore]
		public Visibility EffectsVisibility => string.IsNullOrEmpty(Effects) ? Visibility.Collapsed : Visibility.Visible;

		public bool IsSecret => HasTag(GameTag.SECRET);

		public bool IsSpell => GetTag(GameTag.CARDTYPE) == (int)CardType.SPELL;

		public bool IsHeroPower => GetTag(GameTag.CARDTYPE) == (int)CardType.HERO_POWER;

		public bool IsCurrentPlayer => HasTag(GameTag.CURRENT_PLAYER);

		public bool HasCardId => !string.IsNullOrEmpty(CardId);

		public bool IsInZone(Zone zone) => HasTag(GameTag.ZONE) && GetTag(GameTag.ZONE) == (int)zone;

		public bool IsControlledBy(int controllerId) => HasTag(GameTag.CONTROLLER) && GetTag(GameTag.CONTROLLER) == controllerId;

		public bool HasTag(GameTag tag) => GetTag(tag) > 0;

		public int GetTag(GameTag tag)
		{
			int value;
			Tags.TryGetValue(tag, out value);
			return value;
		}

		public void SetTag(GameTag tag, int value)
		{
			if(!Tags.ContainsKey(tag))
				Tags.Add(tag, value);
			else
				Tags[tag] = value;
		}

		public void SetCardCount(int count) => Card.Count = count;

		public override string ToString()
		{
			var card = Database.GetCardFromId(CardId);
			var cardName = card != null ? card.Name : "";
			var hide = Info.Hidden && (IsInHand || IsInDeck);
			return $"id={Id}, cardId={(hide ? "" : CardId)}, cardName={(hide ? "" : cardName)}, zonePos={GetTag(GameTag.ZONE_POSITION)},Info={{{Info}}}";
        }
	}

	public class EntityInfo
	{
		private readonly Entity _entity;
		public EntityInfo(Entity entity)
		{
			_entity = entity;
		}

		public int Turn { get; set; }

		public CardMark CardMark
		{
			get
			{
				if(Hidden)
					return CardMark.None;
				if(_entity.CardId == HearthDb.CardIds.NonCollectible.Neutral.TheCoin
					   || _entity.CardId == HearthDb.CardIds.NonCollectible.Neutral.TradePrinceGallywix_GallywixsCoinToken)
					return CardMark.Coin;
				if(Returned)
					return CardMark.Returned;
				if(Created || Stolen)
					return CardMark.Created;
				if(Mulliganed)
					return CardMark.Mulliganed;
				return CardMark.None;
			}
		}

		public bool Discarded { get; set; }
		public bool Returned { get; set; }
		public bool Mulliganed { get; set; }
		public bool Stolen => OriginalController > 0 && OriginalController != _entity.GetTag(GameTag.CONTROLLER);
		public bool Created { get; set; }
		public bool HasOutstandingTagChanges { get; set; }
		public int OriginalController { get; set; }
		public bool Hidden { get; set; }
		public int CostReduction { get; set; }
		public Zone? OriginalZone { get; set; }
		public bool CreatedInDeck => OriginalZone == Zone.DECK;
		public bool CreatedInHand => OriginalZone == Zone.HAND;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("turn=" + Turn);
			if(CardMark != CardMark.None)
				sb.Append(", mark=" + CardMark);
			if(Discarded)
				sb.Append(", discarded=true");
			if(Returned)
				sb.Append(", returned=true");
			if(Mulliganed)
				sb.Append(", mulliganed=true");
			if(Stolen)
				sb.Append(", stolen=true");
			if(Created)
				sb.Append(", created=true");
			if(OriginalZone.HasValue)
				sb.Append(", originalZone=" + OriginalZone);
			return sb.ToString();
		}
	}
}