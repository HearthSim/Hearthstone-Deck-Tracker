using HearthMirror.Objects;

namespace HearthWatcher.Providers;

public interface IMulliganStateProvider
{
	MulliganState? State { get; }
}
