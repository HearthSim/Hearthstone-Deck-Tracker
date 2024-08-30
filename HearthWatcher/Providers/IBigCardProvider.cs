using System.Collections.Generic;
using HearthMirror.Objects;

namespace HearthWatcher.Providers;

public interface IBigCardProvider
{
	BigCardState? State { get; }
}
