using System;
using Newtonsoft.Json;

namespace HSReplay.Responses
{
	public class Archetype
	{
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("player_class")]
		public int PlayerClass { get; set; }

		[JsonProperty("player_class_name")]
		public string PlayerClassName { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("standard_ccp_signature_core")]
		public Signature StandardCoreSignature { get; set; }

		public class Signature
		{
			[JsonProperty("as_of")]
			public DateTime AsOf { get; set; }

			[JsonProperty("format")]
			public int Format { get; set; }

			[JsonProperty("components")]
			public int[] Components { get; set; }
		}
	}
}
