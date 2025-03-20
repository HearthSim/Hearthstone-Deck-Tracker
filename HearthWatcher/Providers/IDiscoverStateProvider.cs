using HearthMirror.Objects;

namespace HearthWatcher.Providers;

public interface IDiscoverStateProvider
{
	DiscoverState? State { get; }
}
