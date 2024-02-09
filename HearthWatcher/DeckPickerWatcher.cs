using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Linq;
using System.Threading.Tasks;
using HearthMirror.Objects;

namespace HearthWatcher;

public class DeckPickerWatcher
{
	public delegate void DeckPickerEventHandler(object sender, DeckPickerEventArgs args);

	private readonly IDeckPickerProvider _provider;
	private readonly int _delay;
	private bool _running;
	private bool _watch;
	private DeckPickerEventArgs _prev;

	public DeckPickerWatcher(IDeckPickerProvider deckPickerProvider, int delay = 16)
	{
		_provider = deckPickerProvider ?? throw new ArgumentNullException(nameof(deckPickerProvider));
		_delay = delay;
	}

	public event DeckPickerEventHandler Change;

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
			var curr = new DeckPickerEventArgs(
				_provider.DeckPickerState?.VisualsFormatType ?? VisualsFormatType.VFT_UNKNOWN,
				_provider.DecksOnPage ?? new(),
				_provider.DeckPickerState?.SelectedDeck,
				(_provider.DeckPickerState?.IsModeSwitching ?? false) || _provider.IsBlurActive
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
