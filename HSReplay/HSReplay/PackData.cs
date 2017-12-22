using Newtonsoft.Json;

namespace HSReplay
{
	public class PackData
	{
		[JsonProperty("account_hi")]
		public ulong AccountHi { get; set; }

		[JsonProperty("account_lo")]
		public ulong AccountLo { get; set; }

		[JsonProperty("booster_type")]
		public int BoosterType { get; set; }

		/// <summary>
		/// In ISO 8601 format
		/// </summary>
		[JsonProperty("date")]
		public string Date { get; set; }

		/// <summary>
		/// Must contain exactly 5 cards
		/// </summary>
		[JsonProperty("cards")]
		public CardData[] Cards { get; set; }
	}
}
