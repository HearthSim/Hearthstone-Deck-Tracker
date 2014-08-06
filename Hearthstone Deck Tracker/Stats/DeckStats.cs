using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Hearthstone_Deck_Tracker.Stats
{
	public class DeckStats
	{
		public string Name;
		[XmlArray(ElementName = "Games")]
		[XmlArrayItem(ElementName = "Game")]
		public List<GameStats> Games;
		public DeckStats()
		{
			Games = new List<GameStats>();
		}
		public DeckStats(string name)
		{
			Name = name;
			Games = new List<GameStats>();
		}

		public void AddGameResult(GameResult result, string opponentHero)
		{
			Games.Add(new GameStats(result, opponentHero));
		}
		public void AddGameResult(GameStats gameStats)
		{
			Games.Add(gameStats);
		}
	}
	public class DeckStatsList
	{
		[XmlArray(ElementName = "DeckStats")]
		[XmlArrayItem(ElementName = "Deck")]
		public List<DeckStats> DeckStats; 
		public DeckStatsList()
		{
			DeckStats = new List<DeckStats>();
		}

		private static DeckStatsList _instance;
		public static DeckStatsList Instance
		{
			get { return _instance ?? (_instance = new DeckStatsList()); }
		}
		public static void Load()
		{
			var file = Config.Instance.HomeDir + "DeckStats.xml";
			_instance = XmlManager<DeckStatsList>.Load(file);
		}
		public static void Save()
		{
			var file = Config.Instance.HomeDir + "DeckStats.xml";
			XmlManager<DeckStatsList>.Save(file, Instance);
		}
	}
}
