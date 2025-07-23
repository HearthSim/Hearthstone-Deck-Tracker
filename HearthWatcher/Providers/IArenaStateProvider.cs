using System.Collections.Generic;
using HearthMirror.Objects;

namespace HearthWatcher.Providers;

public interface IArenaStateProvider
{
	ArenaState? GetState(int? deckListVersion, int? redraftDeckListVersion, ArenaState.ScryCache? cache);
}
