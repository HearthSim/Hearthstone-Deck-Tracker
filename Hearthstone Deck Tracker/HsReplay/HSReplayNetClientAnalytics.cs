﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay.Onboarding;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.Updating;
using Hearthstone_Deck_Tracker.Utility.ValueMoments;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using HSReplay.ClientAnalytics;
using Mercenaries_Deck_Tracker.Utility.ValueMoments.Actions;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal sealed class HSReplayNetClientAnalytics
	{
		private static readonly Lazy<ClientAnalyticsClient> Client;
		private static readonly int[] Ports = { 17881, 17882, 17883, 17884, 17885, 17886, 17887, 17888, 17889 };

		private static event Action? OnboardingComplete;

		public static NewUserOnboardingViewModel OnboardingViewModel { get; } = new();

		static HSReplayNetClientAnalytics()
		{
			Client = new Lazy<ClientAnalyticsClient>(LoadClient);
		}

		public static async void Initialize()
		{
			HSReplayNetHelper.Authenticating += OnAuthenticating;
			HSReplayNetHelper.CollectionUploaded += CollectionUploaded;
			OnboardingViewModel.Continue += () => RunOnboarding("https://hsreplay.net/hdt/installed/").Forget();
			OnboardingComplete += () => OnAppStart();

			if(await EnsureOnboarded())
				OnAppStart();
		}

		private static readonly Queue<VMAction> _actionBuffer = new();
		private static void TrackAction(VMAction action)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;

			try
			{
#if !DEBUG
				if(!ValueMomentManager.ShouldSendEventToMixPanel(action, action.ValueMoments))
					return;
				if(TryGetToken(out var token))
				{
					while(_actionBuffer.Count > 0)
					{
						var buffered = _actionBuffer.Dequeue();
						Client.Value.TrackEvent(token, buffered.Name, buffered).Forget();
					}
					Client.Value.TrackEvent(token, action.Name, action).Forget();
				}
				else
				{
					// We might not have a token yet if this event was fired before onboarding on first install.
					_actionBuffer.Enqueue(action);
				}
#else
				Log.Debug($"{action.Name}: {JsonConvert.SerializeObject(action)}");
#endif
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		private static void OnAppStart()
		{
			var isFirstStart = ConfigManager.PreviousVersion == null;
			if(isFirstStart)
				TrackAction(new InstallAction());
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
				TrackAction(new FirstHSCollectionUploadAction(collection.Size()));
		}

		public static void OnConstructedMatchEnds(GameStats gameStats, GameMode gameMode, GameType gameType, bool spectator, GameMetrics gameMetrics)
		{
			var heroCard = Database.GetCardFromId(gameStats.PlayerHeroCardId);
			if(heroCard == null)
				return;

			var heroDbfId = heroCard.DbfId;
			var heroName = heroCard.Name ?? "";
			var matchResult = gameStats.Result;
			var starLevel = gameMode == GameMode.Ranked ? gameStats.StarLevel : 0;

			if(spectator)
				TrackAction(new EndSpectateMatchHearthstoneAction(heroDbfId, heroName, matchResult, gameMode, gameType, starLevel, gameMetrics));
			else
				TrackAction(new EndMatchHearthstoneAction(heroDbfId, heroName, matchResult, gameMode, gameType, starLevel, gameMetrics));
		}

		public static void OnBattlegroundsMatchEnds(string? heroCardId, int finalPlacement, int finalTurn, GameStats gameStats, GameMetrics gameMetrics, GameType gameType, bool spectator)
		{
			var bgHeroCard = Database.GetCardFromId(heroCardId);
			if(bgHeroCard == null)
				return;

			var heroDbfId = bgHeroCard.DbfId;
			var heroName = bgHeroCard.Name ?? "";
			var bgsRating = gameStats.BattlegroundsRatingAfter;

			if(spectator)
				TrackAction(new EndSpectateMatchBattlegroundsAction(heroDbfId, heroName, finalPlacement, finalTurn, gameType, bgsRating, gameMetrics));
			else
				TrackAction(new EndMatchBattlegroundsAction(heroDbfId, heroName, finalPlacement, finalTurn, gameType, bgsRating, gameMetrics));
		}

		public static void OnMercenariesMatchEnds(GameStats gameStats, GameMetrics gameMetrics, GameType gameType, bool spectator)
		{
			var matchResult = gameStats.Result;

			if(spectator)
				TrackAction(new EndSpectateMatchMercenariesAction(matchResult, gameType, gameMetrics));
			else
				TrackAction(new EndMatchMercenariesAction(matchResult, gameType, gameMetrics));
		}

		public static void OnSetInitialConfigLanguage(string source, string language, string systemLanguage, string gameLanguage, string systemRegionName)
		{
			TrackAction(new SetInitialLanguageAction(source, language, systemLanguage, gameLanguage, systemRegionName));
		}

		public static void OnCopyDeck(CopyDeckAction.Action action)
		{
			TrackAction(new CopyDeckAction(Franchise.HSConstructed, action));
		}

		public static void OnScreenshotDeck(ClickAction.Action action)
		{
			TrackAction(new ClickAction(Franchise.HSConstructed, action));
		}

		public static void OnShowPersonalStats(ClickAction.Action action, SubFranchise? subFranchise)
		{
			var subFranchiseArray = subFranchise != null ? new[] { (SubFranchise)subFranchise } : new SubFranchise[] { };

			TrackAction(new ClickAction(Franchise.HSConstructed, action, subFranchiseArray));
		}

		public static void OnArenaRunStarts(ArenaDraftInfo draftInfo, bool arenaOverlayVisible, bool trialsActivated, int trialsRemaining)
		{
			TrackAction(new ArenaDraftStartAction(draftInfo, arenaOverlayVisible, trialsActivated, trialsRemaining));
		}

		public static void TryTrackToastClick(Franchise franchise, ToastAction.Toast toastNameEnum)
		{
			TrackAction(new ToastAction(franchise, toastNameEnum));
		}

		public static void OnSquirrelRemoteChanged(SquirrelRemote oldValue, SquirrelRemote newValue)
		{
			TrackAction(new SquirrelRemoteAction(oldValue, newValue));
		}

		public static void OnClickSubscribeNowLink(Franchise franchise, ClickSubscribeNowAction.Button button, int? trialsRemaining)
		{
			OnClickSubscribeNowLink(franchise, null, button, trialsRemaining);
		}

		public static void OnClickSubscribeNowLink(Franchise franchise, SubFranchise[]? subFranchise, ClickSubscribeNowAction.Button button, int? trialsRemaining)
		{
			TrackAction(new ClickSubscribeNowAction(franchise, subFranchise, button, trialsRemaining));
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

			// otherwise, show the onboarding screen
			OnboardingViewModel.IsVisible = true;
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
