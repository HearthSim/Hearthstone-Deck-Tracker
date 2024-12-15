using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HearthWatcher;

public class MulliganTooltipWatcher
{
	public delegate void MulliganTooltipEventHandler(object sender, MulliganTooltipArgs args);

	private readonly IMulliganTooltipProvider _provider;
	private readonly int _delay;
	private bool _running;
	private bool _watch;
	private MulliganTooltipArgs? _prev = null;

	public MulliganTooltipWatcher(IMulliganTooltipProvider mulliganTooltipProvider, int delay = 16)
	{
		_provider = mulliganTooltipProvider ?? throw new ArgumentNullException(nameof(mulliganTooltipProvider));
		_delay = delay;
	}

	public event MulliganTooltipEventHandler Change;

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
			var curr = new MulliganTooltipArgs(
				state?.ZoneSize ?? 0,
				state?.ZonePosition ?? 0,
				state?.IsTooltipOnRight ?? false,
				state?.TooltipCards.Select(card => card.CardId).ToArray() ?? new string[] { }
			);
			if(_prev != null && curr.Equals(_prev))
				continue;
			Change?.Invoke(this, curr);
			_prev = curr;
		}
		_prev = null;
		_running = false;
	}
}
