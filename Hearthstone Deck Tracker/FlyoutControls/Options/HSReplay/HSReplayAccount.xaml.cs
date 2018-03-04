using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using static System.Windows.Visibility;
using static Hearthstone_Deck_Tracker.HsReplay.Enums.AccountStatus;

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.HSReplay
{
	public partial class HSReplayAccount : INotifyPropertyChanged
	{
		private bool _logoutButtonEnabled = true;
		private bool _logoutTriggered;
		private bool _claimTokenButtonEnabled;

		public HSReplayAccount()
		{
			InitializeComponent();
			HSReplayNetOAuth.AccountDataUpdated += () =>
			{
				Update();
				LogoutTriggered = false;
			};
			HSReplayNetOAuth.LoggedOut += () =>
			{
				Update();
				LogoutTriggered = false;
			};
			HSReplayNetOAuth.UploadTokenClaimed += () => OnPropertyChanged(nameof(UploadTokenClaimed));
			Account.Instance.TokenClaimedChanged += () => OnPropertyChanged(nameof(UploadTokenClaimed));
			ConfigWrapper.ReplayAutoUploadChanged += () => OnPropertyChanged(nameof(ReplayUploadingEnabled));
			ConfigWrapper.CollectionSyncingChanged += () => OnPropertyChanged(nameof(CollectionSyncingEnabled));
		}

		public bool IsAuthenticated => HSReplayNetOAuth.IsFullyAuthenticated;

		public Visibility ReplaysClaimedVisibility =>
			Account.Instance.Status == Anonymous || HSReplayNetOAuth.IsFullyAuthenticated ? Collapsed : Visible;

		public Visibility LoginInfoVisibility =>
			Account.Instance.Status == Anonymous || HSReplayNetOAuth.IsFullyAuthenticated ? Visible : Collapsed;

		public bool IsPremiumUser =>
			HSReplayNetOAuth.AccountData?.IsPremium?.Equals("true", StringComparison.InvariantCultureIgnoreCase) ?? false;

		public Visibility LogoutWarningVisibility => LogoutTriggered ? Visible : Collapsed;

		public bool LogoutTriggered
		{
			get => _logoutTriggered;
			set
			{
				_logoutTriggered = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(LogoutWarningVisibility));
			}
		}

		public bool LogoutButtonEnabled
		{
			get => _logoutButtonEnabled;
			set
			{
				_logoutButtonEnabled = value; 
				OnPropertyChanged();
			}
		}

		public string BattleTag => HSReplayNetOAuth.AccountData?.BattleTag ?? Account.Instance.Username ?? string.Empty;

		public ICommand LogoutCommand => new Command(async () =>
		{
			if(LogoutTriggered)
			{
				LogoutButtonEnabled = false;
				await HSReplayNetOAuth.Logout();
				LogoutButtonEnabled = true;
			}
			else
				LogoutTriggered = true;
		});

		public bool ClaimTokenButtonEnabled
		{
			get => _claimTokenButtonEnabled;
			set
			{
				_claimTokenButtonEnabled = value; 
				OnPropertyChanged();
			}
		}

		public ICommand EnableCollectionSyncingCommand => new Command(() => ConfigWrapper.CollectionSyncingEnabled = true);

		public ICommand EnableReplayUploadingCommand => new Command(() => ConfigWrapper.HsReplayAutoUpload = true);

		public ICommand PremiumInfoCommand => new Command(() =>
		{
			var url = Helper.BuildHsReplayNetUrl("premium", "options_account_premium_info");
			Helper.TryOpenUrl(url);
		});

		public ICommand AccountSettingsCommand => new Command(() =>
		{
			var url = Helper.BuildHsReplayNetUrl("account", "options_account_settings");
			Helper.TryOpenUrl(url);
		});

		public ICommand ClaimUploadTokenCommand => new Command(async () =>
		{
			ClaimTokenButtonEnabled = false;
			if(!Account.Instance.TokenClaimed.HasValue)
				await ApiWrapper.UpdateUploadTokenStatus();
			if(Account.Instance.TokenClaimed == false)
				await HSReplayNetOAuth.ClaimUploadToken(Account.Instance.UploadToken);
			ClaimTokenButtonEnabled = true;
		});

		public bool ReplayUploadingEnabled => ConfigWrapper.HsReplayAutoUpload;

		public bool CollectionSyncingEnabled => ConfigWrapper.CollectionSyncingEnabled;

		public bool UploadTokenClaimed => Account.Instance.TokenClaimed ?? false;

		public void Update()
		{
			OnPropertyChanged(nameof(BattleTag));
			OnPropertyChanged(nameof(IsAuthenticated));
			OnPropertyChanged(nameof(ReplaysClaimedVisibility));
			OnPropertyChanged(nameof(IsPremiumUser));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
