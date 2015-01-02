using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.ModelBinding;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;

namespace Hearthstone_Deck_Tracker.Hearthstone.Entities
{
	[Serializable]
	public class Entity
	{
		public Dictionary<GAME_TAG, int> Tags { get; set; }
		public TAG_ZONE Zone { get; set; }
		public string Name { get; set; }
		public int Id { get; set; }
		public string CardId { get; set; }
		public string Type { get; set; }
		public int ZonePos { get; set; }
		public int Player { get; set; }
		public bool IsPlayer { get; set; }

		private Card _cachedCard;
		private Card Card
		{
			get { return _cachedCard ?? (_cachedCard = (Game.GetCardFromId(CardId) ?? new Card(string.Empty, null, "unknown", "unknown", "unknown", 0, "unknown", 0, 1, "", 0, 0, "unknown", null, 0, ""))); }
		}


		public int Attack
		{
			get { return GetTag(GAME_TAG.ATK); }
		}

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

		public int Health
		{
			get { return GetTag(GAME_TAG.HEALTH) - GetTag(GAME_TAG.DAMAGE); }
		}

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

		public int Cost
		{
			get
			{
				if(HasTag(GAME_TAG.COST))
					return GetTag(GAME_TAG.COST);
				return Card.Cost;
			}
		}

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

		public ImageBrush Background
		{
			get
			{
				return Card.Background;
			}
		}

		public string LocalizedName
		{
			get
			{
				return Card.LocalizedName;
			}
		}

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

		public Visibility EffectsVisibility
		{
			get { return string.IsNullOrEmpty(Effects) ? Visibility.Collapsed : Visibility.Visible; }
		}
		public Entity()
		{
			Tags = new Dictionary<GAME_TAG, int>();
		}
		public Entity(int id)
		{
			Tags = new Dictionary<GAME_TAG, int>();
			Id = id;
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

			if(tag == GAME_TAG.ZONE)
				Zone = (TAG_ZONE)value;
			
			//Logger.WriteLine(string.Format("[id={0} cardId={1} name={2} TAG={3}] {4} -> {5}", Id, CardId, Name, tag, prevVal, value));
		}
	}
}
