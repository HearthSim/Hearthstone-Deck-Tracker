using HearthMirror.Objects;

namespace HearthWatcher.Providers;

public interface IBoardStateProvider
{
	BoardState? BoardState { get; }
}
