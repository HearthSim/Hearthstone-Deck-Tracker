using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Stats
{
	public class GameStats
	{
		public string OpponentHero;
		public bool Coin;
		public GameResult Result;
		public int Turns;
		public DateTime StartTime;
		public DateTime EndTime;

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
