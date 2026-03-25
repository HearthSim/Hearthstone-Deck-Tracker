using System;
using System.Threading.Tasks;
using HearthMirror.Objects;
using HearthWatcher.Providers;

namespace HearthWatcher;

public class MulliganStateWatcher
{
	public delegate void MulliganStateEventHandler(object sender, MulliganState args);

	private readonly IMulliganStateProvider _provider;
	private readonly int _delay;
	private bool _running;
	private bool _watch;
	private MulliganState? _prev = null;

	public MulliganStateWatcher(IMulliganStateProvider mulliganTooltipProvider, int delay = 16)
	{
		_provider = mulliganTooltipProvider ?? throw new ArgumentNullException(nameof(mulliganTooltipProvider));
		_delay = delay;
	}

	public event MulliganStateEventHandler Change;

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

			var curr = _provider.State;
			if(curr == null)
				continue;

			if(_prev != null && curr.Equals(_prev))
				continue;
			Change?.Invoke(this, curr);
			_prev = curr;
		}
		_prev = null;
		_running = false;
	}
}
