using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.RemoteData
{
	internal partial class RemoteData
	{
		public class MercenariesTask
		{
			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("visitor_id")]
			public int VisitorId { get; set; }

			[JsonProperty("mercenary_id")]
			public int MercenaryId { get; set; }

			[JsonProperty("mercenary_default_card_id")]
			public string MercenaryDefaultCardId { get; set; }

			[JsonProperty("title")]
			public string Title { get; set; }

			[JsonProperty("description")]
			public string Description { get; set; }

			[JsonProperty("quota")]
			public int Quota { get; set; }

		}
	}
}
