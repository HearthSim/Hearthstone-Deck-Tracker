using System;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
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
			CollectionSyncLimiter = new RateLimiter(3, TimeSpan.FromMinutes(2));
			ConfigWrapper.CollectionSyncingChanged += () => SyncCollection().Forget();
			CollectionHelper.OnCollectionChanged += () => SyncCollection().Forget();
			if(!Account.Instance.CollectionState.Any())
			{
				void ShowToast()
				{
					ToastManager.ShowCollectionUpdatedToast();
					CollectionUploaded -= ShowToast;
				}
				CollectionUploaded += ShowToast;
			}
		}

		public static event Action CollectionUploaded;
		public static event Action CollectionUploadThrottled;
		public static event Action CollectionAlreadyUpToDate;
		public static event Action<bool> Authenticating;

		public static async Task TryAuthenticate()
		{
			Authenticating?.Invoke(true);
			if(await HSReplayNetOAuth.Authenticate())
			{
				if(!await HSReplayNetOAuth.UpdateAccountData())
					ErrorManager.AddError("HSReplay.net Error",
						"Could not load HSReplay.net account status."
						+ " Please try again later.");
				await SyncCollection();
			}
			else
				ErrorManager.AddError("Could not authenticate with HSReplay.net",
					"Please try running HDT as administrator "
					+ "(right-click the exe and select 'Run as administrator').\n"
					+ "If that does not help please try again later.", true);
			Authenticating?.Invoke(false);
		}

		public static async Task UpdateAccount()
		{
			await HSReplayNetOAuth.UpdateAccountData();
			if(!Account.Instance.TokenClaimed.HasValue)
				await ApiWrapper.UpdateUploadTokenStatus();
			if(Account.Instance.TokenClaimed == false)
				await HSReplayNetOAuth.ClaimUploadToken(Account.Instance.UploadToken);
		}

		public static async Task SyncCollection()
		{
			if(!Config.Instance.SyncCollection || !HSReplayNetOAuth.IsFullyAuthenticated)
				return;
			var collection = await CollectionHelper.GetCollection();
			if(collection == null)
				return;
			var hash = collection.GetHashCode();
			var account = collection.AccountHi + "-" + collection.AccountLo;
			if(Account.Instance.CollectionState.TryGetValue(account, out var state) && state.Hash == hash)
			{
				Log.Debug("Collection ready up-to-date");
				CollectionAlreadyUpToDate?.Invoke();
				return;
			}
			await CollectionSyncLimiter.Run(async () =>
			{
				if(await HSReplayNetOAuth.UpdateCollection(collection))
				{
					Account.Instance.CollectionState[account] = new Account.SyncState(hash);
					Account.Save();
					Log.Debug("Collection synced");
					CollectionUploaded?.Invoke();
				}
				else
					ErrorManager.AddError("HSReplay.net error", "Could not update your collection. Please try again later.");
			}, () =>
			{
				Log.Debug("Waiting for rate limit...");
				CollectionUploadThrottled?.Invoke();
			});
		}
	}
}
