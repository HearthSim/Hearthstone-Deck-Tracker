using System;
using System.Threading.Tasks;
using HearthMirror.Objects;
using HearthWatcher.EventArgs;
using HearthWatcher.Providers;

namespace HearthWatcher;

/// <summary>
/// Watches both play zones. Replaces the old OpponentBoardStateWatcher: the opposing zone is
/// still what Battlegrounds cares about (it is Bob's shop), but the friendly zone is needed for
/// anything drawn over the player's own board.
/// </summary>
public class PlayZoneWatcher
{
	public delegate void PlayZoneEventHandler(object sender, BoardStateArgs args);

	private readonly IBoardStateProvider _provider;
	private readonly int _delay;
	private bool _running;
	private bool _watch;
	private BoardStateArgs? _prev = null;

	public PlayZoneWatcher(IBoardStateProvider boardStateProvider, int delay = 16)
	{
		_provider = boardStateProvider ?? throw new ArgumentNullException(nameof(boardStateProvider));
		_delay = delay;
	}

	public event PlayZoneEventHandler? Change;

	public void Run()
	{
		_watch = true;
		if(!_running)
			Update();
	}

	public void Stop() => _watch = false;

	private static PlayZoneArgs? ToArgs(PlayZoneState? state)
		=> state == null ? null : new PlayZoneArgs(state.BoardCards, state.MousedOverSlot);

	private async void Update()
	{
		_running = true;
		while(_watch)
		{
			await Task.Delay(_delay);
			if(!_watch)
				break;

			var state = _provider.BoardState;
			var curr = new BoardStateArgs(ToArgs(state?.Friendly), ToArgs(state?.Opposing));
			if(curr.Equals(_prev))
				continue;
			Change?.Invoke(this, curr);
			_prev = curr;
		}
		_prev = null;
		_running = false;
	}
}
