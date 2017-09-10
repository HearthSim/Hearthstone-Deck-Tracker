#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.Themes;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	[Serializable]
	public class Card : ICloneable, INotifyPropertyChanged
	{
		[NonSerialized]
		private HearthDb.Card _dbCard;

		private readonly Regex _overloadRegex = new Regex(@"Overload:.+?\((?<value>(\d+))\)");
		private int _count;
		private string _englishText;
		private int _inHandCount;
		private bool _isCreated;
		private bool _loaded;
		private string _localizedName;
		private int? _overload;
		private string _text;
		private bool _wasDiscarded;
		private string _id;

		[NonSerialized]
		private static readonly Dictionary<string, Dictionary<int, CardImageObject>> CardImageCache =
			new Dictionary<string, Dictionary<int, CardImageObject>>();

		[XmlIgnore]
		public List<string> AlternativeNames = new List<string>();

		[XmlIgnore]
		public List<string> AlternativeTexts = new List<string>();

		public string Id
		{
			get { return _id; }
			set
			{
				_id = value;
				if(_dbCard == null)
					Load();
			}
		}

		[XmlIgnore]
		public int DbfIf => _dbCard?.DbfId ?? 0;

		/// The mechanics attribute, such as windfury or taunt, comes from the cardDB json file
		[XmlIgnore]
		public string[] Mechanics;

		[XmlIgnore]
		public string PlayerClass;

		[XmlIgnore]
		public Rarity Rarity;

		public Card()
		{
			Count = 1;
		}

		public Card(string id, string playerClass, Rarity rarity, string type, string name, int cost, string localizedName, int inHandCount,
		            int count, string text, string englishText, int attack, int health, string race, string[] mechanics, int? durability,
		            string artist, string set, List<string> alternativeNames = null, List<string> alternativeTexts = null, HearthDb.Card dbCard = null)
		{
			Id = id;
			PlayerClass = playerClass;
			Rarity = rarity;
			Type = type;
			Name = name;
			Cost = cost;
			LocalizedName = localizedName;
			InHandCount = inHandCount;
			Count = count;
			Text = text;
			EnglishText = englishText;
			Attack = attack;
			Health = health;
			Race = race;
			Durability = durability;
			Mechanics = mechanics;
			Artist = artist;
			Set = set;
			if(alternativeNames != null)
				AlternativeNames = alternativeNames;
			if(alternativeTexts != null)
				AlternativeTexts = alternativeTexts;
			_dbCard = dbCard;
		}

		private Locale? _selectedLanguage;

		private Locale SelectedLanguage
		{
			get
			{
				if(_selectedLanguage.HasValue)
					return _selectedLanguage.Value;
				if(!Enum.TryParse(Config.Instance.SelectedLanguage, out Locale lang))
					lang = Locale.enUS;
				_selectedLanguage = lang;
				return _selectedLanguage.Value;
			}
		}

		public Card(HearthDb.Card dbCard)
		{
			_dbCard = dbCard;
			Id = dbCard.Id;
			Count = 1;
			PlayerClass = HearthDbConverter.ConvertClass(dbCard.Class);
			Rarity = dbCard.Rarity;
			Type = HearthDbConverter.CardTypeConverter(dbCard.Type);
			Name = dbCard.GetLocName(Locale.enUS);
			Cost = dbCard.Cost;
			LocalizedName = dbCard.GetLocName(SelectedLanguage);
			Text = dbCard.GetLocText(SelectedLanguage);
			EnglishText = dbCard.GetLocText(Locale.enUS);
			Attack = dbCard.Attack;
			Health = dbCard.Health;
			Race = HearthDbConverter.RaceConverter(dbCard.Race);
			Durability = dbCard.Durability > 0 ? (int?)dbCard.Durability : null;
			Mechanics = dbCard.Mechanics;
			Artist = dbCard.ArtistName;
			Set = HearthDbConverter.SetConverter(dbCard.Set);
			foreach(var altLangStr in Config.Instance.AlternativeLanguages)
			{
				if(Enum.TryParse(altLangStr, out Locale altLang))
				{
					AlternativeNames.Add(dbCard.GetLocName(altLang));
					AlternativeTexts.Add(dbCard.GetLocText(altLang));
				}
			}
			_loaded = true;
		}

		public int Count
		{
			get { return _count; }
			set
			{
				_count = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(Background));
			}
		}

		[XmlIgnore]
		public bool Jousted { get; set; }

		[XmlIgnore]
		public int Attack { get; set; }

		[XmlIgnore]
		public int Health { get; set; }

		[XmlIgnore]
		public string Text
		{
			get { return CleanUpText(_text); }
			set { _text = value; }
		}

		[XmlIgnore]
		public string FormattedText => CleanUpText(_text, false) ?? "";

		[XmlIgnore]
		public string EnglishText
		{
			get { return CleanUpText(string.IsNullOrEmpty(_englishText) ? Text : _englishText); }
			set { _englishText =value; }
		}

		[XmlIgnore]
		public string AlternativeLanguageText => GetAlternativeText(false);

		[XmlIgnore]
		public string FormattedAlternativeLanguageText => GetAlternativeText(true);

		private string GetAlternativeText(bool formatted)
		{
			var result = "";
			for(var i = 0; i < AlternativeNames.Count; ++i)
			{
				if(i > 0)
					result += "-\n";
				result += "[" + AlternativeNames[i] + "]\n";
				if(AlternativeTexts[i] != null)
					result += CleanUpText(AlternativeTexts[i], !formatted) + "\n";
			}
			return result.TrimEnd(' ', '\n');
		}

		[XmlIgnore]
		public Visibility ShowAlternativeLanguageTextInTooltip => AlternativeNames.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

		[XmlIgnore]
		public bool HasVisibleStats => Type != "Spell" && Type != "Enchantment" && Type != "Hero Power" && !IsPlayableHeroCard;

		[XmlIgnore]
		public Visibility ShowIconsInTooltip => HasVisibleStats ? Visibility.Visible : Visibility.Hidden;

		[XmlIgnore]
		public Visibility ShowArmorIconInTooltip => IsPlayableHeroCard ? Visibility.Visible : Visibility.Hidden;

		[XmlIgnore]
		public Visibility ShowHealthValueInTooltip => HasVisibleStats || IsPlayableHeroCard ? Visibility.Visible : Visibility.Hidden;

		[XmlIgnore]
		public string Set { get; set; }

		public CardSet? CardSet => _dbCard?.Set;

		[XmlIgnore]
		public string Race { get; set; }

		[XmlIgnore]
		public string RaceOrType => Race ?? Type;

		[XmlIgnore]
		public int? Durability { get; set; }

		[XmlIgnore]
		public int ArmorDurabilityOrHealth => (IsPlayableHeroCard ? _dbCard?.Armor : Durability) ?? Health;

		[XmlIgnore]
		public string Type { get; set; }

		[XmlIgnore]
		public string Name { get; set; }

		[XmlIgnore]
		public int Cost { get; set; }

		[XmlIgnore]
		public bool IsPlayableHeroCard => Type == "Hero" && CardSet != HearthDb.Enums.CardSet.CORE && CardSet != HearthDb.Enums.CardSet.HERO_SKINS;

		[XmlIgnore]
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

		public int DustCost
		{
			get
			{
				switch(Rarity)
				{
					case Rarity.COMMON:
						return 40;
					case Rarity.RARE:
						return 100;
					case Rarity.EPIC:
						return 400;
					case Rarity.LEGENDARY:
						return 1600;
				}
				return 0;
			}
		}

		[XmlIgnore]
		public string Artist { get; set; }

		[XmlIgnore]
		public string LocalizedName
		{
			get { return string.IsNullOrEmpty(_localizedName) ? Name : _localizedName; }
			set { _localizedName = value; }
		}

		public string[] EntourageCardIds => _dbCard != null ? _dbCard.EntourageCardIds : new string[0];

		[XmlIgnore]
		public int InHandCount
		{
			get { return _inHandCount; }
			set
			{
				_inHandCount = value;
				OnPropertyChanged();
			}
		}

		[XmlIgnore]
		public bool IsClassCard => GetPlayerClass != "Neutral";

		[XmlIgnore]
		public bool IsCreated
		{
			get { return _isCreated; }
			set
			{
				_isCreated = value;
				OnPropertyChanged();
			}
		}

		[XmlIgnore]
		public bool WasDiscarded
		{
			get { return _wasDiscarded; }
			set
			{
				_wasDiscarded = value;
				OnPropertyChanged();
			}
		}

		public string GetPlayerClass => PlayerClass ?? "Neutral";

		public bool IsClass(string playerClass)
		{
			if(GetPlayerClass == playerClass)
				return true;
			var classGroup = _dbCard?.Entity.GetTag(GameTag.MULTI_CLASS_GROUP) ?? 0;
			if(classGroup == 0)
				return false;
			return Helper.MultiClassGroups[(MultiClassGroup)classGroup]
				.Any(x => string.Equals(x.ToString(), playerClass, StringComparison.CurrentCultureIgnoreCase));
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

		public SolidColorBrush ColorOpponent => new SolidColorBrush(Colors.White);

		public string CardFileName => Name.ToLowerInvariant().Replace(' ', '-').Replace(":", "").Replace("'", "-").Replace(".", "").Replace("!", "").Replace(",", "");

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

		public DrawingBrush Background
		{
			get
			{
				if(Id == null || Name == null)
					return new DrawingBrush();
				var cardImageObj = new CardImageObject(this);
				if(CardImageCache.TryGetValue(Id, out Dictionary<int, CardImageObject> cache))
				{
					if(cache.TryGetValue(cardImageObj.GetHashCode(), out CardImageObject cached))
						return cached.Image;
				}
				try
				{
					var image = ThemeManager.GetBarImageBuilder(this).Build();
					if (image.CanFreeze)
						image.Freeze();
					cardImageObj = new CardImageObject(image, this);
					if(cache == null)
					{
						cache = new Dictionary<int, CardImageObject>();
						CardImageCache.Add(Id, cache);
					}
					cache.Add(cardImageObj.GetHashCode(), cardImageObj);
					return cardImageObj.Image;
				}
				catch(Exception ex)
				{
					Log.Error($"Image builder failed: {ex.Message}");
					return new DrawingBrush();
				}
			}
		}

		internal void Update() => OnPropertyChanged(nameof(Background));
		internal void UpdateHighlight() => OnPropertyChanged(nameof(Highlight));

		public ImageBrush Highlight => ThemeManager.CurrentTheme?.HighlightImage ?? new ImageBrush();

		[XmlIgnore]
		public bool HighlightInHand { get; set; }

		[XmlIgnore]
		public string FlavorText => CleanUpText(_dbCard?.GetLocFlavorText(SelectedLanguage)) ?? "";
		
		[XmlIgnore]
		public string FormattedFlavorText => CleanUpText(_dbCard?.GetLocFlavorText(SelectedLanguage), false) ?? "";

		[XmlIgnore]
		public bool Collectible => _dbCard?.Collectible ?? false;

		public object Clone() => new Card(Id, PlayerClass, Rarity, Type, Name, Cost, LocalizedName, InHandCount, Count, _text, EnglishText, Attack,
										  Health, Race, Mechanics, Durability, Artist, Set, AlternativeNames, AlternativeTexts, _dbCard);

		public override string ToString() => Name + "(" + Count + ")";

		public override bool Equals(object card)
		{
			if(!(card is Card))
				return false;
			var c = (Card)card;
			return c.Name == Name;
		}

		public bool EqualsWithCount(Card card) => card.Id == Id && card.Count == Count;

		public override int GetHashCode() => Name.GetHashCode();

		public void Load()
		{
			if(_loaded)
				return;

			var stats = Database.GetCardFromId(Id);
			if(stats == null)
				return;
			PlayerClass = stats.PlayerClass;
			Rarity = stats.Rarity;
			Type = stats.Type;
			Name = stats.Name;
			Cost = stats.Cost;
			LocalizedName = stats.LocalizedName;
			InHandCount = stats.InHandCount;
			Text = stats._text;
			EnglishText = stats.EnglishText;
			Attack = stats.Attack;
			Health = stats.Health;
			Race = stats.Race;
			Durability = stats.Durability;
			Mechanics = stats.Mechanics;
			Artist = stats.Artist;
			Set = stats.Set;
			AlternativeNames = stats.AlternativeNames;
			AlternativeTexts = stats.AlternativeTexts;
			_dbCard = stats._dbCard;
			_loaded = true;
			OnPropertyChanged();
		}

		private static string CleanUpText(string text, bool replaceTags = true)
		{
			if (replaceTags)
				text = text?.Replace("<b>", "").Replace("</b>", "").Replace("<i>", "").Replace("</i>", "");
			return text?.Replace("$", "").Replace("#", "").Replace("\\n", "\n").Replace("[x]", "");
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	internal class CardImageObject
	{
		public DrawingBrush Image { get; }
		public int Count { get; }
		public bool Jousted { get; }
		public bool ColoredFrame { get; }
		public bool ColoredGem { get; }
		public bool Created { get; }
		public string Theme { get; }
		public int TextColorHash { get; }

		public CardImageObject(DrawingBrush image, Card card) : this(card)
		{
			Image = image;
		}

		public CardImageObject(Card card)
		{
			Count = card.Count;
			Jousted = card.Jousted;
			ColoredFrame = Config.Instance.RarityCardFrames;
			ColoredGem = Config.Instance.RarityCardGems;
			Theme = ThemeManager.CurrentTheme?.Name;
			TextColorHash = card.ColorPlayer.Color.GetHashCode();
			Created = card.IsCreated;
		}

		public override bool Equals(object obj)
		{
			var cardObj = obj as CardImageObject;
			return cardObj != null && Equals(cardObj);
		}

		protected bool Equals(CardImageObject other)
			=> Count == other.Count && Jousted == other.Jousted && ColoredFrame == other.ColoredFrame && ColoredGem == other.ColoredGem
				&& string.Equals(Theme, other.Theme) && TextColorHash == other.TextColorHash && Created == other.Created;

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Count;
				hashCode = (hashCode * 397) ^ Jousted.GetHashCode();
				hashCode = (hashCode * 397) ^ ColoredFrame.GetHashCode();
				hashCode = (hashCode * 397) ^ ColoredGem.GetHashCode();
				hashCode = (hashCode * 397) ^ (Theme?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ TextColorHash;
				hashCode = (hashCode * 397) ^ Created.GetHashCode();
				return hashCode;
			}
		}
	}
}
