using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using BobsBuddy.Utils;
using HearthDb.Enums;
using HearthMirror;
using HearthMirror.Enums;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using HSReplay.Requests;
using Newtonsoft.Json;
using Point = System.Drawing.Point;


namespace Hearthstone_Deck_Tracker.Controls.Overlay.Arena;

[Flags]
public enum Synergy
{
	None     = 0,
	Receives = 1,
	Provides = 2,
	Both = Receives | Provides,
}

public enum ScreenBehavior
{
	None,
	SlideIn,
	SlideOut,
	FadeIn,
	FadeOut
}

public class DeckListTileViewModel : ViewModel
{
	public string CardId { get; }
	public int Count { get; }
	public string? CardName => new Hearthstone.Card(CardId).LocalizedName;
	public int DbfId  => new Hearthstone.Card(CardId).DbfId;

	public DeckListTileViewModel(string cardId, int count)
	{
		CardId = cardId;
		Count = count;
	}

	public Synergy Synergy
	{
		get => GetProp(Synergy.None);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ShowSynergy));
			OnPropertyChanged(nameof(ReceivesSynergy));
			OnPropertyChanged(nameof(ProvidesSynergy));
			OnPropertyChanged(nameof(BothSynergy));
			OnPropertyChanged(nameof(HasTooltip));
		}
	}

	public bool ShowSynergy => Synergy != Synergy.None;

	public bool ReceivesSynergy => Synergy == Synergy.Receives;
	public bool ProvidesSynergy => Synergy == Synergy.Provides;
	public bool BothSynergy => Synergy == Synergy.Both;

	public ArenaState.ActorInfo? HoveredChoiceActor { get; set; }
	public string? HoveredChoiceCardName => new Hearthstone.Card(HoveredChoiceActor?.CardId ?? "").LocalizedName;

	public IEnumerable<Inline> TooltipText
	{
		get
		{
			if(SuggestRemove)
			{
				yield return new Run(LocUtil.Get("ArenaRedraft_SuggestedDiscard"));
				yield break;
			}

			if(Synergy == Synergy.None)
				yield break;

			IEnumerable<Inline> Parse(string str, string? firstCard, string? secondCard)
			{
				var firstIndex = str.IndexOf("{0}", StringComparison.Ordinal);
				var secondIndex = str.IndexOf("{1}", StringComparison.Ordinal);
				yield return new Run(str.Substring(0, firstIndex));
				yield return new Run(firstCard) { FontWeight = FontWeights.Bold };
				yield return new Run(str.Substring(firstIndex + 3, secondIndex - (firstIndex + 3)));
				yield return new Run(secondCard) { FontWeight = FontWeights.Bold };
				yield return new Run(str.Substring(secondIndex + 3));
			}

			if((Synergy & Synergy.Receives) > 0)
			{
				foreach(var inline in Parse(LocUtil.Get("ArenaPick_SynergyImprove"), HoveredChoiceCardName, CardName))
					yield return inline;
			}

			if(Synergy == Synergy.Both)
				yield return new LineBreak();

			if((Synergy & Synergy.Provides) > 0)
			{
				foreach(var inline in Parse(LocUtil.Get("ArenaPick_SynergyImprove"), CardName, HoveredChoiceCardName))
					yield return inline;
			}
		}
	}

	public bool HasTooltip => (Config.Instance.ShowArenaDeckSynergies && Synergy != Synergy.None) ||
	                          (Config.Instance.ShowArenaRedraftDiscard && SuggestRemove);

	public double? ArensmithScore
	{
		get => GetProp<double?>(null);
		set => SetProp(value);
	}

	public Visibility ArensmithScoreVisibility
	{
		get => GetProp(Visibility.Collapsed);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(HasTooltip));
		}
	}

	public bool SuggestRemove
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(HasTooltip));
		}
	}
}

public class ArenaPickHelperViewModel : ViewModel
{
	public ArenaPickHelperViewModel()
	{
		Watchers.ArenaStateWatcher.OnCardHover += UpdateHoveredActor;
		Watchers.ArenaStateWatcher.OnScrollChange += UpdateScroll;
		Watchers.ArenaStateWatcher.OnDeckListChange += UpdateDeckList;
		Watchers.ArenaStateWatcher.OnRedraftDeckListChange += UpdateRedraftDeckList;
		Watchers.ArenaStateWatcher.OnClientStateChanged += UpdateClientState;
		Watchers.ArenaStateWatcher.OnIsAnimatingChanged += UpdateAnimating;
		Watchers.ArenaStateWatcher.OnIsPackageSelectOpen += UpdatePackageSelect;
		Watchers.ArenaStateWatcher.OnIsUndergroundChanged += UpdateUnderground;
		Watchers.ArenaStateWatcher.OnArenaSeasonIdChanged += UpdateArenaSeasonId;
		Watchers.ArenaStateWatcher.OnHeroZoomed += UpdateHeroZoomed;
		Watchers.ArenaStateWatcher.OnChoicesChanged += UpdateChoices;
		Watchers.ArenaStateWatcher.OnHeroPicked += UpdateHero;
		Watchers.ArenaStateWatcher.OnTrayBigCardChanged += UpdateTrayBigCard;
		HSReplayNetOAuth.LoggedOut += () => Reset(true);
		HSReplayNetOAuth.AccountDataUpdated += () =>
		{
			// re-trigger UpdateChoices if we're premium right now and have a pick, but no stats
			if(
				(HSReplayNetOAuth.AccountData?.IsPremium ?? false) && _choices != null && CardStats == null && HeroStats == null)
			{
				UpdateChoices(_choices);
			}
		};
	}

