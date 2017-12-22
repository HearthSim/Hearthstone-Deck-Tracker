using Newtonsoft.Json;

namespace HSReplay.Responses
{
	/// <summary>
	/// Response from GET to https://hsreplay.net/api/v1/tokens/AUTH_TOKEN
	/// </summary>
	public class AccountStauts
	{
		[JsonProperty("key")]
		public string Key { get; set; }

		[JsonProperty("user")]
		public User User { get; set; }

		[JsonProperty("test_data")]
		public bool TestData { get; set; }
	}

	public class User
	{
		internal User()
		{
		}

		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("username")]
		public string Username { get; set; }

		[JsonProperty("battletag")]
		public string BattleTag { get; set; }

		[JsonProperty("is_premium")]
		public string IsPremium { get; set; }
	}
}
