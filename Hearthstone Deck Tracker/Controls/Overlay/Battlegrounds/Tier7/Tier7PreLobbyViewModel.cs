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
			HSReplayNetOAuth.AccountDataUpdated += () => Update(false).Forget();
			HSReplayNetOAuth.LoggedOut += () => Update(false).Forget();
			Remote.Config.Loaded += (_) =>
			{
				OnPropertyChanged(nameof(UserState));
				OnPropertyChanged(nameof(PanelMinWidth));
			};
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
		public Visual? ChevronIcon => Core.Overlay.Tier7PreLobby.TryFindResource("chevron_" + (IsCollapsed ? "down" : "up")) as Visual;
		public int? PanelMinWidth => UserState is UserState.Authenticated or UserState.Subscribed ? 264 : 214;

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
		public async Task Update(bool checkAccountStatus)
		{
			if(UserState == UserState.Disabled)
				return;

			if(_isUpdatingAccount)
			{
				// AccountDataUpdated event was likely triggered by the
				// UpdateAccountData request below. Skip this update.
				return;
			}

			if(Core.Game.CurrentMode != Mode.BACON)
				return;

			if(await Debounce.WasCalledAgain(50))
			{
				// Debounce to avoid multiple invocations of this when the log
				// is being (re-)read and contains multiple scene changes in
				// and out of BACON.
				return;
			}

			if(!HSReplayNetOAuth.IsFullyAuthenticated || HSReplayNetOAuth.AccountData == null)
			{
				UserState = UserState.Anonymous;
				AllTimeHighMMR = null;
				TrialTimeRemaining = null;
				Username = null;
				return;
			}

			if(checkAccountStatus)
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

			if(!HSReplayNetOAuth.AccountData.IsTier7)
			{
				if(UserState != UserState.Authenticated)
					UserState = UserState.Loading;
				AllTimeHighMMR = null;
				await Tier7Trial.Update();
				TrialTimeRemaining = Tier7Trial.TimeRemaining;
				TrialUsesRemaining = Tier7Trial.RemainingTrials ?? 0;
				Username = Reflection.GetBattleTag()?.Name ?? HSReplayNetOAuth.AccountData.Username;
				UserState = UserState.Authenticated;
				return;
			}

			if(UserState != UserState.Subscribed)
				UserState = UserState.Loading;
			TrialTimeRemaining = null;
			int? allTimeFromApi = null;
			var acc = Reflection.GetAccountId();
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
			RefreshAccountVisibility = Visibility.Visible;
		});

		public ICommand MyStatsCommand => new Command(() =>
		{
			var acc = Reflection.GetAccountId();
			var queryParams = acc != null ? new[] { $"hearthstone_account={acc.Hi}-{acc.Lo}" } : null;
			var url = Helper.BuildHsReplayNetUrl("battlegrounds/mine/", "bgs_lobby_my_stats", queryParams);
			Helper.TryOpenUrl(url);
		});

		public ICommand RefreshAccountCommand => new Command(async () =>
		{
			RefreshAccountEnabled = false;
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
		Anonymous,
		Authenticated,
		Subscribed,
		Disabled
	}

	public class UserStateToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (value as UserState?)== (parameter as UserState?) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
	}
}