	private void UpdateUnderground(bool isUnderground)
	{
		IsUnderground = isUnderground;
	}

	private void UpdateArenaSeasonId(int arenaSeasonId)
	{
		ArenaSeasonId = arenaSeasonId;
	}

	private void UpdateTrayBigCard(ArenaState.BigCard? bigCard)
	{
	}

	private void UpdatePackageSelect(bool isPackageSelectOpen)
	{
		IsPackageSelectOpen = isPackageSelectOpen;
	}

	private string _chosenHero = "";
	private void UpdateHero(string chosenHero)
	{
		_chosenHero = chosenHero;
	}

	private List<ArenaState.DraftChoice>? _choices;
	private async void UpdateChoices(List<ArenaState.DraftChoice> choices)
	{

		if(SessionState is ArenaSessionState.EDITING_DECK)
		{
			Log.Warn("Receives choices while editing deck. Second arena run in progress?");
			return;
		}

		CardStats = null;
		Messages = null;
		HeroStats = null;
		_lastHoveredChoice = null;
		HoveredChoice = null;
		BottomPanelCards = null;
		_choices = choices;
		UpdateTileViewModels();

		var availabilities = await EnsureAvailabilities();
		if(IsUnderground && !availabilities.UndergroundArena)
		{
			Log.Info("Arenasmith is not available for Underground Arena, aborting");
			return;
		}
		else if(!IsUnderground && !availabilities.Arena)
		{
			Log.Info("Arenasmith is not available for Arena, aborting");
			return;
		}

		if(choices.Count == 0)
			return;

		var offered = choices.Select(x => x.Actor.CardId).ToArray();
		var accountId = Reflection.Client.GetAccountId();
		var arenaInfo = Reflection.Client.GetArenaDeck(); // TODO less expensive call?
		var deckId = arenaInfo?.Deck.Id;

		if(accountId == null || !deckId.HasValue)
			return;

		if(string.IsNullOrEmpty(_chosenHero))
		{
			await ArenaTrial.EnsureLoaded(accountId.Hi, accountId.Lo);
			var (starter, recurring) = ArenaTrial.RemainingTrials ?? (0, 0);

			var trialsActivated = false;
			var trialsRemaining = starter + recurring;
			var draftInfo = new ArenaDraftInfo
			{
				PlayerId = $"{accountId.Hi}_{accountId.Lo}",
				DeckId = deckId.Value,
				IsUnderground = IsUnderground
			};
			if(!HSReplayNetOAuth.IsFullyAuthenticated || !(HSReplayNetOAuth.AccountData?.IsPremium ?? false))
			{
				if(starter + recurring == 0)
				{
					Log.Info("No trials left for hero pick, aborting");
					HSReplayNetClientAnalytics.OnArenaRunStarts(draftInfo, arenaOverlayVisible: false, trialsActivated, trialsRemaining);
					return;
				}
				trialsActivated = true;
			}

			if(!Config.Instance.EnableArenasmithOverlay)
				return;

			if(PickedDeck == null)
				Reset();

			var card = new Hearthstone.Card(choices.First().Actor.CardId);
			if(card.TypeEnum != CardType.HERO)
				return;


			// Loading state
			HeroStats = offered.Select(_ => new ArenaPickSingleHeroOptionViewModel(IsUnderground)).ToList();

			var heroData = await MakeRequestHeroPick(offered, ArenaSeasonId, deckId.Value, accountId, IsUnderground);
			ArenaTrial.Clear();

			if(heroData == null)
			{
				ShowErrorMessage = true;
				return;
			}

			ShowErrorMessage = false;

			// @todo: fix this v
			// Api returns class ids in ascending order
			// we need to display them on the same position they were offered to the player
			var offeredClasses = choices.Select(x => Database.GetHeroNameFromId(x.Actor.CardId)).ToArray();
			var idToObjectMap =
				heroData.Data.ToDictionary(d => HearthDbConverter.ConvertClass((CardClass)d.DeckClass));

			var ordered = offeredClasses
				.Select(c => c != null && idToObjectMap.TryGetValue(c, out var obj) ? obj : null)
				.ToArray();

			HeroStats = ordered.Select(s => new ArenaPickSingleHeroOptionViewModel(s, IsUnderground)).ToList();
			HeroPickVisibility = Config.Instance.ShowArenaHeroPicking ? Visibility.Visible : Visibility.Collapsed;
			RelatedCardsVisibility = Config.Instance.ShowArenaRelatedCards ? Visibility.Visible : Visibility.Collapsed;

			HSReplayNetClientAnalytics.OnArenaRunStarts(draftInfo, Config.Instance.EnableArenasmithOverlay, trialsActivated, trialsRemaining);
			return;
		}

		if(!Config.Instance.EnableArenasmithOverlay)
			return;

		// Check if the deck is registered for trials
		if(!HSReplayNetOAuth.IsFullyAuthenticated || !(HSReplayNetOAuth.AccountData?.IsPremium ?? false))
		{
			await ArenaTrial.EnsureLoaded(accountId.Hi, accountId.Lo);
			if(!ArenaTrial.IsDeckResumable(deckId.Value))
			{
				Log.Info("Current deck is not registered for trials, aborting");
				return;
			}
		}

		// Loading state
		CardStats = offered.Select(cardId => new ArenaPickSingleCardOptionViewModel(cardId, IsUnderground)).ToList();
		ArenasmithScoreVisibility = Config.Instance.ShowArenasmithScore ? Visibility.Visible : Visibility.Collapsed;
		RelatedCardsVisibility = Config.Instance.ShowArenaRelatedCards ? Visibility.Visible : Visibility.Collapsed;

		Dictionary<string, ArenaCardPickApiResponse.CardStatsEntry>? pickData = null;
		if(!IsRedraft)
		{
			var picked = TileViewModels.SelectMany(x => Enumerable.Repeat(x.CardId, x.Count)).ToArray();
			// sometimes the game triggers OnChoicesChanged after the last pick, this avoids it
			if(picked.Length == 30)
				return;

			var data = await MakeRequestCardPick(offered,  _chosenHero, picked, ArenaSeasonId, deckId.Value, accountId, IsUnderground);

			pickData = data?.Data;
		}
		else
		{
			var picked = _pickedRedraftDeck?.ToArray() ?? new string[] {};
			var deckCards = PickedDeck?.ToArray() ?? new string[] {};
			var redraftNumber = arenaInfo?.Losses ?? 1;
			var data = await MakeRequestRedraftCardPick(offered,  _chosenHero, picked, deckCards, redraftNumber, ArenaSeasonId, deckId.Value, accountId, IsUnderground);

			pickData = data?.Data;

		}

		ShowErrorMessage = pickData == null;

		CardStats = offered.Select(cardId =>
				new ArenaPickSingleCardOptionViewModel(
					cardId,
					pickData?.TryGetValue(cardId, out var stats) ?? false ? stats : new ArenaCardPickApiResponse.CardStatsEntry(),
					PickedDeck,
					IsUnderground,
					apiError: pickData == null
				)
			).ToList();
	}

