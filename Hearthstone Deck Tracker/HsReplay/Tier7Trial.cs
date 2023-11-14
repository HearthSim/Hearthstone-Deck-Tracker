using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility;
using HSReplay.Responses;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	public static class Tier7Trial
	{
		private static PlayerTrialStatus? _status;
		public static string? Token { get; private set; }
		public static int? RemainingTrials => _status?.TrialsRemaining;

		public static string? TimeRemaining => _status?.HoursUntilReset == null ? null
			: string.Format(LocUtil.Get("BattlegroundsPreLobby_Trial_ResetTimeRemaining_DaysHours"), _status.HoursUntilReset / 24, _status.HoursUntilReset % 24);

		public static async Task<string?> Activate(ulong accountHi, ulong accountLo)
		{
			if(Token != null)
				return null;
			if(_status == null || _status.TrialsRemaining == 0)
				return null;
			Token = await ApiWrapper.ActivatePlayerTrial("tier7-overlay", accountHi, accountLo);
			if(Token != null)
				Core.Game.Metrics.Tier7TrialActivated = true;
			return Token;
		}

		public static async Task Update(ulong accountHi, ulong accountLo)
		{
			if(_status?.HoursUntilReset < 2)
				_status = null;
			_status ??= await ApiWrapper.GetPlayerTrialStatus("tier7-overlay", accountHi, accountLo);
		}

		public static void Clear()
		{
			_status = null;
			Token = null;
		}
	}
}
