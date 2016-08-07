#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Stats
{
	public class LastGames : INotifyPropertyChanged
	{
		private const int MaxGamesCount = 10;
		private const string FileName = "LastGames.xml";
		private List<GameInfo> _gameInfos;
		private bool _hasGames;

		static LastGames()
		{
		}

		private LastGames()
		{
		}

		public static LastGames Instance { get; } = new LastGames();

		private static string FilePath => Path.Combine(Config.AppDataPath, FileName);

		public List<GameInfo> GameInfos => _gameInfos ?? (_gameInfos = Load());

		public List<GameStats> Games => GetGames();

		public bool HasGames
		{
			get { return _hasGames; }
			private set
			{
				_hasGames = value;
				OnPropertyChanged();
			}
		}

		private List<GameStats> GetGames()
		{
			var remove = new List<GameInfo>();
			var games = new List<GameStats>();
			foreach(var gi in GameInfos)
			{
				DeckStats stats;
				if(gi.DeckId != Guid.Empty)
					DeckStatsList.Instance.DeckStats.TryGetValue(gi.DeckId, out stats);
				else if(!string.IsNullOrEmpty(gi.Hero))
					stats = DefaultDeckStats.Instance.GetDeckStats(gi.Hero);
				else
					stats = DefaultDeckStats.Instance.DeckStats.FirstOrDefault(x => x.Games.Any(g => g.GameId == gi.GameId));
				if(stats == null)
					remove.Add(gi);
				else
				{
					var game = stats.Games.FirstOrDefault(x => x.GameId == gi.GameId);
					if(game?.HasReplayFile ?? false)
						games.Add(game);
					else
						remove.Add(gi);
				}
				
			}
			foreach(var game in remove)
				_gameInfos.Remove(game);
			HasGames = games.Any();
			return games;
		} 

		public void Add(GameStats game) => Add(game.DeckId, game.GameId, game.PlayerHero);

		public void Add(Guid deckId, Guid gameId, string hero)
		{
			GameInfos.Insert(0, new GameInfo(deckId, gameId, hero));
			if(GameInfos.Count > MaxGamesCount)
				GameInfos.RemoveAt(MaxGamesCount);
			OnPropertyChanged(nameof(Games));
		}

		public void Remove(GameStats game) => Remove(game.GameId);

		public void Remove(Guid gameId)
		{
			var gameInfo = GameInfos.FirstOrDefault(x => x.GameId == gameId);
			if(gameInfo != null)
				GameInfos.Remove(gameInfo);
			OnPropertyChanged(nameof(Games));
		}

		public void RemoveDeck(Deck deck) => RemoveDeck(deck.DeckId);

		public void RemoveDeck(DeckStats deck) => RemoveDeck(deck.DeckId);

		public void RemoveDeck(Guid deckId)
		{
			if(deckId == Guid.Empty)
				return;
			var games = GameInfos.Where(x => x.DeckId == deckId);
			foreach(var game in games)
				GameInfos.Remove(game);
			OnPropertyChanged(nameof(Games));
		}

		public static void Save()
		{
			try
			{
				XmlManager<List<GameInfo>>.Save(FilePath, Instance._gameInfos);
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		private static List<GameInfo> Load()
		{
			if(!File.Exists(FilePath))
			{
				var games = new List<GameStats>();
				var enumerator = DeckStatsList.Instance.DeckStats.GetEnumerator();
				while(enumerator.MoveNext())
					games.AddRange(enumerator.Current.Value.Games.Where(x => x.HasReplayFile));
				return games.OrderByDescending(x => x.StartTime).Take(10).Select(
						x => new GameInfo { DeckId = x.DeckId, GameId = x.GameId }).ToList();
			}
			try
			{
				return XmlManager<List<GameInfo>>.Load(FilePath);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return new List<GameInfo>();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class GameInfo
	{
		public GameInfo()
		{
		}

		public GameInfo(Guid deckId, Guid gameId, string hero = null)
		{
			DeckId = deckId;
			GameId = gameId;
			if(DeckId == Guid.Empty)
				Hero = hero;
		}

		[XmlAttribute("deckId")]
		public Guid DeckId { get; set; }

		[XmlAttribute("gameId")]
		public Guid GameId { get; set; }

		[XmlAttribute("hero")]
		public string Hero { get; set; }
	}
}
