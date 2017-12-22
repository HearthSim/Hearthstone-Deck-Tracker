using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HSReplay.Responses
{
	public class QueryData
	{
		[JsonProperty("series")]
		public JObject Data;

		[JsonProperty("as_of")]
		public DateTime ServerTimeStamp;
	}
}