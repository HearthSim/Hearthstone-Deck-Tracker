using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using HearthDb.Enums;
using HearthMirror;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan;

public class ConstructedMulliganPreLobbyWidgetViewModel : ViewModel
{
	private RemoteData.SaleData? _data;
	public ConstructedMulliganPreLobbyWidgetViewModel()
	{
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
			OnPropertyChanged(nameof(PanelMinWidth));
		};

		Remote.Config.Loaded += data =>
		{
			_data = data?.Sales?.TraditionalSale;
			OnPropertyChanged(nameof(SaleTagVisibility));
			OnPropertyChanged(nameof(SaleTooltipVisibility));
			OnPropertyChanged(nameof(SaleDescription));
			};
	}

	#region VisualsFormatType

	public VisualsFormatType VisualsFormatType
	{
		get { return GetProp(VisualsFormatType.VFT_UNKNOWN); }

		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(GameType));
			OnPropertyChanged(nameof(FormatType));
			OnPropertyChanged(nameof(ShowOnboardingButton));
			ShowOnboardingNotification = !Config.Instance.MulliganGV2OnboardingSeen && FormatType == FormatType.FT_STANDARD;
			Update().Forget();
		}
	}

	private BnetGameType GameType => VisualsFormatType switch
	{
		VisualsFormatType.VFT_STANDARD => BnetGameType.BGT_RANKED_STANDARD,
		VisualsFormatType.VFT_WILD => BnetGameType.BGT_RANKED_WILD,
		VisualsFormatType.VFT_TWIST => BnetGameType.BGT_RANKED_TWIST,
		VisualsFormatType.VFT_CASUAL => BnetGameType.BGT_CASUAL_WILD,
		_ => BnetGameType.BGT_UNKNOWN,
	};

	public FormatType FormatType => VisualsFormatType switch
	{
		VisualsFormatType.VFT_STANDARD => FormatType.FT_STANDARD,
		VisualsFormatType.VFT_WILD => FormatType.FT_WILD,
		VisualsFormatType.VFT_TWIST => FormatType.FT_TWIST,
		VisualsFormatType.VFT_CASUAL => FormatType.FT_WILD,
		_ => FormatType.FT_UNKNOWN,
	};

	#endregion


	#region Visibiliy
	public bool IsGameCriticalUiOpen
	{
		get { return GetProp(false); }

		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(Visibility));
		}
	}

	public Visibility Visibility => IsGameCriticalUiOpen ? Visibility.Hidden : Visibility.Visible;
	#endregion

	public void InvalidateUserState()
	{
		UserState = UserState.Loading;
	}

	public UserState UserState
	{
		get
		{
			if(Remote.Config.Data?.MulliganGuide?.Disabled ?? false)
				return UserState.Disabled;
			return GetProp(UserState.Loading);
		}

		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(PanelMinWidth));
		}
	}

	public void OnFocus() => PossiblySubscribed = true;
	private bool PossiblySubscribed
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(RefreshSubscriptionState));
		}
	}

	public RefreshSubscriptionState RefreshSubscriptionState
	{
		get {
			if((TrialUsesRemaining > 0 && !PossiblySubscribed) || IsAuthenticated == null)
			{
				return RefreshSubscriptionState.Hidden;
			}

			return IsAuthenticated == true ? RefreshSubscriptionState.Refresh : RefreshSubscriptionState.SignIn;
		}
	}

	public int? TrialUsesRemaining
	{
		get => GetProp<int?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(RefreshSubscriptionState));
		}
	}


	public Visibility SaleTagVisibility
	{
		get
		{
			if(_data == null || !_data.Enabled)
				return Visibility.Collapsed;

			return Visibility.Visible;
		}
	}

	public Visibility SaleTooltipVisibility
	{
		get
		{
			if(ShowOnboardingNotification)
				return Visibility.Collapsed;
			if(_data == null || !_data.Enabled)
				return Visibility.Collapsed;

			if(Config.Instance.IgnoreTraditionalSaleId >= _data.Id)
				return Visibility.Collapsed;

			return Visibility.Visible;
		}
	}

	public string SaleDescription
	{
		get
		{
			if(_data == null || !_data.Enabled)
				return string.Empty;

			return string.Format(LocUtil.Get("PreLobbySale_Blackfriday_Description"), _data.Discount);
		}
	}

	public bool IsCollapsed
	{
		get => GetProp(!Config.Instance.ShowMulliganGuidePreLobby);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ChevronIcon));
		}
	}
	public Visual? ChevronIcon => Application.Current.TryFindResource("chevron_" + (IsCollapsed ? "down" : "up")) as Visual;
	public int? PanelMinWidth => UserState is UserState.ValidPlayer or UserState.Subscribed ? 264 : 214;

	public string? TrialTimeRemaining
	{
		get => GetProp<string?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(ResetTimeVisibility));
		}
	}

	public Visibility? ResetTimeVisibility => TrialTimeRemaining != null ? Visibility.Visible : Visibility.Collapsed;

	public bool RefreshAccountEnabled { get => GetProp(true); set => SetProp(value); }

	public string? Username { get => GetProp<string?>(null); set => SetProp(value); }

	public async Task Update()
	{
		if(UserState == UserState.Disabled)
			return;

		if(VisualsFormatType is VisualsFormatType.VFT_UNKNOWN or VisualsFormatType.VFT_TWIST or VisualsFormatType.VFT_CASUAL or VisualsFormatType.VFT_CLASSIC)
			return;

		if(HSReplayNetOAuth.AccountUpdateInProgress)
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
				HSReplayNetOAuth.AccountUpdateInProgress = true;
				// (Unrelativ to the event) If we want to cut down the request
				// volume here in the future we can only make this request for
				// premium subscribers (still need to happen right here, not below to
				// handle the case where premium ran out).
				await HSReplayNetOAuth.UpdateAccountData();
				HSReplayNetOAuth.AccountUpdateInProgress = false;
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
			await MulliganGuideTrial.Update(acc.Hi, acc.Lo);
			TrialTimeRemaining = MulliganGuideTrial.TimeRemaining;
			TrialUsesRemaining = MulliganGuideTrial.RemainingTrials ?? 0;
			Core.Overlay.ConstructedMulliganGuidePreLobbyViewModel.IsOutOfTrials = TrialUsesRemaining == 0;
			UserState = UserState.ValidPlayer;
			return;
		}

		Core.Overlay.ConstructedMulliganGuidePreLobbyViewModel.IsOutOfTrials = false;

		TrialTimeRemaining = null;
		UserState = UserState.Subscribed;
	}

	public void Reset()
	{
		UserState = UserState.Loading;
		TrialTimeRemaining = null;
		Username = null;
		VisualsFormatType = VisualsFormatType.VFT_UNKNOWN;
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
		var isStandard = FormatType == FormatType.FT_STANDARD;
		var campaign = isStandard ? "constructed_lobby_subscribe" : "constructed_lobby_subscribe_wild";
		var url = Helper.BuildHsReplayNetUrl("premium/", campaign);
		Helper.TryOpenUrl(url);
		PossiblySubscribed = true;
		var button = isStandard ? ClickSubscribeNowAction.Button.ConstructedPreLobby : ClickSubscribeNowAction.Button.ConstructedPreLobbyWild;
		HSReplayNetClientAnalytics.OnClickSubscribeNowLink(
			Franchise.HSConstructed, button, TrialUsesRemaining
		);
	});

	public ICommand MyStatsCommand => new Command(() =>
	{
		var acc = Reflection.Client.GetAccountId();
		var queryParams = acc != null ? new[] { $"hearthstone_account={acc.Hi}-{acc.Lo}" } : null;
		var url = Helper.BuildHsReplayNetUrl("decks/mine/", "constructed_lobby_my_stats", queryParams);
		Helper.TryOpenUrl(url);
	});

	public ICommand RefreshAccountCommand => new Command(async () =>
	{
		RefreshAccountEnabled = false;
		InvalidateUserState();
		await Task.WhenAll(HSReplayNetOAuth.UpdateAccountData(), Task.Delay(3000));
		RefreshAccountEnabled = true;
	});

	public ICommand CloseSaleTooltipCommand => new Command(() =>
	{
		Config.Instance.IgnoreTraditionalSaleId = _data?.Id ?? -1;
		Config.Save();
		OnPropertyChanged(nameof(SaleTooltipVisibility));
	});

	public string HSReplayIcon => "/HearthstoneDeckTracker;component/Resources/hsreplay_logo_white.png";

	#region Onboarding

	public bool ShowOnboardingButton => FormatType == FormatType.FT_STANDARD;

	public bool IsOnboardingVisible
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			if(value)
			{
				Config.Instance.MulliganGV2OnboardingSeen = true;
				Config.Save();
				HSReplayNetClientAnalytics.OnOpenedOnboardingModal();
			}
		}
	}

	public ICommand ToggleOnboardingCommand => new Command(() =>
	{
		IsOnboardingVisible = !IsOnboardingVisible;
	});

	public bool ShowOnboardingNotification
	{
		get => GetProp(false);
		set
		{
			if(!ShowOnboardingNotification && value)
			{
				HSReplayNetClientAnalytics.OnOnboardingNotificationShowed();
			}
			SetProp(value);
			OnPropertyChanged(nameof(SaleTooltipVisibility));
		}
	}

	public ICommand CloseOnboaringNotificationCommand => new Command(() =>
	{
		ShowOnboardingNotification = false;
		Config.Instance.MulliganGV2OnboardingSeen = true;
		Config.Save();
		HSReplayNetClientAnalytics.OnOnboardingNotificationDismissed();
	});

	public ICommand OnboardingLearnMoreCommand => new Command(() =>
	{
		IsOnboardingVisible = true;
		ShowOnboardingNotification = false;
		HSReplayNetClientAnalytics.OnLearnMoreOnboardingNotification();
	});

	#endregion

}

public enum UserState
{
	Loading,
	UnknownPlayer,
	ValidPlayer,
	Subscribed,
	Disabled
}

public enum RefreshSubscriptionState
{
	Hidden,
	SignIn,
	Refresh,
}
