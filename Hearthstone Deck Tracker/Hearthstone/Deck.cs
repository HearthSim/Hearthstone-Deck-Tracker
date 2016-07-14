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
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.Stats;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class Deck : ICloneable, INotifyPropertyChanged
	{
		private const string BaseHearthStatsUrl = @"http://hss.io/d/";

		private readonly string[] _relevantMechanics =
		{
			"Battlecry",
			"Charge",
			"Combo",
			"Deathrattle",
			"Divine Shield",
			"Freeze",
			"Inspire",
			"Secret",
			"Spellpower",
			"Taunt",
			"Windfury",
			"Stealth"
		};

		private bool _archived;

		private ArenaReward _arenaReward = new ArenaReward();
		private List<GameStats> _cachedGames;
		private Guid _deckId;
		private string _hearthStatsIdClone;
		private bool? _isArenaDeck;
		private bool _isSelectedInGui;
		private DateTime _lastCacheUpdate = DateTime.MinValue;
		private string _name;
		private string _note;
		private SerializableVersion _selectedVersion = new SerializableVersion(1, 0);
		private List<string> _tags;

		[XmlArray(ElementName = "Cards")]
		[XmlArrayItem(ElementName = "Card")]
		public ObservableCollection<Card> Cards;

		public string Class;
		public string HearthStatsArenaId;
		public string HearthStatsDeckVersionId;
		public string HearthStatsId;
		public DateTime LastEdited;

		[XmlArray(ElementName = "MissingCards")]
		[XmlArrayItem(ElementName = "Card")]
		public List<Card> MissingCards;

		public string Url;
		public SerializableVersion Version = new SerializableVersion(1, 0);

		[XmlArray(ElementName = "DeckHistory")]
		[XmlArrayItem(ElementName = "Deck")]
		public List<Deck> Versions;

		public Deck()
		{
			Cards = new ObservableCollection<Card>();
			MissingCards = new List<Card>();
			Tags = new List<string>();
			Note = string.Empty;
			Url = string.Empty;
			Name = string.Empty;
			Archived = false;
			SyncWithHearthStats = null;
			HearthStatsId = string.Empty;
			Version = SerializableVersion.Default;
			Versions = new List<Deck>();
			DeckId = Guid.NewGuid();
		}

		public Deck(string name, string className, IEnumerable<Card> cards, IEnumerable<string> tags, string note, string url,
		            DateTime lastEdited, bool archived, List<Card> missingCards, SerializableVersion version, IEnumerable<Deck> versions,
		            bool? syncWithHearthStats, string hearthStatsId, Guid deckId, string hearthStatsDeckVersionId, long hsId = 0,
		            string hearthStatsIdClone = null, SerializableVersion selectedVersion = null, bool? isArenaDeck = null,
		            ArenaReward reward = null)

		{
			Name = name;
			Class = className;
			Cards = new ObservableCollection<Card>();
			MissingCards = missingCards;
			foreach(var card in cards.ToSortedCardList())
				Cards.Add((Card)card.Clone());
			Tags = new List<string>(tags);
			Note = note;
			Url = url;
			LastEdited = lastEdited;
			Archived = archived;
			Version = version;
			SyncWithHearthStats = syncWithHearthStats;
			HearthStatsId = hearthStatsId;
			SelectedVersion = selectedVersion ?? version;
			Versions = new List<Deck>();
			DeckId = deckId;
			if(hearthStatsIdClone != null)
				HearthStatsIdForUploading = hearthStatsIdClone;
			if(isArenaDeck.HasValue)
				IsArenaDeck = isArenaDeck.Value;
			HearthStatsDeckVersionId = hearthStatsDeckVersionId;
			if(versions != null)
			{
				foreach(var d in versions)
					Versions.Add(d.CloneWithNewId(true) as Deck);
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

		public long HsId { get; set; }

		public string Note
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
				OnPropertyChanged(nameof(StandardViableVisibility));
			}
		}

		public bool HearthStatsIdsAlreadyReset { get; set; }

		[XmlIgnore]
		public bool IsSelectedInGui
		{
			get { return _isSelectedInGui; }
			set
			{
				_isSelectedInGui = value;
				OnPropertyChanged(nameof(GetFontWeight));
			}
		}

		public bool IsArenaDeck
		{
			get { return _isArenaDeck ?? (_isArenaDeck = CheckIfArenaDeck()) ?? false; }
			set { _isArenaDeck = value; }
		}

		public bool IsBrawlDeck => Tags.Any(x => x.ToUpper().Contains("BRAWL"));

		public ArenaReward ArenaReward
		{
			get { return IsArenaDeck ? _arenaReward : null; }
			set
			{
				if(IsArenaDeck)
					_arenaReward = value;
			}
		}

		public bool? IsArenaRunCompleted => IsArenaDeck
												? (DeckStats.Games.Count(g => g.Result == GameResult.Win) == 12
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

		public bool? SyncWithHearthStats { get; set; }

		[XmlIgnore]
		public bool HasHearthStatsId => !string.IsNullOrEmpty(HearthStatsId) || !string.IsNullOrEmpty(_hearthStatsIdClone);

		[XmlIgnore]
		public string HearthStatsUrl => HasHearthStatsId
											? BaseHearthStatsUrl + HearthStatsId : (HasHearthStatsArenaId ? BaseHearthStatsUrl + HearthStatsArenaId : "");

		[XmlIgnore]
		public bool HasHearthStatsDeckVersionId => !string.IsNullOrEmpty(HearthStatsDeckVersionId);

		[XmlIgnore]
		public List<SerializableVersion> VersionsIncludingSelf => Versions.Select(x => x.Version).Concat(new[] {Version}).ToList();

		[XmlIgnore]
		public string NameAndVersion
		{
			get
			{
				if(Config.Instance.DeckPickerCaps)
				{
					return Versions.Count == 0
						       ? Name.ToUpperInvariant()
						       : $"{Name.ToUpperInvariant()} (v{SelectedVersion.Major}.{SelectedVersion.Minor})";
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
				return Math.Round(100.0 * wins / (wins + losses), 0) + "%";
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
				return 100.0 * wins / (wins + losses);
			}
		}

		[XmlIgnore]
		public string StatsString => GetRelevantGames().Any() ? $"{WinPercentString} | {WinLossString}" : "NO STATS";

		[XmlIgnore]
		public DateTime LastPlayed => !DeckStats.Games.Any() ? DateTime.MinValue : DeckStats.Games.Max(g => g.StartTime);

		[XmlIgnore]
		public DateTime LastPlayedNewFirst => !DeckStats.Games.Any() ? LastEdited : DeckStats.Games.Max(g => g.StartTime);

		[XmlIgnore]
		public string GetClass => string.IsNullOrEmpty(Class) ? "(No Class Selected)" : "(" + Class + ")";

		[XmlIgnore]
		public FontWeight GetFontWeight => IsSelectedInGui ? FontWeights.Black : FontWeights.Regular;

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
			get { return Tags.Count > 0 ? Tags.Select(x => x.ToUpperInvariant()).Aggregate((t, n) => t + " | " + n) : ""; }
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
				HeroClassAll heroClass;
				if(Enum.TryParse(Class, out heroClass))
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
				DeckStats deckStats;
				return !DeckStatsList.Instance.DeckStats.TryGetValue(DeckId, out deckStats) ? DeckStatsList.Instance.Add(this) : deckStats;
			}
		}

		[XmlIgnore]
		public bool HasVersions => Versions != null && Versions.Count > 0;

		public bool HasHearthStatsArenaId => !string.IsNullOrEmpty(HearthStatsArenaId);

		//I don't know why I need this but I apparently can't serialize anything if versions of a deck have the same hearthstatsid

		public string HearthStatsIdForUploading
		{
			get { return !string.IsNullOrEmpty(HearthStatsId) ? HearthStatsId : _hearthStatsIdClone; }
			set { _hearthStatsIdClone = value; }
		}

		public Visibility VisibilityStats => GetRelevantGames().Any() ? Visibility.Visible : Visibility.Collapsed;

		public Visibility VisibilityNoStats => VisibilityStats == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

		public Visibility NoteVisibility => string.IsNullOrEmpty(Note) ? Visibility.Collapsed : Visibility.Visible;

		public Visibility StandardViableVisibility => StandardViable ? Visibility.Visible : Visibility.Collapsed;

		public bool StandardViable => !IsArenaDeck && !GetSelectedDeckVersion().Cards.Any(x => Helper.WildOnlySets.Contains(x.Set));

		public Visibility ArchivedVisibility => Archived ? Visibility.Visible : Visibility.Collapsed;

		private TimeSpan ValidCacheDuration => new TimeSpan(0, 0, 1);

		private bool CacheIsValid => _cachedGames != null && DateTime.Now - _lastCacheUpdate < ValidCacheDuration;

		[XmlIgnore]
		public List<Mechanic> Mechanics => _relevantMechanics.Select(x => new Mechanic(x, this)).Where(m => m.Count > 0).ToList();

		public object Clone() => new Deck(Name, Class, Cards, Tags, Note, Url, LastEdited, Archived, MissingCards, Version, Versions, SyncWithHearthStats,
										  HearthStatsId, DeckId, HearthStatsDeckVersionId, HsId, HearthStatsIdForUploading, SelectedVersion, _isArenaDeck,
										  ArenaReward);

		public event PropertyChangedEventHandler PropertyChanged;

		public List<GameStats> GetRelevantGames()
		{
			if(CacheIsValid)
				return _cachedGames;
			var filtered = Config.Instance.DisplayedMode == GameMode.All
				               ? DeckStats.Games
				               : (IsArenaDeck
					                  ? DeckStats.Games.Where(g => g.GameMode == GameMode.Arena).ToList()
					                  : DeckStats.Games.Where(g => g.GameMode == Config.Instance.DisplayedMode).ToList());
			switch(Config.Instance.DisplayedTimeFrame)
			{
				case DisplayedTimeFrame.AllTime:
					break;
				case DisplayedTimeFrame.CurrentSeason:
					filtered = filtered.Where(g => g.StartTime > new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).ToList();
					break;
				case DisplayedTimeFrame.ThisWeek:
					filtered = filtered.Where(g => g.StartTime > DateTime.Today.AddDays(-((int)g.StartTime.DayOfWeek + 1))).ToList();
					break;
				case DisplayedTimeFrame.Today:
					filtered = filtered.Where(g => g.StartTime > DateTime.Today).ToList();
					break;
				case DisplayedTimeFrame.Custom:
					if(Config.Instance.CustomDisplayedTimeFrame.HasValue)
						filtered = filtered.Where(g => g.StartTime > Config.Instance.CustomDisplayedTimeFrame.Value).ToList();
					break;
			}
			switch(Config.Instance.DisplayedStats)
			{
				case DisplayedStats.All:
					break;
				case DisplayedStats.Latest:
					filtered = filtered.Where(g => g.BelongsToDeckVerion(this)).ToList();
					break;
				case DisplayedStats.Selected:
					filtered = filtered.Where(g => g.BelongsToDeckVerion(GetSelectedDeckVersion())).ToList();
					break;
				case DisplayedStats.LatestMajor:
					filtered =
						filtered.Where(g => VersionsIncludingSelf.Where(v => v.Major == Version.Major).Select(GetVersion).Any(g.BelongsToDeckVerion))
						        .ToList();
					break;
				case DisplayedStats.SelectedMajor:
					filtered =
						filtered.Where(
						               g =>
						               VersionsIncludingSelf.Where(v => v.Major == SelectedVersion.Major).Select(GetVersion).Any(g.BelongsToDeckVerion))
						        .ToList();
					break;
			}
			_cachedGames = filtered;
			_lastCacheUpdate = DateTime.Now;
			return filtered;
		}

		public void ResetHearthstatsIds()
		{
			HearthStatsArenaId = null;
			HearthStatsDeckVersionId = null;
			HearthStatsId = null;
			_hearthStatsIdClone = null;
			HearthStatsIdsAlreadyReset = true;
		}

		public bool? CheckIfArenaDeck() => !DeckStats.Games.Any() ? (bool?)null : DeckStats.Games.All(g => g.GameMode == GameMode.Arena);

		public Deck GetVersion(int major, int minor) => GetVersion(new SerializableVersion(major, minor));

		public Deck GetVersion(SerializableVersion version) => version == null ? null : (Version == version ? this : Versions.FirstOrDefault(x => x.Version == version));

		public bool HasVersion(SerializableVersion version) => Version == version || Versions.Any(v => v.Version == version);

		public object CloneWithNewId(bool isVersion) => new Deck(Name, Class, Cards, Tags, Note, Url, LastEdited, Archived, MissingCards, Version, Versions, SyncWithHearthStats, "",
																 Guid.NewGuid(), HearthStatsDeckVersionId, HsId, isVersion ? HearthStatsIdForUploading : "", SelectedVersion, _isArenaDeck);

		public void ResetVersions()
		{
			Versions = new List<Deck>();
			Version = SerializableVersion.Default;
			SelectedVersion = Version;
		}

		public Deck GetSelectedDeckVersion() => Versions == null ? this : Versions.FirstOrDefault(d => d.Version == SelectedVersion) ?? this;

		public void SelectVersion(SerializableVersion version) => SelectedVersion = version;

		public void SelectVersion(Deck deck) => SelectVersion(deck.Version);

		public string GetDeckInfo() => $"deckname:{Name.Replace("{", "").Replace("}", "")}, class:{Class}, cards:{Cards.Sum(x => x.Count)}";

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

		public override string ToString() => $"{Name} ({Class})";

		public override bool Equals(object obj)
		{
			var deck = obj as Deck;
			if(deck == null)
				return false;
			if(deck.HasHearthStatsId && HasHearthStatsId)
				return HearthStatsId.Equals(deck.HearthStatsId);
			return DeckId.Equals(deck.DeckId);
		}

		public override int GetHashCode() => HasHearthStatsId ? HasHearthStatsId.GetHashCode() : DeckId.GetHashCode();

		public static List<Card> operator -(Deck first, Deck second)
		{
			if(first == null || second == null)
				return new List<Card>();
			var diff = new List<Card>();
			//removed
			foreach(var c in second.Cards.Where(c => !first.Cards.Contains(c)))
			{
				var cd = c.Clone() as Card;
				if(cd == null)
					continue;
				cd.Count = -cd.Count; //merk as negative for visual
				diff.Add(cd);
			}
			//added
			diff.AddRange(first.Cards.Where(c => !second.Cards.Contains(c)));

			//diff count
			var diffCount =
				first.Cards.Where(c => second.Cards.Any(c2 => c2.Id == c.Id) && second.Cards.First(c2 => c2.Id == c.Id).Count != c.Count);
			foreach(var card in diffCount)
			{
				var cardclone = card.Clone() as Card;
				if(cardclone == null)
					continue;
				cardclone.Count = cardclone.Count - second.Cards.First(c => c.Id == cardclone.Id).Count;
				diff.Add(cardclone);
			}

			return diff;
		}

		public SerializableVersion GetMaxVerion() => VersionsIncludingSelf.OrderByDescending(x => x).First();

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
		}

		public void UpdateStandardIndicatorVisibility() => OnPropertyChanged(nameof(StandardViableVisibility));
	}
}