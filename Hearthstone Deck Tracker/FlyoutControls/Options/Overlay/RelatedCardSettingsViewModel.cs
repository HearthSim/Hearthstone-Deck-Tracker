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
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Settings;

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay;

public class RelatedCardSettingsViewModel : INotifyPropertyChanged
{
	private List<RelatedCardSettingRow> _allRows = new();
	private string _filter = string.Empty;
	private bool _customizedOnly;

	public RelatedCardSettingsViewModel()
	{
		// Rows write to the settings directly, so this is how the Reset button learns that a
		// dropdown changed. Both live as long as the options window, so no unsubscribe.
		RelatedCardVisibilitySettings.Instance.Changed += (_, _) => OnPropertyChanged(nameof(HasAnyOverride));
	}

	public ObservableCollection<RelatedCardSettingRow> Rows { get; } = new();

	private List<RelatedCardSettingRow> AllRows
	{
		get
		{
			if(_allRows.Count == 0)
				_allRows = RelatedCardCatalog.Descriptors.Select(d => new RelatedCardSettingRow(d)).ToList();
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

	public bool CustomizedOnly
	{
		get => _customizedOnly;
		set
		{
			if(_customizedOnly == value)
				return;
			_customizedOnly = value;
			OnPropertyChanged();
			ApplyFilter();
		}
	}

	private void ApplyFilter()
	{
		var rows = AllRows
			.Where(r => (!_customizedOnly || r.OpponentVisibility != CounterVisibility.Auto) && Matches(r, _filter))
			.OrderBy(r => r.DisplayName, StringComparer.CurrentCultureIgnoreCase)
			.ThenBy(r => r.CardId, StringComparer.Ordinal)
			.ToList();

		Rows.Clear();
		foreach(var row in rows)
			Rows.Add(row);

		OnPropertyChanged(nameof(NoResultsVisibility));
	}

	public bool HasMatches(string filter) => AllRows.Any(r => Matches(r, filter));

	private static bool Matches(RelatedCardSettingRow row, string filter)
	{
		if(string.IsNullOrWhiteSpace(filter))
			return true;

		return row.DisplayName.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0
			|| row.CardIdsText.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
	}

	public Visibility NoResultsVisibility => Rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

	public bool HasAnyOverride => RelatedCardVisibilitySettings.Instance.HasAnyOverride;

	public ICommand ResetAllCommand => new Command(() =>
	{
		RelatedCardVisibilitySettings.Instance.ResetAll();
		Load();
	});

	public event PropertyChangedEventHandler? PropertyChanged;

	[NotifyPropertyChangedInvocator]
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class RelatedCardSettingRow : INotifyPropertyChanged
{
	public RelatedCardSettingRow(RelatedCardDescriptor descriptor)
	{
		Descriptor = descriptor;
	}

	public RelatedCardDescriptor Descriptor { get; }

	public string DisplayName => Descriptor.DisplayName;
	public string CardId => Descriptor.CardId;

	/// <summary>All ids of the card, so the tooltip and the filter still find it by any of them.</summary>
	public string CardIdsText => Descriptor.CardIdsText;
	public CardAssetViewModel CardAsset => Descriptor.CardAsset;

	public CounterVisibility OpponentVisibility
	{
		get => RelatedCardVisibilitySettings.Instance.GetOpponent(CardId);
		set
		{
			if(OpponentVisibility == value)
				return;
			RelatedCardVisibilitySettings.Instance.SetOpponent(CardId, value);
			OnPropertyChanged();
		}
	}

	public void Refresh()
	{
		OnPropertyChanged(nameof(DisplayName));
		OnPropertyChanged(nameof(CardAsset));
		OnPropertyChanged(nameof(OpponentVisibility));
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	[NotifyPropertyChangedInvocator]
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
