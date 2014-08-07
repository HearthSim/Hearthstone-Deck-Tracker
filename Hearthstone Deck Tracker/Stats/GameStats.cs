using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Stats
{
	public class GameStats
	{
		public readonly Guid GameId;
		private List<TurnStats> _turnStats;

		public GameStats()
		{
		}

		public GameStats(GameResult result, string opponentHero)
		{
			Coin = false;
			Result = result;
			OpponentHero = opponentHero;
			StartTime = DateTime.Now;
			Logger.WriteLine("Started new game", "Gamestats");
			GameId = Guid.NewGuid();
		}

		private string _gamesDir
		{
			get { return Config.Instance.HomeDir + "Games"; }
		}

		private string _gameFile
		{
			get { return _gamesDir + string.Format(@"\Game_{0}.xml", GameId); }
		}

		public string OpponentHero { get; set; }
		public bool Coin { get; set; }
		public Game.GameMode GameMode { get; set; }
		public GameResult Result { get; set; }
		public int Turns { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		[XmlIgnore]
		[XmlArray(ElementName = "Turns")]
		[XmlArrayItem(ElementName = "Turn")]
		public List<TurnStats> TurnStats
		{
			get { return _turnStats ?? (_turnStats = LoadTurnStats()); }
		}

		[XmlIgnore]
		public string Duration
		{
			get { return (EndTime - StartTime).Minutes + " min"; }
		}

		[XmlIgnore]
		public string GotCoin
		{
			get { return Coin ? "Yes" : "No"; }
			set { Coin = value.ToLower() == "Yes"; }
		}

		protected bool Equals(GameStats other)
		{
			return GameId.Equals(other.GameId);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj)) return false;
			if(ReferenceEquals(this, obj)) return true;
			if(obj.GetType() != this.GetType()) return false;
			return Equals((GameStats)obj);
		}

		public override int GetHashCode()
		{
			return GameId.GetHashCode();
		}

		private List<TurnStats> LoadTurnStats()
		{
			Directory.CreateDirectory(_gamesDir);
			if(GameId != Guid.Empty && File.Exists(_gameFile))
				return XmlManager<List<TurnStats>>.Load(_gameFile);
			return new List<TurnStats>();
		}

		public void DeleteGameFile()
		{
			try
			{
				if(File.Exists(_gameFile))
				{
					File.Delete(_gameFile);
					Logger.WriteLine("Deleted gamefile: " + _gameFile);
				}
			}
			catch(Exception)
			{
				Logger.WriteLine("Error deleting gamefile: " + _gameFile);
			}
		}

		public void GameEnd()
		{
			EndTime = DateTime.Now;
			Logger.WriteLine("Current Game ended after " + Turns + " turns", "Gamestats");
			XmlManager<List<TurnStats>>.Save(_gameFile, TurnStats);
		}

		public void AddPlay(PlayType type, int turn, string cardId)
		{
			var turnStats = TurnStats.FirstOrDefault(t => t.Turn == turn);
			if(turnStats == null)
			{
				turnStats = new TurnStats {Turn = turn};
				TurnStats.Add(turnStats);
			}
			turnStats.AddPlay(type, cardId);
			Logger.WriteLine(string.Format("New play: {0} ({1}, turn: {2})", type, cardId, turn), "GameStats");
		}

		public override string ToString()
		{
			return Result + " vs " + OpponentHero + ", " + StartTime;
		}
	}

	public enum GameResult
	{
		None,
		Win,
		Loss
	}

	public enum PlayType
	{
		PlayerPlay,
		PlayerDraw,
		PlayerGet,
		PlayerMulligan,
		PlayerHandDiscard,
		PlayerDeckDiscard,
		PlayerBackToHand,
		PlayerHeroPower,
		OpponentPlay,
		OpponentDraw,
		OpponentGet,
		OpponentMulligan,
		OpponentHandDiscard,
		OpponentDeckDiscard,
		OpponentBackToHand,
		OpponentSecretTriggered,
		OpponentHeroPower
	}
}