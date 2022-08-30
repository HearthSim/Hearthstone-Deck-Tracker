#region

using System;
using System.Collections.Generic;
using System.Text;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Newtonsoft.Json;
using static HearthDb.CardIds.Collectible;
using static HearthDb.Enums.GameTag;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone.Entities
{
	[Serializable]
	public class Entity
	{
		[NonSerialized]
		private Card? _cachedCard;

		public Entity()
		{
			Tags = new Dictionary<GameTag, int>();
			Info = new EntityInfo(this);
		}

		public Entity(int id) : this()
		{
			Id = id;
		}

		private Entity(int id, Dictionary<GameTag, int> tags, EntityInfo info)
		{
			Id = id;
			Tags = tags;
			Info = info?.CloneWithNewEntity(this) ?? new EntityInfo(this);
		}

		public Entity Clone()
		{
			var entity = new Entity(Id, new Dictionary<GameTag, int>(Tags), Info)
			{
				Name = Name,
				CardId = CardId,
			};
			return entity;
		}

		public void ClearCardId()
		{
			CardId = null;
			Info.ClearCardId();
		}

		public EntityInfo Info { get; }
		public Dictionary<GameTag, int> Tags { get; set; }
		public string? Name { get; set; }
		public int Id { get; set; }
		public string? CardId { get; set; }

		/// <Summary>
		/// This is player entity, NOT the player hero.
		/// </Summary>
		public bool IsPlayer => GetTag(GameTag.PLAYER_ID) == Core.Game.Player.Id;

		[JsonIgnore]
		public bool IsHero => GetTag(GameTag.CARDTYPE) == (int)CardType.HERO;

		[JsonIgnore]
		public bool IsPlayableHero => IsHero && Card.CardSet != CardSet.HERO_SKINS && Card.Collectible;

		[JsonIgnore]
		public bool IsActiveDeathrattle => HasTag(GameTag.DEATHRATTLE) && GetTag(GameTag.DEATHRATTLE) == 1;

		/// <Summary>
		/// This is opponent entity, NOT the opponent hero.
		/// </Summary>
		[JsonIgnore]
		public bool IsOpponent => !IsPlayer && HasTag(GameTag.PLAYER_ID);

		[JsonIgnore]
		public bool IsMinionOrLocation => IsMinion || IsLocation;

		[JsonIgnore]
		public bool IsMinion => GetTag(GameTag.CARDTYPE) == (int)CardType.MINION;

		[JsonIgnore]
		public bool IsLocation => GetTag(GameTag.CARDTYPE) == (int)CardType.LOCATION;

		[JsonIgnore]
		public bool IsPlayableCard => IsMinionOrLocation || IsSpell || IsWeapon || IsPlayableHero;

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
		public bool IsQuest => HasTag(GameTag.QUEST) || IsQuestline;

		[JsonIgnore]
		public bool IsQuestline => HasTag(GameTag.QUESTLINE);

		[JsonIgnore]
		public bool IsQuestlinePart => IsQuestline && GetTag(GameTag.QUESTLINE_PART) > 1;

		[JsonIgnore]
		public bool IsSideQuest => HasTag(GameTag.SIDEQUEST);

		[JsonIgnore]
		public Card Card => _cachedCard ??= Database.GetCardFromId(CardId) ??
				new Card(string.Empty, null, Rarity.FREE, "unknown", "unknown", 0, "unknown", 0, 1, "", "", 0, 0, "unknown", null, 0, "", "", false);

		[JsonIgnore]
		public int Attack => GetTag(GameTag.ATK);

		[JsonIgnore]
		public int Health => GetTag(GameTag.HEALTH) - GetTag(GameTag.DAMAGE);

		[JsonIgnore]
		public int Cost => HasTag(GameTag.COST) ? GetTag(GameTag.COST) : Card.Cost;

		[JsonIgnore]
		public string? LocalizedName => Card.LocalizedName;

		public bool IsSecret => HasTag(GameTag.SECRET);

		public bool IsSpell => GetTag(GameTag.CARDTYPE) == (int)CardType.SPELL;

		public bool IsHeroPower => GetTag(GameTag.CARDTYPE) == (int)CardType.HERO_POWER;

		public bool IsBgsQuestReward => GetTag(GameTag.CARDTYPE) == (int)CardType.BATTLEGROUND_QUEST_REWARD;

		public bool IsCurrentPlayer => HasTag(GameTag.CURRENT_PLAYER);

		public bool HasCardId => !string.IsNullOrEmpty(CardId);

		public int ZonePosition
		{
			get
			{
				var fake = GetTag(GameTag.FAKE_ZONE_POSITION);
				if(fake > 0)
					return fake;
				return GetTag(GameTag.ZONE_POSITION);
			}
		}

		public bool IsInZone(Zone zone)
		{
			if((int)zone <= 0)
				return false;
			var fake = GetTag(GameTag.FAKE_ZONE);
			if(fake > 0)
				return (int)zone == fake;
			return (int)zone == GetTag(GameTag.ZONE);
		}

		public bool IsControlledBy(int controllerId)
		{
			var lettuceController = GetTag(GameTag.LETTUCE_CONTROLLER);
			if(lettuceController > 0)
				return lettuceController == controllerId;
			return HasTag(GameTag.CONTROLLER) && GetTag(GameTag.CONTROLLER) == controllerId;
		}

		public bool IsAttachedTo(int entityId) => GetTag(GameTag.ATTACHED) == entityId;

		public bool IsClass(CardClass cardClass) => GetTag(GameTag.CLASS) == (int)cardClass;

		public bool HasTag(GameTag tag) => GetTag(tag) > 0;

		public bool HasDredge()
		{
			return HasTag(DREDGE) || CardId == Warrior.FromTheDepths;
		}

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
		private string? _latestCardId;

		public EntityInfo(Entity entity)
		{
			_entity = entity;
		}

		public EntityInfo CloneWithNewEntity(Entity entity)
		{
			return new EntityInfo(entity)
			{
				Turn = Turn,
				DrawerId = DrawerId,
				Discarded = Discarded,
				Returned = Returned,
				Mulliganed = Mulliganed,
				Created = Created,
				HasOutstandingTagChanges = HasOutstandingTagChanges,
				OriginalController = OriginalController,
				Hidden = Hidden,
				CostReduction = CostReduction,
				OriginalZone = OriginalZone,
				OriginalCardId = OriginalCardId,
				OriginalEntityWasCreated = OriginalEntityWasCreated,
				GuessedCardState = GuessedCardState,
				LatestCardId = LatestCardId,
				StoredCardIds = StoredCardIds,
				DeckIndex = DeckIndex
			};
		}

		public int Turn { get; set; }

		public CardMark CardMark
		{
			get
			{
				if(Hidden)
					return CardMark.None;
				if(_entity.CardId == HearthDb.CardIds.NonCollectible.Neutral.TheCoinCore
					   || _entity.CardId == HearthDb.CardIds.NonCollectible.Neutral.TradePrinceGallywix_GallywixsCoinToken)
					return CardMark.Coin;
				if(Returned)
					return CardMark.Returned;
				if(Created || Stolen)
					return CardMark.Created;
				if(DrawnByEntity)
					return CardMark.DrawnByEntity;
				if(Mulliganed)
					return CardMark.Mulliganed;
				return CardMark.None;
			}
		}

		public void SetOriginalCardId(int dbfId)
		{
			if(dbfId <= 0)
				return;
			OriginalCardId = Database.GetCardFromDbfId(dbfId)?.Id;
		}

		public void ClearCardId()
		{
			OriginalCardId = null;
			LatestCardId = null;
		}

		public int GetCreatorId()
		{
			if(Hidden)
				return 0;
			var creatorId = _entity.GetTag(GameTag.DISPLAYED_CREATOR);
			if(creatorId == 0)
				creatorId = _entity.GetTag(GameTag.CREATOR);
			return creatorId;
		}

		public int? GetDrawerId()
		{
			return DrawerId;
		}

		public int? DrawerId { get; set; } = null;
		public bool Discarded { get; set; }
		public bool Returned { get; set; }
		public bool Mulliganed { get; set; }
		public bool Stolen => OriginalController > 0 && OriginalController != _entity.GetTag(GameTag.CONTROLLER);
		public bool DrawnByEntity => DrawerId != null;
		public bool Created { get; set; }
		public bool HasOutstandingTagChanges { get; set; }
		public int OriginalController { get; set; }
		public bool Hidden { get; set; }
		public int CostReduction { get; set; }
		public Zone? OriginalZone { get; set; }
		public string? OriginalCardId { get; private set; }
		public bool WasTransformed => !string.IsNullOrEmpty(OriginalCardId);
		public bool CreatedInDeck => OriginalZone == Zone.DECK;
		public bool CreatedInHand => OriginalZone == Zone.HAND;
		public bool? OriginalEntityWasCreated { get; internal set; }
		public GuessedCardState GuessedCardState { get; set; } = GuessedCardState.None;
		public List<string> StoredCardIds { get; set; } = new List<string>();
		public int DeckIndex { get; set; }

		public string? LatestCardId
		{
			get => _latestCardId ?? _entity.CardId;
			set => _latestCardId = value;
		}

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
			if(GuessedCardState != GuessedCardState.None)
				sb.Append(", guessedCardState=" + GuessedCardState);
			if(!string.IsNullOrEmpty(LatestCardId))
				sb.Append(", latestCardId=" + LatestCardId);
			if(StoredCardIds.Count > 0)
				sb.Append(", storedCardIds=[" + string.Join(", ", StoredCardIds) + "]");
			if(DeckIndex != 0)
				sb.Append(", deckIndex=" + DeckIndex);
			return sb.ToString();
		}
	}
}
