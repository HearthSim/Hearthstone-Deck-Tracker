using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.HsReplay;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.HsReplay
{
	[TestClass]
	public class OAuthTokenRefresherTests
	{
		private string _accessToken;
		private bool _fresh;
		private int _refreshCount;
		private TaskCompletionSource<bool> _refreshGate;
		private OAuthTokenRefresher _refresher;

		[TestInitialize]
		public void Setup()
		{
			_refreshCount = 0;
			_refreshGate = new TaskCompletionSource<bool>();
			_refresher = new OAuthTokenRefresher(() => _accessToken, () => _fresh, Refresh);
		}

		private async Task<bool> Refresh()
		{
			_refreshCount++;
			await _refreshGate.Task;
			_accessToken = "refreshed";
			_fresh = true;
			return true;
		}

		[TestMethod]
		public async Task ConcurrentCallsWithExpiredToken_RefreshOnce()
		{
			_accessToken = "expired";
			_fresh = false;

			var calls = Enumerable.Range(0, 5).Select(_ => _refresher.EnsureFresh()).ToList();
			_refreshGate.SetResult(true);
			var results = await Task.WhenAll(calls);

			Assert.IsTrue(results.All(x => x));
			Assert.AreEqual(1, _refreshCount);
			Assert.AreEqual("refreshed", _refresher.AccessToken);
		}

		[TestMethod]
		public async Task ConcurrentRejectionsOfSameToken_RefreshOnce()
		{
			_accessToken = "revoked";
			_fresh = true;

			var calls = Enumerable.Range(0, 5).Select(_ => _refresher.RefreshRejected("revoked")).ToList();
			_refreshGate.SetResult(true);
			var results = await Task.WhenAll(calls);

			Assert.IsTrue(results.All(x => x));
			Assert.AreEqual(1, _refreshCount);
			Assert.AreEqual("refreshed", _refresher.AccessToken);
		}

		[TestMethod]
		public async Task RejectionOfReplacedToken_DoesNotRefresh()
		{
			_accessToken = "current";
			_fresh = true;

			var result = await _refresher.RefreshRejected("previous");

			Assert.IsTrue(result);
			Assert.AreEqual(0, _refreshCount);
			Assert.AreEqual("current", _refresher.AccessToken);
		}

		[TestMethod]
		public async Task FreshToken_DoesNotRefresh()
		{
			_accessToken = "current";
			_fresh = true;

			var result = await _refresher.EnsureFresh();

			Assert.IsTrue(result);
			Assert.AreEqual(0, _refreshCount);
		}
	}
}
