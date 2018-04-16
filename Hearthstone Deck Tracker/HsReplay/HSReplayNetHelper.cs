using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.Toasts;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal static class HSReplayNetHelper
	{
		private static readonly RateLimiter CollectionSyncLimiter;

		static HSReplayNetHelper()
		{
			CollectionSyncLimiter = new RateLimiter(6, TimeSpan.FromMinutes(2));
			ConfigWrapper.CollectionSyncingChanged += () => SyncCollection().Forget();
			CollectionHelper.OnCollectionChanged += () => SyncCollection().Forget();
			CollectionUploaded += () =>
			{
				ToastManager.ShowCollectionUpdatedToast();
				Influx.OnCollectionSynced(true);
			};
			CollectionUploadError += () => Influx.OnCollectionSynced(false);
			BlizzardAccountClaimed += Influx.OnBlizzardAccountClaimed;
			AuthenticationError += Influx.OnOAuthLoginComplete;
			Authenticating += authenticating =>
			{
				if(authenticating)
					Influx.OnOAuthLoginInitiated();
			};
			HSReplayNetOAuth.LoggedOut += Influx.OnOAuthLogout;
			HSReplayNetOAuth.Authenticated += () => Influx.OnOAuthLoginComplete(AuthenticationErrorType.None);
		}

		public static event Action CollectionUploaded;
		public static event Action CollectionUploadError;
		public static event Action CollectionUploadThrottled;
		public static event Action CollectionAlreadyUpToDate;
		public static event Action<bool> Authenticating;
		public static event Action<bool> BlizzardAccountClaimed;
		public static event Action<AuthenticationErrorType> AuthenticationError;

		public static async Task TryAuthenticate(string successUrl = null, string errorUrl = null)
		{
			Authenticating?.Invoke(true);
			if(await HSReplayNetOAuth.Authenticate(successUrl, errorUrl))
			{
				if(!await HSReplayNetOAuth.UpdateAccountData())
				{
					ErrorManager.AddError("HSReplay.net Error",
						"Could not load HSReplay.net account status."
						+ " Please try again later.");
					AuthenticationError?.Invoke(AuthenticationErrorType.AccountData);
				}
				await SyncCollection();
			}
			else
			{
				ErrorManager.AddError("Could not authenticate with HSReplay.net",
					"Please try running HDT as administrator "
					+ "(right-click the exe and select 'Run as administrator').\n"
					+ "If that does not help please try again later.", true);
				AuthenticationError?.Invoke(AuthenticationErrorType.Authentication);
			}
			Authenticating?.Invoke(false);
		}

		public static async Task UpdateAccount()
		{
			if(HSReplayNetOAuth.IsAuthenticatedForAnything())
			{
				await HSReplayNetOAuth.UpdateAccountData();
				if(string.IsNullOrEmpty(Account.Instance.UploadToken)
					|| !Account.Instance.TokenClaimed.HasValue
					|| (!HSReplayNetOAuth.AccountData?.UploadTokens.Contains(Account.Instance.UploadToken) ?? false))
					await ApiWrapper.UpdateUploadTokenStatus();
				if(Account.Instance.TokenClaimed == false && !string.IsNullOrEmpty(Account.Instance.UploadToken))
					await HSReplayNetOAuth.ClaimUploadToken(Account.Instance.UploadToken);
			}
			else
				ApiWrapper.UpdateUploadTokenStatus().Forget();
		}

		public static async Task SyncCollection()
		{
			if(!Config.Instance.SyncCollection || !HSReplayNetOAuth.IsFullyAuthenticated)
				return;
			var collection = await CollectionHelper.GetCollection();
			if(collection == null)
				return;
			var hash = collection.GetHashCode();
			var hi = collection.AccountHi;
			var lo = collection.AccountLo;
			var account = hi + "-" + lo;
			if(Account.Instance.CollectionState.TryGetValue(account, out var state) && state.Hash == hash)
			{
				Log.Debug("Collection ready up-to-date");
				state.Date = DateTime.Now;
				Account.Save();
				CollectionAlreadyUpToDate?.Invoke();
				return;
			}
			await CollectionSyncLimiter.Run(async () =>
			{
				if(!HSReplayNetOAuth.AccountData?.BlizzardAccounts?.Any(x => x.AccountHi == hi && x.AccountLo == lo) ?? true)
				{
					var response = await HSReplayNetOAuth.ClaimBlizzardAccount(hi, lo, collection.BattleTag);
					var success = response == HSReplayNetOAuth.ClaimBlizzardAccountResponse.Success;
					BlizzardAccountClaimed?.Invoke(success);
					if(success)
						HSReplayNetOAuth.UpdateAccountData().Forget();
					else if(response == HSReplayNetOAuth.ClaimBlizzardAccountResponse.TokenAlreadyClaimed)
					{
						ErrorManager.AddError("HSReplay.net error",
							$"Your blizzard account ({collection.BattleTag}, {account}) is already attached to another"
							+ " HSReplay.net Account. Please contact us at contact@hsreplay.net"
							+ " if this is not correct.");
						return;
					}
					else
					{
						ErrorManager.AddError("HSReplay.net error",
							$"Could not attach your Blizzard account ({collection.BattleTag}, {account}) to"
							+ $" HSReplay.net Account ({HSReplayNetOAuth.AccountData?.Username})."
							+ " Please try again later or contact us at contact@hsreplay.net if this persists.");
						return;
					}
				}
				if(await HSReplayNetOAuth.UpdateCollection(collection))
				{
					Account.Instance.CollectionState[account] = new Account.SyncState(hash);
					Account.Save();
					Log.Debug("Collection synced");
					CollectionUploaded?.Invoke();
				}
				else
				{
					ErrorManager.AddError("HSReplay.net error",
						"Could not update your collection. Please try again later.\n"
						+ "If this problem persists please try logging out and back in"
						+ " under 'options > hsreplay.net > my account'");
					CollectionUploadError?.Invoke();
				}
			}, () =>
			{
				Log.Debug("Waiting for rate limit...");
				CollectionUploadThrottled?.Invoke();
			});
		}

		public static void OpenDecksUrlWithCollection(string campaign)
		{
			var query = new List<string>();
			if(CollectionHelper.TryGetCollection(out var collection) && collection != null)
			{
				var region = Helper.GetRegion(collection.AccountHi);
				query.Add($"hearthstone_account={(int)region}-{collection.AccountLo}");
			}
			Helper.TryOpenUrl(Helper.BuildHsReplayNetUrl("decks", campaign, query, new[] { "maxDustCost=0" }));
		}

		public enum AuthenticationErrorType
		{
			None,
			Authentication,
			AccountData
		}
	}
}
