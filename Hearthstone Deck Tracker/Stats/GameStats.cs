﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Stats
{
	public class GameStats
	{
		private readonly string[] _hsClasses = new[] {"Druid", "Hunter", "Mage", "Priest", "Paladin", "Shaman", "Rogue", "Warlock", "Warrior"};
		public Guid GameId;
		private List<TurnStats> _turnStats;

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
			Logger.WriteLine("Started new game", "Gamestats");
			GameId = Guid.NewGuid();
		}

		private string _gamesDir
		{
			get { return Config.Instance.DataDir + "Games"; }
		}

		private string _gameFile
		{
			get { return _gamesDir + string.Format(@"\Game_{0}.xml", GameId); }
		}

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
		
		[XmlIgnore]
		public BitmapImage OpponentHeroImage
		{
			get
			{
				if(!_hsClasses.Contains(OpponentHero))
					return new BitmapImage();
				var uri = new Uri(string.Format("../Resources/{0}_small.png", OpponentHero.ToLower()), UriKind.Relative);
				return new BitmapImage(uri);
			}
		}
		 
		[XmlIgnore]
		public BitmapImage PlayerHeroImage
		{
			get
			{
				if(!_hsClasses.Contains(PlayerHero))
					return new BitmapImage();
				var uri = new Uri(string.Format("../Resources/{0}_small.png", PlayerHero.ToLower()), UriKind.Relative);
				return new BitmapImage(uri);
			}
		}


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
		public int SortableDuration
		{
			get { return (EndTime - StartTime).Minutes; }
		}

		[XmlIgnore]
		public string GotCoin
		{
			get { return Coin ? "Yes" : "No"; }
			set { Coin = value.ToLower() == "Yes"; }
		}

		public GameStats CloneWithNewId()
		{
			var newGame = new GameStats(Result, OpponentHero, PlayerHero) {StartTime = StartTime, EndTime = EndTime, Coin = Coin, GameMode = GameMode, Turns = Turns, _turnStats = LoadTurnStats(), IsClone = true};
			newGame.Save();
			return newGame;
		}

		protected bool Equals(GameStats other)
		{
			return GameId.Equals(other.GameId);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj)) return false;
			if(ReferenceEquals(this, obj)) return true;
			if(obj.GetType() != GetType()) return false;
			return Equals((GameStats)obj);
		}

		public override int GetHashCode()
		{
			return GameId.GetHashCode();
		}


        private void ResolveSecrets(IEnumerable<TurnStats> newturnstats)
        {
            var unresolvedSecrets = 0;
            var triggeredSecrets = 0;
            TurnStats.Play candidateSecret = null;

            foreach (var turn in newturnstats)
            {
                foreach (var play in turn.Plays)
                {
                    // is secret play
                    if ((play.Type == PlayType.OpponentHandDiscard && play.CardId == "") ||
                        play.Type == PlayType.OpponentSecretPlayed)
                    {
                        unresolvedSecrets++;
                        candidateSecret = play;
                        play.Type = PlayType.OpponentSecretPlayed;
                    }
                    else if (play.Type == PlayType.OpponentSecretTriggered)
                    {
                        if (unresolvedSecrets == 1 && candidateSecret != null)
                        {
                            candidateSecret.CardId = play.CardId;
                        }
                        triggeredSecrets++;
                        if (triggeredSecrets == unresolvedSecrets)
                        {
                            triggeredSecrets = 0;
                            unresolvedSecrets = 0;

                        }
                    }
                }
            }
        }

        private List<TurnStats> LoadTurnStats()
        {
            Directory.CreateDirectory(_gamesDir);
            if (GameId != Guid.Empty && File.Exists(_gameFile))
            {
                var newturnstats = XmlManager<List<TurnStats>>.Load(_gameFile);
                ResolveSecrets(newturnstats);
                return newturnstats;
            }
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
			Save();
		}

		private void Save()
		{
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
}