	public bool ShowErrorMessage
    {
    	get => GetProp(false);
    	set => SetProp(value);
	}

	private void UpdateAnimating(bool isAnimating)
	{
		IsAnimating = isAnimating;
	}

	private void UpdateHeroZoomed(ArenaState.ActorInfo? hero)
	{
		IsHeroZoomed = hero != null;
	}

	private void UpdateClientState((ArenaClientStateType ClientState, ArenaSessionState SessionState) state)
	{
		if(ClientState is ArenaClientStateType.Normal_Landing or ArenaClientStateType.Underground_Landing
		   && state.ClientState is ArenaClientStateType.Normal_Draft or ArenaClientStateType.Underground_Draft)
		{
			StateChange = ScreenBehavior.SlideIn;
		}
		else if(ClientState is ArenaClientStateType.Normal_Draft or ArenaClientStateType.Underground_Draft
		        && state.ClientState is ArenaClientStateType.Normal_Landing or ArenaClientStateType.Underground_Landing)
		{
			StateChange = ScreenBehavior.SlideOut;
		}
		else if(state.ClientState is ArenaClientStateType.Normal_Draft or ArenaClientStateType.Underground_Draft
		        or ArenaClientStateType.Normal_Redraft or ArenaClientStateType.Underground_Redraft
		        or ArenaClientStateType.Underground_DeckEdit or ArenaClientStateType.Normal_DeckEdit)
		{
			StateChange = ScreenBehavior.FadeIn;
		}
		else if(state.ClientState is ArenaClientStateType.Underground_Ready && state.SessionState is ArenaSessionState.EDITING_DECK)
		{
			StateChange = ScreenBehavior.FadeIn;
		}
		else
			StateChange = ScreenBehavior.FadeOut;

		ClientState = state.ClientState;
		SessionState = state.SessionState;
		IsDrafting = state.ClientState switch
		{
			ArenaClientStateType.Normal_Draft => true,
			ArenaClientStateType.Underground_Draft => true,
			ArenaClientStateType.Normal_Redraft => true,
			ArenaClientStateType.Underground_Redraft => true,
			_ => false,
		};
	}

