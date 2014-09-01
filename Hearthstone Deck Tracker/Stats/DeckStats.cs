using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

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
			var file = Config.Instance.HomeDir + "DeckStats.xml";
			_instance = XmlManager<DeckStatsList>.Load(file);

            analyzeAllGames();
		}

		public static void Save()
		{
			var file = Config.Instance.HomeDir + "DeckStats.xml";
			XmlManager<DeckStatsList>.Save(file, Instance);
		}

        public static Dictionary<String, Dictionary<String, int>> predictiondictionary;

        struct cardpercent
        {
            public string cardid;
            public float percent;
        };

        static public void doPrediction(String enemy, int turnnumber)
        {
            Dictionary<String, int> innerhash = predictiondictionary[enemy + turnnumber];
            cardpercent newcardpercent;
            List<cardpercent> cardpredictions = new List<cardpercent>();

            float numpossiblecards = 0;

            foreach (String cardid in innerhash.Keys)
            {
                numpossiblecards += innerhash[cardid];
            }

            foreach (String cardid in innerhash.Keys)
            {
                float thiscardcount = (float)innerhash[cardid];
                newcardpercent.cardid = cardid;
                newcardpercent.percent = thiscardcount / numpossiblecards;
                cardpredictions.Add(newcardpercent);
            }
            List<cardpercent> SortedList = cardpredictions.OrderBy(o => (1.0 -  o.percent) ).ToList();
            String predictionstring = "\nPrediction for Turn " + turnnumber + "\n\n";
            int i;
            for (i = 0; i < 7 && i < SortedList.Count; i++)
            {
                Hearthstone.Card card = Hearthstone.Game.GetCardFromId(SortedList[i].cardid);
                string percentstring = (SortedList[i].percent * 100.0).ToString("0.0");
                predictionstring += card.Name + " " + percentstring + "%" + "\n";
            }
            predictionText = predictionstring;

        }
        public static String predictionText;

        public static void analyzeAllGames()
        {
            

            predictiondictionary = new Dictionary<String, Dictionary<String, int>>();

            List<DeckStats> mydeckstats = DeckStatsList.Instance.DeckStats;
            foreach (DeckStats deckstats in mydeckstats)
            {
                foreach (GameStats game in deckstats.Games)
                {
                    string enemyname = game.OpponentHero;
                    foreach (TurnStats turn in game.TurnStats)
                    {
                        int turnnumbner = turn.Turn;
                        string enemy_turn_hashid = enemyname + turnnumbner;
                        Dictionary<String, int> innerdictionary;
                        if (predictiondictionary.ContainsKey(enemy_turn_hashid))
                        {
                            innerdictionary = predictiondictionary[enemy_turn_hashid];
                        }
                        else
                        {
                            innerdictionary = new Dictionary<String, int>();
                            predictiondictionary.Add(enemy_turn_hashid, innerdictionary);
                        }

                        foreach (TurnStats.Play play in turn.Plays)
                        {
                            if (play.Type == PlayType.OpponentPlay)
                            {
                                string cardid = play.CardId; 

                                /// first create/get the inner hash
                                /// 
                                if (innerdictionary.ContainsKey(cardid))
                                {
                                    int count = innerdictionary[cardid];
                                    count++;
                                    innerdictionary[cardid] = count;
                                }
                                else
                                {
                                    innerdictionary.Add(cardid, 1);
                                }
                            }
                        }
                    }
                }
            }

        }
	}
}