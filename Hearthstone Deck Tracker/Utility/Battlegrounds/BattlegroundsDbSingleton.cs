using System;
using HearthMirror;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.RemoteData;

namespace Hearthstone_Deck_Tracker.Utility.Battlegrounds;

public static class BattlegroundsDbSingleton
{
	private static readonly Lazy<BattlegroundsDb> Solos = new(() => BattlegroundsDb.FromLiveMetaPeriod(Remote.BattlegroundsLiveMetaPeriod));
	private static readonly Lazy<BattlegroundsDb> Duos = new(() => BattlegroundsDb.FromLiveMetaPeriod(Remote.BattlegroundsDuosLiveMetaPeriod));

	/// <summary>
	/// The assembled database for the game mode, kept up to date with its live meta period.
	/// </summary>
	public static BattlegroundsDb Get(bool isDuos) => (isDuos ? Duos : Solos).Value;

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
			: Get(Core.Game.IsBattlegroundsDuosMatch);

	public static bool TryLoadMinionPool(out BattlegroundsMinionPool pool)
	{
		pool = null!;
		if(Core.Game.CurrentGameStats?.GameId is not Guid gameId)
			return false;
		var minionPool = Reflection.Client.GetBattlegroundsMinionPool();
		if(minionPool?.Cards is not { Count: > 0 })
			return false;
		var db = BattlegroundsDb.FromMinionPool(minionPool, Get(Core.Game.IsBattlegroundsDuosMatch));
		_minionPoolDb = (gameId, db);
		if(db.DarkParadox is { } darkParadox)
			Log.Info($"Dark Paradox in the minion pool: {darkParadox.Id} (tier {darkParadox.TechLevel})");
		pool = minionPool;
		return true;
	}
}
