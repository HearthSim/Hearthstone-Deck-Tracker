using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using WPFLocalizeExtension.Engine;

namespace Hearthstone_Deck_Tracker.Utility.MVVM;

/// <summary>
/// Mark property as localized. Usually used when calling LocUtil for a localized string.
/// Setting this attribute will cause OnPropertyChanged to be automatically called on the
/// property when the selected language for the application changes.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class LocalizedPropAttribute : Attribute
{
}

/// <summary>
/// Notifies about all [LocalizedProp] properties of the given type whenever the selected
/// language for the application changes. <see cref="ViewModel"/> sets this up automatically,
/// controls implementing INotifyPropertyChanged themselves have to create one in their constructor.
/// It only notifies for as long as it is referenced, so keep it in a field.
/// </summary>
public class LocalizedPropNotifier
{
	// a view model is constructed per card tile on every browser filter change, and reflecting over every
	// property on each instance came out at roughly 0.2ms per view model in a profile
	private static readonly ConcurrentDictionary<Type, IReadOnlyList<string>> LocalizedPropNamesByType = new();

	// LocalizeDictionary lives for the whole session, so we subscribe to it once and keep the individual
	// notifiers weakly: view models and controls (such as the virtualized groups in the minions browser)
	// are discarded all the time
	private static readonly List<WeakReference<LocalizedPropNotifier>> Notifiers = new();

	static LocalizedPropNotifier()
	{
		LocalizeDictionary.Instance.PropertyChanged += LocalizeDictionary_OnPropertyChanged;
	}

	private static void LocalizeDictionary_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if(e.PropertyName != nameof(LocalizeDictionary.Culture))
			return;
		foreach(var notifier in TakeAliveNotifiers())
			notifier.NotifyAll();
	}

	private static List<LocalizedPropNotifier> TakeAliveNotifiers()
	{
		var alive = new List<LocalizedPropNotifier>();
		lock(Notifiers)
		{
			Notifiers.RemoveAll(weak =>
			{
				if(!weak.TryGetTarget(out var notifier))
					return true;
				alive.Add(notifier);
				return false;
			});
		}
		return alive;
	}

	private readonly IReadOnlyList<string> _propNames;
	private readonly Action<string> _onPropertyChanged;

	public LocalizedPropNotifier(Type type, Action<string> onPropertyChanged)
	{
		_onPropertyChanged = onPropertyChanged;
		_propNames = LocalizedPropNamesByType.GetOrAdd(type, t =>
			t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
				.Where(p => p.GetCustomAttributes(typeof(LocalizedPropAttribute), true).Any())
				.Select(x => x.Name)
				.ToList());
		if(_propNames.Count == 0)
			return;
		lock(Notifiers)
			Notifiers.Add(new WeakReference<LocalizedPropNotifier>(this));
	}

	private void NotifyAll()
	{
		foreach(var name in _propNames)
			_onPropertyChanged(name);
	}
}
