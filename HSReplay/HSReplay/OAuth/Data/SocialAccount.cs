using HSReplay.Responses;
using Newtonsoft.Json;

namespace HSReplay.OAuth.Data
{
	public class SocialAccount<T>
	{
		[JsonProperty("user")]
		public User User { get; set; }

		[JsonProperty("uid")]
		public int Uid { get; set; }

		[JsonProperty("provider")]
		public string Provider { get; set; }

		[JsonProperty("extra_data")]
		public T ExtraData { get; set; }
	}
}
