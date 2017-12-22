using System.Collections.Generic;
using Newtonsoft.Json;

namespace HSReplay.Responses
{
	public class ListResponse<T>
	{
		[JsonProperty("count")]
		public int Count { get; set; }

		[JsonProperty("previous")]
		public string Previous { get; set; }

		[JsonProperty("next")]
		public string Next { get; set; }

		[JsonProperty("results")]
		public List<T> Results { get; set; }
	}
}
