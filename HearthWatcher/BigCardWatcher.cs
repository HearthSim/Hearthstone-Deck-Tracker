using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HearthWatcher;

public class BigCardStateWatcher
{
	public delegate void BigCardEventHandler(object sender, BigCardArgs args);

	private readonly IBigCardProvider _provider;
	private readonly int _delay;
	private bool _running;
	private bool _watch;
	private BigCardArgs _prev = null;

	public BigCardStateWatcher(IBigCardProvider bigCardProvider, int delay = 16)
	{
		_provider = bigCardProvider ?? throw new ArgumentNullException(nameof(bigCardProvider));
		_delay = delay;
	}

	public event BigCardEventHandler Change;

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
			var curr = new BigCardArgs(
				state?.TooltipHeights ?? new List<float>(),
				state?.EnchantmentHeights ?? new List<float>(),
				state?.CardId ?? "",
				state?.ZonePosition ?? 0,
				state?.ZoneSize ?? 0,
				state?.Side ?? 0,
				state?.IsHand ?? false
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
