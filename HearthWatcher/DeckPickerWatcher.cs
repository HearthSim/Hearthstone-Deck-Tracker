using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Threading.Tasks;
using HearthMirror.Objects;

namespace HearthWatcher;

public class DeckPickerWatcher : PollingWatcher
{
	public delegate void DeckPickerEventHandler(object sender, DeckPickerEventArgs args);

	private readonly IDeckPickerProvider _provider;
	private DeckPickerEventArgs? _prev;

	public DeckPickerWatcher(IDeckPickerProvider deckPickerProvider, int delay = 16) : base(delay)
	{
		_provider = deckPickerProvider ?? throw new ArgumentNullException(nameof(deckPickerProvider));
	}

	public event DeckPickerEventHandler? Change;

	protected override Task<bool> TickAsync()
	{
		var curr = new DeckPickerEventArgs(
			_provider.DeckPickerState?.VisualsFormatType ?? VisualsFormatType.VFT_UNKNOWN,
			_provider.DecksOnPage ?? new(),
			_provider.DeckPickerState?.SelectedDeck,
			(_provider.DeckPickerState?.IsModeSwitching ?? false) || _provider.IsBlurActive || (_provider.DeckPickerState?.SetRotationOpen ?? false)
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
