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
		public GameResult Result;
		public DateTime StartTime;
		public DateTime EndTime;

		public GameStats() { }
		
		public GameStats(GameResult result, string opponentHero)
		{
			Result = result;
			OpponentHero = opponentHero;
			StartTime = DateTime.Now;
		}

		public void GameEnd()
		{
			EndTime = DateTime.Now;
		}

		public bool AllFieldsSet()
		{
			return Result != GameResult.None && EndTime != default(DateTime);
		}
	}

	public enum GameResult
	{
		None, Win, Loss
	}
}
