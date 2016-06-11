#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static Hearthstone_Deck_Tracker.Enums.PlayType;

#endregion

namespace Hearthstone_Deck_Tracker.Stats
{
	public class GameStats : INotifyPropertyChanged
	{
		private Guid? _deckId;
		private string _deckName;
		private string _deckNameAndVersion;
		public Guid GameId;
		public string HearthStatsId;
		private Format _format = Enums.Format.Standard;
		private string _note;
		private string _playerHero;
		private string _opponentHero;
		private bool _coin;
		private GameMode _gameMode;
		private GameResult _result;
		private int _turns;
		private string _playerName;
		private string _opponentName;
		private int _rank;
		private int _stars;
		private int _legendRank;
		private Region _region;
		private int _opponentLegendRank;

		public GameStats()
		{
		}

		public GameStats(GameResult result, string opponentHero, string playerHero)
		{
			Coin = false;
			Result = result;
			GameMode = GameMode.None;
			OpponentHero = opponentHero;
			PlayerHero = playerHero;
			StartTime = DateTime.Now;
			GameId = Guid.NewGuid();
		}

		//playerhero does not get loaded from xml for some reason
		public string PlayerHero
		{
			get { return _playerHero; }
			set
			{
				_playerHero = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(PlayerHeroImage));
			}
		}

