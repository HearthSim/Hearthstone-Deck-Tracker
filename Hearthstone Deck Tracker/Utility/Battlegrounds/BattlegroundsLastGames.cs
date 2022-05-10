using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Utility.Battlegrounds
{
	[XmlRoot("BgsLastGames")]
	public sealed class BattlegroundsLastGames
	{
		private static readonly Lazy<BattlegroundsLastGames> LazyInstance = new Lazy<BattlegroundsLastGames>(Load);

		public static BattlegroundsLastGames Instance = LazyInstance.Value;

		[XmlElement("Game")]
		public List<GameItem> Games { get; set; } = new List<GameItem>();

		private static string ConfigPath => Path.Combine(Config.Instance.ConfigDir, "BgsLastGames.xml");

		private static BattlegroundsLastGames Load()
		{
			if(!File.Exists(ConfigPath))
				return new BattlegroundsLastGames();
			try
			{
				return XmlManager<BattlegroundsLastGames>.Load(ConfigPath);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
			return new BattlegroundsLastGames();
		}

		public void AddGame(string time, string hero, int rating, int ratingAfter, int placemenent, bool save = true)
		{
			RemoveGame(time, false);
			Games.Add(new GameItem(time, hero, rating, ratingAfter, placemenent));
			if(save)
				Save();
		}

		public void RemoveGame(string time, bool save = true)
		{
			var existing = Games.FirstOrDefault(x => x.Time != null && x.Time.Equals(time));
			if(existing != null)
				Games.Remove(existing);
			if(save)
				Save();
		}

		public static void Save()
		{
			try
			{
				XmlManager<BattlegroundsLastGames>.Save(ConfigPath, Instance);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		public void Reset()
		{
			Games.Clear();
			Save();
		}

		public class GameItem
		{
			public GameItem(string time, string hero, int rating, int ratingAfter, int placemenent)
			{
				Time = time;
				Hero = hero;
				Rating = rating;
				RatingAfter = ratingAfter;
				Placement = placemenent;
			}

			public GameItem()
			{
			}

			[XmlAttribute("Time")]
			public string? Time { get; set; }

			[XmlAttribute("Hero")]
			public string? Hero { get; set; }

			[XmlAttribute("Rating")]
			public int Rating { get; set; }

			[XmlAttribute("RatingAfter")]
			public int RatingAfter { get; set; }

			[XmlAttribute("Placemenent")]
			public int Placement { get; set; }

		}
	}
}
