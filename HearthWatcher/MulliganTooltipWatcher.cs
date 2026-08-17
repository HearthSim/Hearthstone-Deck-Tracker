using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HearthWatcher;

public class MulliganTooltipWatcher : PollingWatcher
{
	public delegate void MulliganTooltipEventHandler(object sender, MulliganTooltipArgs args);

	private readonly IMulliganTooltipProvider _provider;
	private MulliganTooltipArgs? _prev;

	public MulliganTooltipWatcher(IMulliganTooltipProvider mulliganTooltipProvider, int delay = 16) : base(delay)
	{
		_provider = mulliganTooltipProvider ?? throw new ArgumentNullException(nameof(mulliganTooltipProvider));
	}

	public event MulliganTooltipEventHandler? Change;

	protected override Task<bool> TickAsync()
	{
		var state = _provider.State;
		var curr = new MulliganTooltipArgs(
			state?.ZoneSize ?? 0,
			state?.ZonePosition ?? 0,
			state?.IsTooltipOnRight ?? false,
			state?.TooltipCards.Select(card => card.CardId).ToArray() ?? new string[] { }
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
