using System.Threading.Tasks;
using HearthMirror.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using HSReplay.Responses;

namespace Hearthstone_Deck_Tracker.HsReplay;

public static class ArenaTrial
{
	static ArenaTrial()
	{
		Watchers.ArenaStateWatcher.OnClientStateChanged	+= (state) =>
		{
			if(state.ClientState is not (ArenaClientStateType.None or ArenaClientStateType.Normal_Landing or ArenaClientStateType.Underground_Landing))
				Clear();
		};
	}

	private static ArenaTrialStatus? _status;
	public static (int StarterTrialsRemaining, int RecurringTrialsRemaining)? RemainingTrials => _status != null ? (_status.StarterTrialsRemaining, _status.RecurringTrialsRemaining) : null;
	public static int? MaxRecurringTrials => _status?.MaxRecurringTrials;

	public static string? TimeRemaining => _status?.HoursUntilReset == null ? null
		: string.Format(LocUtil.Get("BattlegroundsPreLobby_Trial_ResetTimeRemaining_DaysHours"), _status.HoursUntilReset / 24, _status.HoursUntilReset % 24);

	public static bool IsDeckResumable(long deckId) => _status?.ResumableDeckIds.Contains(deckId) ?? false;

	public static async Task Update(ulong accountHi, ulong accountLo)
	{
		_status ??= await ApiWrapper.GetArenaTrialStatus(accountHi, accountLo);
	}

	public static async Task EnsureLoaded(ulong accountHi, ulong accountLo)
	{
		if(_status == null)
			await Update(accountHi, accountLo);
	}

	public static void Clear()
	{
		_status = null;
	}
}
