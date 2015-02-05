#region

using System;
using Hearthstone_Deck_Tracker.Stats;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.API.Objects
{
	public class GameStatsObject
	{
		public int klass_id { get; set; }
		public bool coin { get; set; }
		public string created_at { get; set; }
		public int duration { get; set; }
		public int id { get; set; }
		public int mode_id { get; set; }
		public string notes { get; set; }
		public int numturns { get; set; }
		public int oppclass_id { get; set; }
		public string oppname { get; set; }
		public int result_id { get; set; }

		public GameStats ToGameStats(string versionId, string deckId, int ranklvl)
		{
			try
			{
				return new GameStats(Dictionaries.GameResultDict[result_id], Dictionaries.HeroDict[oppclass_id], Dictionaries.HeroDict[klass_id])
				{
					GameMode = Dictionaries.GameModeDict[mode_id],
					OpponentName = oppname,
					Turns = numturns,
					Coin = coin,
					HearthStatsId = id.ToString(),
					HearthStatsDeckId = deckId,
					HearthStatsDeckVersionId = versionId,
					Note = notes,
					Rank = ranklvl,
					StartTime = DateTime.Parse(created_at),
					EndTime = DateTime.Parse(created_at).AddSeconds(duration),
				};
			}
			catch(Exception e)
			{
				Logger.WriteLine("error converting GameStatsObject " + e, "HearthStatsAPI");
				return null;
			}
		}
	}
}