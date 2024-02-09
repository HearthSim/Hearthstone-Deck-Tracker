using System.Collections.Generic;
using HearthMirror.Objects;

namespace HearthWatcher.Providers;

public interface IDeckPickerProvider
{
	DeckPickerState? DeckPickerState { get; }
	List<CollectionDeckBoxVisual?>? DecksOnPage { get; }
	bool IsBlurActive { get; }
}