		public string OpponentHero
		{
			get { return _opponentHero; }
			set
			{
				_opponentHero = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(OpponentHeroImage));
			}
		}

		public bool Coin
		{
			get { return _coin; }
			set
			{
				_coin = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(GotCoin));
			}
		}

		public GameMode GameMode
		{
			get { return _gameMode; }
			set { _gameMode = value;
				OnPropertyChanged();
			}
		}

		public GameResult Result
		{
			get { return _result; }
			set { _result = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(ResultString));
				OnPropertyChanged(nameof(ResultTextColor));
			}
		}

		public int Turns
		{
			get { return _turns; }
			set { _turns = value;
				OnPropertyChanged();
			}
		}

		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public string Note
		{
			get { return _note; }
			set
			{
				_note = value; 
				OnPropertyChanged();
			}
		}

		public bool IsClone { get; set; }

		public string PlayerName
		{
			get { return _playerName; }
			set { _playerName = value;
				OnPropertyChanged();
			}
		}

		public string OpponentName
		{
			get { return _opponentName; }
			set { _opponentName = value;
				OnPropertyChanged();
			}
		}

		public string ReplayFile { get; set; }

		public bool WasConceded { get; set; }

		public int Rank
		{
			get { return _rank; }
			set
			{
				_rank = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(RankString));
			}
		}

		public int Stars
		{
			get { return _stars; }
			set
			{
				_stars = value;
				OnPropertyChanged();
			}
		}

		public int LegendRank
		{
			get { return _legendRank; }
			set { _legendRank = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(RankString));
			}
		}

		public int OpponentLegendRank
		{
			get { return _opponentLegendRank; }
			set
			{
				_opponentLegendRank = value;
				OnPropertyChanged();
			}
		}

		public int OpponentRank { get; set; }

		public int PlayerCardbackId { get; set; }

		public int OpponentCardbackId { get; set; }

		public int FriendlyPlayerId { get; set; }

		public int ScenarioId { get; set; }

		public Region Region
		{
			get { return _region; }
			set { _region = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(RegionString));
			}
		}

		public Format? Format
		{
			get
			{
				return (GameMode == GameMode.Ranked || GameMode == GameMode.Casual) ? (Format?)_format : null;
			}
			set
			{
				if(value.HasValue)
				{
					_format = value.Value;
					OnPropertyChanged();
				}
			}
		}

		public Guid DeckId
		{
			get
			{
				if(_deckId.HasValue)
					return _deckId.Value;
				var deck = DeckList.Instance.Decks.FirstOrDefault(d => d.DeckStats.Games.Any(g => g == this));
				_deckId = deck?.DeckId ?? Guid.Empty;
				return _deckId.Value;
			}
			set { _deckId = value; }
		}

		public string DeckName
		{
			get
			{
				if(!string.IsNullOrEmpty(_deckName))
					return _deckName;
				var deck = DeckList.Instance.Decks.FirstOrDefault(d => d.DeckId == DeckId);
				_deckName = deck?.Name ?? "none";
				return _deckName;
			}
			set { _deckName = value;
				OnPropertyChanged();
			}
		}

		[XmlIgnore]
		public string DeckNameAndVersion
		{
			get
			{
				if (!string.IsNullOrEmpty(_deckNameAndVersion))
					return _deckNameAndVersion;
				var deck = DeckList.Instance.Decks.FirstOrDefault(d => d.DeckId == DeckId)?.GetVersion(PlayerDeckVersion);
				_deckNameAndVersion = deck?.NameAndVersion ?? "none";
				return _deckNameAndVersion;
			}
			set { _deckNameAndVersion = value;
				OnPropertyChanged();
			}
		}

		[XmlIgnore]
		public SolidColorBrush ResultTextColor
		{
			get
			{
				var c = Colors.Black;
				if(Result == GameResult.Win)
					c = Colors.Green;
				else if(Result == GameResult.Loss)
					c = Colors.Red;
				return new SolidColorBrush(c);
			}
		}

		[XmlIgnore]
		public string RegionString => Region == Region.UNKNOWN ? "-" : Region.ToString();

		[XmlIgnore]
		public bool HasRank => Rank > 0 && Rank <= 25;

		[XmlIgnore]
		public bool HasLegendRank => LegendRank > 0;

		[XmlIgnore]
		public string RankString => GameMode == GameMode.Ranked ? (HasLegendRank ? $"L{LegendRank}" : (HasRank ? Rank.ToString() : "-")) : "-";

		[XmlIgnore]
		public int SortableRank => GameMode == GameMode.Ranked ? (HasLegendRank ? -int.MaxValue + LegendRank : (HasRank ? Rank : int.MaxValue)) : int.MaxValue;

		[XmlIgnore]
		public string ResultString => Result + (WasConceded ? "*" : "");

		public SerializableVersion PlayerDeckVersion { get; set; }

		public bool IsAssociatedWithDeckVersion => PlayerDeckVersion != null || !string.IsNullOrEmpty(HearthStatsDeckVersionId);

		[XmlIgnore]
		public string PlayerDeckVersionString => PlayerDeckVersion != null ? PlayerDeckVersion.ToString("v{M}.{m}") : SerializableVersion.Default.ToString("v{M}.{m}");

		[XmlIgnore]
		public ToolTip ResultToolTip => new ToolTip {Content = "conceded", Visibility = (WasConceded ? Visibility.Visible : Visibility.Hidden)};

		[XmlIgnore]
		public bool HasReplayFile => ReplayFile != null && File.Exists(Path.Combine(Config.Instance.ReplayDir, ReplayFile));

		[XmlIgnore]
		public bool CanGetOpponentDeck => OpponentCards.Any();

		[XmlIgnore]
		public bool CanSelectDeck => DeckList.Instance.Decks.Any(d => d.DeckId == DeckId);

		[XmlIgnore]
		public BitmapImage OpponentHeroImage
		{
			get
			{
				HeroClassAll oppHero;
				return Enum.TryParse(OpponentHero, out oppHero) ? ImageCache.GetClassIcon(oppHero) : new BitmapImage();
			}
		}

		[XmlIgnore]
		public BitmapImage PlayerHeroImage
		{
			get
			{
				HeroClassAll playerHero;
				return Enum.TryParse(PlayerHero, out playerHero) ? ImageCache.GetClassIcon(playerHero) : new BitmapImage();
			}
		}

		[XmlIgnore]
		public string Duration => (EndTime - StartTime).Minutes + " min";

		[XmlIgnore]
		public int SortableDuration => (EndTime - StartTime).Minutes;

		[XmlIgnore]
		public string GotCoin
		{
			get { return Coin ? "Yes" : "No"; }
			set { Coin = value.ToLower() == "Yes"; }
		}

		[XmlIgnore]
		public bool HasHearthStatsId => !string.IsNullOrEmpty(HearthStatsId);

		public string HearthStatsDeckId { get; set; }
		public string HearthStatsDeckVersionId { get; set; }

		public bool HasHearthStatsDeckVersionId => !string.IsNullOrEmpty(HearthStatsDeckVersionId) && int.Parse(HearthStatsDeckVersionId) > 0;

		public bool HasHearthStatsDeckId => !string.IsNullOrEmpty(HearthStatsDeckId) && int.Parse(HearthStatsDeckId) > 0;

		public bool BelongsToDeckVerion(Deck deck) => PlayerDeckVersion == deck.Version
													  || (HasHearthStatsDeckVersionId && HearthStatsDeckVersionId == deck.HearthStatsDeckVersionId)
													  || (!HasHearthStatsDeckVersionId && HasHearthStatsDeckId && HearthStatsDeckId == deck.HearthStatsId)
													  || !IsAssociatedWithDeckVersion && deck.Version == new SerializableVersion(1, 0);

		public GameStats CloneWithNewId()
		{
			var newGame = new GameStats(Result, OpponentHero, PlayerHero)
			{
				StartTime = StartTime,
				EndTime = EndTime,
				Coin = Coin,
				GameMode = GameMode,
				Turns = Turns,
				PlayerName = PlayerName,
				OpponentName = OpponentName,
				ReplayFile = ReplayFile,
				WasConceded = WasConceded,
				PlayerDeckVersion = PlayerDeckVersion,
				IsClone = true
			};
			return newGame;
		}

		protected bool Equals(GameStats other) => GameId.Equals(other.GameId);

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
				return false;
			if(ReferenceEquals(this, obj))
				return true;
			if(obj.GetType() != GetType())
				return false;
			return Equals((GameStats)obj);
		}

		public override int GetHashCode() => GameId.GetHashCode();

		public void GameEnd()
		{
			EndTime = DateTime.Now;
			Log.Info("Current Game ended after " + Turns + " turns");
		}

		public override string ToString() => Result + " vs " + OpponentHero + ", " + StartTime;

		public void ResetHearthstatsIds()
		{
			HearthStatsDeckId = null;
			HearthStatsDeckVersionId = null;
			HearthStatsId = null;
		}

		[XmlArray(ElementName = "PlayerCards")]
		[XmlArrayItem(ElementName = "Card")]
		public List<TrackedCard> PlayerCards { get; set; } = new List<TrackedCard>();
		[XmlArray(ElementName = "OpponentCards")]
		[XmlArrayItem(ElementName = "Card")]
		public List<TrackedCard> OpponentCards { get; set; } = new List<TrackedCard>();

		public void SetPlayerCards(Deck deck, List<Card> revealedCards)
		{
			PlayerCards.Clear();
			foreach(var c in revealedCards)
			{
				var card = PlayerCards.FirstOrDefault(x => x.Id == c.Id);
				if(card != null)
					card.Count++;
				else
					PlayerCards.Add(new TrackedCard(c.Id, c.Count));
			}
			if(deck != null)
			{
				foreach(var c in deck.Cards)
				{
					var e = PlayerCards.FirstOrDefault(x => x.Id == c.Id);
					if(e == null)
						PlayerCards.Add(new TrackedCard(c.Id, c.Count, c.Count));
					else if(c.Count > e.Count)
					{
						e.Unconfirmed = c.Count - e.Count;
						e.Count = c.Count;
					}
				}
			}
		}

		public void SetOpponentCards(List<Card> revealedCards)
		{
			OpponentCards.Clear();
			foreach(var c in revealedCards)
			{
				var card = OpponentCards.FirstOrDefault(x => x.Id == c.Id);
				if(card != null)
					card.Count++;
				else
					OpponentCards.Add(new TrackedCard(c.Id, c.Count));
			}
		}


		public bool ShouldSerializeFormat() => Format.HasValue;
		public bool ShouldSerializeNote() => !string.IsNullOrEmpty(Note);
		public bool ShouldSerializePlayerCards() => PlayerCards.Any();
		public bool ShouldSerializeOpponentCards() => OpponentCards.Any();
		public bool ShouldSerializeRank() => Rank > 0;
		public bool ShouldSerializeStars() => Stars > 0;
		public bool ShouldSerializeLegendRank() => LegendRank > 0;
		public bool ShouldSerializeOpponentRank() => OpponentRank > 0;
		public bool ShouldSerializeOpponentLegendRank() => OpponentLegendRank > 0;
		public bool ShouldSerializeRegion() => Region != Region.UNKNOWN;
		public bool ShouldSerializeIsClone() => IsClone;
		public bool ShouldSerializePlayerCardbackId() => PlayerCardbackId > 0;
		public bool ShouldSerializeOpponentCardbackId() => OpponentCardbackId > 0;
		public bool ShouldSerializeFriendlyPlayerId() => FriendlyPlayerId > 0;
		public bool ShouldSerializeScenarioId() => ScenarioId > 0;

		#region Obsolete

		private string GameFile => Config.Instance.DataDir + $@"Games\Game_{GameId}.xml";

		internal List<TurnStats> LoadTurnStats()
		{
			if(GameId == Guid.Empty || !File.Exists(GameFile))
				return new List<TurnStats>();
			try
			{
				return XmlManager<List<TurnStats>>.Load(GameFile);
			}
			catch(Exception ex)
			{
				Log.Error($"Error loading file: {GameFile}/n{ex}");
			}
			return new List<TurnStats>();
		}

		internal void DeleteGameFile()
		{
			try
			{
				if(!File.Exists(GameFile))
					return;
				File.Delete(GameFile);
				Log.Info("Deleted gamefile: " + GameFile);
			}
			catch(Exception ex)
			{
				Log.Error($"Error deleting gamefile: {GameFile}/n{ex}");
			}
		}
		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}