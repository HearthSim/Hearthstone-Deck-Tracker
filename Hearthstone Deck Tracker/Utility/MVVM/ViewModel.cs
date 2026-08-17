using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Annotations;

namespace Hearthstone_Deck_Tracker.Utility.MVVM;

public class ViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	[NotifyPropertyChangedInvocator]
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	// Properties are read from the UI thread via data binding and written from arbitrary
	// background threads (async continuations, game event handlers), so this needs to be
	// thread-safe to avoid corrupting the backing store's internal state.
	private readonly ConcurrentDictionary<string, object?> _data = new();

	protected T? GetProp<T>(T defaultValue, [CallerMemberName] string memberName = "")
	{
		return _data.TryGetValue(memberName, out var value) ? (T?)value : defaultValue;
	}

	protected void SetProp<T>(T value, [CallerMemberName] string memberName = "")
	{
		if(_data.TryGetValue(memberName, out var current) && (value?.Equals(current) ?? false))
			return;
		_data[memberName] = value;
		OnPropertyChanged(memberName);
	}

	protected bool TryGetProp<T>(string memberName, out T? value)
	{
		if(_data.TryGetValue(memberName, out var v) && v != default)
		{
			value = (T?)v;
			return true;
		}

		value = default;
		return false;
	}

	private readonly LocalizedPropNotifier _localizedPropNotifier;

	protected ViewModel()
	{
		// This allows us to annotate (getter only) properties with [LocalizedProp] to automatically
		// update them when the language changes.
		_localizedPropNotifier = new LocalizedPropNotifier(GetType(), OnPropertyChanged);
	}
}
