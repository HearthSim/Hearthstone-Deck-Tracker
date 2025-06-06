#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.Stats;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.HsReplay.Utility;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class Deck : ICloneable, INotifyPropertyChanged
	{
		private const string LocNoStats = "Deck_StatsString_NoStats";

		private readonly string[] _relevantMechanics =
		{
			"Battlecry",
			"Charge",
			"Combo",
			"Deathrattle",
			"Divine Shield",
			"Freeze",
			"Inspire",
			"Lifesteal",
			"Poisonous",
			"Secret",
			"Spellpower",
			"Taunt",
			"Windfury",
			"Stealth",
			"Recruit",
			"Discover"
		};

		private bool _archived;

		private ArenaReward _arenaReward = new();
		private List<GameStats>? _cachedGames;
		private SerializableVersion? _cachedVersion;
		private Guid _deckId;
		private bool? _isArenaDeck;
		private bool? _isDungeonDeck;
		private bool? _isDuelsDeck;
		private DateTime _lastCacheUpdate = DateTime.MinValue;
		private string _name = string.Empty;
		private string? _note;
		private SerializableVersion _selectedVersion = new(1, 0);
		private List<string> _tags = new();

		[XmlArray(ElementName = "Cards")]
		[XmlArrayItem(ElementName = "Card", IsNullable = false)]
		public ObservableCollection<Card> Cards;

		[XmlArray(ElementName = "Sideboards")]
		[XmlArrayItem(ElementName = "Sideboard", IsNullable = false)]
		public List<Sideboard> Sideboards;

		public string? Class;
		public DateTime LastEdited;

		[XmlArray(ElementName = "MissingCards")]
		[XmlArrayItem(ElementName = "Card")]
		public List<Card> MissingCards;

		public string? Url;
		public SerializableVersion Version = new(1, 0);

		[XmlArray(ElementName = "DeckHistory")]
		[XmlArrayItem(ElementName = "Deck")]
		public List<Deck> Versions;

		public event Action? OnStatsUpdated;

		public Deck()
		{
			Cards = new ObservableCollection<Card>();
			Sideboards = new List<Sideboard>();
			MissingCards = new List<Card>();
			Tags = new List<string>();
			Note = string.Empty;
			Url = string.Empty;
			Name = string.Empty;
			Archived = false;
			Version = SerializableVersion.Default;
			Versions = new List<Deck>();
			DeckId = Guid.NewGuid();
		}

		public Deck(string name, string? className, IEnumerable<Card> cards, List<Sideboard> sideboards, IEnumerable<string>? tags, string? note, string? url,
		            DateTime lastEdited, bool archived, List<Card> missingCards, SerializableVersion version, IEnumerable<Deck> versions,
		            Guid deckId, long hsId = 0, SerializableVersion? selectedVersion = null, bool? isArenaDeck = null,
		            ArenaReward? reward = null)

		{
			Name = name;
			Class = className;
			Cards = new ObservableCollection<Card>();
			foreach(var card in cards.ToSortedCardList())
				Cards.Add((Card)card.Clone());
			Sideboards = sideboards.Select(x => (Sideboard)x.Clone()).ToList();
			MissingCards = missingCards;
			Tags = new List<string>(tags);
			Note = note;
			Url = url;
			LastEdited = lastEdited;
			Archived = archived;
			Version = version;
			SelectedVersion = selectedVersion ?? version;
			Versions = new List<Deck>();
			DeckId = deckId;
			if(isArenaDeck.HasValue)
				IsArenaDeck = isArenaDeck.Value;
			if(versions != null)
			{
				foreach(var d in versions)
					Versions.Add((Deck)d.CloneWithNewId(true));
			}
			if(reward != null)
				_arenaReward = reward;
			HsId = hsId;
		}

		public bool Archived
		{
			get { return _archived; }
			set
			{
				_archived = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(ArchivedVisibility));
			}
		}

		public static event Action<Deck>? ArchivedChanged;
		public void Archive(bool archive)
		{
			if(Archived == archive)
				return;
			Archived = archive;
			Edited();
			ArchivedChanged?.Invoke(this);
		}

		public long HsId { get; set; }

		public string? Note
		{
			get { return _note; }
			set
			{
				_note = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(NoteVisibility));
			}
		}

		public SerializableVersion SelectedVersion
		{
			get { return VersionsIncludingSelf.Contains(_selectedVersion) ? _selectedVersion : Version; }
			set
			{
				_selectedVersion = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(NameAndVersion));
				OnPropertyChanged(nameof(WildIndicatorVisibility));
				SelectedVersionChanged?.Invoke();
			}
		}

		public event Action? SelectedVersionChanged;

		public bool IsArenaDeck
		{
			get { return _isArenaDeck ?? (_isArenaDeck = CheckIfArenaDeck()) ?? false; }
			set { _isArenaDeck = value; }
		}

		public bool IsDungeonDeck
		{
			get { return _isDungeonDeck ?? (_isDungeonDeck = CheckIfDungeonDeck()) ?? false; }
			set { _isDungeonDeck = value; }
		}

		public bool IsDuelsDeck
		{
			get { return _isDuelsDeck ?? (_isDuelsDeck = CheckIfDuelsDeck()) ?? false; }
			set { _isDuelsDeck = value; }
		}

		public bool IsBrawlDeck => Tags.Any(x => x.ToUpper().Contains("BRAWL"));

		public ArenaReward? ArenaReward
		{
			get { return IsArenaDeck ? _arenaReward : null; }
			set
			{
				if(IsArenaDeck && value != null)
					_arenaReward = value;
			}
		}

		public bool? IsArenaRunCompleted {
			get
			{
				if(!IsArenaDeck) return null;

				var isUnderground = CheckIfUndergroundArena() ?? false;

				var maxWins = isUnderground ? 12 : 5;
				var maxLosses = isUnderground ? 3 : 2;

				return IsArenaDeck
					? (DeckStats.Games.Count(g => g.Result == GameResult.Win) == maxWins
					   || DeckStats.Games.Count(g => g.Result == GameResult.Loss) == maxLosses) as bool? : null;
			}
		}

		public bool? IsDungeonRunCompleted => IsDungeonDeck
												? (DeckStats.Games.Count(g => g.Result == GameResult.Win) == 8
												   || DeckStats.Games.Count(g => g.Result == GameResult.Loss) == 1) as bool? : null;

		public bool? IsDuelsRunCompleted => IsDuelsDeck ? (DeckStats.Games.Count(g => g.Result == GameResult.Win) == 12
												   || DeckStats.Games.Count(g => g.Result == GameResult.Loss) == 3) as bool? : null;

		public Guid DeckId
		{
			get { return _deckId == Guid.Empty ? (_deckId = Guid.NewGuid()) : _deckId; }
			set { _deckId = value; }
		}

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(NameAndVersion));
			}
		}

		[XmlIgnore]
		public List<SerializableVersion> VersionsIncludingSelf => Versions.Select(x => x.Version).Concat(new[] {Version}).ToList();

		[XmlIgnore]
		public string? NameAndVersion
		{
			get
			{
				if(Config.Instance.DeckPickerCaps)
				{
					return Versions.Count == 0
						       ? Name?.ToUpperInvariant()
						       : $"{Name?.ToUpperInvariant()} (v{SelectedVersion.Major}.{SelectedVersion.Minor})";
				}
				return Versions.Count == 0 ? Name : $"{Name} (v{SelectedVersion.Major}.{SelectedVersion.Minor})";
			}
		}

		[XmlIgnore]
		public string WinLossString
		{
			get
			{
				var relevantGames = GetRelevantGames();
				if(relevantGames.Count == 0)
					return "0-0";
				return $"{relevantGames.Count(g => g.Result == GameResult.Win)}-{relevantGames.Count(g => g.Result == GameResult.Loss)}";
			}
		}

		[XmlIgnore]
		public string WinPercentString
		{
			get
			{
				var relevantGames = GetRelevantGames();
				if(relevantGames.Count == 0)
					return "-";
				var wins = relevantGames.Count(g => g.Result == GameResult.Win);
				var losses = relevantGames.Count(g => g.Result == GameResult.Loss);
				var total = wins + losses;
				if(total == 0)
					return "-";
				return Math.Round(100.0 * wins / total, 0) + "%";
			}
		}

		[XmlIgnore]
		public double WinPercent
		{
			get
			{
				var relevantGames = GetRelevantGames();
				if(relevantGames.Count == 0)
					return 0.0;
				var wins = relevantGames.Count(g => g.Result == GameResult.Win);
				var losses = relevantGames.Count(g => g.Result == GameResult.Loss);
				var total = wins + losses;
				if(total == 0)
					return 0.0;
				return 100.0 * wins / total;
			}
		}

		[XmlIgnore]
		public string StatsString => GetRelevantGames().Any() ? $"{WinPercentString} | {WinLossString}" : LocUtil.Get(LocNoStats, true);

		[XmlIgnore]
		public DateTime LastPlayed => !DeckStats.Games.Any() ? DateTime.MinValue : DeckStats.Games.Max(g => g.StartTime);

		[XmlIgnore]
		public DateTime LastPlayedNewFirst => !DeckStats.Games.Any() ? LastEdited : DeckStats.Games.Max(g => g.StartTime);

		[XmlIgnore]
		public string GetClass => string.IsNullOrEmpty(Class) ? "(No Class Selected)" : "(" + Class + ")";

		[XmlArray(ElementName = "Tags")]
		[XmlArrayItem(ElementName = "Tag")]
		public List<string> Tags
		{
			get { return _tags; }
			set
			{
				_tags = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(TagList));
			}
		}

		[XmlIgnore]
		public string TagList
		{
			get { return Tags?.Count > 0 ? Tags.Select(x => x.ToUpperInvariant()).Aggregate((t, n) => t + " | " + n) : ""; }
		}

		[XmlIgnore]
		public SolidColorBrush ClassColorBrush => new SolidColorBrush(ClassColor);

		[XmlIgnore]
		public Color ClassColor => Helper.GetClassColor(Class, false);

		[XmlIgnore]
		public BitmapImage ClassImage
		{
			get
			{
				if(Enum.TryParse(Class, out HeroClassAll heroClass))
					return ImageCache.GetClassIcon(heroClass);
				return new BitmapImage();
			}
		}

		[XmlIgnore]
		public BitmapImage HeroImage => ClassImage;

		public DeckStats DeckStats
		{
			get
			{
				if(!DeckStatsList.Instance.DeckStats.TryGetValue(DeckId, out var stats))
					stats = DeckStatsList.Instance.Add(this);
				return stats;
			}
		}

		public void AddGameResult(GameStats gameStats)
		{
			_cachedGames = null;
			DeckStats.Games.Add(gameStats);
			StatsUpdated();
		}

		public void RemoveGameResult(GameStats gameStats)
		{
			_cachedGames = null;
			var success = DeckStats.Games.Remove(gameStats);
			if(success)
				StatsUpdated();
		}

		[XmlIgnore]
		public bool HasVersions => Versions != null && Versions.Count > 0;

		private string? _shortId;
		public string ShortId => _shortId ?? (_shortId = ShortIdHelper.GetShortId(this));

		public Visibility VisibilityStats => GetRelevantGames().Any() ? Visibility.Visible : Visibility.Collapsed;

		public Visibility VisibilityNoStats => VisibilityStats == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

		public Visibility NoteVisibility => string.IsNullOrEmpty(Note) ? Visibility.Collapsed : Visibility.Visible;

		public Visibility WildIndicatorVisibility => IsArenaDeck || !IsWildDeck ? Visibility.Collapsed : Visibility.Visible;

		public bool StandardViable => !IsArenaDeck && !IsWildDeck && !IsClassicDeck;

		public bool IsWildDeck => GetSelectedDeckVersion().Cards.Any(x => Helper.WildOnlySets.Contains(x.Set));

		public bool IsClassicDeck => GetSelectedDeckVersion().Cards.All(x => Helper.ClassicOnlySets.Contains(x.Set));

		// TODO: This should be more dynamic
		public bool IsTwistDeck => GetSelectedDeckVersion().Cards.All(x => Helper.TwistSets.Contains(x.Set) && !x.IsNeutral);

		public Visibility ArchivedVisibility => Archived ? Visibility.Visible : Visibility.Collapsed;

		private TimeSpan ValidCacheDuration => new TimeSpan(0, 0, 1);

		private bool CacheIsValid => _cachedGames != null && _cachedVersion == SelectedVersion && DateTime.Now - _lastCacheUpdate < ValidCacheDuration;

		[XmlIgnore]
		public List<Mechanic> Mechanics => _relevantMechanics.Select(x => new Mechanic(x, this)).Where(m => m.Count > 0).ToList();

		public object Clone() => new Deck(Name, Class, Cards, Sideboards, Tags, Note, Url, LastEdited, Archived, MissingCards, Version, Versions,
										  DeckId, HsId, SelectedVersion, _isArenaDeck, ArenaReward);

		public event PropertyChangedEventHandler? PropertyChanged;

		public FormatType GuessFormatType()
		{
			if (IsClassicDeck)
				return FormatType.FT_CLASSIC;
			if (IsTwistDeck)
				return FormatType.FT_TWIST;
			if (IsWildDeck)
				return FormatType.FT_WILD;
			return FormatType.FT_STANDARD;
		}

		public List<GameStats> GetRelevantGames()
		{
			if(CacheIsValid)
				return _cachedGames!;
			var filtered = Config.Instance.DisplayedMode == GameMode.All
				               ? DeckStats.Games
				               : (IsArenaDeck
					                  ? DeckStats.Games.Where(g => g.GameMode == GameMode.Arena).ToList()
					                  : DeckStats.Games.Where(g => g.GameMode == Config.Instance.DisplayedMode));
			switch(Config.Instance.DisplayedTimeFrame)
			{
				case DisplayedTimeFrame.AllTime:
					break;
				case DisplayedTimeFrame.LastSeason:
					filtered = filtered.Where(g => g.StartTime >= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1)
												&& g.StartTime < new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
					break;
				case DisplayedTimeFrame.CurrentSeason:
					filtered = filtered.Where(g => g.StartTime > new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
					break;
				case DisplayedTimeFrame.ThisWeek:
					filtered = filtered.Where(g => g.StartTime > DateTime.Today.AddDays(-((int)g.StartTime.DayOfWeek + 1)));
					break;
				case DisplayedTimeFrame.Today:
					filtered = filtered.Where(g => g.StartTime > DateTime.Today);
					break;
				case DisplayedTimeFrame.Custom:
					if(Config.Instance.CustomDisplayedTimeFrame.HasValue)
						filtered = filtered.Where(g => g.StartTime > Config.Instance.CustomDisplayedTimeFrame.Value);
					break;
			}
			switch(Config.Instance.DisplayedStats)
			{
				case DisplayedStats.All:
					break;
				case DisplayedStats.Latest:
					filtered = filtered.Where(g => g.BelongsToDeckVerion(this));
					break;
				case DisplayedStats.Selected:
					filtered = filtered.Where(g => g.BelongsToDeckVerion(GetSelectedDeckVersion()));
					break;
				case DisplayedStats.LatestMajor:
					filtered =
						filtered.Where(g => VersionsIncludingSelf.Where(v => v.Major == Version.Major).Select(GetVersion).Any(g.BelongsToDeckVerion));
					break;
				case DisplayedStats.SelectedMajor:
					filtered = filtered.Where(g => VersionsIncludingSelf.Where(v => v.Major == SelectedVersion.Major)
						.Select(GetVersion).Any(g.BelongsToDeckVerion));
					break;
			}
			_cachedGames = filtered.ToList();
			_lastCacheUpdate = DateTime.Now;
			_cachedVersion = SelectedVersion;
			return _cachedGames;
		}

		public bool? CheckIfArenaDeck() => !DeckStats.Games.Any() ? (bool?)null : DeckStats.Games.All(g => g.GameMode == GameMode.Arena);

		public bool? CheckIfUndergroundArena() => !DeckStats.Games.Any() ? (bool?)null : DeckStats.Games.All(g => g.GameType == GameType.GT_UNDERGROUND_ARENA);

		public bool? CheckIfDungeonDeck() => !DeckStats.Games.Any() ? (bool?)null : DeckStats.Games.All(g => g.IsDungeonMatch);

		public bool? CheckIfDuelsDeck() => !DeckStats.Games.Any() ? (bool?)null : DeckStats.Games.All(g => g.IsPVPDungeonMatch);

		public Deck GetVersion(int major, int minor) => GetVersion(new SerializableVersion(major, minor));

		public Deck GetVersion(SerializableVersion version) => Version == version ? this : Versions.FirstOrDefault(x => x.Version == version);

		public bool HasVersion(SerializableVersion version) => Version == version || Versions.Any(v => v.Version == version);

		public object CloneWithNewId(bool isVersion) => new Deck(Name, Class, Cards, Sideboards, Tags, Note, Url, LastEdited, Archived, MissingCards, Version, Versions,
																 Guid.NewGuid(), HsId, SelectedVersion, _isArenaDeck);

		public void ResetVersions()
		{
			Versions = new List<Deck>();
			Version = SerializableVersion.Default;
			SelectedVersion = Version;
		}

		public Deck GetSelectedDeckVersion() => Versions == null ? this : Versions.FirstOrDefault(d => d.Version == SelectedVersion) ?? this;

		public void SelectVersion(SerializableVersion version) => SelectedVersion = version;

		public void SelectVersion(Deck deck) => SelectVersion(deck.Version);

		public string GetDeckInfo() => $"deckname:{Name?.Replace("{", "").Replace("}", "")}, class:{Class}, cards:{Cards.Sum(x => x.Count)}";

		/// returns the number of cards in the deck with mechanics matching the newmechanic.
		/// The mechanic attribute, such as windfury or taunt, comes from the cardDB json file
		public int GetMechanicCount(string newmechanic) => Cards.Where(card => card.Mechanics != null)
																.Sum(card => card.Mechanics.Count(mechanic => mechanic.Equals(newmechanic)) * card.Count);

		public int GetNumTaunt() => GetMechanicCount("Taunt");
		public int GetNumBattlecry() => GetMechanicCount("Battlecry");
		public int GetNumImmuneToSpellpower() => GetMechanicCount("ImmuneToSpellpower");
		public int GetNumSpellpower() => GetMechanicCount("Spellpower");
		public int GetNumOneTurnEffect() => GetMechanicCount("OneTurnEffect");
		public int GetNumCharge() => GetMechanicCount("Charge") + GetMechanicCount("GrantCharge");
		public int GetNumFreeze() => GetMechanicCount("Freeze");
		public int GetNumAdjacentBuff() => GetMechanicCount("AdjacentBuff");
		public int GetNumSecret() => GetMechanicCount("Secret");
		public int GetNumDeathrattle() => GetMechanicCount("Deathrattle");
		public int GetNumWindfury() => GetMechanicCount("Windfury");
		public int GetNumDivineShield() => GetMechanicCount("Divine Shield");
		public int GetNumCombo() => GetMechanicCount("Combo");

		public bool ContainsSet(string set) => Cards.Any(card => card.Set == set);

		public bool ContainsSet(CardSet set) => Cards.Any(card => card.CardSet == set);

		public override string ToString() => $"{Name} ({Class})";

		public override bool Equals(object? obj)
		{
			var deck = obj as Deck;
			if(deck == null)
				return false;
			if(!Version.Equals(deck.Version))
				return false;
			return DeckId.Equals(deck.DeckId);
		}

		public override int GetHashCode() => DeckId.GetHashCode();

		public static List<Card> operator -(Deck? first, Deck? second)
		{
			if(first == null || second == null)
				return new List<Card>();

			return second.Cards.ToDiffCardList(first.Cards.ToList());
		}

		public List<Card> GetSideboardDiff(Deck newDeck)
		{
			var curDeckSideboardCards = Sideboards.SelectMany(s => s.Cards).ToList();
			var newDeckSideboardCards = newDeck.Sideboards.SelectMany(s => s.Cards).ToList();

			return curDeckSideboardCards.ToDiffCardList(newDeckSideboardCards);
		}

		public SerializableVersion GetMaxVersion() => VersionsIncludingSelf.OrderByDescending(x => x).First();

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Edited() => LastEdited = DateTime.Now;

		public void StatsUpdated()
		{
			OnPropertyChanged(nameof(StatsString));
			OnPropertyChanged(nameof(LastPlayed));
			OnPropertyChanged(nameof(LastPlayedNewFirst));
			OnPropertyChanged(nameof(WinLossString));
			OnPropertyChanged(nameof(WinPercent));
			OnPropertyChanged(nameof(WinPercentString));
			OnPropertyChanged(nameof(VisibilityStats));
			OnPropertyChanged(nameof(VisibilityNoStats));
			OnStatsUpdated?.Invoke();
		}

		public void UpdateWildIndicatorVisibility() => OnPropertyChanged(nameof(WildIndicatorVisibility));
	}

	public class Sideboard : ICloneable
	{

		public Sideboard()
		{
			OwnerCardId = "";
			Cards = new List<Card>();
		}

		public Sideboard(string ownerCardId, List<Card> cards)
		{
			OwnerCardId = ownerCardId;
			Cards = cards;
		}

		[XmlAttribute("Owner")]
		public string OwnerCardId { get; set; }

		[XmlArray(ElementName = "Cards")]
		[XmlArrayItem(ElementName = "Card", IsNullable = false)]
		public List<Card> Cards { get; set; }

		public object Clone() => new Sideboard(OwnerCardId, Cards.Select(c => (Card)c.Clone()).ToList());
	}
}
