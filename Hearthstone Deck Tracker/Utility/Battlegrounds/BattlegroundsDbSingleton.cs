using System;
using HearthMirror;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;

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
		var db = BattlegroundsDb.FromMinionPool(pool, Instance);
		_minionPoolDb = (gameId, db);
		if(db.DarkParadox is { } darkParadox)
			Log.Info($"Dark Paradox in the minion pool: {darkParadox.Id} (tier {darkParadox.TechLevel})");
		return true;
	}
}
