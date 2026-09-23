using System;
using HearthMirror;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Utility.Battlegrounds;

public class BattlegroundsDbSingleton : Singleton<BattlegroundsDb>
{
	private BattlegroundsDbSingleton()
	{
	}

	private static (Guid GameId, BattlegroundsDb Db)? _minionPoolDb;

	/// <summary>
	/// The minion pool of the current match once it has been read from the game, the assembled database otherwise.
	/// </summary>
	public static BattlegroundsDb Current =>
		_minionPoolDb is { } poolDb
		&& !Core.Game.IsInMenu
		&& Core.Game.IsBattlegroundsMatch
		&& poolDb.GameId == Core.Game.CurrentGameStats?.GameId
			? poolDb.Db
			: Instance;

	public static bool TryLoadMinionPool()
	{
		if(Core.Game.CurrentGameStats?.GameId is not Guid gameId)
			return false;
		var pool = Reflection.Client.GetBattlegroundsMinionPool();
		if(pool?.Cards is not { Count: > 0 })
			return false;
		_minionPoolDb = (gameId, BattlegroundsDb.FromMinionPool(pool, Instance));
		return true;
	}
}
