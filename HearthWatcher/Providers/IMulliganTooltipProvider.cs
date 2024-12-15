using HearthMirror.Objects;

namespace HearthWatcher.Providers;

public interface IMulliganTooltipProvider
{
	MulliganTooltipState? State { get; }
}
