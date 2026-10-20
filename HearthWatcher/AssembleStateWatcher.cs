using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HearthWatcher.EventArgs;
using HearthWatcher.Providers;

namespace HearthWatcher;

public class AssembleStateWatcher : PollingWatcher
{
	public delegate void AssembleStateEventHandler(object sender, AssembleStateArgs args);

	private readonly IAssembleStateProvider _provider;
	private AssembleStateArgs? _prev;

	public AssembleStateWatcher(IAssembleStateProvider assembleStateProvider, int delay = 16) : base(delay)
	{
		_provider = assembleStateProvider ?? throw new ArgumentNullException(nameof(assembleStateProvider));
	}

	public event AssembleStateEventHandler? Change;

	protected override Task<bool> TickAsync()
	{
		var state = _provider.State;
		// A concealed screen (the player hid it to look at the board) shows nothing to hover.
		var isShowing = state is { IsConcealed: false };
		var curr = isShowing
			? new AssembleStateArgs(
				true,
				state!.Value.TargetEntityId ?? 0,
				state.Value.SelectedChoiceEntityId ?? 0,
				state.Value.ChoiceEntityIds ?? new List<int>(),
				state.Value.HoveredChoiceIndex ?? -1,
				state.Value.HoveredHalf ?? 0)
			: new AssembleStateArgs(false, 0, 0, new List<int>(), -1, 0);
		if(_prev == null || !curr.Equals(_prev))
		{
			_prev = curr;
			Dispatch(() => Change?.Invoke(this, curr));
		}
		return Task.FromResult(false);
	}

	protected override void OnLoopEnd() => _prev = null;
}