	public bool IsHeroZoomed
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ShowStats));
		}
	}

	public bool IsAnimating
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ShowStats));
			OnPropertyChanged(nameof(ShowBottom));
		}
	}


	public (bool Arena, bool UndergroundArena)? ArenasmithAvailabilities
	{
		get => GetProp<(bool, bool)?>(null);
		set
		{
			SetProp(value);
		}
	}

	private async Task<(bool Arena, bool UndergroundArena)> EnsureAvailabilities()
	{
		if(Remote.Config.Data?.Arenasmith?.Disabled ?? false)
		{
			ArenasmithAvailabilities = (false, false);
			return ArenasmithAvailabilities.Value;
		}

		if(ArenasmithAvailabilities.HasValue)
			return ArenasmithAvailabilities.Value;

		var status = await ApiWrapper.GetArenasmithStatus();
		if(status == null)
			return (false, false);

		ArenasmithAvailabilities = (
			Arena: status.Data.TryGetValue(BnetGameType.BGT_ARENA.ToString(), out var a) && a.IsArenasmithAvailable,
			UndergroundArena: status.Data.TryGetValue(BnetGameType.BGT_UNDERGROUND_ARENA.ToString(), out var u) && u.IsArenasmithAvailable
		);
		return ArenasmithAvailabilities.Value;
	}

	public bool IsUnderground
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
		}
	}

	public int ArenaSeasonId
	{
		get => GetProp(0);
		set
		{
			SetProp(value);
		}
	}

	// ClientState is not reliable for this, as it sometimes uses Underground_Draft for redraft
	public bool IsRedraft => SessionState is ArenaSessionState.REDRAFTING or ArenaSessionState.MIDRUN_REDRAFT_PENDING;

	public bool IsEditingDeck => SessionState is ArenaSessionState.EDITING_DECK;

	public bool IsPackageSelectOpen
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ShowStats));
			OnPropertyChanged(nameof(ShowBottom));
		}
	}

	public bool ShowStats => !IsAnimating && !IsHeroZoomed && !IsPackageSelectOpen && (CardStats != null || HeroStats != null);

	public ScreenBehavior StateChange
	{
		get => GetProp(ScreenBehavior.None);
		set => SetProp(value);
	}


	public ArenaClientStateType ClientState
	{
		get => GetProp(ArenaClientStateType.None);
		set => SetProp(value);
	}

	public ArenaSessionState SessionState
	{
		get => GetProp(ArenaSessionState.INVALID);
		set => SetProp(value);
	}

	public bool IsDrafting
	{
		get => GetProp(false);
		set => SetProp(value);
	}

	private readonly List<DeckListTileViewModel> _noTiles = new();
	public List<DeckListTileViewModel> TileViewModels
	{
		get => GetProp(_noTiles)!;
		set => SetProp(value);
	}

	public List<DeckListTileViewModel> RedraftTileViewModels
	{
		get => GetProp(_noTiles)!;
		set => SetProp(value);
	}

	public bool HoveringBottomDirectionTrigger
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ShowBottom));
			OnPropertyChanged(nameof(EnableBottomDirectionTrigger));
			UpdateTileViewModels();
			UpdateCardListDirectionTriggers();
		}
	}

	public bool HoveringPanel
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ShowBottom));
			UpdateTileViewModels();
			UpdateCardListDirectionTriggers();
		}
	}

	private readonly HashSet<int> _hoveringCardListDirection = new();

	public void HoveringCardListDirection(int index, bool hovering)
	{
		if(hovering)
			_hoveringCardListDirection.Add(index);
		else
			_hoveringCardListDirection.Remove(index);
		UpdateTileViewModels();
		UpdateCardListDirectionTriggers();
		OnPropertyChanged(nameof(EnableCardListTrigger));
	}

	public bool HoveringCardList
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			UpdateTileViewModels();
			OnPropertyChanged(nameof(EnableCardListTrigger));
		}
	}

	public bool EnableCardListDirectionTrigger0 => ShouldEnableCardListDirectionTrigger(0);
	public bool EnableCardListDirectionTrigger1 => ShouldEnableCardListDirectionTrigger(1);
	public bool EnableCardListDirectionTrigger2 => ShouldEnableCardListDirectionTrigger(2);

	private bool ShouldEnableCardListDirectionTrigger(int index)
	{
		return HoveredChoice?.Actor.Index == index
		       || _hoveringCardListDirection.Contains(index) && _hoveringCardListDirection.Count == 1;
	}

	private void UpdateCardListDirectionTriggers()
	{
		OnPropertyChanged(nameof(EnableCardListDirectionTrigger0));
		OnPropertyChanged(nameof(EnableCardListDirectionTrigger1));
		OnPropertyChanged(nameof(EnableCardListDirectionTrigger2));
	}

	public bool EnableCardListTrigger => HoveredChoice != null || _hoveringCardListDirection.Any() || HoveringCardList;

	private void UpdateDeckList(List<HearthMirror.Objects.Card> cardIds)
	{
		// This happens before we get the next choices.
		// To avoid flickering of various elements related
		// to the hovered choice we clear those here.
		HoveredChoice = null;
		_lastHoveredChoice = null;

		PickedDeck = cardIds.SelectMany(x => Enumerable.Repeat(x.Id, x.Count)).ToList();
		TileViewModels = cardIds.Select(x => new DeckListTileViewModel(x.Id, x.Count)).ToList();
		UpdateTileViewModels();

		// Number of tiles may have changed, update scroll position
		UpdateScroll(_scrollValue);

		if(SessionState is ArenaSessionState.EDITING_DECK)
		{
			var deckClass = (int?)Database.GetCardFromId(_chosenHero)?.CardClass ?? 0;
			OnDeckEditing(deckClass);
		}
	}

	private void UpdateRedraftDeckList(List<HearthMirror.Objects.Card> cardIds)
	{
		// This happens before we get the next choices.
		// To avoid flickering of various elements related
		// to the hovered choice we clear those here.
		HoveredChoice = null;
		_lastHoveredChoice = null;

		_pickedRedraftDeck = cardIds.SelectMany(x => Enumerable.Repeat(x.Id, x.Count)).ToList();
		RedraftTileViewModels = cardIds.Select(x => new DeckListTileViewModel(x.Id, x.Count)).ToList();
		SynergiesVisibility = Config.Instance.ShowArenaDeckSynergies ? Visibility.Visible : Visibility.Collapsed;
		UpdateTileViewModels();

		// Number of tiles may have changed, update scroll position
		UpdateScroll(_scrollValue);
	}

	private void UpdateTileViewModels()
	{
		var choice = HoveredChoice;
		if(choice == null && _lastHoveredChoice != null && _choices?.Count > 0 && (_hoveringCardListDirection.Any() || HoveringCardList || HoveringBottomDirectionTrigger || HoveringPanel))
			choice = _lastHoveredChoice;
		var activeCard = CardStats?.ElementAtOrDefault(choice?.Actor.Index ?? -1)
			?.CardStats;

		var directEnhanced = activeCard?.RelatedCards.EnhancedByCardIds?.Direct ?? new List<string>();
		var indirectEnhanced = activeCard?.RelatedCards.EnhancedByCardIds?.Indirect ?? new List<string>();
		var enhancedByCardsIds = directEnhanced.Concat(indirectEnhanced).ToList();

		var directEnabled = activeCard?.RelatedCards.CardIdsEnabled?.Direct ?? new List<string>();
		var indirectEnabled = activeCard?.RelatedCards.CardIdsEnabled?.Indirect ?? new List<string>();
		var enabledCardsIds = directEnabled.Concat(indirectEnabled).ToList();

		foreach(var tile in TileViewModels)
		{
			tile.HoveredChoiceActor = choice?.Actor;
			tile.Synergy = Synergy.None;
			if(enhancedByCardsIds?.Contains(tile.CardId) ?? false)
				tile.Synergy |= Synergy.Provides;
			if(enabledCardsIds?.Contains(tile.CardId) ?? false)
				tile.Synergy |= Synergy.Receives;
		}

		foreach(var tile in RedraftTileViewModels)
		{
			tile.HoveredChoiceActor = choice?.Actor;
			tile.Synergy = Synergy.None;
			if(enhancedByCardsIds?.Contains(tile.CardId) ?? false)
				tile.Synergy |= Synergy.Provides;
			if(enabledCardsIds?.Contains(tile.CardId) ?? false)
				tile.Synergy |= Synergy.Receives;
		}
	}

	private readonly Dictionary<int, ArenaCardStats?> _arenaCardStatsCache = new();

	public async void OnDeckEditing(int classNumber)
	{
		if(!Config.Instance.EnableArenasmithOverlay)
			return;
		CardStats = null;
		Messages = null;
		if(!_arenaCardStatsCache.TryGetValue(classNumber, out var stats))
		{
			 // @todo: refactor this. We call this in a few places, but should probably not call it at all.
			var arenaInfo = Reflection.Client.GetArenaDeck();
			var accountId = Reflection.Client.GetAccountId();
			var redraftNumber = arenaInfo?.Losses ?? 1;
			var deckId = arenaInfo?.Deck.Id;

			if(accountId == null || !deckId.HasValue)
				return;

			// Check if the deck is registered for trials
			if(!HSReplayNetOAuth.IsFullyAuthenticated || !(HSReplayNetOAuth.AccountData?.IsPremium ?? false))
			{
				await ArenaTrial.EnsureLoaded(accountId.Hi, accountId.Lo);
				if(!ArenaTrial.IsDeckResumable(deckId.Value))
				{
					Log.Info("Current deck is not registered for trials, aborting");
					return;
				}
			}

			stats = await MakeRequestEditDeck(_chosenHero, redraftNumber, ArenaSeasonId, arenaInfo?.Deck.Id ?? -1, accountId, IsUnderground);
			 _arenaCardStatsCache.Add(classNumber, stats);
		}

		RedraftDiscardVisibility = Config.Instance.ShowArenaRedraftDiscard ? Visibility.Visible : Visibility.Collapsed;

		foreach(var tile in TileViewModels)
		{
			if(stats != null && stats.Data.TryGetValue(tile.CardId, out var entry) && entry.Score != null)
			{
				tile.ArensmithScore = Math.Round(entry.Score.Value);
				tile.ArensmithScoreVisibility = RedraftDiscardVisibility;
			}
		}

		var floorIndex = Math.Max(0, TileViewModels.Sum(x => x.Count) - 30) - 1;
		var scoreFloor = TileViewModels
			.Where(x => x.ArensmithScore != null)
			.SelectMany(x => Enumerable.Repeat(x.ArensmithScore, x.Count))
			.OrderBy(x => x)
			.ElementAtOrDefault(floorIndex);

		foreach(var tile in TileViewModels)
		{
			if(tile.ArensmithScore <= scoreFloor)
				tile.SuggestRemove = true;
		}

		UpdateScroll(_scrollValue);
	}

	private ArenaState.DraftChoice? _lastHoveredChoice;
	public ArenaState.DraftChoice? HoveredChoice
	{
		get => GetProp<ArenaState.DraftChoice?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ShowBottom));
			UpdateCardListDirectionTriggers();
			OnPropertyChanged(nameof(EnableCardListTrigger));
			OnPropertyChanged(nameof(EnableBottomDirectionTrigger));
		}
	}

	private readonly CardAssetViewModel _defaultCardAsset = new CardAssetViewModel(null, CardAssetType.Tile);
	public CardAssetViewModel HoveredAsset
	{
		get => GetProp(_defaultCardAsset)!;
		set => SetProp(value);
	}

	private readonly Thickness _zero = new();
	public Thickness ScrollOffset
	{
		get => GetProp(_zero);
		set => SetProp(value);
	}

	public Thickness RedraftScrollOffset
	{
		get => GetProp(_zero);
		set => SetProp(value);
	}

	private const double ScrollBottomPadding = 0.1275;
	private const double VisibleScrollItems = 21.5;
	private const double ScrollItemHeight = 40.75;
	private const double RedraftPanelHeightPx = 251.0;
	private const double RedraftPanelHeaderHeightPx = 38.0;
	private double _scrollValue;
	public void UpdateScroll(double scrollValue)
	{
		_scrollValue = scrollValue;

		var totalHeightInItems = TileViewModels.Count + ScrollBottomPadding;
		if(IsRedraft)
			totalHeightInItems += RedraftPanelHeightPx / ScrollItemHeight;

		var overflow = ScrollItemHeight * Math.Max(0, totalHeightInItems - VisibleScrollItems);
		var scrollOffset = overflow * -_scrollValue;

		var deckOffset = IsRedraft ? scrollOffset + RedraftPanelHeightPx : scrollOffset;
		ScrollOffset = new Thickness(0, deckOffset, 0, 0);

		var redraftOffset = scrollOffset + RedraftPanelHeaderHeightPx;
		RedraftScrollOffset = new Thickness(0, redraftOffset, 0, 0);
	}

	public ImageSource? PanelBackground
	{
		get => GetProp<ImageSource?>(null);
		set => SetProp(value);
	}

	public async void UpdateHoveredActor(ArenaState.DraftChoice? choice)
	{
		var task = Debounce.WasCalledAgain(250);
		HoveredChoice = null;
		UpdateTileViewModels();
		if(choice != null)
		{
			if(await task)
				return;

			var img = await Task.Run(() =>
			{
				var hwnd = User32.GetHearthstoneWindow();
				var clientRect = new User32.Rect();
				User32.GetClientRect(hwnd, ref clientRect);
				var clientWidth = clientRect.right - clientRect.left;
				var clientHeight = clientRect.bottom - clientRect.top;


				var innerWidth = clientHeight * 4.0 / 3.0;
				var offset = (clientWidth - innerWidth) * 0.5;

				var left = offset + 60.0 / 1440 * innerWidth;
				var width = (1440 * (3.25 / 4.25) - 60 - 60) / 1440 * innerWidth;

				var top = (1080.0 - 99 - 312) / 1080 * clientHeight;
				var height = 312.0 / 1080 * clientHeight;

				var bmp = ScreenCapture.CaptureWindow(hwnd, new Point((int)Math.Round(left) + 1, (int)Math.Round(top) + 1), (int)Math.Round(width) - 1, (int)Math.Round(height) - 1);

				return new GaussianBlur(bmp).Process((int)(bmp.Height * 0.005));
			});

			PanelBackground = img.ToImageSource();

			HoveredChoice = choice; // No await after this to avoid panel appearing too early, with old cards

			var card = new Hearthstone.Card(choice.Actor.CardId);
			if(card.TypeEnum == CardType.HERO)
			{

				var heroStats = HeroStats?.ElementAtOrDefault(choice.Actor.Index);
				var className = LocUtil.Get(
					Database.CardClassName.TryGetValue((CardClass)(heroStats?.Data?.DeckClass ?? 0), out var c) ? c
						: "");
				BottomPanelCards = heroStats?.Data?.ClassDeckSignature.Data.Keys
					.Select(x => new Hearthstone.Card(x)).ToList() ?? new List<Hearthstone.Card>();
				BottomPanelTitle = $"{className} – {LocUtil.Get("ArenaPick_ClassTopCards")}";
				IsRelatedCardsSorted = false;
				_lastHoveredChoice = choice;
				Messages = null;
			}
			else
			{
				if(CardStats?.Count >= choice.Actor.Index)
				{
					HoveredAsset = new CardAssetViewModel(card, CardAssetType.Tile);

					var stats = CardStats[choice.Actor.Index];
					var cards = stats.CardStats?.RelatedCards.GeneratedCardIds?.Generated?.Select(x => new Hearthstone.Card(x)).ToList()
								?? new List<Hearthstone.Card>();
					BottomPanelCards = cards;

					IsRelatedCardsSorted = stats.CardStats?.RelatedCards.GeneratedCardIds?.IsSorted ?? false;

					if(cards.Count > 0)
					{
						var totalRelated = stats.CardStats?.RelatedCards.GeneratedCardIds?.TotalCards ?? 0;
						if(cards.Count < totalRelated)
							BottomPanelTitle =
								$"{card.LocalizedName} – {LocUtil.Get("ArenaPick_RelatedCards")} {cards.Count}/{totalRelated}";
						else
							BottomPanelTitle = $"{card.LocalizedName} – {LocUtil.Get("ArenaPick_RelatedCards")} {cards.Count}";
					}
					else
						BottomPanelTitle = $"{card.LocalizedName}";

					UpdateTileViewModels();

					Messages = ConvertMessages(CardStats?.ElementAtOrDefault(choice.Actor.Index)?.CardStats?.Messages)?.ToList();

					_lastHoveredChoice = choice;
					UpdateCardListDirectionTriggers();
				}
			}

			BottomPanelDirectionShape = choice.Actor.Index switch
			{
				0 => PointCollection.Parse("130 225, 385 225, 1017 675,  60 675"),
				1 => PointCollection.Parse("410 225, 665 225, 1017 675,  60 675"),
				_ => PointCollection.Parse("692 225, 947 225, 1017 675,  60 675"),
			};
		}
	}

	private static IEnumerable<string>? ConvertMessages(List<ArenaCardPickApiResponse.Message>? messages)
	{
		return messages?.SelectMany(ConvertMessage);
	}

	private static IEnumerable<string> ConvertMessage(ArenaCardPickApiResponse.Message message)
	{
		switch(message.Type)
		{
			case MessageType.LowSynergy:
				var lowSynergyContent = message.ParseContent<LowSynergyMessageContent>();
				yield return string.Format(LocUtil.Get("ArenaPick_Message_LowSynergy"), lowSynergyContent.RemainingPicks ?? 0);
				break;
			case MessageType.Highlander:
				var highlanderContent = message.ParseContent<HighlanderMessageContent>();
				yield return string.Format(LocUtil.Get("ArenaPick_Message_Highlander"), highlanderContent.HighlanderCardId ?? "");
				break;
			case MessageType.SoftHighlander:
				var softHighlanderContent = message.ParseContent<SoftHighlanderMessageContent>();
				yield return string.Format(LocUtil.Get("ArenaPick_Message_SoftHighlander"), softHighlanderContent.HighlanderCardId ?? "");
				break;
			case MessageType.HighlanderChances:
				yield return LocUtil.Get("ArenaPick_Message_HighlanderChance");
				break;
			case MessageType.VeryRare:
				var veryRareContent = message.ParseContent<VeryRareMessageContent>();
				yield return string.Format(LocUtil.Get("ArenaPick_Message_VeryRare"), veryRareContent.PercentDrafts ?? 0);
				break;
			case MessageType.QuestHelps:
				var questHelpsContent = message.ParseContent<QuestHelpsMessageContent>();
				yield return string.Format(LocUtil.Get("ArenaPick_Message_HelpsQuest"), questHelpsContent.QuestCardId ?? "");
				break;
		}
	}

	private readonly PointCollection _emptyPointCollection = new();

	public bool EnableBottomDirectionTrigger => HoveredChoice != null || HoveringBottomDirectionTrigger;

	public PointCollection BottomPanelDirectionShape
	{
		get => GetProp(_emptyPointCollection)!;
		set => SetProp(value);
	}

	public bool HasBottomPanelCards => BottomPanelCards?.Count > 0;
	public List<Hearthstone.Card>? BottomPanelCards
	{
		get => GetProp<List<Hearthstone.Card>?>(null) ?? new List<Hearthstone.Card>();
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ShowBottom));
			OnPropertyChanged(nameof(HasBottomPanelCards));
			OnPropertyChanged(nameof(BottomPanelColumnCount));
		}
	}

	public int BottomPanelColumnCount => ShowMessages ? 4 : 6;

	public string BottomPanelTitle
	{
		get => GetProp("")!;
		set => SetProp(value);
	}

	public bool ShowBottom
	{
		get
		{
			if(IsPackageSelectOpen || IsAnimating || IsHeroZoomed)
				return false;
			if(HoveredChoice == null && (_lastHoveredChoice == null || (!HoveringBottomDirectionTrigger && !HoveringPanel)))
				return false;
			return Config.Instance.ShowArenaRelatedCards && (BottomPanelCards?.Count > 0 || Messages?.Count > 0);
		}
	}

	public bool IsRelatedCardsSorted
	{
		get => GetProp(false);
		set => SetProp(value);
	}

	private List<string>? PickedDeck
	{
		get => GetProp<List<string>?>(null);
		set {
			SetProp(value);
		}
	}


	private List<string>? _pickedRedraftDeck;

	public Visibility HeroPickVisibility
	{
		get => GetProp(Visibility.Visible);
		set => SetProp(value);
	}

	public List<ArenaPickSingleHeroOptionViewModel>? HeroStats
	{
		get => GetProp<List<ArenaPickSingleHeroOptionViewModel>?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ShowStats));
		}
	}

	public Visibility ArenasmithScoreVisibility
	{
		get => GetProp(Visibility.Visible);
		set => SetProp(value);
	}

	public List<ArenaPickSingleCardOptionViewModel>? CardStats
	{
		get => GetProp<List<ArenaPickSingleCardOptionViewModel>?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ShowStats));
		}
	}

	public Visibility RelatedCardsVisibility
	{
		get => GetProp(Visibility.Visible);
		set => SetProp(value);
	}

	public Visibility SynergiesVisibility
	{
		get => GetProp(Visibility.Visible);
		set => SetProp(value);
	}

	public Visibility RedraftDiscardVisibility
	{
		get => GetProp(Visibility.Visible);
		set
		{
			SetProp(value);
			if(IsEditingDeck)
			{
				foreach(var tile in TileViewModels)
				{
					tile.ArensmithScoreVisibility = value;
				}
			}
		}
	}


	public bool ShowMessages => Messages?.Count > 0;

	public  List<string>? Messages
	{
		get => GetProp<List<string>?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ShowBottom));
			OnPropertyChanged(nameof(ShowMessages));
			OnPropertyChanged(nameof(BottomPanelColumnCount));
		}
	}

	public void Reset(bool keepGameState = false)
	{
		HeroStats = null;
		CardStats = null;
		Messages = null;
		TileViewModels = new List<DeckListTileViewModel>();
		RedraftTileViewModels = new List<DeckListTileViewModel>();
		_lastHoveredChoice = null;
		ArenasmithAvailabilities = null;
		if(!keepGameState)
		{
			_chosenHero = "";
		}
	}

	private Task<ArenaHeroPickApiResponse?> MakeRequestHeroPick(
		IEnumerable<string> offeredHeroes,
		int arenaSeasonId,
		long deckId,
		AccountId accountId,
		bool isUnderground
	)
	{
		var parameters = new ArenaHeroPickParams(
			new List<string>(), // TODO dual class arena
			offeredHeroes
		)
		{
			ArenaSeason = arenaSeasonId,
			PlayerRegion = (int)Helper.GetRegion(accountId.Hi),
			AccountLo = accountId.Lo,
			DeckId = deckId,
			GameType = isUnderground ? (int)BnetGameType.BGT_UNDERGROUND_ARENA : (int)BnetGameType.BGT_ARENA
		};

		Log.Debug(JsonConvert.SerializeObject(parameters)); // TODO removeme
		return ApiWrapper.GetArenaHeroPickStats<ArenaHeroPickApiResponse>(parameters);
	}

	private string? _lastApiCallParameters;

	private Task<ArenaCardPickApiResponse?> MakeRequestCardPick(
		IEnumerable<string> offeredCards,
		string pickedHero,
		IEnumerable<string> pickedDeck,
		int arenaSeasonId,
		long deckId,
		AccountId accountId,
		bool isUnderground
	)
	{
		var parameters = new ArenaCardPickParams(pickedHero, pickedDeck, offeredCards)
		{
			ArenaSeason = arenaSeasonId,
			PlayerRegion = (int)Helper.GetRegion(accountId.Hi),
			AccountLo = accountId.Lo,
			DeckId = deckId,
			GameType = isUnderground ? (int)BnetGameType.BGT_UNDERGROUND_ARENA : (int)BnetGameType.BGT_ARENA,
		};

		var parametersJson = JsonConvert.SerializeObject(parameters);
		Log.Debug(parametersJson); // TODO removeme
		if(_lastApiCallParameters == parametersJson)
		{
			Log.Debug("Last API call parameters DID NOT change");
		}
		_lastApiCallParameters = parametersJson;

		return ApiWrapper.GetArenaCardPickStats<ArenaCardPickApiResponse>(parameters);
	}

	private Task<ArenaCardPickApiResponse?> MakeRequestRedraftCardPick(
		IEnumerable<string> offeredCards,
		string pickedHero,
		IEnumerable<string> pickedRedraftDeck,
		IEnumerable<string> deckCardIds,
		int redraftNumber,
		int arenaSeasonId,
		long deckId,
		AccountId accountId,
		bool isUnderground
	)
	{
		var parameters = new ArenaCardPickParams(pickedHero, pickedRedraftDeck, offeredCards)
		{
			ArenaSeason = arenaSeasonId,
			PlayerRegion = (int)Helper.GetRegion(accountId.Hi),
			AccountLo = accountId.Lo,
			DeckId = deckId,
			GameType = isUnderground ? (int)BnetGameType.BGT_UNDERGROUND_ARENA : (int)BnetGameType.BGT_ARENA,
			RedraftNumber = redraftNumber,
			DeckCardIds = deckCardIds
		};

		var parametersJson = JsonConvert.SerializeObject(parameters);
		Log.Debug(parametersJson); // TODO removeme
		if(_lastApiCallParameters == parametersJson)
		{
			Log.Debug("Last API call parameters DID NOT change");
		}
		_lastApiCallParameters = parametersJson;
		return ApiWrapper.GetArenaCardPickStats<ArenaCardPickApiResponse>(parameters);
	}

	private Task<ArenaCardStats?> MakeRequestEditDeck(
		// IEnumerable<string> deckCardIds, -- not used so we can cache this
		string heroCardId,
		int redraftNumber,
		int arenaSeasonId,
		long deckId,
		AccountId accountId,
		bool isUnderground)
	{
		var parameters = new ArenaScoreDeckParams(heroCardId) {
			ArenaSeason = arenaSeasonId,
			PlayerRegion = (int)Helper.GetRegion(accountId.Hi),
			AccountLo = accountId.Lo,
			DeckId = deckId,
			GameType = isUnderground ? (int)BnetGameType.BGT_UNDERGROUND_ARENA : (int)BnetGameType.BGT_ARENA,
			RedraftNumber = redraftNumber,
		};

		return ApiWrapper.ScoreArenaDeck<ArenaCardStats>(parameters);
	}
}
