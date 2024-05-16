using HearthMirror.Objects;

namespace HearthWatcher.Providers
{
	public interface IBattlegroundsLeaderboardProvider
	{
		int? BattlegroundsLeaderboardHoveredEntityId { get; }
	}
}
