using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.RelatedCardsPanel;

public class RelatedCardsPanelViewModel : ViewModel
{
	public RelatedCardsPanelViewModel()
	{
		// CanUseFilters depends on OutfinderTrial.HasAccess, which can flip at any time
		// (login/logout, account data refresh, a trial being activated) independent of
		// the Cards property. Without this, the branding row and filter section would
		// only refresh premium state the next time a new card pool is loaded into an
		// already-open panel.
		HSReplayNetOAuth.Authenticated += RaiseEntitlementChanged;
		HSReplayNetOAuth.LoggedOut += RaiseEntitlementChanged;
		HSReplayNetOAuth.AccountDataUpdated += RaiseEntitlementChanged;
		OutfinderTrial.OnTrialActivated += RaiseEntitlementChanged;
		DisplayModeChanged += OnDisplayModeChanged;
	}

	private static event Action? DisplayModeChanged;

	public static void RaiseDisplayModeChanged() => DisplayModeChanged?.Invoke();

	private void OnDisplayModeChanged()
	{
		OnPropertyChanged(nameof(UseCardTiles));
		OnPropertyChanged(nameof(ShowCardGrid));
	}

	public bool UseCardTiles => Config.Instance.OutfinderUseCardTiles;

	public bool ShowCardGrid => !UseCardTiles;

	private void RaiseEntitlementChanged()
	{
		OnPropertyChanged(nameof(CanUseFilters));
		OnPropertyChanged(nameof(ShowFilterBar));
		OnPropertyChanged(nameof(ShowFilterDrawer));
	}

