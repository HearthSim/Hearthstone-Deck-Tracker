using Newtonsoft.Json;

namespace HSReplay.OAuth.Data
{
	public class TwitchUserData
	{
		[JsonProperty("display_name")]
		public string DisplayName { get; set; }
	}
}
