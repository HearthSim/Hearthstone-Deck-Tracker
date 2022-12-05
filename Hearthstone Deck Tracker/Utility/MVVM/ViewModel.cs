using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Annotations;

namespace Hearthstone_Deck_Tracker.Utility.MVVM
{
	public class ViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private readonly Dictionary<string, object?> _data = new();

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
	}
}
