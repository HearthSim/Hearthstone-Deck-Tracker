using HearthMirror.Objects;

namespace HearthWatcher.Providers;

public interface IAssembleStateProvider
{
	AssembleState? State { get; }
}
