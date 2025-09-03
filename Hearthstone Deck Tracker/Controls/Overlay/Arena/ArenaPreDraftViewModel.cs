using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using HearthDb.Enums;
using HearthMirror;
using HearthMirror.Enums;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Arena;

public class ArenaPreDraftViewModel : ViewModel
{
	public ArenaPreDraftViewModel()
	{
		Watchers.ArenaStateWatcher.OnClientStateChanged += UpdateClientState;
		Watchers.ArenaStateWatcher.OnIsUndergroundChanged += UpdateUnderground;
		Watchers.ArenaStateWatcher.OnDeckIdChanged += UpdateDeckId;
		HSReplayNetOAuth.AccountDataUpdated += () =>
		{
			InvalidateUserState();
			Update().Forget();
		};
		HSReplayNetOAuth.LoggedOut += () =>
		{
			InvalidateUserState();
			Update().Forget();
		};
		Remote.Config.Loaded += (_) =>
		{
			OnPropertyChanged(nameof(UserState));
		};
	}

	private void UpdateClientState((ArenaClientStateType ClientState, ArenaSessionState SessionState) state)
	{
		var isOnLanding = state.ClientState is ArenaClientStateType.Normal_Landing or ArenaClientStateType.Underground_Landing;
		var isOneClickAwayFromDrafting = isOnLanding && state.SessionState is ArenaSessionState.NO_RUN or ArenaSessionState.DRAFTING or ArenaSessionState.REDRAFTING;
		var draftState = isOneClickAwayFromDrafting
			? state.SessionState == ArenaSessionState.NO_RUN ? DraftState.PreDraft : DraftState.MidDraft
			: DraftState.Other;
		DraftState = draftState;
	}

	private void UpdateUnderground(bool isUnderground)
	{
		IsUnderground = isUnderground;
		EnsureAvailabilities().Forget();
	}


	public void OnFocus()
	{
		PossiblySubscribed = true;
	}

