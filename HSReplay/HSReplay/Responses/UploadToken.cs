using Newtonsoft.Json;

namespace HSReplay.Responses
{
	/// <summary>
	/// Response from POST to https://hsreplay.net/api/v1/tokens/
	/// </summary>
	public class UploadToken
	{
		[JsonProperty("key")]
		public string Key { get; set; }

		[JsonProperty("user")]
		public string User { get; set; }

		[JsonProperty("test_data")]
		public bool TestData { get; set; }
	}
}