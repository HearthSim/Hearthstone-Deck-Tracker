using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.RelatedCardsPanel;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Assemble;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.AssemblePanel;

/// <summary>
/// The Assemble panel, opened by right-clicking an Assemble card: the middle cards it can offer, and
/// once one is clicked, the options offered with it. Like the Outfinder panel, premium adds keyword
/// and cost filters, which apply to whichever cards are showing.
/// </summary>
public class AssemblePanelViewModel : ViewModel
{
	private const int CardsPerRow = 3;

	public AssemblePanelViewModel()
	{
		// See RelatedCardsPanelViewModel: entitlement can change while the panel is open.
		HSReplayNetOAuth.Authenticated += RaiseEntitlementChanged;
		HSReplayNetOAuth.LoggedOut += RaiseEntitlementChanged;
		HSReplayNetOAuth.AccountDataUpdated += RaiseEntitlementChanged;
		OutfinderTrial.OnTrialActivated += RaiseEntitlementChanged;
	}

	private void RaiseEntitlementChanged()
	{
		OnPropertyChanged(nameof(CanUseFilters));
		OnPropertyChanged(nameof(ShowFilterBar));
		OnPropertyChanged(nameof(ShowFilterDrawer));
	}

	private List<AssembleCardItem> _items = new();

	public AssemblePool? Pool
	{
		get => GetProp<AssemblePool?>(null);
		set
		{
			SetProp(value);
			SelectedMiddle = null;
		}
	}

	/// <summary>
	/// The middle card whose options are showing, or null while the middle cards are.
	/// </summary>
	public AssembleMiddleCard? SelectedMiddle
	{
		get => GetProp<AssembleMiddleCard?>(null);
		private set
		{
			SetProp(value);
			_items = value != null
				? value.Options.Select(option => (AssembleCardItem)new AssembleOptionItem(option, value.Card)).ToList()
				: Pool?.MiddleCards.Select(middle => (AssembleCardItem)new AssembleMiddleItem(middle)).ToList()
				  ?? new List<AssembleCardItem>();
			OnPropertyChanged(nameof(IsShowingOptions));
			OnPropertyChanged(nameof(HeaderText));
			OnPropertyChanged(nameof(SubheaderText));
			RebuildKeywordFilters();
			RebuildCostFilters();
			OnFilteredItemsChanged();
		}
	}

	public bool IsShowingOptions => SelectedMiddle != null;

	[LocalizedProp]
	public string HeaderText => Pool == null ? ""
		: SelectedMiddle == null
			? string.Format(LocUtil.Get("TheOutfinder_Label_AssembleMiddlePool"), Pool.AssembleCard.LocalizedName, Pool.MiddleCards.Count)
			: $"{Pool.AssembleCard.LocalizedName} → {SelectedMiddle.Card.LocalizedName}";

	[LocalizedProp]
	public string SubheaderText => SelectedMiddle == null
		? LocUtil.Get("TheOutfinder_Label_AssembleMiddleHint")
		: string.Format(LocUtil.Get("TheOutfinder_Label_AssembleOptions"), SelectedMiddle.Options.Count);

	private ICommand? _selectMiddleCommand;
	public ICommand SelectMiddleCommand => _selectMiddleCommand ??= new Command<AssembleCardItem>(item =>
	{
		if(item is AssembleMiddleItem middle)
			SelectedMiddle = middle.Middle;
	});

	private ICommand? _backCommand;
	public ICommand BackCommand => _backCommand ??= new Command(() => SelectedMiddle = null);

	public event Action? OnClose;

	private ICommand? _closeCommand;
	public ICommand CloseCommand => _closeCommand ??= new Command(() => OnClose?.Invoke());

	// --- Filters, as in RelatedCardsPanelViewModel ---

	public bool CanUseFilters => OutfinderTrial.HasAccess;

