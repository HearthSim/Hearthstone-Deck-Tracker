using System;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal static class HSReplayNetHelper
	{
		public static event Action<bool> Authenticating;

		public static async Task TryAuthenticate()
		{
			Authenticating?.Invoke(true);
			if(await HSReplayNetOAuth.Authenticate())
			{
				if(await HSReplayNetOAuth.UpdateAccountData())
					await SyncCollection();
				else
					ErrorManager.AddError("Could not load HSReplay.net account status", "Please try again later.");
			}
			else
				ErrorManager.AddError("Could not authenticate with HSReplay.net", "Please try again later.");
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
	}
}
