using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.RemoteData
{
	internal partial class RemoteData
	{
		public class Mercenary
		{
			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("collectible")]
			public bool Collectible { get; set; }

			[JsonProperty("art_variation_ids")]
			public List<string> ArtVariationIds { get; set; } = new List<string>();

			[JsonProperty("abilities")]
			public List<MercenaryAbility> Abilities { get; set; } = new List<MercenaryAbility>();
		}

		public class MercenaryAbility
		{
			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("tier_ids")]
			public List<string> TierDbfIds { get; set; } = new List<string>();
		}
	}
}
