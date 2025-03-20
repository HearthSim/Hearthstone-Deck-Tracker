using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HearthWatcher.EventArgs;
using HearthWatcher.Providers;

namespace HearthWatcher;

public class DiscoverStateWatcher
{
	public delegate void DiscoverStateEventHandler(object sender, DiscoverStateArgs args);

	private readonly IDiscoverStateProvider _provider;
	private readonly int _delay;
	private bool _running;
	private bool _watch;
	private DiscoverStateArgs _prev = null;

	public DiscoverStateWatcher(IDiscoverStateProvider discoverStateProvider, int delay = 16)
	{
		_provider = discoverStateProvider ?? throw new ArgumentNullException(nameof(discoverStateProvider));
		_delay = delay;
	}

	public event DiscoverStateEventHandler Change;

	public void Run()
	{
		_watch = true;
		if(!_running)
			Update();
	}

	public void Stop() => _watch = false;

	private async void Update()
	{
		_running = true;
		while(_watch)
		{
			await Task.Delay(_delay);
			if(!_watch)
				break;

			var state = _provider.State;
			var curr = new DiscoverStateArgs(
				state?.CardId ?? "",
				state?.ZonePosition ?? 0,
				state?.ZoneSize ?? 0
			);
			if(curr.Equals(_prev))
				continue;
			Change?.Invoke(this, curr);
			_prev = curr;
		}
		_prev = null;
		_running = false;
	}
}
