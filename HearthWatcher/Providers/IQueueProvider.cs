using HearthMirror.Enums;

namespace HearthWatcher.Providers
{
	public interface IQueueProvider
	{
		FindGameState? FindGameState { get; }
	}
}
