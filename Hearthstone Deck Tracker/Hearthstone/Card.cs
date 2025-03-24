#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.Tooltips;
using Hearthstone_Deck_Tracker.Hearthstone.CardExtraInfo;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Hearthstone_Deck_Tracker.Utility.Themes;
using NuGet;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public struct CardWinrates
	{
		public float MulliganWinrate;
		public float? BaseWinrate;
	}

	[Serializable]
	public class Card : ViewModel, ICloneable, ICardTooltip
	{
		public string Id { get; init; }

		[NonSerialized]
		private HearthDb.Card? _data;
		public HearthDb.Card? Data => _data ??= HearthDb.Cards.All.TryGetValue(Id, out var data) ? data : null;

		public bool IsKnownCard => Data != null;

		[NonSerialized]
		private static readonly Dictionary<string, Dictionary<int, CardImageObject>> CardImageCache = new();


		// Typo - Do not remove this for Plugin compatibility
		[Obsolete("Use DbfId instead", true), UsedImplicitly]
		public int DbfIf => Data?.DbfId ?? 0;

		public int DbfId => Data?.DbfId ?? 0;

		// Can be overwritten
		[XmlIgnore]
		public string[]? Mechanics
		{
			get => GetProp<string[]?>(null) ?? Data?.Mechanics;
			set => SetProp(value);
		}

		// Can be overwritten
		[XmlIgnore]
		public int Attack
		{
			get => GetProp<int?>(null) ?? Data?.Attack ?? 0;
			set => SetProp(value);
		}

		// Can be overwritten
		[XmlIgnore]
		public int Health
		{
			get => GetProp<int?>(null) ?? Data?.Health ?? 0;
			set => SetProp(value);
		}

		// Can be overwritten
		[XmlIgnore]
		public int Cost
		{
			get => GetProp<int?>(null) ?? Data?.Cost ?? 0;
			set => SetProp(value);
		}

		public string? PlayerClass => Data != null ? HearthDbConverter.ConvertClass(Data.Class) : null;

		public Rarity Rarity => Data?.Rarity ?? Rarity.INVALID;

		[XmlIgnore]
		public bool BaconCard { get; set; }

		public bool IsBaconMinion => BaconCard && TypeEnum == CardType.MINION;

		[XmlIgnore]
		public bool BaconTriple;

		[XmlIgnore]
		public int DeckListIndex;

		[XmlIgnore]
		public Player? ControllerPlayer { get; set; }

		public List<Card>? RelatedCards
		{
			get
			{
				if(ControllerPlayer == null)
					return null;
				var relatedCards = Core.Game.RelatedCardsManager.GetCardWithRelatedCards(Id)?.GetRelatedCards(ControllerPlayer).WhereNotNull().ToList() ?? new();
				// Get related cards from Entity
				if (relatedCards.IsEmpty())
				{
					foreach(var entity in ControllerPlayer.Deck.Where(x => x.CardId == Id))
						relatedCards.AddRange(entity.Info.StoredCardIds.Select(Database.GetCardFromId).WhereNotNull());
				}

				if(relatedCards.IsEmpty())
					return null;
				return relatedCards;
			}
		}

		public void UpdateRelatedCards() => OnPropertyChanged(nameof(RelatedCards));


		private static Locale? _selectedLanguage;
		private static Locale SelectedLanguage => _selectedLanguage ??= Enum.TryParse(Helper.GetCardLanguage(), out Locale lang) ? lang : Locale.enUS;

		public Card()
		{
			Id = "unknown";
		}

		public Card(string id)
		{
			Id = id;
		}

		public Card(HearthDb.Card data, bool baconCard = false)
		{
			_data = data;
			Id = data.Id;
			BaconCard = baconCard;
		}

		public int Count
		{
			get => GetProp(1);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(Background));
			}
		}

		[XmlIgnore]
		public bool IsMulliganOption
		{
			get => GetProp(false);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(Background));
			}
		}

		[XmlIgnore]
		public CardWinrates? CardWinrates
		{
			get => GetProp<CardWinrates?>(null);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(Background));
			}
		}

		[XmlIgnore]
		public bool Jousted
		{
			get => GetProp(false);
			set => SetProp(value);
		}

		public string Text => CleanUpText(Data?.GetLocText(SelectedLanguage));

		public string? EnglishText => CleanUpText(Data?.GetLocText(Locale.enUS));

		public string? Set => Data != null ? HearthDbConverter.SetConverter(Data.Set) : null;

		public CardSet? CardSet => Data?.Set;

		public CardClass CardClass => Data?.Class ?? CardClass.INVALID;

		public int TechLevel => Data?.Entity.GetTag(GameTag.TECH_LEVEL) ?? 0;

		public int BattlegroundsSkinParentId => Data?.Entity.GetTag(GameTag.BACON_SKIN_PARENT_ID) ?? 0;

		public int GetTag(GameTag gameTag) => Data?.Entity.GetTag(gameTag) ?? 0;

		public Race? RaceEnum => Data?.Race;
		public Race? SecondaryRaceEnum => Data?.SecondaryRace;

		public int LettuceCooldown => Data?.Entity.GetTag(GameTag.LETTUCE_COOLDOWN_CONFIG) ?? 0;

		public string? Race => Data != null ? HearthDbConverter.RaceConverter(Data.Race) : null;

		public bool HasRace(Race race)
		{
			if(race == RaceEnum || race == SecondaryRaceEnum)
				return true;
			if(race == HearthDb.Enums.Race.BLANK)
				return false;
			if(RaceEnum == HearthDb.Enums.Race.ALL || SecondaryRaceEnum == HearthDb.Enums.Race.ALL)
				return true;
			return false;
		}

		public bool IsMech() => HasRace(HearthDb.Enums.Race.MECHANICAL);

		public bool IsDemon() => HasRace(HearthDb.Enums.Race.DEMON);

		public bool IsBeast() => HasRace(HearthDb.Enums.Race.BEAST);

		public bool IsMurloc() => HasRace(HearthDb.Enums.Race.MURLOC);

		public bool IsDragon() => HasRace(HearthDb.Enums.Race.DRAGON);

		public bool IsPirate() => HasRace(HearthDb.Enums.Race.PIRATE);

		public bool IsElemental() => HasRace(HearthDb.Enums.Race.ELEMENTAL);

		public bool IsQuillboar() => HasRace(HearthDb.Enums.Race.QUILBOAR);

		public bool IsUndead() => HasRace(HearthDb.Enums.Race.UNDEAD);

		public bool IsNaga() => HasRace(HearthDb.Enums.Race.NAGA);

		public bool isDraenei() => HasRace(HearthDb.Enums.Race.DRAENEI);

		public bool IsEmptyRace() => RaceEnum == HearthDb.Enums.Race.INVALID && SecondaryRaceEnum == HearthDb.Enums.Race.INVALID;

		public string? Type => Data != null ? HearthDbConverter.CardTypeConverter(Data.Type) : null;

		public CardType? TypeEnum => Data?.Type;

		public string? Name => Data?.GetLocName(Locale.enUS);

		public bool HideStats => Data?.Entity.GetTag(GameTag.HIDE_STATS) == 1;
		public bool HideCost => Data?.Entity.GetTag(GameTag.HIDE_COST) == 1 || (Cost == 0 && (EnglishText?.Contains("Passive") ?? false));

		private static readonly Regex _overloadRegex = new(@"Overload:.+?\((?<value>(\d+))\)");
		private int? _overload;
		public int Overload
		{
			get
			{
				if(_overload.HasValue)
					return _overload.Value;
				var overload = -1;
				if(!string.IsNullOrEmpty(EnglishText))
				{
					var match = _overloadRegex.Match(EnglishText);
					if(match.Success)
						int.TryParse(match.Groups["value"].Value, out overload);
					_overload = overload;
				}
				return overload;
			}
		}

		public int DustCost => Rarity switch
		{
			Rarity.COMMON => 40,
			Rarity.RARE => 100,
			Rarity.EPIC => 400,
			Rarity.LEGENDARY => 1600,
			_ => 0
		};

		public string? LocalizedName => Data?.GetLocName(SelectedLanguage) ?? Name;

		[XmlIgnore]
		public int InHandCount
		{
			get => GetProp(0);
			set => SetProp(value);
		}

		public bool IsClassCard => GetPlayerClass != "Neutral";

		[XmlIgnore]
		public bool IsCreated
		{
			get => GetProp(false);
			set => SetProp(value);
		}

		[XmlIgnore]
		public bool WasDiscarded
		{
			get => GetProp(false);
			set => SetProp(value);
		}

		[XmlIgnore]
		public ICardExtraInfo? ExtraInfo
		{
			get => GetProp<ICardExtraInfo?>(null);
			set => SetProp(value);
		}

		public string GetPlayerClass => PlayerClass ?? "Neutral";

		public bool IsClass(string? playerClass)
		{
			if(playerClass == null)
				return false;
			return GetClasses().Contains(playerClass);
		}

		public bool IsNeutral => GetPlayerClass == "Neutral" && (Data?.Entity.GetTag(GameTag.MULTIPLE_CLASSES) ?? 0) == 0;

		public IEnumerable<string> GetClasses()
		{
			var multipleClasses = Data?.Entity.GetTag(GameTag.MULTIPLE_CLASSES) ?? 0;
			if(multipleClasses == 0)
			{
				yield return GetPlayerClass;
				yield break;
			}

			var cardClass = 1;
			while(multipleClasses != 0)
			{
				if(1 == (multipleClasses & 1))
				{
					var className = HearthDbConverter.ConvertClass((CardClass)cardClass);
					yield return className ?? "Neutral";
				}
				multipleClasses >>= 1;
				cardClass++;
			}
		}

		private static readonly Dictionary<GameTag, CardClass> TOURIST_MAP = new ()
		{
			{ GameTag.MAGE_TOURIST, CardClass.MAGE },
			{ GameTag.PALADIN_TOURIST, CardClass.PALADIN },
			{ GameTag.WARRIOR_TOURIST, CardClass.WARRIOR },
			{ GameTag.HUNTER_TOURIST, CardClass.HUNTER },
			{ GameTag.ROGUE_TOURIST, CardClass.ROGUE },
			{ GameTag.DRUID_TOURIST, CardClass.DRUID },
			{ GameTag.SHAMAN_TOURIST, CardClass.SHAMAN },
			{ GameTag.WARLOCK_TOURIST, CardClass.WARLOCK },
			{ GameTag.PRIEST_TOURIST, CardClass.PRIEST },
			{ GameTag.DEMON_HUNTER_TOURIST, CardClass.DEMONHUNTER },
			{ GameTag.DEATH_KNIGHT_TOURIST, CardClass.DEATHKNIGHT }
		};

		private static readonly Dictionary<CardClass, CardClass> TOURIST_VISIT_MAP = new ()
		{
			{ CardClass.ROGUE, CardClass.PALADIN },
			{ CardClass.WARLOCK, CardClass.ROGUE },
			{ CardClass.DEATHKNIGHT, CardClass.WARLOCK },
			{ CardClass.SHAMAN, CardClass.DEATHKNIGHT },
			{ CardClass.DEMONHUNTER, CardClass.SHAMAN },
			{ CardClass.PRIEST, CardClass.DEMONHUNTER },
			{ CardClass.HUNTER, CardClass.PRIEST },
			{ CardClass.WARRIOR, CardClass.HUNTER },
			{ CardClass.DRUID, CardClass.WARRIOR },
			{ CardClass.MAGE, CardClass.DRUID },
		};

		public bool IsTourist => Data?.Entity.GetTag(GameTag.TOURIST) > 0;

		public bool CanBeVisitedByTourist => !IsTourist && CardSet == HearthDb.Enums.CardSet.ISLAND_VACATION;

		public string? GetTouristClass()
		{
			if(!IsTourist)
				return null;

			foreach (var tag in TOURIST_MAP)
			{
				if (Data?.Entity.GetTag(tag.Key) > 0)
					return HearthDbConverter.ConvertClass(tag.Value);
			}

			return null;
		}

		public string? GetTouristVisitClass()
		{
			if(!CanBeVisitedByTourist)
				return null;

			var cardClass = Data?.Class ?? CardClass.INVALID;


			if (TOURIST_VISIT_MAP.TryGetValue(cardClass, out var visitClass))
				return HearthDbConverter.ConvertClass(visitClass);

			return null;
		}

		public GameTag? GetFaction()
		{
			var factions = new[]
			{
				GameTag.KABAL,
				GameTag.GRIMY_GOONS,
				GameTag.JADE_LOTUS,
				GameTag.PROTOSS,
				GameTag.TERRAN,
				GameTag.ZERG
			};

			return factions.FirstOrDefault(faction => Data?.Entity.GetTag(faction) > 0);
		}

		public bool ZilliaxCustomizableFunctionalModule => Data?.Entity.GetTag(GameTag.ZILLIAX_CUSTOMIZABLE_FUNCTIONALMODULE) > 0;

		public bool ZilliaxCustomizableCosmeticModule => Data?.Entity.GetTag(GameTag.ZILLIAX_CUSTOMIZABLE_COSMETICMODULE) > 0;

		public Card DeckbuildingCard
		{
			get
			{
				if(ZilliaxCustomizableCosmeticModule)
				{
					return Database.GetCardFromId(HearthDb.CardIds.Collectible.Neutral.ZilliaxDeluxe3000) ?? this;
				}

				return this;
			}
		}

		public SolidColorBrush ColorPlayer
		{
			get
			{
				Color color;
				if(HighlightInHand && Config.Instance.HighlightCardsInHand)
					color = Colors.GreenYellow;
				else if(WasDiscarded && Config.Instance.HighlightDiscarded)
					color = Colors.IndianRed;
				else
					color = Colors.White;
				return new SolidColorBrush(color);
			}
		}

		public static FontFamily DefaultFont => Helper.UseLatinFont() ? new FontFamily(new Uri("pack://application:,,,/"), "./Resources/#Chunkfive") : new FontFamily();

		public static FontWeight DefaultFontWeight => Helper.UseLatinFont() ? FontWeights.Normal : FontWeights.Bold;

		private CardBarImageBuilder? GetImageBuilder()
		{
			if(BaconCard)
			{
				var theme = ThemeManager.FindTheme("dark");
				if(theme != null)
					return new DarkBarImageBuilder(this, theme.Directory);
			}
			return ThemeManager.GetBarImageBuilder(this);
		}

		private int _backgroundImageTries = 0;
		public DrawingBrush Background
		{
			get
			{
				var cardImageObj = new CardImageObject(this);
				if(CardImageCache.TryGetValue(Id, out var cache))
				{
					if(cache.TryGetValue(cardImageObj.GetHashCode(), out var cached))
						return cached.Image ?? new DrawingBrush();
				}
				try
				{
					var image = GetImageBuilder()?.Build(async (success) => {
						// This only gets invoked if the image was not available
						// in memory or on disk.
						if(_backgroundImageTries > 3)
							return;
						_backgroundImageTries++;

						// Ensure the Background_get call completed before
						// remove the entry from cache.
						await Task.Yield();
						CardImageCache.Remove(Id);

						if(success)
						{
							// Force background to update when card image becomes available.
							// If loading the art was not sucessful we don't
							// force an update, but just try again next time.
							Update();
						}
					}) ?? new DrawingBrush();

					if (image.CanFreeze)
						image.Freeze();
					cardImageObj = new CardImageObject(image, this);
					if(cache == null)
					{
						cache = new Dictionary<int, CardImageObject>();
						CardImageCache.Add(Id, cache);
					}
					cache.Add(cardImageObj.GetHashCode(), cardImageObj);
					return cardImageObj.Image ?? new DrawingBrush();
				}
				catch(Exception ex)
				{
					Log.Error($"Image builder failed: {ex.Message}");
					return new DrawingBrush();
				}
			}
		}

		static Card()
		{
			CardDefsManager.CardsChanged += ReloadTileImages;
			ThemeManager.ThemeChanged += ReloadTileImages;
			Helper.CardLanguageChanged += ReloadTileImages;
		}

		public static void ReloadTileImages()
		{
			CardImageCache.Clear();
			_selectedLanguage = null;
		}

		internal void Update() => OnPropertyChanged(nameof(Background));
		internal void UpdateHighlight() => OnPropertyChanged(nameof(Highlight));

		public ImageBrush Highlight => ThemeManager.CurrentTheme?.HighlightImage ?? new ImageBrush();

		[XmlIgnore]
		public bool HighlightInHand
		{
			get => GetProp(false);
			set => SetProp(value);
		}

		public string FormattedFlavorText => CleanUpText(Data?.GetLocFlavorText(SelectedLanguage), false) ?? "";

		public bool Collectible => Data?.Collectible ?? false;

		public object Clone()
		{
			var card = _data != null ? new Card(_data) : new Card(Id);

			if(TryGetProp(nameof(Count), out int count))
				card.Count = count;
			if(TryGetProp(nameof(InHandCount), out int inHandCount))
				card.InHandCount = inHandCount;
			if(TryGetProp(nameof(Attack), out int attack))
				card.Attack = attack;
			if(TryGetProp(nameof(Health), out int health))
				card.Health = health;
			if(TryGetProp(nameof(Cost), out int cost))
				card.Cost = cost;
			if(TryGetProp(nameof(Mechanics), out string[]? mechanics))
				card.Mechanics = mechanics;

			card.BaconCard = BaconCard;
			card.ControllerPlayer = ControllerPlayer;

			return card;
		}

		public override string ToString() => Name + "(" + Count + ")";

		public override bool Equals(object card)
		{
			if(!(card is Card))
				return false;
			var c = (Card)card;
			return c.Id == Id;
		}

		public bool EqualsWithCount(Card card) => card.Id == Id && card.Count == Count;

		public override int GetHashCode() => (Id ?? "").GetHashCode();

		private static string CleanUpText(string? text, bool replaceTags = true)
		{
			if (replaceTags)
				text = text?.Replace("<b>", "").Replace("</b>", "").Replace("<i>", "").Replace("</i>", "") ?? string.Empty;
			return text?.Replace("$", "").Replace("#", "").Replace("\\n", "\n").Replace("[x]", "") ?? string.Empty;
		}

		public void UpdateTooltip(CardTooltipViewModel viewModel)
		{
			viewModel.Card = this;
			viewModel.ShowTriple = BaconCard;
			viewModel.RelatedCards = RelatedCards;
		}
	}

	internal class CardImageObject
	{
		public DrawingBrush? Image { get; }
		public int Count { get; }
		public int Cost { get; }
		public bool Jousted { get; }
		public bool ColoredFrame { get; }
		public bool ColoredGem { get; }
		public bool Created { get; }
		public string? Theme { get; }
		public int TextColorHash { get; }
		public bool BaconCard { get; }
		public CardWinrates? CardWinrates { get;  }
		public bool IsMulliganOption { get; }
		public ICardExtraInfo? ExtraInfo { get; }


		public CardImageObject(DrawingBrush image, Card card) : this(card)
		{
			Image = image;
		}

		public CardImageObject(Card card)
		{
			Count = card.Count;
			Cost = card.Cost;
			Jousted = card.Jousted;
			ColoredFrame = Config.Instance.RarityCardFrames;
			ColoredGem = Config.Instance.RarityCardGems;
			Theme = ThemeManager.CurrentTheme?.Name;
			TextColorHash = card.ColorPlayer.Color.GetHashCode();
			Created = card.IsCreated;
			BaconCard = card.BaconCard;
			CardWinrates = card.CardWinrates;
			IsMulliganOption = card.IsMulliganOption;
			ExtraInfo = card.ExtraInfo?.Clone() as ICardExtraInfo;
		}

		public override bool Equals(object obj)
		{
			var cardObj = obj as CardImageObject;
			return cardObj != null && Equals(cardObj);
		}

		protected bool Equals(CardImageObject other)
			=> Count == other.Count && Cost == other.Cost && Jousted == other.Jousted && ColoredFrame == other.ColoredFrame && ColoredGem == other.ColoredGem
				&& string.Equals(Theme, other.Theme) && TextColorHash == other.TextColorHash && Created == other.Created && BaconCard == other.BaconCard
				&& (CardWinrates?.Equals(other.CardWinrates) ?? CardWinrates.HasValue == other.CardWinrates.HasValue) && IsMulliganOption == other.IsMulliganOption
				&& ExtraInfo?.CardNameSuffix == other.ExtraInfo?.CardNameSuffix;

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Count + 1;
				hashCode = (hashCode * 397) ^ (Cost + 1);
				hashCode = (hashCode * 397) ^ (Jousted.GetHashCode() + 1);
				hashCode = (hashCode * 397) ^ (ColoredFrame.GetHashCode() + 1);
				hashCode = (hashCode * 397) ^ (ColoredGem.GetHashCode() + 1);
				hashCode = (hashCode * 397) ^ (Theme?.GetHashCode() ?? 1);
				hashCode = (hashCode * 397) ^ TextColorHash;
				hashCode = (hashCode * 397) ^ (Created.GetHashCode() + 1);
				hashCode = (hashCode * 397) ^ (BaconCard.GetHashCode() + 1);
				hashCode = (hashCode * 397) ^ (CardWinrates?.GetHashCode() ?? 1);
				hashCode = (hashCode * 397) ^ (IsMulliganOption.GetHashCode() + 1);
				hashCode = (hashCode * 397) ^ (ExtraInfo?.CardNameSuffix?.GetHashCode() ?? 1);
				return hashCode;
			}
		}
	}
}
