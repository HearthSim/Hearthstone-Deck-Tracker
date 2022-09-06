using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HearthDb.Deckstrings;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HSReplay.ClientAnalytics;
using Microsoft.Win32;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal sealed class HSReplayNetClientAnalytics
	{
		private static readonly Lazy<ClientAnalyticsClient> Client;
		private static readonly int[] Ports = { 17881, 17882, 17883, 17884, 17885, 17886, 17887, 17888, 17889 };

		private static event Action? OnboardingComplete;

		static HSReplayNetClientAnalytics()
		{
			Client = new Lazy<ClientAnalyticsClient>(LoadClient);
		}

		public static async void Initialize()
		{
			HSReplayNetHelper.Authenticating += OnAuthenticating;
			HSReplayNetHelper.CollectionUploaded += CollectionUploaded;
			OnboardingComplete += () => OnAppStart();

			if(await EnsureOnboarded())
				OnAppStart();
		}

		private static void TrackEvent(string eventName, object properties)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			if(TryGetToken(out var token))
				Client.Value.TrackEvent(token, eventName, properties).Forget();
		}

		private static void OnAppStart()
		{
			var isFirstStart = ConfigManager.PreviousVersion == null;
			if(isFirstStart)
			{
				TrackEvent("first_app_start", new
				{
					app_version = Helper.GetCurrentVersion().ToVersionString(true),
				});
			}
		}

		private static void OnAuthenticating(bool authenticating)
		{
			// we only want to identify our token if authentication just completed
			// we do not use the Authenticated event because it seems to hang up HSReplayNetHelper.TryAuthenticate
			// so instead just check for the authentication ending and an account being present
			if(authenticating || !HSReplayNetOAuth.IsFullyAuthenticated)
				return;

			try
			{
				if(TryGetToken(out var token))
					IdentifyToken(token).Forget();
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}
		private static ClientAnalyticsClient LoadClient()
		{
			return new ClientAnalyticsClient(Helper.GetUserAgent());
		}

		private static void CollectionUploaded(Collection collection, bool firstUpload)
		{
			if(firstUpload)
			{
				TrackEvent("first_collection_upload", new { collection_size = collection.Size() });
			}
		}

		public static void TryTrackToastClick(string toastId)
		{
			TrackEvent("toast_click", new {
				toast = toastId
			});
		}

		public static void TryTrackEndFirstDailyBattlegroundsMatch(int finalPlacement)
		{
			TrackEvent("end_first_daily_bgs_match", new
			{
				is_overlay_enabled = !Config.Instance.HideOverlay,
				is_minion_overlay_enabled = Config.Instance.ShowBattlegroundsTiers,
				is_bobs_buddy_enabled = Config.Instance.RunBobsBuddy,
				is_session_recap_enabled = Config.Instance.ShowSessionRecap,
				is_hero_notification_enabled = Config.Instance.ShowBattlegroundsToast,
				final_placement = finalPlacement,
			});
		}

		public static async Task<bool> EnsureOnboarded()
		{
			if(TryGetToken(out var _))
			{
				// if we already have an analytics token, there is nothing to do
				return true;
			}

			if(HSReplayNetOAuth.IsFullyAuthenticated)
			{
				// if we're logged in, we can generate & persist token, and attempt to link it to the user
				if(await TryGetAndSaveIdentifiedToken())
				{
					return true;
				}

				// if the identification failed, we can still proceed as if the user were logged out
			}

			if(Config.Instance.OnboardingSeen)
			{
				// if the has already seen the onboarding screen and gotten this far, give up - we'll generate our own token
				var token = ClientAnalyticsClient.GenerateOfflineToken();
				TrySaveToken(token);
				return true;
			}

			// otherwise, show the onboarding flyout
			Core.MainWindow.SetNewUserOnboarding(true);
			return false;
		}

		public static async Task RunOnboarding(string finalUrl)
		{
			if(!await DoOnboarding(finalUrl))
			{
				// onboarding has failed - try one last time to invent a token and write it to registry
				var token = ClientAnalyticsClient.GenerateOfflineToken();
				TrySaveToken(token);
			}

			OnboardingComplete?.Invoke();

			// at this point we've tried everything we can - if we don't have a token, we just give up the session
			// in future we could invent a single-session token here and keep it in memory
		}

		private static async Task<bool> DoOnboarding(string finalUrl)
		{
			string url;
			try
			{
				url = Client.Value.GetGatewayUrl(Ports);
			}
			catch(Exception e)
			{
				Log.Error(e);
				return false;
			}

			if(string.IsNullOrEmpty(url))
			{
				// at this point we were unable to open our HTTP listener
				// for the benefit of the user, we can still try to open the final url - if it fails, it doesn't matter
				Helper.TryOpenUrl(finalUrl);
				return false;
			}

			var callbackTask = Client.Value.ReceiveOnboardingCallback(finalUrl);
			if(!Helper.TryOpenUrl(url))
			{
				// at this point the listener is open in callbackTask, but we were unable to open the URL
				// give up so we don't pester the user with an error message
				return false;
			}

			var token = await callbackTask;

			if(token == null)
			{
				// we probably got a missing or invalid token as our URL param
				return false;
			}

			return TrySaveToken(token);
		}

		private static async Task<bool> IdentifyToken(string token)
		{
			var newToken = await HSReplayNetOAuth.IdentifyClientAnalyticsToken(token);
			if(newToken != null)
			{
				return TrySaveToken(newToken);
			}

			return false;
		}

		private static async Task<bool> TryGetAndSaveIdentifiedToken()
		{
			var newToken = await HSReplayNetOAuth.IdentifyClientAnalyticsToken();
			if(newToken != null)
			{
				return TrySaveToken(newToken);
			}

			return false;
		}

		internal static bool TryGetToken(out string token)
		{
			token = "";

			try
			{
				var hearthsim = GetOrCreateSubkey(Registry.CurrentUser, @"SOFTWARE\HearthSim", true);
				var common = GetOrCreateSubkey(hearthsim, @"Common", true);

				var possibleToken = common.GetValue("UserAnalyticsToken");

				hearthsim.Close();
				common.Close();

				if(possibleToken is string { Length: > 5 } stringToken)
				{
					token = stringToken;
					return true;
				}

				return false;
			}
			catch
			{
				return false;
			}
		}
		public static bool TrySaveToken(string token)
		{
			try
			{
				var hearthsim = GetOrCreateSubkey(Registry.CurrentUser, @"SOFTWARE\HearthSim", true);
				var common = GetOrCreateSubkey(hearthsim, @"Common", true);

				common.SetValue("UserAnalyticsToken", token, RegistryValueKind.String);

				hearthsim.Close();
				common.Close();
				return true;
			}
			catch
			{
				return false;
			}
		}
		
		private static RegistryKey GetOrCreateSubkey(RegistryKey key, string subkeyName, bool writable = false)
		{
			var subKey = key.OpenSubKey(subkeyName, writable);
			if (subKey == null) {
				subKey = key.CreateSubKey(subkeyName, writable);
			}

			return subKey;
		}
	}
}
