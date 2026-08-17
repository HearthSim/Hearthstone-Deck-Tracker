using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Threading.Tasks;

namespace HearthWatcher;

public class SceneWatcher : PollingWatcher
{
	public delegate void SceneEventHandler(object sender, SceneEventArgs args);

	private readonly ISceneProvider _provider;
	private SceneEventArgs? _prev;

	public SceneWatcher(ISceneProvider sceneProvider, int delay = 16) : base(delay)
	{
		_provider = sceneProvider ?? throw new ArgumentNullException(nameof(sceneProvider));
	}

	public event SceneEventHandler? Change;

	protected override Task<bool> TickAsync()
	{
		var state = _provider.State;
		var curr = new SceneEventArgs(
			state?.PrevMode ?? 0,
			state?.Mode ?? 0,
			state?.SceneLoaded ?? false,
			state?.Transitioning ?? false
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
