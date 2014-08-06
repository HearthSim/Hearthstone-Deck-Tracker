using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Hearthstone_Deck_Tracker.Stats
{
	public class GameStats
	{
		public string OpponentHero { get; set; }
		public bool Coin { get; set; }
		public GameResult Result { get; set; }
		public int Turns { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		[XmlIgnore]
		public string Duration { get { return (EndTime - StartTime).Minutes + " min"; } }

		[XmlIgnore]
		public string GotCoin { get { return Coin ? "Yes" : "No"; } set { Coin = value.ToLower() == "Yes"; } }

		public GameStats() { }
		
		public GameStats(GameResult result, string opponentHero)
		{
			Coin = false;
			Result = result;
			OpponentHero = opponentHero;
			StartTime = DateTime.Now;
			Logger.WriteLine("Started new game", "Gamestats");
		}

		public void GameEnd()
		{
			EndTime = DateTime.Now;
			Logger.WriteLine("Current Game ended after " + Turns + " turns", "Gamestats");
		}
	}

	public enum GameResult
	{
		None, Win, Loss
	}
}
