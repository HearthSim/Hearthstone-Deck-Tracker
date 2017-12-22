using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using HSReplay.Responses;
using HSReplay.Web;
using Newtonsoft.Json;

namespace HSReplay
{
	/// <summary>
	///     API client for hsreplay.net
	/// </summary>
	public class HsReplayClient
	{
		private readonly string _apiKey;
		private readonly bool _testData;
		private readonly WebClient _webClient;
		private readonly ClientConfig _config;

		/// <summary>
		/// </summary>
		/// <param name="apiKey">hsreplay.net API key</param>
		/// <param name="userAgent">userAgent included in webrequests</param>
		/// <param name="testData">Set to true when not uploading actual user data.</param>
		public HsReplayClient(string apiKey, string userAgent = "", bool testData = false, ClientConfig config = null)
		{
			_apiKey = apiKey;
			_webClient = new WebClient(userAgent);
			_testData = testData;
			_config = config ?? new ClientConfig();
		}

		private Header GetAuthHeader(string token) => new Header("Authorization", $"Token {token}");

		private Header ApiHeader => new Header("X-Api-Key", _apiKey);

		/// <summary>
		///     Creates a new upload token
		/// </summary>
		/// <returns>Created upload token</returns>
		public async Task<string> CreateUploadToken()
		{
			var content = _testData ? JsonConvert.SerializeObject(new {test_data = true}) : null;
			using(var response = await _webClient.PostJsonAsync(_config.TokensUrl, content, false, ApiHeader))
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
			{
				var token = JsonConvert.DeserializeObject<UploadToken>(reader.ReadToEnd());
				if(string.IsNullOrEmpty(token.Key))
					throw new Exception("Reponse contained no upload-token.");
				return token.Key;
			}
		}

		/// <summary>
		///     Returns a url, which allows for the token to be claimed.
		///     The user has to open this url in a browser for the claiming to complete.
		/// </summary>
		/// <param name="token">Auth token</param>
		/// <returns>Url for account claiming.</returns>
		public async Task<string> GetClaimAccountUrl(string token)
		{
			using(var response = await _webClient.PostJsonAsync(_config.ClaimAccountUrl, string.Empty, false, ApiHeader, GetAuthHeader(token)))
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
				return JsonConvert.DeserializeObject<AccountClaim>(reader.ReadToEnd()).Url;
		}

		/// <summary>
		///     Returns the status of a given auth token.
		/// </summary>
		/// <param name="token">Auth token</param>
		/// <returns>Status of given auth token.</returns>
		public async Task<AccountStauts> GetAccountStatus(string token)
		{
			using(var response = await _webClient.GetAsync($"{_config.TokensUrl}{token}/", ApiHeader, GetAuthHeader(token)))
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
				return JsonConvert.DeserializeObject<AccountStauts>(reader.ReadToEnd());
		}

		/// <summary>
		///     Creates a new log upload request. This request is then passed to UploadLog().
		///     The request contains a ShortId, which will be the game URL on the website.
		///     Use LogValidation.LogValidator.Validate to ensure the log is valid before making this request.
		/// </summary>
		/// <param name="metaData">Meta data about the match.</param>
		/// <param name="token">Auth token</param>
		/// <returns>Upload request, containing the future game URL</returns>
		public async Task<LogUploadRequest> CreateUploadRequest(UploadMetaData metaData, string token)
		{
			var content = JsonConvert.SerializeObject(metaData);
			using(var response = await _webClient.PostJsonAsync(_config.UploadRequestUrl, content, true, ApiHeader, GetAuthHeader(token)))
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
			{
				var reponse = reader.ReadToEnd();
				return JsonConvert.DeserializeObject<LogUploadRequest>(reponse);
			}
		}

		/// <summary>
		///     Uploads the given log.
		///     The URL to of the replay is included in the LogUploadRequest object.
		/// </summary>
		/// <param name="request">Created by CreateUploadRequest()</param>
		/// <param name="log">Log to be uploaded</param>
		/// <returns></returns>
		public async Task UploadLog(LogUploadRequest request, IEnumerable<string> log) => await UploadLog(request.PutUrl, log);

		/// <summary>
		///     Uploads the given log.
		///     The URL to of the replay is included in the LogUploadRequest object.
		/// </summary>
		/// <param name="putUrl">Found in LogUploadRequest.PutUrl</param>
		/// <param name="log">Log to be uploaded</param>
		/// <returns></returns>
		public async Task UploadLog(string putUrl, IEnumerable<string> log)
			=> (await _webClient.PutAsync(putUrl, string.Join(Environment.NewLine, log), true)).Close();

		/// <summary>
		///    Uploads the given PackData.
		///    PackData.Cards must contain exactly 5 cards.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="token">Auth token</param>
		/// <returns>Response string</returns>
		public async Task<string> UploadPack(PackData data, string token)
		{
			var content = JsonConvert.SerializeObject(data);
			using(var response = await _webClient.PostJsonAsync(_config.UploadPackUrl, content, false, ApiHeader, GetAuthHeader(token)))
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
				return reader.ReadToEnd();
		}

		/// <summary>
		///     Returns QueryData object containing a list deck shortIds with avilable data.
		/// </summary>
		/// <param name="token">Auth token</param>
		/// <returns>Returns QueryData object containing a list deck shortIds with avilable data.</returns>
		public async Task<QueryData> GetAvailableDecks(string token) => await GetQueryData(_config.DeckInventoryUrl, token);

		/// <summary>
		///     Returns QueryData object contains winrates for provided deck id.
		/// </summary>
		/// <param name="deckId">Deck shortId of target deck</param>
		/// <param name="wild">Request wild data for target deck</param>
		/// <param name="token">Auth token</param>
		/// <returns>Returns QueryData object contains winrates for provided deck id.</returns>
		public async Task<QueryData> GetDeckWinrates(string deckId, bool wild, string token)
		{
			var query = new NameValueCollection {["deck_id"] = deckId};
			if(wild)
				query["GameType"] = "RANKED_WILD";
			var url = BuildUrl(_config.DeckWinrateUrl, query);
			return await GetQueryData(url, token);
		}

		public async Task<List<Archetype>> GetArchetypes(string token)
		{
			using(var response = await _webClient.GetAsync($"{_config.ArchetypesUrl}", ApiHeader, GetAuthHeader(token)))
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
				return JsonConvert.DeserializeObject<List<Archetype>>(reader.ReadToEnd());
		}

		public async Task<QueryData> GetArchetypeMatchups(string token)
		{
			var query = new NameValueCollection {["GameType"] = "RANKED_STANDARD"};
			var url = BuildUrl(_config.ArchetypeMatchupsUrl, query);
			return await GetQueryData(url, token);
		}

		private string BuildUrl(string url, NameValueCollection parameters)
		{
			if(parameters == null || !parameters.HasKeys())
				return url;
			var uriBuilder = new UriBuilder(url);
			var query = HttpUtility.ParseQueryString(uriBuilder.Query);
			query.Add(parameters);
			uriBuilder.Query = query.ToString();
			return uriBuilder.Uri.ToString();
		}

		private async Task<QueryData> GetQueryData(string url, string token)
		{
			using(var response = await _webClient.GetAsync(url, ApiHeader, GetAuthHeader(token)))
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
				return JsonConvert.DeserializeObject<QueryData>(reader.ReadToEnd());
		}
	}
}