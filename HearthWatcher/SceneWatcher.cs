using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Threading.Tasks;

namespace HearthWatcher;

public class SceneWatcher
{
	public delegate void SceneEventHandler(object sender, SceneEventArgs args);

	private readonly ISceneProvider _provider;
	private readonly int _delay;
	private bool _running;
	private bool _watch;
	private SceneEventArgs _prev = null;

	public SceneWatcher(ISceneProvider sceneProvider, int delay = 16)
	{
		_provider = sceneProvider ?? throw new ArgumentNullException(nameof(sceneProvider));
		_delay = delay;
	}

	public event SceneEventHandler Change;

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
			var curr = new SceneEventArgs(
				state?.PrevMode ?? 0,
				state?.Mode ?? 0,
				state?.SceneLoaded ?? false,
				state?.Transitioning ?? false
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
