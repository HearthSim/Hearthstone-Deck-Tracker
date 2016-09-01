using HearthMirror.Objects;

namespace HearthWatcher
{
	public interface IArenaProvider
	{
		ArenaInfo GetArenaInfo();
		Card[] GetDraftChoices();
	}
}
