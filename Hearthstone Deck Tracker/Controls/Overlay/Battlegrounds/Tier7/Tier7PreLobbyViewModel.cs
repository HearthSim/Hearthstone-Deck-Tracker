using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Tier7
{
	public class Tier7PreLobbyViewModel : ViewModel
	{
		private RemoteData.SaleData? _data;

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

			Remote.Config.Loaded += data =>
			{
				_data = data?.Sales?.BattlegroundsSale;
				OnPropertyChanged(nameof(SaleTagVisibility));
				OnPropertyChanged(nameof(SaleTooltipVisibility));
				OnPropertyChanged(nameof(SaleDescription));

			};
		}

		#region Mode
		public SelectedBattlegroundsGameMode BattlegroundsGameMode
		{
			get
			{
				return GetProp(SelectedBattlegroundsGameMode.UNKNOWN);
			}
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(AllTimeHighMMRVisibility));
			}
		}
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

		public string? AllTimeHighMMR
		{
			get => GetProp<string?>(null);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(AllTimeHighMMRVisibility));
			}
		}

		public Visibility AllTimeHighMMRVisibility
		{
			get
			{
				if(AllTimeHighMMR == null || BattlegroundsGameMode != SelectedBattlegroundsGameMode.SOLO)
				{
					return Visibility.Collapsed;
				}
				return Visibility.Visible;
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
				if(_data == null || !_data.Enabled)
					return Visibility.Collapsed;

				if(Config.Instance.IgnoreBattlegroundsSaleId >= _data.Id)
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
			}
			else
			{
				IsAuthenticated = false;
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
			UserState = UserState.Subscribed;
		}

		public void Reset()
		{
			UserState = UserState.Loading;
			AllTimeHighMMR = null;
			TrialTimeRemaining = null;
			Username = null;
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
			var url = Helper.BuildHsReplayNetUrl("battlegrounds/tier7/", "bgs_lobby_subscribe");
			Helper.TryOpenUrl(url);
			PossiblySubscribed = true;
			HSReplayNetClientAnalytics.OnClickSubscribeNowLink(
				Franchise.Battlegrounds, ClickSubscribeNowAction.Button.BattlegroundsPreLobby, TrialUsesRemaining
			);
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

		public ICommand CloseSaleTooltipCommand => new Command(() =>
		{
			Config.Instance.IgnoreBattlegroundsSaleId = _data?.Id ?? -1;
			Config.Save();
			OnPropertyChanged(nameof(SaleTooltipVisibility));
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
