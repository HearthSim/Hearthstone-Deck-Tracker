#region

using System;
using System.Collections.Generic;
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
		public bool IsPlayer { get; set; }

		[JsonIgnore]
		public bool IsOpponent
		{
			get { return !IsPlayer && HasTag(GAME_TAG.PLAYER_ID); }
		}

		[JsonIgnore]
		public bool IsMinion
		{
			get { return HasTag(GAME_TAG.CARDTYPE) && GetTag(GAME_TAG.CARDTYPE) == (int)TAG_CARDTYPE.MINION; }
		}

		[JsonIgnore]
		public bool IsWeapon
		{
			get { return HasTag(GAME_TAG.CARDTYPE) && GetTag(GAME_TAG.CARDTYPE) == (int)TAG_CARDTYPE.WEAPON; }
		}

		[JsonIgnore]
		public bool IsInHand
		{
			get { return IsInZone(TAG_ZONE.HAND); }
		}

		[JsonIgnore]
		public bool IsInPlay
		{
			get { return IsInZone(TAG_ZONE.PLAY); }
		}

		[JsonIgnore]
		public bool IsInGraveyard
		{
			get { return IsInZone(TAG_ZONE.GRAVEYARD); }
		}

		[JsonIgnore]
		public Card Card
		{
			get
			{
				return _cachedCard
				       ?? (_cachedCard =
				           (GameV2.GetCardFromId(CardId)
				            ?? new Card(string.Empty, null, "unknown", "unknown", "unknown", 0, "unknown", 0, 1, "", "", 0, 0, "unknown", null, 0, "",
				                        "")));
			}
		}

		[JsonIgnore]
		public int Attack
		{
			get { return GetTag(GAME_TAG.ATK); }
		}

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
		public int Health
		{
			get { return GetTag(GAME_TAG.HEALTH) - GetTag(GAME_TAG.DAMAGE); }
		}

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
		public int Cost
		{
			get
			{
				if(HasTag(GAME_TAG.COST))
					return GetTag(GAME_TAG.COST);
				return Card.Cost;
			}
		}

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
		public ImageBrush Background
		{
			get { return Card.Background; }
		}

		[JsonIgnore]
		public string LocalizedName
		{
			get { return Card.LocalizedName; }
		}

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
		public Visibility EffectsVisibility
		{
			get { return string.IsNullOrEmpty(Effects) ? Visibility.Collapsed : Visibility.Visible; }
		}

		public bool IsInZone(TAG_ZONE zone)
		{
			return HasTag(GAME_TAG.ZONE) && GetTag(GAME_TAG.ZONE) == (int)zone;
		}

		public bool IsControlledBy(int controllerId)
		{
			return HasTag(GAME_TAG.CONTROLLER) && GetTag(GAME_TAG.CONTROLLER) == controllerId;
		}

		public bool HasTag(GAME_TAG tag)
		{
			return GetTag(tag) > 0;
		}

		public int GetTag(GAME_TAG tag)
		{
			int value;
			Tags.TryGetValue(tag, out value);
			return value;
		}

		public void SetTag(GAME_TAG tag, int value)
		{
			var prevVal = 0;
			if(!Tags.ContainsKey(tag))
				Tags.Add(tag, value);
			else
			{
				prevVal = Tags[tag];
				Tags[tag] = value;
			}
			//Logger.WriteLine(string.Format("[id={0} cardId={1} name={2} TAG={3}] {4} -> {5}", Id, CardId, Name, tag, prevVal, value));
		}

		public void SetCardCount(int count)
		{
			Card.Count = count;
		}
	}
}