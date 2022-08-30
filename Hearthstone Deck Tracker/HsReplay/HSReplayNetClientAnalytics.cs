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
			OnboardingComplete += () => OnAppStart(true);

			if(await EnsureOnboarded())
				OnAppStart(false);
		}

		private static void TrackEvent(string eventName)
		{
			TrackEvent(eventName, new {});
		}

		private static void TrackEvent(string eventName, object properties)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			if(TryGetToken(out var token))
				Client.Value.TrackEvent(token, eventName, properties).Forget();
		}

		private static void OnAppStart(bool isOnboarding)
		{
			TrackEvent("app_start", new
			{
				app_version = Helper.GetCurrentVersion().ToVersionString(true),
				is_first_start = ConfigManager.PreviousVersion == null,
				is_autostart = Config.Instance.StartWithWindows,
				is_onboarding = isOnboarding
			});
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

		private static void CollectionUploaded(Collection collection)
		{
			TrackEvent("collection_upload", new { collection_size = collection.Size() });
		}

		public static void TryTrackToastClick(string toastId)
		{
			TrackEvent("toast_click", new {
				toast = toastId
			});
		}

		/**
		 * Keep track of the match starts we've already tracked to prevent double-emits when the Deck changes.
		 */
		private static HashSet<long> MatchStarts = new HashSet<long>();

		public static void TryTrackMatchStart(
			BnetGameType bnetGameType,
			HearthDb.Deckstrings.Deck? deck,
			bool isSpectator,
			DateTime idempotencyTimestamp
		) {
			if(MatchStarts.Contains(idempotencyTimestamp.ToUnixTime()))
				return;


			var deckstring = "";
			try
			{
				deckstring = deck != null ? DeckSerializer.Serialize(deck, false) : null;
			}
			catch
			{
				// likely due to unknown cards, doesn't matter.
			}
			TrackEvent("match_start", new
			{
				hearthstone_bnet_game_type = (int) bnetGameType,
				deck_string = deckstring,
				is_spectator = isSpectator,
			});
			MatchStarts.Add(idempotencyTimestamp.ToUnixTime());
		}

		public static void TryTrackBattlegroundsHeroPick(Card hero, BnetGameType bnetGameType)
		{
			TrackEvent("battlegrounds_hero_pick", new
			{
				battlegrounds_hero_dbf_id = hero.DbfId,
				battlegrounds_hero_name = hero.Name,
				hearthstone_bnet_game_type = (int)bnetGameType,
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

		private static bool TryGetToken(out string token)
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
