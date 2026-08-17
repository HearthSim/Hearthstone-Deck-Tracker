using System;
using System.Threading.Tasks;
using HearthWatcher.EventArgs;
using HearthWatcher.Providers;

namespace HearthWatcher;

public class SpecialShopChoicesStateWatcher : PollingWatcher
{
	public delegate void SpecialShopChoicesStateEventHandler(object sender, SpecialShopChoicesArgs args);

	private readonly ISpecialShopChoicesProvider _provider;
	private SpecialShopChoicesArgs? _prev;
	public SpecialShopChoicesArgs? CurrentState => _prev;

	public SpecialShopChoicesStateWatcher(ISpecialShopChoicesProvider opponentBoardProvider, int delay = 16) : base(delay)
	{
		_provider = opponentBoardProvider ?? throw new ArgumentNullException(nameof(opponentBoardProvider));
	}

	public event SpecialShopChoicesStateEventHandler? Change;

	protected override Task<bool> TickAsync()
	{
		var state = _provider.SpecialShopChoicesState;
		var curr = new SpecialShopChoicesArgs(
			state?.IsActive ?? false,
			state?.BoardCards ?? new System.Collections.Generic.List<HearthMirror.Objects.BoardCard>(),
			state?.MousedOverSlot ?? -1
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
