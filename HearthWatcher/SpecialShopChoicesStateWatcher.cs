using System;
using System.Threading.Tasks;
using HearthWatcher.EventArgs;
using HearthWatcher.Providers;

namespace HearthWatcher;

public class SpecialShopChoicesStateWatcher
{
	public delegate void SpecialShopChoicesStateEventHandler(object sender, SpecialShopChoicesArgs args);

	private readonly ISpecialShopChoicesProvider _provider;
	private readonly int _delay;
	private bool _running;
	private bool _watch;
	private SpecialShopChoicesArgs _prev = null;

	public SpecialShopChoicesStateWatcher(ISpecialShopChoicesProvider opponentBoardProvider, int delay = 16)
	{
		_provider = opponentBoardProvider ?? throw new ArgumentNullException(nameof(opponentBoardProvider));
		_delay = delay;
	}

	public event SpecialShopChoicesStateEventHandler Change;

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

			var state = _provider.SpecialShopChoicesState;
			var curr = new SpecialShopChoicesArgs(
				state?.IsActive ?? false,
				state?.BoardCards ?? new System.Collections.Generic.List<HearthMirror.Objects.BoardCard>(),
				state?.MousedOverSlot ?? -1
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
