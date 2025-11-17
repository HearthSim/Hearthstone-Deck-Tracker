using HearthMirror.Objects;

namespace HearthWatcher.Providers;

public interface IOpponentBoardProvider
{
	OpponentBoardState? OpponentBoardState { get; }
}
