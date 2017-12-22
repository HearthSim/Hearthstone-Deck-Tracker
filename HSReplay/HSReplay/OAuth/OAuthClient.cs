using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HSReplay.OAuth.Data;
using HSReplay.Web;
using Newtonsoft.Json;

namespace HSReplay.OAuth
{
	public partial class OAuthClient
	{
		private const string AuthUrl = "https://hsreplay.net/oauth2/authorize/";
		private const string TokenUrl = "https://hsreplay.net/oauth2/token/";
		private readonly string _clientId;
		private TokenData _tokenData;
		private CallbackListener _listener;
		private string _state;
		private readonly WebClient _webClient;

		public OAuthClient(string clientId, string userAgent, TokenData tokenData = null)
		{
			_clientId = clientId;
			_tokenData = tokenData;
			_webClient = new WebClient(userAgent);
		}

		private string BuildAuthUrl(Scope[] scopes, string state, string redirectUrl)
		{
			var query = new Dictionary<string, string>
			{
				["response_type"] = "code",
				["client_id"] = _clientId,
				["redirect_uri"] = redirectUrl,
				["scope"] = string.Join(" ", scopes.Select(x => x.Name)),
				["state"] = state
			};
			return AuthUrl + "?" + ToQueryString(query);
		}

		private string BuildTokenQuery(string code, string redirectUrl)
		{
			var query = new Dictionary<string, string>
			{
				["grant_type"] = "authorization_code",
				["code"] = code,
				["client_id"] = _clientId,
				["redirect_uri"] = redirectUrl
			};
			return ToQueryString(query);
		}

		private string BuildRefreshQuery()
		{
			var query = new Dictionary<string, string>
			{
				["grant_type"] = "refresh_token",
				["refresh_token"] = _tokenData.RefreshToken,
				["client_id"] = _clientId,
			};
			return ToQueryString(query);
		}

		private string ToQueryString(Dictionary<string, string> query)
		{
			return string.Join("&", query.Select(x => $"{x.Key}={x.Value}"));
		}

		public async Task<TokenData> GetToken(string authCode, string redirectUrl)
		{
			var payload = BuildTokenQuery(authCode, redirectUrl);
			using(var response = await _webClient.PostAsync(TokenUrl, payload, false, ContentType.UrlEncoded))
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
			{
				_tokenData = JsonConvert.DeserializeObject<TokenData>(reader.ReadToEnd());
				return _tokenData;
			}
		}

		public async Task<TokenData> RefreshToken()
		{
			if(_tokenData == null)
				throw new Exception("No token data available.");
			var payload = BuildRefreshQuery();
			using(var response = await _webClient.PostAsync(TokenUrl, payload, false, ContentType.UrlEncoded))
			using(var responseStream = response.GetResponseStream())
			using(var reader = new StreamReader(responseStream))
			{
				_tokenData = JsonConvert.DeserializeObject<TokenData>(reader.ReadToEnd());
				return _tokenData;
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="scopes"></param>
		/// <param name="ports"></param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="HttpListenerException"></exception>
		/// <returns></returns>
		public string GetAuthenticationUrl(Scope[] scopes, int[] ports)
		{
			_listener = CallbackListener.Create(ports);
			if(_listener == null)
				return null;
			_state = Guid.NewGuid().ToString();
			return BuildAuthUrl(scopes, _state, _listener.RedirectUrl);
		}

		private Header AuthHeader => new Header("Authorization", $"{_tokenData.TokenType} {_tokenData.AccessToken}");

		public async Task<AuthData> ReceiveAuthenticationCallback(string successUrl, string errorUrl)
		{
			if(_listener == null)
				return null;
			using(_listener)
			{
				try
				{
					var code = await _listener.Listen(_state, successUrl, errorUrl);
					return new AuthData(code, _listener.RedirectUrl);
				}
				catch(Exception)
				{
					return null;
				}
			}
		}
	}
}

