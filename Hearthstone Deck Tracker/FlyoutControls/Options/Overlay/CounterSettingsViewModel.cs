using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Settings;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay;

public class CounterSettingsViewModel : INotifyPropertyChanged
{
	private List<CounterSettingRow> _allRows = new();
	private string _filter = string.Empty;

	public ObservableCollection<CounterSettingRow> ConstructedRows { get; } = new();
	public ObservableCollection<CounterSettingRow> BattlegroundsRows { get; } = new();

	public void Load()
	{
		if(_allRows.Count == 0)
			_allRows = CounterCatalog.Descriptors.Select(d => new CounterSettingRow(d)).ToList();

		foreach(var row in _allRows)
			row.Refresh();

		ApplyFilter();
		OnPropertyChanged(nameof(HasAnyOverride));
	}

	public string Filter
	{
		get => _filter;
		set
		{
			if(_filter == value)
				return;
			_filter = value;
			OnPropertyChanged();
			ApplyFilter();
		}
	}

	private void ApplyFilter()
	{
		Fill(ConstructedRows, _allRows.Where(r => !r.Descriptor.IsBattlegroundsCounter));
		Fill(BattlegroundsRows, _allRows.Where(r => r.Descriptor.IsBattlegroundsCounter));

		OnPropertyChanged(nameof(ConstructedVisibility));
		OnPropertyChanged(nameof(BattlegroundsVisibility));
		OnPropertyChanged(nameof(NoResultsVisibility));
	}

	private void Fill(ObservableCollection<CounterSettingRow> target, IEnumerable<CounterSettingRow> source)
	{
		var rows = source
			.Where(Matches)
			.OrderBy(r => r.DisplayName, StringComparer.CurrentCultureIgnoreCase)
			.ToList();

		target.Clear();
		foreach(var row in rows)
			target.Add(row);
	}

	private bool Matches(CounterSettingRow row)
	{
		if(string.IsNullOrWhiteSpace(_filter))
			return true;

		return row.DisplayName.IndexOf(_filter, StringComparison.CurrentCultureIgnoreCase) >= 0
			|| row.Descriptor.CounterId.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0;
	}

	public Visibility ConstructedVisibility => ConstructedRows.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
	public Visibility BattlegroundsVisibility => BattlegroundsRows.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

	public Visibility NoResultsVisibility =>
		ConstructedRows.Count == 0 && BattlegroundsRows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

	public bool HasAnyOverride => CounterVisibilitySettings.Instance.HasAnyOverride;

	public ICommand ResetAllCommand => new Command(() =>
	{
		CounterVisibilitySettings.Instance.ResetAll();
		Load();
	});

	public event PropertyChangedEventHandler? PropertyChanged;

	[NotifyPropertyChangedInvocator]
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class CounterSettingRow : INotifyPropertyChanged
{
	public CounterSettingRow(CounterDescriptor descriptor)
	{
		Descriptor = descriptor;
	}

	public CounterDescriptor Descriptor { get; }

	public string DisplayName => Descriptor.DisplayName;
	public string CounterId => Descriptor.CounterId;
	public CardAssetViewModel CardAsset => Descriptor.CardAsset;

	private static IReadOnlyList<CounterVisibilityOption>? _visibilityOptions;

	public static IReadOnlyList<CounterVisibilityOption> VisibilityOptions => _visibilityOptions ??= new[]
	{
		new CounterVisibilityOption(CounterVisibility.Auto, LocUtil.Get("OptionsCounters_Mode_Auto")),
		new CounterVisibilityOption(CounterVisibility.Enabled, LocUtil.Get("OptionsCounters_Mode_Enabled")),
		new CounterVisibilityOption(CounterVisibility.Disabled, LocUtil.Get("OptionsCounters_Mode_Disabled")),
	};

	public bool IsOpponentSupported => !Descriptor.IsBattlegroundsCounter;

	public string? OpponentUnsupportedTooltip =>
		IsOpponentSupported ? null : LocUtil.Get("OptionsCounters_OpponentUnsupportedTooltip");

	public CounterVisibility PlayerVisibility
	{
		get => CounterVisibilitySettings.Instance.Get(CounterId, true);
		set
		{
			if(PlayerVisibility == value)
				return;
			CounterVisibilitySettings.Instance.Set(CounterId, true, value);
			OnPropertyChanged();
		}
	}

	public CounterVisibility OpponentVisibility
	{
		get => CounterVisibilitySettings.Instance.Get(CounterId, false);
		set
		{
			if(OpponentVisibility == value)
				return;
			CounterVisibilitySettings.Instance.Set(CounterId, false, value);
			OnPropertyChanged();
		}
	}

	public void Refresh()
	{
		OnPropertyChanged(nameof(DisplayName));
		OnPropertyChanged(nameof(CardAsset));
		OnPropertyChanged(nameof(PlayerVisibility));
		OnPropertyChanged(nameof(OpponentVisibility));
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	[NotifyPropertyChangedInvocator]
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class CounterVisibilityOption
{
	public CounterVisibilityOption(CounterVisibility value, string label)
	{
		Value = value;
		Label = label;
	}

	public CounterVisibility Value { get; }
	public string Label { get; }
}
