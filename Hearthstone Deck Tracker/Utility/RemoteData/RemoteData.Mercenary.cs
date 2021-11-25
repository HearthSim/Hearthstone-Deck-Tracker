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
			public string? Name { get; set; }

			[JsonProperty("collectible")]
			public bool Collectible { get; set; }

			[JsonProperty("skinDbfIds")]
			public List<int> ArtVariationIds { get; set; } = new List<int>();

			[JsonProperty("specializations")]
			public List<MercenarySpecialization> Specializations { get; set; } = new List<MercenarySpecialization>();
		}

		public class MercenarySpecialization
		{
			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("abilities")]
			public List<MercenaryAbility> Abilities { get; set; } = new List<MercenaryAbility>();
		}

		public class MercenaryAbility
		{
			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("tiers")]
			public List<MercenaryAbilityTier> Tiers { get; set; } = new List<MercenaryAbilityTier>();
		}

		public class MercenaryAbilityTier
		{
			[JsonProperty("tier")]
			public int Tier { get; set; }

			[JsonProperty("dbf_id")]
			public int DbfId { get; set; }
		}
	}
}
