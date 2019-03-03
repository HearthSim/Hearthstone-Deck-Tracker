using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Hearthstone_Deck_Tracker.Live.Data
{
	public class Payload
	{
		[JsonProperty("type")]
		[JsonConverter(typeof(StringEnumConverter))]
		public DataType Type { get; set; }

		[JsonProperty("data")]
		public object Data { get; set; }

		[JsonProperty("version")]
		public int Version => 3;
	}
}
