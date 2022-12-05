using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility;
using HSReplay.Responses;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	public static class Tier7Trial
	{
		private static Tier7TrialStatus? _status;
		public static bool IsActive { get; private set; }
		public static int? RemainingTrials => _status?.TrialsRemaining;

		public static string? TimeRemaining => _status?.HoursUntilReset == null ? null
			: string.Format(LocUtil.Get("BattlegroundsPreLobby_Trial_ResetTimeRemaining_DaysHours"), _status.HoursUntilReset / 24, _status.HoursUntilReset % 24);

		public static async Task<bool> Activate()
		{
			if(IsActive)
				return true;
			if(_status == null || _status.TrialsRemaining == 0)
				return false;
			var response = await HSReplayNetOAuth.MakeRequest(client => client.ActivateTier7Trial());
			IsActive = response != null;
			if(IsActive)
				Core.Game.Metrics.Tier7TrialActivated = true;
			return IsActive;
		}

		public static async Task Update()
		{
			if(_status?.HoursUntilReset < 2)
				_status = null;
			_status ??= await HSReplayNetOAuth.MakeRequest(c => c.GetTier7TrialStatus());
		}

		public static void Clear()
		{
			_status = null;
			IsActive = false;
		}
	}
}
