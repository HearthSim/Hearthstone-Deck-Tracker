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

	public ObservableCollection<object> Rows { get; } = new();

	private List<CounterSettingRow> AllRows
	{
		get
		{
			if(_allRows.Count == 0)
				_allRows = CounterCatalog.Descriptors.Select(d => new CounterSettingRow(d)).ToList();
			return _allRows;
		}
	}

	public void Load()
	{
		foreach(var row in AllRows)
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
		var rows = AllRows
			.Where(r => Matches(r, _filter))
			.OrderBy(r => r.DisplayName, StringComparer.CurrentCultureIgnoreCase)
			.ToList();
		var constructed = rows.Where(r => !r.Descriptor.IsBattlegroundsCounter).ToList();
		var battlegrounds = rows.Where(r => r.Descriptor.IsBattlegroundsCounter).ToList();

		Rows.Clear();
		AddGroup("OptionsCounters_Group_Traditional", constructed);
		AddGroup("OptionsCounters_Group_Battlegrounds", battlegrounds);

		OnPropertyChanged(nameof(NoResultsVisibility));
	}

	private void AddGroup(string titleKey, List<CounterSettingRow> rows)
	{
		if(rows.Count == 0)
			return;
		Rows.Add(new CounterGroupHeader(LocUtil.Get(titleKey)));
		foreach(var row in rows)
			Rows.Add(row);
	}

	public bool HasMatches(string filter) => AllRows.Any(r => Matches(r, filter));

	private static bool Matches(CounterSettingRow row, string filter)
	{
		if(string.IsNullOrWhiteSpace(filter))
			return true;

		return row.DisplayName.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0
			|| row.Descriptor.CounterId.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
	}

	public Visibility NoResultsVisibility => Rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

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
	public CardAssetViewModel? CardAsset => Descriptor.CardAsset;
	public string? IconSource => Descriptor.IconSource;

	private static IReadOnlyList<CounterVisibilityOption>? _visibilityOptions;

	public static IReadOnlyList<CounterVisibilityOption> VisibilityOptions => _visibilityOptions ??= new[]
	{
		new CounterVisibilityOption(CounterVisibility.Auto, LocUtil.Get("OptionsCounters_Mode_Auto")),
		new CounterVisibilityOption(CounterVisibility.Enabled, LocUtil.Get("OptionsCounters_Mode_Enabled")),
		new CounterVisibilityOption(CounterVisibility.Disabled, LocUtil.Get("OptionsCounters_Mode_Disabled")),
	};

	public bool IsPlayerSupported => Descriptor.SupportsPlayer;
	public bool IsOpponentSupported => Descriptor.SupportsOpponent;

	public string? PlayerUnsupportedTooltip => Descriptor.PlayerUnsupportedTooltip;
	public string? OpponentUnsupportedTooltip => Descriptor.OpponentUnsupportedTooltip;

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

public class CounterGroupHeader
{
	public CounterGroupHeader(string title)
	{
		Title = title;
	}

	public string Title { get; }
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
