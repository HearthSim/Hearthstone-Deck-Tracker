#region

using System;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.API.Objects
{
	public class GameStatsObject
	{
		public int klass_id { get; set; }
		public bool coin { get; set; }
		public string created_at { get; set; }
		public int? duration { get; set; }
		public int id { get; set; }
		public int mode_id { get; set; }
		public string notes { get; set; }
		public int? numturns { get; set; }
		public int oppclass_id { get; set; }
		public string oppname { get; set; }
		public int result_id { get; set; }

		public GameStats ToGameStats(string versionId, string deckId, int ranklvl)
		{
			try
			{
				DateTime createdAt;
				if(!DateTime.TryParse(created_at, out createdAt))
					createdAt = DateTime.Now;
				return new GameStats(Dictionaries.GameResultDict[result_id], Dictionaries.HeroDict[oppclass_id], Dictionaries.HeroDict[klass_id])
				{
					GameMode = Dictionaries.GameModeDict[mode_id],
					OpponentName = oppname ?? "",
					Turns = numturns ?? 0,
					Coin = coin,
					HearthStatsId = id.ToString(),
					HearthStatsDeckId = deckId,
					HearthStatsDeckVersionId = versionId,
					Note = notes ?? "",
					Rank = ranklvl,
					StartTime = createdAt,
					EndTime = createdAt.AddSeconds(duration ?? 0),
					HearthstoneBuild = null
				};
			}
			catch(Exception e)
			{
				Log.Error("(HearthStatsAPI) error converting GameStatsObject: " + e);
				return null;
			}
		}
	}
}