	public bool IsFilterOpen
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ShowFilterDrawer));
		}
	}

	private ICommand? _toggleFilterCommand;
	public ICommand ToggleFilterCommand => _toggleFilterCommand ??= new Command(() => IsFilterOpen = !IsFilterOpen);

	public List<KeywordFilterButton>? KeywordFilters
	{
		get => GetProp<List<KeywordFilterButton>?>(null);
		private set
		{
			SetProp(value);
			OnFilterSetChanged();
		}
	}

	public List<CostFilterButton>? CostFilters
	{
		get => GetProp<List<CostFilterButton>?>(null);
		private set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ShowCostFilters));
			OnFilterSetChanged();
		}
	}

	private void OnFilterSetChanged()
	{
		OnPropertyChanged(nameof(HasAnyFilter));
		OnPropertyChanged(nameof(ShowFilterBar));
		OnPropertyChanged(nameof(ShowFilterDrawer));
	}

	public bool ShowCostFilters => (CostFilters?.Count ?? 0) > 2;

	public bool HasAnyFilter => KeywordFilters != null || ShowCostFilters;

	public bool ShowFilterBar => CanUseFilters && HasAnyFilter;

	public bool ShowFilterDrawer => CanUseFilters && IsFilterOpen && HasAnyFilter;

	private string? _activeKeyword;
	private readonly HashSet<int> _activeCosts = new();

	public bool HasActiveFilter => _activeKeyword != null || _activeCosts.Count > 0;

	private ICommand? _selectKeywordCommand;
	public ICommand SelectKeywordCommand => _selectKeywordCommand ??= new Command<string>(keyword =>
	{
		_activeKeyword = _activeKeyword == keyword ? null : keyword;
		foreach(var button in KeywordFilters ?? new List<KeywordFilterButton>())
			button.IsActive = button.KeywordName == _activeKeyword;
		OnFilteredItemsChanged();
	});

	private ICommand? _selectCostCommand;
	public ICommand SelectCostCommand => _selectCostCommand ??= new Command<int>(cost =>
	{
		var button = CostFilters?.FirstOrDefault(b => b.Cost == cost);
		if(button == null)
			return;
		if(_activeCosts.Contains(cost))
			_activeCosts.Remove(cost);
		else
			_activeCosts.Add(cost);
		button.IsActive = _activeCosts.Contains(cost);
		OnFilteredItemsChanged();
	});

	private void RebuildKeywordFilters()
	{
		_activeKeyword = null;
		IsFilterOpen = false;

		var keywords = RelatedCardsManager.RelatedCardsSummaryKeywords;
		if(keywords == null || _items.Count == 0)
		{
			KeywordFilters = null;
			return;
		}

		var cardIds = new HashSet<string>(_items.Select(item => item.Card.Id));
		var filters = keywords
			.Select(kvp => (Keyword: kvp.Key, MatchCount: kvp.Value.Count(cardIds.Contains)))
			.Where(x => x.MatchCount > 0)
			.OrderByDescending(x => x.MatchCount)
			.ThenBy(x => x.Keyword)
			.Select(x => new KeywordFilterButton(x.Keyword, RelatedCardsManager.LocalizeKeywordName(x.Keyword), x.MatchCount))
			.ToList();
		KeywordFilters = filters.Count > 0 ? filters : null;
	}

	private void RebuildCostFilters()
	{
		_activeCosts.Clear();
		CostFilters = _items.Count == 0 ? null : _items
			.Select(item => item.Card.Cost)
			.Distinct()
			.OrderBy(cost => cost)
			.Select(cost => new CostFilterButton(cost))
			.ToList();
	}

	private List<AssembleCardItem> FilteredItems
	{
		get
		{
			if(!HasActiveFilter)
				return _items;
			HashSet<string>? matchIds = null;
			if(_activeKeyword != null)
				RelatedCardsManager.RelatedCardsSummaryKeywords?.TryGetValue(_activeKeyword, out matchIds);
			return _items
				.Where(item => (matchIds == null || matchIds.Contains(item.Card.Id))
				               && (_activeCosts.Count == 0 || _activeCosts.Contains(item.Card.Cost)))
				.ToList();
		}
	}

	/// <summary>
	/// The cards showing, in rows, so the list can virtualize by row.
	/// </summary>
	public List<List<AssembleCardItem>> FilteredCardRows
	{
		get => GetProp<List<List<AssembleCardItem>>?>(null) ?? new List<List<AssembleCardItem>>();
		private set => SetProp(value);
	}

	public string FilterMatchText
	{
		get => GetProp("") ?? "";
		private set => SetProp(value);
	}

	private void OnFilteredItemsChanged()
	{
		var items = FilteredItems;
		FilteredCardRows = items
			.Select((item, i) => (Item: item, Row: i / CardsPerRow))
			.GroupBy(x => x.Row)
			.Select(row => row.Select(x => x.Item).ToList())
			.ToList();
		FilterMatchText = $"{items.Count}/{_items.Count}";
		OnPropertyChanged(nameof(HasActiveFilter));
	}
}
