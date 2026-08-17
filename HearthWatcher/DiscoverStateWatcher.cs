using System;
using System.Threading.Tasks;
using HearthWatcher.EventArgs;
using HearthWatcher.Providers;

namespace HearthWatcher;

public class DiscoverStateWatcher : PollingWatcher
{
	public delegate void DiscoverStateEventHandler(object sender, DiscoverStateArgs args);

	private readonly IDiscoverStateProvider _provider;
	private DiscoverStateArgs? _prev;

	public DiscoverStateWatcher(IDiscoverStateProvider discoverStateProvider, int delay = 16) : base(delay)
	{
		_provider = discoverStateProvider ?? throw new ArgumentNullException(nameof(discoverStateProvider));
	}

	public event DiscoverStateEventHandler? Change;

	protected override Task<bool> TickAsync()
	{
		var state = _provider.State;
		var curr = new DiscoverStateArgs(
			state?.CardId ?? "",
			state?.ZonePosition ?? 0,
			state?.ZoneSize ?? 0,
			state?.EntityId ?? 0
		);
		if(_prev == null || !curr.Equals(_prev))
		{
			_prev = curr;
			Dispatch(() => Change?.Invoke(this, curr));
		}
		return Task.FromResult(false);
	}

	protected override void OnLoopEnd() => _prev = null;
}
