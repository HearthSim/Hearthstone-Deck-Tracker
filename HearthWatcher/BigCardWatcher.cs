using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HearthWatcher;

public class BigCardStateWatcher : PollingWatcher
{
	public delegate void BigCardEventHandler(object sender, BigCardArgs args);

	private readonly IBigCardProvider _provider;
	private BigCardArgs? _prev;

	public BigCardStateWatcher(IBigCardProvider bigCardProvider, int delay = 16) : base(delay)
	{
		_provider = bigCardProvider ?? throw new ArgumentNullException(nameof(bigCardProvider));
	}

	public event BigCardEventHandler? Change;

	protected override Task<bool> TickAsync()
	{
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
		if(_prev == null || !curr.Equals(_prev))
		{
			_prev = curr;
			Dispatch(() => Change?.Invoke(this, curr));
		}
		return Task.FromResult(false);
	}

	protected override void OnLoopEnd() => _prev = null;
}
