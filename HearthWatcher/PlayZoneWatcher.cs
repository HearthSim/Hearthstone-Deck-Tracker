using System;
using System.Threading.Tasks;
using HearthMirror.Objects;
using HearthWatcher.EventArgs;
using HearthWatcher.Providers;

namespace HearthWatcher;

public class PlayZoneWatcher : PollingWatcher
{
	public delegate void PlayZoneEventHandler(object sender, BoardStateArgs args);

	private readonly IBoardStateProvider _provider;
	private BoardStateArgs? _prev;

	public PlayZoneWatcher(IBoardStateProvider boardStateProvider, int delay = 16) : base(delay)
	{
		_provider = boardStateProvider ?? throw new ArgumentNullException(nameof(boardStateProvider));
	}

	public event PlayZoneEventHandler? Change;

	private static PlayZoneArgs? ToArgs(PlayZoneState? state)
		=> state == null ? null : new PlayZoneArgs(state.BoardCards, state.MousedOverSlot);

	protected override Task<bool> TickAsync()
	{
		var state = _provider.BoardState;
		var curr = new BoardStateArgs(ToArgs(state?.Friendly), ToArgs(state?.Opposing));
		if(_prev == null || !curr.Equals(_prev))
		{
			_prev = curr;
			Dispatch(() => Change?.Invoke(this, curr));
		}
		return Task.FromResult(false);
	}

	protected override void OnLoopEnd() => _prev = null;
}
