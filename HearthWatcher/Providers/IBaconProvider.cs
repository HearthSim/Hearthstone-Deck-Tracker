using HearthMirror.Objects;

namespace HearthWatcher.Providers
{
	public interface IBaconProvider
	{
		SelectedBattlegroundsGameMode? SelectedBattlegroundsGameMode { get; }
	}
}
