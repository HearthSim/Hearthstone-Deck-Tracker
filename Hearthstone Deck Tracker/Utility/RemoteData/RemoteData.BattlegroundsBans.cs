using Newtonsoft.Json;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Utility.RemoteData
{
	internal partial class RemoteData
	{
		public class BattlegroundsBans
		{
			[JsonProperty("by_anomaly")]
			public List<BattlegroundsAnomalyBans> ByAnomaly { get; set; } = new List<BattlegroundsAnomalyBans>();
		}

		public class BattlegroundsAnomalyBans
		{
			[JsonProperty("anomaly_dbf_id")]
			public int AnomalyDbfId { get; set; }

			[JsonProperty("banned_minion_ids")]
			public string[] BannedMinionIds { get; set; } = new string[0];
		}
	}
}
