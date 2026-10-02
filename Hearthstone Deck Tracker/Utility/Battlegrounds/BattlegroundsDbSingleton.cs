using System;
using HearthMirror;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.RemoteData;

namespace Hearthstone_Deck_Tracker.Utility.Battlegrounds;

public static class BattlegroundsDbSingleton
{
	/// <summary>
	/// Replaces the database of a game mode rather than updating it, so holders of the previous one can tell it
	/// changed by reference.
	/// </summary>
	private class GameModeDb
	{
		private readonly DataLoader<RemoteData.RemoteData.MetaPeriod?> _metaPeriod;
		private RemoteData.RemoteData.MetaPeriod? _lastMetaPeriod;
		private BattlegroundsDb? _db;

		public GameModeDb(DataLoader<RemoteData.RemoteData.MetaPeriod?> metaPeriod, Action onUpdated)
		{
			_metaPeriod = metaPeriod;
			metaPeriod.Loaded += data =>
			{
				// keep the last good data when a reload fails
				if(data == null)
					return;
				_lastMetaPeriod = data;
				_db = new BattlegroundsDb(data);
				onUpdated();
			};
			CardDefsManager.CardsChanged += () => _db = null;
		}

		public BattlegroundsDb Db => _db ??= new BattlegroundsDb(_metaPeriod.Data ?? _lastMetaPeriod);
	}

	private static readonly GameModeDb Solos = new(Remote.BattlegroundsLiveMetaPeriod, () => Updated?.Invoke(false));
	private static readonly GameModeDb Duos = new(Remote.BattlegroundsDuosLiveMetaPeriod, () => Updated?.Invoke(true));

	/// <summary>
	/// The assembled database for the game mode, rebuilt whenever its live meta period loads.
	/// </summary>
	public static BattlegroundsDb Get(bool isDuos) => (isDuos ? Duos : Solos).Db;

	/// <summary>
	/// Raised with the game mode after its live meta period has loaded and its database has been replaced.
	/// </summary>
	public static event Action<bool>? Updated;

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
