using System.Threading.Tasks;
using HearthMirror.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using HSReplay.Responses;

namespace Hearthstone_Deck_Tracker.HsReplay;

public static class ArenasmithStatusManager
{
	static ArenasmithStatusManager()
	{
		/*Watchers.ArenaStateWatcher.OnClientStateChanged	+= (state) =>
		{
			if(state.ClientState is not (ArenaClientStateType.None or ArenaClientStateType.Normal_Landing or ArenaClientStateType.Underground_Landing))
				Clear();
		};*/
	}

	private static ArenasmithStatus? _status;

	public static async Task Update(ulong accountHi, ulong accountLo)
	{
		_status ??= await ApiWrapper.GetArenasmithStatus();
	}

	public static void Clear()
	{
		_status = null;
	}
}
