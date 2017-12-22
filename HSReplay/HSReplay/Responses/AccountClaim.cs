using Newtonsoft.Json;

namespace HSReplay.Responses
{
	/// <summary>
	/// Response from POST to https://hsreplay.net/api/v1/claim_account/
	/// </summary>
	public class AccountClaim
	{
		[JsonProperty("full_url")]
		public string Url { get; set; }
	}
}