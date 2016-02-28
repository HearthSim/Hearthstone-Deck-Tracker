#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone.Entities
{
	[Serializable]
	public class Entity
	{
		private Card _cachedCard;

		public Entity()
		{
			Tags = new Dictionary<GAME_TAG, int>();
		}

		public Entity(int id)
		{
			Tags = new Dictionary<GAME_TAG, int>();
			Id = id;
		}

		public Dictionary<GAME_TAG, int> Tags { get; set; }
		public string Name { get; set; }
		public int Id { get; set; }
		public string CardId { get; set; }

		/// <Summary>
		/// This is player entity, NOT the player hero.
		/// </Summary>
		public bool IsPlayer { get; set; }

		[JsonIgnore]
		public bool IsHero => Database.IsHero(CardId);

		[JsonIgnore]
		public bool IsActiveDeathrattle => HasTag(GAME_TAG.DEATHRATTLE) && GetTag(GAME_TAG.DEATHRATTLE) == 1;

		/// <Summary>
		/// This is opponent entity, NOT the opponent hero.
		/// </Summary>
		[JsonIgnore]
		public bool IsOpponent => !IsPlayer && HasTag(GAME_TAG.PLAYER_ID);

		[JsonIgnore]
		public bool IsMinion => HasTag(GAME_TAG.CARDTYPE) && GetTag(GAME_TAG.CARDTYPE) == (int)TAG_CARDTYPE.MINION;

		[JsonIgnore]
		public bool IsWeapon => HasTag(GAME_TAG.CARDTYPE) && GetTag(GAME_TAG.CARDTYPE) == (int)TAG_CARDTYPE.WEAPON;

		[JsonIgnore]
		public bool IsInHand => IsInZone(TAG_ZONE.HAND);

		[JsonIgnore]
		public bool IsInPlay => IsInZone(TAG_ZONE.PLAY);

		[JsonIgnore]
		public bool IsInGraveyard => IsInZone(TAG_ZONE.GRAVEYARD);

		[JsonIgnore]
		public Card Card
			=>
				_cachedCard
				?? (_cachedCard =
					(Database.GetCardFromId(CardId)
					 ?? new Card(string.Empty, null, Rarity.Free, "unknown", "unknown", 0, "unknown", 0, 1, "", "", 0, 0, "unknown", null, 0, "", "")))
			;

		[JsonIgnore]
		public int Attack => GetTag(GAME_TAG.ATK);

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
		public int Health => GetTag(GAME_TAG.HEALTH) - GetTag(GAME_TAG.DAMAGE);

		[JsonIgnore]
		public SolidColorBrush HealthTextColor
		{
			get
			{
				var color = Colors.White;
				if(GetTag(GAME_TAG.DAMAGE) > 0)
					color = Colors.Red;
				else if(!string.IsNullOrEmpty(CardId) && Health > Card.Health)
					color = Colors.LawnGreen;

				return new SolidColorBrush(color);
			}
		}

		[JsonIgnore]
		public int Cost => HasTag(GAME_TAG.COST) ? GetTag(GAME_TAG.COST) : Card.Cost;

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
		public ImageBrush Background => Card.Background;

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
				var effects = "";
				if(HasTag(GAME_TAG.DIVINE_SHIELD))
					effects += "Divine Shield";
				if(HasTag(GAME_TAG.TAUNT))
					effects += (string.IsNullOrEmpty(effects) ? "" : "\n") + "Taunt";
				if(HasTag(GAME_TAG.STEALTH))
					effects += (string.IsNullOrEmpty(effects) ? "" : "\n") + "Stealth";
				if(HasTag(GAME_TAG.SILENCED))
					effects += (string.IsNullOrEmpty(effects) ? "" : "\n") + "Silenced";
				if(HasTag(GAME_TAG.FROZEN))
					effects += (string.IsNullOrEmpty(effects) ? "" : "\n") + "Frozen";
				if(HasTag(GAME_TAG.ENRAGED))
					effects += (string.IsNullOrEmpty(effects) ? "" : "\n") + "Enraged";
				return effects;
			}
		}

		[JsonIgnore]
		public Visibility EffectsVisibility => string.IsNullOrEmpty(Effects) ? Visibility.Collapsed : Visibility.Visible;

		public bool IsSecret => HasTag(GAME_TAG.SECRET);

		public bool IsSpell => GetTag(GAME_TAG.CARDTYPE) == (int)TAG_CARDTYPE.SPELL;

		public bool IsHeroPower => GetTag(GAME_TAG.CARDTYPE) == (int)TAG_CARDTYPE.HERO_POWER;

		public bool IsInZone(TAG_ZONE zone) => HasTag(GAME_TAG.ZONE) && GetTag(GAME_TAG.ZONE) == (int)zone;

		public bool IsControlledBy(int controllerId) => HasTag(GAME_TAG.CONTROLLER) && GetTag(GAME_TAG.CONTROLLER) == controllerId;

		public bool HasTag(GAME_TAG tag) => GetTag(tag) > 0;

		public int GetTag(GAME_TAG tag)
		{
			int value;
			Tags.TryGetValue(tag, out value);
			return value;
		}

		public void SetTag(GAME_TAG tag, int value)
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
			return $"id={Id}, cardId={CardId}, cardName={cardName}";
		}
	}
}