using HearthMirror.Objects;

namespace HearthWatcher.Providers
{
	public interface IArenaProvider
	{
		ArenaInfo GetArenaInfo();
		Card[] GetDraftChoices();
	}
}
