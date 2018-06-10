#region

using System;
using System.Collections.Generic;
using System.Text;
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
		// TODO: Replace CardSet check once the ICC update goes live
		public bool IsPlayableHero => IsHero && Card.CardSet != CardSet.CORE && Card.CardSet != CardSet.HERO_SKINS && Card.Collectible;

		[JsonIgnore]
		public bool IsActiveDeathrattle => HasTag(GameTag.DEATHRATTLE) && GetTag(GameTag.DEATHRATTLE) == 1;

		/// <Summary>
		/// This is opponent entity, NOT the opponent hero.
		/// </Summary>
		[JsonIgnore]
		public bool IsOpponent => !IsPlayer && HasTag(GameTag.PLAYER_ID);

		[JsonIgnore]
		public bool IsMinion => GetTag(GameTag.CARDTYPE) == (int)CardType.MINION;

		[JsonIgnore]
		public bool IsPlayableCard => IsMinion || IsSpell || IsWeapon || IsPlayableHero;

		[JsonIgnore]
		public bool IsWeapon => GetTag(GameTag.CARDTYPE) == (int)CardType.WEAPON;

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
		public bool IsQuest => HasTag(GameTag.QUEST);

		[JsonIgnore]
		public Card Card => _cachedCard ??
			(_cachedCard = Database.GetCardFromId(CardId) ??
				new Card(string.Empty, null, Rarity.FREE, "unknown", "unknown", 0, "unknown", 0, 1, "", "", 0, 0, "unknown", null, 0, "", ""));

		[JsonIgnore]
		public int Attack => GetTag(GameTag.ATK);

		[JsonIgnore]
		public int Health => GetTag(GameTag.HEALTH) - GetTag(GameTag.DAMAGE);

		[JsonIgnore]
		public int Cost => HasTag(GameTag.COST) ? GetTag(GameTag.COST) : Card.Cost;

		[JsonIgnore]
		public string LocalizedName => Card.LocalizedName;

		public bool IsSecret => HasTag(GameTag.SECRET);

		public bool IsSpell => GetTag(GameTag.CARDTYPE) == (int)CardType.SPELL;

		public bool IsHeroPower => GetTag(GameTag.CARDTYPE) == (int)CardType.HERO_POWER;

		public bool IsCurrentPlayer => HasTag(GameTag.CURRENT_PLAYER);

		public bool HasCardId => !string.IsNullOrEmpty(CardId);

		public bool IsInZone(Zone zone) => HasTag(GameTag.ZONE) && GetTag(GameTag.ZONE) == (int)zone;

		public bool IsControlledBy(int controllerId) => HasTag(GameTag.CONTROLLER) && GetTag(GameTag.CONTROLLER) == controllerId;

		public bool IsClass(CardClass cardClass) => GetTag(GameTag.CLASS) == (int)cardClass;

		public bool HasTag(GameTag tag) => GetTag(tag) > 0;

		public int GetTag(GameTag tag) => Tags.TryGetValue(tag, out var value) ? value : 0;

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

		public void SetOriginalCardId(int dbfId)
		{
			if(dbfId <= 0)
				return;
			OriginalCardId = Database.GetCardFromDbfId(dbfId).Id;
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
		public string OriginalCardId { get; private set; }
		public bool WasTransformed => !string.IsNullOrEmpty(OriginalCardId);
		public bool CreatedInDeck => OriginalZone == Zone.DECK;
		public bool CreatedInHand => OriginalZone == Zone.HAND;
		public bool? OriginalEntityWasCreated { get; internal set; }

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
			if(HasOutstandingTagChanges)
				sb.Append(", hasOutstandingTagChanges=true");
			if(Hidden)
				sb.Append(", hidden=true");
			if(OriginalController > 0)
				sb.Append(", OriginalController=" + OriginalController);
			if(!string.IsNullOrEmpty(OriginalCardId))
				sb.Append(", OriginalCardId=" + OriginalCardId);
			return sb.ToString();
		}
	}
}
