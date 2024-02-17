using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using HearthMirror;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Hearthstone_Deck_Tracker.Utility.RemoteData;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Tier7
{
	public class Tier7PreLobbyViewModel : ViewModel
	{
		public Tier7PreLobbyViewModel()
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
		}

		#region Visibiliy
		public bool IsModalOpen
		{
			get { return GetProp(false); }

			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(Visibility));
			}
		}

		public Visibility Visibility => IsModalOpen ? Visibility.Hidden : Visibility.Visible;
		#endregion

		public void InvalidateUserState()
		{
			UserState = UserState.Loading;
		}

		public UserState UserState
		{
			get
			{
				if(Remote.Config.Data?.Tier7?.Disabled ?? false)
					return UserState.Disabled;
				return GetProp(UserState.Loading);
			}

			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(PanelMinWidth));
			}
		}

		public RefreshSubscriptionState RefreshSubscriptionState
		{
			get => GetProp(RefreshSubscriptionState.Hidden);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(RefreshSubscriptionState));
			}
		}

		public int? TrialUsesRemaining { get => GetProp<int?>(null); set => SetProp(value); }
		public string? AllTimeHighMMR { get => GetProp<string?>(null); set => SetProp(value); }
		public Visibility AllTimeHighMMRVisibility { get => GetProp(Visibility.Collapsed); set => SetProp(value); }

		public bool IsCollapsed
		{
			get => GetProp(Config.Instance.Tier7OverlayCollapsed);
			set
			{
				SetProp(value);
				Config.Instance.Tier7OverlayCollapsed = value;
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

		public Visibility RefreshAccountVisibility { get => GetProp(Visibility.Collapsed); set => SetProp(value); }
		public bool RefreshAccountEnabled { get => GetProp(true); set => SetProp(value); }

		public string? Username { get => GetProp<string?>(null); set => SetProp(value); }

		private bool _isUpdatingAccount;
		public async Task Update()
		{
			if(UserState == UserState.Disabled)
				return;

			if(_isUpdatingAccount)
			{
				// AccountDataUpdated event was likely triggered by the
				// UpdateAccountData request below. Skip this update.
				return;
			}

			var ownsTier7 = false;
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
				ownsTier7 = HSReplayNetOAuth.AccountData.IsTier7;

				// Update the Refresh button, as it's otherwise only updated after a click on GET PREMIUM
				if(ownsTier7)
				{
					RefreshSubscriptionState = RefreshSubscriptionState.Hidden;
				}
				else if(RefreshSubscriptionState == RefreshSubscriptionState.SignIn)
				{
					RefreshSubscriptionState = RefreshSubscriptionState.Refresh;
				}
			}
			else
			{
				IsAuthenticated = false;
				if(RefreshSubscriptionState == RefreshSubscriptionState.Refresh)
				{
					RefreshSubscriptionState = RefreshSubscriptionState.SignIn;
				}
			}

			var acc = Reflection.Client.GetAccountId();

			Username = Reflection.Client.GetBattleTag()?.Name ?? HSReplayNetOAuth.AccountData?.Username ?? null;

			if(!ownsTier7)
			{
				AllTimeHighMMR = null;
				if (acc == null)
				{
					// unable to get AccountHi/AccountLo, not eligible for trials
					UserState = UserState.UnknownPlayer;
					return;
				}
				await Tier7Trial.Update(acc.Hi, acc.Lo);
				TrialTimeRemaining = Tier7Trial.TimeRemaining;
				TrialUsesRemaining = Tier7Trial.RemainingTrials ?? 0;
				UserState = UserState.ValidPlayer;
				return;
			}

			TrialTimeRemaining = null;
			int? allTimeFromApi = null;
			if(acc != null)
			{
				var response = await HSReplayNetOAuth.MakeRequest(c => c.GetAllTimeBGsMMR(acc.Hi, acc.Lo));
				allTimeFromApi = response?.AllTimeHighMMR;
			}
			var currentMMR = Core.Game.BattlegroundsRatingInfo?.Rating;
			AllTimeHighMMR = (allTimeFromApi, currentMMR) switch
			{
				(int api, int curr) => Math.Max(api, curr).ToString(),
				(int api, null) => api.ToString(),
				(null, int curr) => curr.ToString(),
				(null, null) => null,
			};
			AllTimeHighMMRVisibility = allTimeFromApi == null ? Visibility.Collapsed : Visibility.Visible;
			UserState = UserState.Subscribed;
		}

		public void Reset()
		{
			UserState = UserState.Loading;
			AllTimeHighMMR = null;
			TrialTimeRemaining = null;
			Username = null;
		}

		public bool? IsAuthenticated { get; set; }
		public ICommand SignInCommand => new Command(() => {
			HSReplayNetHelper.TryAuthenticate().Forget();

			if(Helper.OptionsMain != null)
			{
				Helper.OptionsMain.TreeViewItemHSReplayAccount.IsSelected = true;
				Core.MainWindow.FlyoutOptions.IsOpen = true;
			}
		});

		public ICommand SubscribeNowCommand => new Command(() =>
		{
			var url = Helper.BuildHsReplayNetUrl("battlegrounds/tier7/", "bgs_lobby_subscribe");
			Helper.TryOpenUrl(url);
			RefreshSubscriptionState = IsAuthenticated == true ? RefreshSubscriptionState.Refresh : RefreshSubscriptionState.SignIn;
		});

		public ICommand MyStatsCommand => new Command(() =>
		{
			var acc = Reflection.Client.GetAccountId();
			var queryParams = acc != null ? new[] { $"hearthstone_account={acc.Hi}-{acc.Lo}" } : null;
			var url = Helper.BuildHsReplayNetUrl("battlegrounds/mine/", "bgs_lobby_my_stats", queryParams);
			Helper.TryOpenUrl(url);
		});

		public ICommand RefreshAccountCommand => new Command(async () =>
		{
			RefreshAccountEnabled = false;
			InvalidateUserState();
			await Task.WhenAll(HSReplayNetOAuth.UpdateAccountData(), Task.Delay(3000));
			RefreshAccountEnabled = true;
		});
	}

	internal record SubscriberData
	{
		public int SeasonMaxMMR { get; init;  }
		public int AllTimeMaxMMR { get; init;  }
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
}
