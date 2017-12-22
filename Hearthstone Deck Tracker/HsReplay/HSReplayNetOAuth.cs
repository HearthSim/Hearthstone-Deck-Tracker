﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.HsReplay.Data;
using Hearthstone_Deck_Tracker.Live.Data;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HSReplay.OAuth;
using HSReplay.OAuth.Data;
using HSReplay.Responses;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	// ReSharper disable InconsistentNaming
	internal sealed class HSReplayNetOAuth
	{
		private static readonly JsonSerializer<OAuthData> Serializer;
		private static readonly Lazy<OAuthClient> Client;
		private static readonly Lazy<OAuthData> Data;

		private static readonly int[] Ports = { 17781, 17782, 17783, 17784, 17785, 17786, 17787, 17788, 17789 };
		private const string HSReplayNetClientId = "jIpNwuUWLFI6S3oeQkO3xlW6UCnfogw1IpAbFXqq";
		private const string TwitchExtensionId = "apwln3g3ia45kk690tzabfp525h9e1";
		private const string SuccessUrl = "https://hsdecktracker.net/hsreplaynet/oauth_success/";
		private const string ErrorUrl = "https://hsdecktracker.net/hsreplaynet/oauth_error/";


		static HSReplayNetOAuth()
		{
			Serializer = new JsonSerializer<OAuthData>("hsreplay_oauth", true);
			Data = new Lazy<OAuthData>(Serializer.Load);
			Client = new Lazy<OAuthClient>(LoadClient);
		}

		private static OAuthClient LoadClient()
		{
			return new OAuthClient(HSReplayNetClientId, Helper.GetUserAgent(), Data.Value.TokenData);
		}

		public static void Save() => Serializer.Save(Data.Value);

		public static async Task<bool> Authenticate()
		{
			Log.Info("Authenticating with HSReplay.net...");
			string url;
			try
			{
				url = Client.Value.GetAuthenticationUrl(new[] { Scope.ReadSocialAccounts }, Ports);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
			if(string.IsNullOrEmpty(url))
			{
				Log.Error("Authentication failed, could not create callback listener");
				return false;
			}
			var callbackTask = Client.Value.ReceiveAuthenticationCallback(SuccessUrl, ErrorUrl);
			if(!Helper.TryOpenUrl(url))
				ErrorManager.AddError("Could not open browser to complete authentication.", $"Please go to '{url}' to continue authentication.", true);
			Log.Info("Waiting for callback...");
			var data = await callbackTask;
			if(data == null)
			{
				Log.Error("Authentication failed, received no data");
				return false;
			}
			Data.Value.Code = data.Code;
			Data.Value.RedirectUrl = data.RedirectUrl;
			Log.Info("Authentication complete");
			await UpdateToken();
			Save();
			return true;
		}

		public static async Task<bool> UpdateToken()
		{
			var data = Data.Value;
			if(data.TokenData != null && (DateTime.Now - data.TokenDataCreatedAt).TotalSeconds < data.TokenData.ExpiresIn)
				return true;
			if(string.IsNullOrEmpty(data.Code) || string.IsNullOrEmpty(data.RedirectUrl))
			{
				Log.Error("Could not update token, we don't have a code or redirect url.");
				return false;
			}
			if(!string.IsNullOrEmpty(data.TokenData?.RefreshToken))
			{
				Log.Info("Refreshing token data...");
				try
				{
					var tokenData = await Client.Value.RefreshToken();
					if(tokenData != null)
					{
						SaveTokenData(tokenData);
						return true;
					}
				}
				catch(Exception e)
				{
					Log.Error(e);
				}
			}
			try
			{
				Log.Info("Fetching new token...");
				var tokenData = await Client.Value.GetToken(data.Code, data.RedirectUrl);
				if(tokenData == null)
				{
					Log.Error("We did not receive any token data.");
					return false;
				}
				SaveTokenData(tokenData);
				return true;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
		}

		public static async Task<bool> UpdateTwitchUsers()
		{
			Log.Info("Fetching twitch accounts...");
			try
			{
				if(!await UpdateToken())
				{
					Log.Error("Could not update token data");
					return false;
				}
				var twitchAccounts = await Client.Value.GetTwitchAccounts();
				Data.Value.TwitchUsers = twitchAccounts;
				Save();
				Log.Info($"Saved {twitchAccounts.Count} account(s): {string.Join(", ", twitchAccounts.Select(x => x.Username))}");
				return twitchAccounts.Count != 0;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
		}

		public static async Task<bool> UpdateAccountData()
		{
			Log.Info("Updating account data...");
			try
			{
				if(!await UpdateToken())
				{
					Log.Error("Could not update token data");
					return false;
				}
				var account = await Client.Value.GetHSReplayNetAccount();
				Data.Value.Account = account;
				Save();
				Log.Info($"Found account: {account?.Username ?? "None"}");
				return account != null;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}
		}

		public static bool IsAuthenticated => !string.IsNullOrEmpty(Data.Value.Code);

		public static List<TwitchAccount> TwitchUsers => Data.Value.TwitchUsers;

		public static User AccountData => Data.Value.Account;

		public static void SaveTokenData(TokenData data)
		{
			Data.Value.TokenData = data;
			Data.Value.TokenDataCreatedAt = DateTime.Now;
			Save();
			Log.Info("Saved token data");
		}

		public static async Task SendTwitchPayload(Payload payload)
		{
			try
			{
				if(!await UpdateToken())
				{
					Log.Error("Could not update token data");
					return;
				}
				var response = await Client.Value.SendTwitchUpdate(Config.Instance.SelectedTwitchUser, TwitchExtensionId, payload);
				Log.Debug(response);
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