	private bool PossiblySubscribed
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(RefreshSubscriptionState));
		}
	}

	public bool IsGameCriticalUiOpen
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(Visibility));
		}
	}

	public Visibility Visibility => !IsGameCriticalUiOpen && DraftState is DraftState.PreDraft or DraftState.MidDraft
		? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;

	public void Reset()
	{
		ArenasmithAvailabilities = null;
		UserState = UserState.Loading;
		TrialTimeRemaining = null;
		Username = null;
		IsAuthenticated = null;
		PossiblySubscribed = false;
		ArenaTrial.Clear();
	}


	public bool IsUnderground
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ArenasmithAvailable));
		}
	}

	public (bool Arena, bool UndergroundArena)? ArenasmithAvailabilities
	{
		get => GetProp<(bool, bool)?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ArenasmithAvailable));
		}
	}

	public ArenasmithAvailability ArenasmithAvailable =>
		(IsUnderground ? ArenasmithAvailabilities?.UndergroundArena : ArenasmithAvailabilities?.Arena) switch
		{
			null => ArenasmithAvailability.Loading,
			true => ArenasmithAvailability.Available,
			false => ArenasmithAvailability.Unavailable,
		};

	private async Task EnsureAvailabilities()
	{
		if(Remote.Config.Data?.Arenasmith?.Disabled ?? false)
		{
			ArenasmithAvailabilities = (false, false);
			return;
		}

		if(ArenasmithAvailabilities is not null)
			return;

		var status = await ApiWrapper.GetArenasmithStatus();
		if(status == null)
			return;

		ArenasmithAvailabilities = (
			Arena: status.Data.TryGetValue(BnetGameType.BGT_ARENA.ToString(), out var a) && a.IsArenasmithAvailable,
			UndergroundArena: status.Data.TryGetValue(BnetGameType.BGT_UNDERGROUND_ARENA.ToString(), out var u) && u.IsArenasmithAvailable
		);
	}

	private bool _updatingAvailability;


	public DraftState DraftState
	{
		get => GetProp(DraftState.Other);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(Visibility));
			Update().Forget();
		}
	}

	private void UpdateDeckId(long deckId)
	{
		DeckId = deckId > 0 ? deckId : null;
		Update().Forget();
	}

	public long? DeckId
	{
		get => GetProp<long?>(null);
		set
		{
			SetProp(value);
		}
	}

	public bool IsTrialEnabledForDeck
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
		}
	}

	public void InvalidateUserState()
	{
		UserState = UserState.Invalidated;
	}

	public UserState UserState
	{
		get
		{
			return GetProp(UserState.Loading);
		}

		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(Visibility));
		}
	}

	public RefreshSubscriptionState RefreshSubscriptionState
	{
		get {
			if(IsAuthenticated != false && !PossiblySubscribed && StarterTrialsRemaining > 0 && RecurringTrialsRemaining > 0)
			{
				return RefreshSubscriptionState.Hidden;
			}

			return IsAuthenticated == true ? RefreshSubscriptionState.Refresh : RefreshSubscriptionState.SignIn;
		}
	}

	public int? StarterTrialsRemaining
	{
		get => GetProp<int?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(RefreshSubscriptionState));
			OnPropertyChanged(nameof(RemainingTrials));
		}
	}

	public int? RecurringTrialsRemaining
	{
		get => GetProp<int?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(RefreshSubscriptionState));
			OnPropertyChanged(nameof(RemainingTrials));
		}
	}

	public int? MaxTrialUses
	{
		get => GetProp<int?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(RemainingTrials));
		}
	}

	public string RemainingTrials => StarterTrialsRemaining is > 0
				? $"{StarterTrialsRemaining + (RecurringTrialsRemaining ?? 0)}"
				: MaxTrialUses != null
					? $"{RecurringTrialsRemaining ?? 0}/{MaxTrialUses}"
					: RecurringTrialsRemaining != null ?
						RecurringTrialsRemaining.ToString() :
						"?";

	public bool IsCollapsed
	{
		get => GetProp(Config.Instance.ArenasmithPreLobbyTrialsCollapsed);
		set
		{
			SetProp(value);
			Config.Instance.ArenasmithPreLobbyTrialsCollapsed = value;
			OnPropertyChanged(nameof(ChevronIcon));
		}
	}
	public Visual? ChevronIcon => Application.Current.TryFindResource("chevron_" + (IsCollapsed ? "down" : "up")) as Visual;

	public string? TrialTimeRemaining
	{
		get => GetProp<string?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ResetTimeVisibility));
		}
	}

	public Visibility? ResetTimeVisibility => StarterTrialsRemaining == 0 && TrialTimeRemaining != null ? Visibility.Visible : Visibility.Collapsed;

	public bool RefreshAccountEnabled { get => GetProp(true); set => SetProp(value); }

	public string? Username { get => GetProp<string?>(null); set => SetProp(value); }

	private bool _isUpdatingAccount;
	public async Task Update()
	{
		if(DraftState is DraftState.PreDraft or DraftState.MidDraft && !_updatingAvailability)
		{
			_updatingAvailability = true;
			try
			{
				await EnsureAvailabilities();
			}
			finally
			{
				_updatingAvailability = false;
			}
		}

		if(_isUpdatingAccount)
		{
			// AccountDataUpdated event was likely triggered by the
			// UpdateAccountData request below. Skip this update.
			return;
		}

		var ownsPremium = false;
		if(HSReplayNetOAuth.IsFullyAuthenticated && HSReplayNetOAuth.AccountData != null)
		{
			if(UserState == UserState.Loading)
			{
				// This will fire a HSReplayNetOAuth.AccountDataUpdated event. We
				// set a flag for the duration of the update check to avoid
				// infinite recursion here.
				_isUpdatingAccount = true;
				// (Unrelativ to the event) If we want to cut down the request
				// volume here in the future we can only make this request for
				// tier7 subscribers (still need to happen right here, not below to
				// handle the case where tier7 ran out).
				await HSReplayNetOAuth.UpdateAccountData();
				_isUpdatingAccount = false;
			}

			IsAuthenticated = true;
			ownsPremium = HSReplayNetOAuth.AccountData.IsPremium;
		}
		else
		{
			IsAuthenticated = false;
		}

		var acc = Reflection.Client.GetAccountId();

		Username = Reflection.Client.GetBattleTag()?.Name ?? HSReplayNetOAuth.AccountData?.Username ?? null;

		if(!ownsPremium)
		{
			if (acc == null)
			{
				// unable to get AccountHi/AccountLo, not eligible for trials
				UserState = UserState.UnknownPlayer;
				return;
			}
			await ArenaTrial.EnsureLoaded(acc.Hi, acc.Lo);
			TrialTimeRemaining = ArenaTrial.TimeRemaining;
			var remainingTrials = ArenaTrial.RemainingTrials;
			StarterTrialsRemaining = remainingTrials?.StarterTrialsRemaining;
			RecurringTrialsRemaining = remainingTrials?.RecurringTrialsRemaining;
			MaxTrialUses = ArenaTrial.MaxRecurringTrials ?? 0;
			UserState = UserState.TrialPlayer;
			IsTrialEnabledForDeck = DeckId.HasValue && ArenaTrial.IsDeckResumable(DeckId.Value);
			return;
		}

		TrialTimeRemaining = null;
		UserState = UserState.Subscriber;
	}


	public bool? IsAuthenticated
	{
		get => GetProp<bool?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(RefreshSubscriptionState));
		}
	}

	public ICommand SubscribeNowCommand => new Command(() =>
	{
		var url = Helper.BuildHsReplayNetUrl("arenasmith/", "arena_lobby_subscribe");
		Helper.TryOpenUrl(url);
		PossiblySubscribed = true;
		HSReplayNetClientAnalytics.OnClickSubscribeNowLink(
			Franchise.HSConstructed, new[] { SubFranchise.Arena}, ClickSubscribeNowAction.Button.ArenaPreLobby,
			StarterTrialsRemaining != null || RecurringTrialsRemaining != null ? (StarterTrialsRemaining ?? 0) + (RecurringTrialsRemaining ?? 0) : 0
		);
	});

	public ICommand RefreshAccountCommand => new Command(async () =>
	{
		RefreshAccountEnabled = false;
		UserState = UserState.Loading;
		await Task.WhenAll(Update(), Task.Delay(3000));
		RefreshAccountEnabled = true;
		PossiblySubscribed = true;
	});

	public ICommand ViewArenaStatsCommand => new Command(() =>
	{
		var url = Helper.BuildHsReplayNetUrl("arena/cards", "arena_lobby_view_stats", fragmentParams: new[] { "gameType=UNDERGROUND_ARENA" });
		Helper.TryOpenUrl(url);
		PossiblySubscribed = true;
	});
}

public enum DraftState
{
	Other,
	PreDraft,
	MidDraft,
}

public enum ArenasmithAvailability
{
	Loading,
	Available,
	Unavailable,
}

public enum UserState
{
	Loading,
	UnknownPlayer,
	TrialPlayer,
	Subscriber,
	Invalidated,
}

public enum RefreshSubscriptionState
{
	Hidden,
	SignIn,
	Refresh,
}