	public string? CardName
	{
		get => GetProp<string?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(HeaderText));
		}
	}

	[LocalizedProp]
	public string HeaderText => CardName != null
		? string.Format(LocUtil.Get("TheOutfinder_Label_PoolHeader"), CardName, Cards?.Count ?? 0)
		: LocUtil.Get("TheOutfinder_Label_DefaultHeader");

	private List<List<RelatedCardItem>>? _cardRows;

	public List<Hearthstone.Card>? Cards
	{
		get => GetProp<List<Hearthstone.Card>?>(null);
		set
		{
			SetProp(value);
			_cardRows = BuildCardRows(value);
			InvalidateFilteredItems();
			OnPropertyChanged(nameof(CardRows));
			OnPropertyChanged(nameof(HeaderText));
			RebuildKeywordFilters();
			RebuildCostFilters();
			OnPropertyChanged(nameof(FilteredCardRows));
			OnPropertyChanged(nameof(FilteredCards));
			OnPropertyChanged(nameof(FilterMatchText));
		}
	}

	private static List<List<RelatedCardItem>>? BuildCardRows(List<Hearthstone.Card>? cards) =>
		cards?
			.Select((c, i) => (Item: new RelatedCardItem(c), Row: i / 3))
			.GroupBy(x => x.Row)
			.Select(g => g.Select(x => x.Item).ToList())
			.ToList();

	public List<List<RelatedCardItem>>? CardRows => _cardRows;

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

	// --- Keyword filter ---

	public List<KeywordFilterButton>? KeywordFilters
	{
		get => GetProp<List<KeywordFilterButton>?>(null);
		private set
		{
			SetProp(value);
			OnPropertyChanged(nameof(HasAnyFilter));
			OnPropertyChanged(nameof(ShowFilterBar));
			OnPropertyChanged(nameof(ShowFilterDrawer));
		}
	}

	public string? ActiveKeyword
	{
		get => GetProp<string?>(null);
		private set
		{
			SetProp(value);
			InvalidateFilteredItems();
			OnPropertyChanged(nameof(FilteredCardRows));
			OnPropertyChanged(nameof(FilteredCards));
			OnPropertyChanged(nameof(FilterMatchText));
			OnPropertyChanged(nameof(HasActiveFilter));
		}
	}

	private ICommand? _selectKeywordCommand;
	public ICommand SelectKeywordCommand => _selectKeywordCommand ??= new Command<string>(keyword =>
	{
		var deactivating = ActiveKeyword == keyword;
		if(KeywordFilters != null)
			foreach(var btn in KeywordFilters)
				btn.IsActive = false;
		ActiveKeyword = deactivating ? null : keyword;
		if(!deactivating)
		{
			var active = KeywordFilters?.FirstOrDefault(b => b.KeywordName == keyword);
			if(active != null)
				active.IsActive = true;
		}
	});

	// --- Cost filter ---

	private readonly HashSet<int> _activeCosts = new();

	public List<CostFilterButton>? CostFilters
	{
		get => GetProp<List<CostFilterButton>?>(null);
		private set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ShowCostFilters));
			OnPropertyChanged(nameof(HasAnyFilter));
			OnPropertyChanged(nameof(ShowFilterBar));
			OnPropertyChanged(nameof(ShowFilterDrawer));
		}
	}

	public bool ShowCostFilters => (CostFilters?.Count ?? 0) > 2;

	private ICommand? _selectCostCommand;
	public ICommand SelectCostCommand => _selectCostCommand ??= new Command<int>(cost =>
	{
		var btn = CostFilters?.FirstOrDefault(b => b.Cost == cost);
		if(btn == null) return;

		if(_activeCosts.Contains(cost))
		{
			_activeCosts.Remove(cost);
			btn.IsActive = false;
		}
		else
		{
			_activeCosts.Add(cost);
			btn.IsActive = true;
		}

		InvalidateFilteredItems();
		OnPropertyChanged(nameof(FilteredCardRows));
		OnPropertyChanged(nameof(FilteredCards));
		OnPropertyChanged(nameof(FilterMatchText));
		OnPropertyChanged(nameof(HasActiveFilter));
	});

	public bool HasActiveFilter => ActiveKeyword != null || _activeCosts.Count > 0;

	public bool HasAnyFilter => KeywordFilters != null || ShowCostFilters;

	public bool CanUseFilters => OutfinderTrial.HasAccess;

	public bool ShowFilterBar => CanUseFilters && HasAnyFilter;

	public bool ShowFilterDrawer => CanUseFilters && IsFilterOpen;

	private List<RelatedCardItem>? _filteredItemsCache;
	private bool _filteredItemsDirty = true;

	private void InvalidateFilteredItems() => _filteredItemsDirty = true;

	private List<RelatedCardItem>? FilteredItems
	{
		get
		{
			if(_filteredItemsDirty)
			{
				_filteredItemsCache = ComputeFilteredItems();
				_filteredItemsDirty = false;
			}
			return _filteredItemsCache;
		}
	}

	private List<RelatedCardItem>? ComputeFilteredItems()
	{
		if(_cardRows == null)
			return null;

		var hasKeyword = ActiveKeyword != null;
		var hasCost = _activeCosts.Count > 0;

		var items = _cardRows.SelectMany(row => row);

		if(!hasKeyword && !hasCost)
			return items.ToList();

		HashSet<string>? matchIds = null;
		if(hasKeyword)
			RelatedCardsManager.RelatedCardsSummaryKeywords?.TryGetValue(ActiveKeyword!, out matchIds);

		return items
			.Where(item =>
				(matchIds == null || matchIds.Contains(item.Card.Id)) &&
				(!hasCost || _activeCosts.Contains(item.Card.Cost)))
			.ToList();
	}

	public List<RelatedCardItem>? FilteredCards => FilteredItems;

	public List<List<RelatedCardItem>>? FilteredCardRows
	{
		get
		{
			if(_cardRows == null)
				return null;

			if(!HasActiveFilter)
				return _cardRows;

			var filtered = FilteredItems!
				.Select((item, i) => (Item: item, Row: i / 3))
				.GroupBy(x => x.Row)
				.Select(g => g.Select(x => x.Item).ToList())
				.ToList();

			return filtered.Count > 0 ? filtered : null;
		}
	}

	public string FilterMatchText => $"{FilteredItems?.Count ?? 0}/{Cards?.Count ?? 0}";

	private void RebuildKeywordFilters()
	{
		ActiveKeyword = null;
		IsFilterOpen = false;

		var keywords = RelatedCardsManager.RelatedCardsSummaryKeywords;
		if(Cards == null || keywords == null)
		{
			KeywordFilters = null;
			return;
		}

		var cardIdSet = new HashSet<string>(Cards.Select(c => c.Id));
		var filters = keywords
			.Select(kvp => new
			{
				Keyword = kvp.Key,
				MatchCount = kvp.Value.Count(id => cardIdSet.Contains(id))
			})
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
		InvalidateFilteredItems();
		OnPropertyChanged(nameof(HasActiveFilter));

		if(Cards == null || Cards.Count == 0)
		{
			CostFilters = null;
			return;
		}

		CostFilters = Cards
			.Select(c => c.Cost)
			.Distinct()
			.OrderBy(c => c)
			.Select(c => new CostFilterButton(c))
			.ToList();
	}

	public event Action? OnClose;

	private ICommand? _closeCommand;
	public ICommand CloseCommand => _closeCommand ??= new Command(() => OnClose?.Invoke());
}
