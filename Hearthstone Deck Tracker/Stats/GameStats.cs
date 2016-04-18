#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static Hearthstone_Deck_Tracker.Enums.PlayType;

#endregion

namespace Hearthstone_Deck_Tracker.Stats
{
	public class GameStats
	{
		private Guid? _deckId;
		private string _deckName;
		private List<TurnStats> _turnStats;
		public Guid GameId;
		public string HearthStatsId;

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
			HearthstoneBuild = Helper.GetHearthstoneBuild();
		}

		private string GamesDir => Config.Instance.DataDir + "Games";

		private string GameFile => GamesDir + $@"\Game_{GameId}.xml";

		//playerhero does not get loaded from xml for some reason
		public string PlayerHero { get; set; }
		public string OpponentHero { get; set; }
		public bool Coin { get; set; }
		public GameMode GameMode { get; set; }
		public GameResult Result { get; set; }
		public int Turns { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string Note { get; set; }
		public bool IsClone { get; set; }
		public string PlayerName { get; set; }
		public string OpponentName { get; set; }
		public bool VerifiedHeroes { get; set; }
		public string ReplayFile { get; set; }
		public bool WasConceded { get; set; }
		public int Rank { get; set; }
		public int OpponentRank { get; set; }
		public Region Region { get; set; }
		public int? HearthstoneBuild { get; set; }

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
				_deckName = deck != null ? deck.Name : "none";
				return _deckName;
			}
			set { _deckName = value; }
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
		public string RankString => HasRank && GameMode == GameMode.Ranked ? Rank.ToString() : "-";

		[XmlIgnore]
		public int SortableRank => HasRank && GameMode == GameMode.Ranked ? Rank : -1;

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
		public bool CanGetOpponentDeck => TurnStats.Any();

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
		[XmlArray(ElementName = "Turns")]
		[XmlArrayItem(ElementName = "Turn")]
		public List<TurnStats> TurnStats => _turnStats ?? (_turnStats = LoadTurnStats());

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

		public HsReplayInfo HsReplay { get; set; }

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
				_turnStats = LoadTurnStats(),
				PlayerName = PlayerName,
				OpponentName = OpponentName,
				ReplayFile = ReplayFile,
				WasConceded = WasConceded,
				VerifiedHeroes = VerifiedHeroes,
				PlayerDeckVersion = PlayerDeckVersion,
				HearthstoneBuild = HearthstoneBuild,
				HsReplay = HsReplay,
				IsClone = true
			};
			newGame.Save();
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

		private void ResolveSecrets(IEnumerable<TurnStats> newTurnStats)
		{
			var unresolvedSecrets = 0;
			var triggeredSecrets = 0;
			TurnStats.Play candidateSecret = null;

			foreach(var play in newTurnStats.SelectMany(turn => turn.Plays))
			{
				// is secret play
				if((play.Type == OpponentHandDiscard && play.CardId == "") || play.Type == OpponentSecretPlayed)
				{
					unresolvedSecrets++;
					candidateSecret = play;
					play.Type = OpponentSecretPlayed;
				}
				else if(play.Type == OpponentSecretTriggered)
				{
					if(unresolvedSecrets == 1 && candidateSecret != null)
						candidateSecret.CardId = play.CardId;
					triggeredSecrets++;
					if(triggeredSecrets == unresolvedSecrets)
					{
						triggeredSecrets = 0;
						unresolvedSecrets = 0;
					}
				}
			}
		}

		private List<TurnStats> LoadTurnStats()
		{
			Directory.CreateDirectory(GamesDir);
			if(GameId == Guid.Empty || !File.Exists(GameFile))
				return new List<TurnStats>();
			try
			{
				var newturnstats = XmlManager<List<TurnStats>>.Load(GameFile);
				ResolveSecrets(newturnstats);
				return newturnstats;
			}
			catch(Exception ex)
			{
				Log.Error($"Error loading file: {GameFile}/n{ex}");
			}
			return new List<TurnStats>();
		}

		public void DeleteGameFile()
		{
			try
			{
				LastGames.Instance.Remove(GameId);
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

		public void GameEnd()
		{
			EndTime = DateTime.Now;
			Log.Info("Current Game ended after " + Turns + " turns");
			Save();
		}

		private void Save() => XmlManager<List<TurnStats>>.Save(GameFile, TurnStats);

		public void AddPlay(PlayType type, int turn, string cardId)
		{
			var turnStats = TurnStats.FirstOrDefault(t => t.Turn == turn);
			if(turnStats == null)
			{
				turnStats = new TurnStats {Turn = turn};
				TurnStats.Add(turnStats);
			}
			turnStats.AddPlay(type, cardId);
		}

		public override string ToString() => $"[{GameMode}] {Result} VS. {OpponentName} ({OpponentHero}), {StartTime.ToString("g")}";

		public void ResetHearthstatsIds()
		{
			HearthStatsDeckId = null;
			HearthStatsDeckVersionId = null;
			HearthStatsId = null;
		}

		public Deck GetOpponentDeck()
		{
			var ignoreCards = new List<Card>();
			var deck = new Deck {Class = OpponentHero};
			foreach(var play in TurnStats.SelectMany(turn => turn.Plays))
			{
				switch(play.Type)
				{
					case OpponentPlay:
					case OpponentDeckDiscard:
					case OpponentHandDiscard:
					case OpponentSecretTriggered:
					{
						var card = Database.GetCardFromId(play.CardId);
						if(Database.IsActualCard(card) && (card.PlayerClass == null || card.PlayerClass == OpponentHero))
						{
							if(ignoreCards.Contains(card))
							{
								ignoreCards.Remove(card);
								continue;
							}
							var deckCard = deck.Cards.FirstOrDefault(c => c.Id == card.Id);
							if(deckCard != null)
								deckCard.Count++;
							else
								deck.Cards.Add(card);
						}
					}
						break;
					case OpponentBackToHand:
					{
						var card = Database.GetCardFromId(play.CardId);
						if(Database.IsActualCard(card))
							ignoreCards.Add(card);
					}
						break;
				}
			}
			return deck;
		}
	}
}