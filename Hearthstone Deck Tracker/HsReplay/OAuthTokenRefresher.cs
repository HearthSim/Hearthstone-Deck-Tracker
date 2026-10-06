using System;
using System.Threading;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	// the server replaces the access token in place on refresh, so refreshes must never overlap
	internal sealed class OAuthTokenRefresher
	{
		private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
		private readonly Func<string?> _getAccessToken;
		private readonly Func<bool> _isFresh;
		private readonly Func<Task<bool>> _refresh;

		public OAuthTokenRefresher(Func<string?> getAccessToken, Func<bool> isFresh, Func<Task<bool>> refresh)
		{
			_getAccessToken = getAccessToken;
			_isFresh = isFresh;
			_refresh = refresh;
		}

		public string? AccessToken => _getAccessToken();

		public async Task<bool> EnsureFresh()
		{
			if(_isFresh())
				return true;
			await _lock.WaitAsync();
			try
			{
				// another caller may have refreshed while we were waiting
				return _isFresh() || await _refresh();
			}
			finally
			{
				_lock.Release();
			}
		}

		public async Task<bool> RefreshRejected(string? rejectedAccessToken)
		{
			await _lock.WaitAsync();
			try
			{
				if(_getAccessToken() != rejectedAccessToken)
				{
					Log.Info("Rejected access token was already replaced");
					return true;
				}
				return await _refresh();
			}
			finally
			{
				_lock.Release();
			}
		}
	}
}
