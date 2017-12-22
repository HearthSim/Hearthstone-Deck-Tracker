using Newtonsoft.Json;

namespace HSReplay
{
	public class CardData
	{
		[JsonProperty("card")]
		public string CardId { get; set; }

		[JsonProperty("premium", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool Premium { get; set; }
	}
}