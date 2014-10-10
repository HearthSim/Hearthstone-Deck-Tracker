using System.Collections.Generic;
using System.Xml.Serialization;

namespace Hearthstone_Deck_Tracker.Stats
{
    public class DeckStats
    {
        [XmlArray(ElementName = "Games")]
        [XmlArrayItem(ElementName = "Game")]
        public List<GameStats> Games;

        public string Name;

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
        private static DeckStatsList _instance;

        [XmlArray(ElementName = "DeckStats")]
        [XmlArrayItem(ElementName = "Deck")]
        public List<DeckStats> DeckStats;

        public DeckStatsList()
        {
            DeckStats = new List<DeckStats>();
        }

        public static DeckStatsList Instance
        {
            get { return _instance ?? (_instance = new DeckStatsList()); }
        }

        public static void Load()
        {
            var file = Config.Instance.DataDir + "DeckStats.xml";
            _instance = XmlManager<DeckStatsList>.Load(file);
        }

        public static void Save()
        {
            var file = Config.Instance.DataDir + "DeckStats.xml";
            XmlManager<DeckStatsList>.Save(file, Instance);
        }
    }
}