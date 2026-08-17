using System;
using System.Threading.Tasks;
using HearthMirror.Objects;
using HearthWatcher.Providers;

namespace HearthWatcher;

public class MulliganStateWatcher : PollingWatcher
{
	public delegate void MulliganStateEventHandler(object sender, MulliganState args);

	private readonly IMulliganStateProvider _provider;
	private MulliganState? _prev;

	public MulliganStateWatcher(IMulliganStateProvider mulliganTooltipProvider, int delay = 16) : base(delay)
	{
		_provider = mulliganTooltipProvider ?? throw new ArgumentNullException(nameof(mulliganTooltipProvider));
	}

	public event MulliganStateEventHandler? Change;

	protected override Task<bool> TickAsync()
	{
		var curr = _provider.State;
		if(curr == null)
			return Task.FromResult(false);

		if(_prev == null || !curr.Equals(_prev))
		{
			_prev = curr;
			Dispatch(() => Change?.Invoke(this, curr));
		}
		return Task.FromResult(false);
	}

	protected override void OnLoopEnd() => _prev = null;
}
