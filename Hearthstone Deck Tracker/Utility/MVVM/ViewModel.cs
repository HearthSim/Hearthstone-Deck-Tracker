using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Annotations;
using WPFLocalizeExtension.Engine;

namespace Hearthstone_Deck_Tracker.Utility.MVVM;

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

	private readonly List<string> _localizedPropNames;
	protected ViewModel()
	{
		// This allows us to annotate (getter only) properties with [LozalizedProp] to automatically
		// update them when the language changes.
		_localizedPropNames = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
			.Where(p => p.GetCustomAttributes(typeof(LocalizedPropAttribute), true).Any())
			.Select(x => x.Name)
			.ToList();
		if(_localizedPropNames.Count > 0)
			LocalizeDictionary.Instance.PropertyChanged += LocalizeDictionary_OnPropertyChanged;
	}

	~ViewModel()
	{
		if(_localizedPropNames.Count > 0)
			LocalizeDictionary.Instance.PropertyChanged -= LocalizeDictionary_OnPropertyChanged;
	}

	private void LocalizeDictionary_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if(e.PropertyName != nameof(LocalizeDictionary.Instance.Culture))
			return;
		foreach(var name in _localizedPropNames)
			OnPropertyChanged(name);
	}

	/// <summary>
	/// Mark property as localized. Usually used when calling LocUtil for a localized string.
	/// Setting this attribute will cause OnPropertyChanged to be automatically called on the
	/// property when the selected language for the application changes.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	protected class LocalizedPropAttribute : Attribute
	{
	}
}
