using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using HSReplay.OAuth.Data;
using HSReplay.Responses;
using HSReplay.Web;
using Newtonsoft.Json;

namespace HSReplay.OAuth
{
	public partial class OAuthClient
	{
		private const string GamesUrl = "https://hsreplay.net/api/v1/games/";
		private const string TwitchAccountsUrl = "https://hsreplay.net/api/v1/account/social/twitch/";
		// ReSharper disable once InconsistentNaming
		private const string HSReplayNetAccountUrl = "https://hsreplay.net/api/v1/account/";
		private const string TwitchDataUrl = "https://twitch-ebs.hearthsim.net/send/";


		public async Task<string> GetGames(string username)
		{
			username = HttpUtility.UrlEncode(username);
			using(var response = await _webClient.GetAsync($"{GamesUrl}?username={username}", AuthHeader))
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
				return reader.ReadToEnd();
		}

		public async Task<List<TwitchAccount>> GetTwitchAccounts()
		{
			using(var response = await _webClient.GetAsync($"{TwitchAccountsUrl}", AuthHeader))
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
			{
				var data = JsonConvert.DeserializeObject<ListResponse<SocialAccount<TwitchUserData>>>(reader.ReadToEnd());
				if(data.Count == 0)
					return new List<TwitchAccount>();
				return data.Results.Select(x => new TwitchAccount {Id = x.Uid, Username = x.ExtraData.DisplayName}).ToList();
			}
		}

		// ReSharper disable once InconsistentNaming
		public async Task<User> GetHSReplayNetAccount()
		{
			using(var response = await _webClient.GetAsync($"{HSReplayNetAccountUrl}", AuthHeader))
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
				return JsonConvert.DeserializeObject<User>(reader.ReadToEnd());
		}

		public async Task<string> SendTwitchUpdate(int userId, string extClientId, object payload)
		{
			var data = JsonConvert.SerializeObject(payload);
			var user = new Header("X-Twitch-User-Id", userId.ToString());
			var ext = new Header("X-Twitch-Client-Id", extClientId);
			using(var response = await _webClient.PostJsonAsync(TwitchDataUrl, data, true, AuthHeader, user, ext))
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
				return reader.ReadToEnd();
		}
	}
}
