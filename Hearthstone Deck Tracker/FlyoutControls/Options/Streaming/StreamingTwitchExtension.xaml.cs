using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Live;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Twitch;
using HSReplay.OAuth;
using HSReplay.OAuth.Data;

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Streaming
{
	public partial class StreamingTwitchExtension : INotifyPropertyChanged
	{
		private TwitchAccount _selectedTwitchUser;
		private bool _twitchAccountLinked;
		private bool _twitchStreamLive;

		public string HSReplayUserName = HSReplayNetOAuth.AccountData?.Username;

		public StreamingTwitchExtension()
		{
			InitializeComponent();
			LiveDataManager.OnStreamingChecked += streaming => TwitchStreamLive = streaming;
			HSReplayNetOAuth.AccountDataUpdated += () =>
			{
				UpdateAccountName();
				RefreshTwitchAccounts();
			};
			HSReplayNetOAuth.LoggedOut += () => OnPropertyChanged(nameof(IsAuthenticated));
		}

		public SolidColorBrush SelectedColor => Helper.BrushFromHex(Config.Instance.StreamingOverlayBackground);

		public ICommand AuthenticateCommand => new Command(async () => await HSReplayNetHelper.TryAuthenticate());

		public bool IsAuthenticated => HSReplayNetOAuth.IsAuthenticatedFor(Scope.ReadSocialAccounts);

		public bool TwitchExtensionEnabled
		{
			get => Config.Instance.SendTwitchExtensionData;
			set
			{
				Config.Instance.SendTwitchExtensionData = value;
				Config.Save();
				OnPropertyChanged();
				if(!Core.Game.IsInMenu)
				{
					if(value)
						LiveDataManager.WatchBoardState();
					else
						LiveDataManager.Stop();
				}
			}
		}

		public bool TwitchAccountLinked
		{
			get => _twitchAccountLinked;
			set
			{
				_twitchAccountLinked = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(SetupComplete));
			}
		}

		public bool SetupComplete => IsAuthenticated && TwitchAccountLinked;

		public bool TwitchStreamLive
		{
			get => _twitchStreamLive;
			set
			{
				_twitchStreamLive = value;
				OnPropertyChanged();
			}
		}

		public TwitchAccount SelectedTwitchUser
		{
			get => _selectedTwitchUser;
			set
			{
				if(_selectedTwitchUser != value)
				{
					_selectedTwitchUser = value;
					OnPropertyChanged();
					var newId = value?.Id ?? 0;
					if(Config.Instance.SelectedTwitchUser != newId)
					{
						Config.Instance.SelectedTwitchUser = newId;
						Config.Save();
					}
				}
			}
		}

		public List<TwitchAccount> AvailableTwitchAccounts => HSReplayNetOAuth.TwitchUsers;

		public bool MultipleTwitchAccounts => AvailableTwitchAccounts?.Count > 1;

		public ICommand RefreshTwitchAccountsCommand => new Command(RefreshTwitchAccounts);

		public ICommand LinkTwitchAccountCommand => new Command(() =>
		{
			Helper.TryOpenUrl("https://hsreplay.net/account/social/connections/");
			AwaitingTwitchAccountConnection = true;
		});

		public bool AwaitingTwitchAccountConnection { get; private set; }

		public ICommand InstallTwitchExtensionCommand =>
			new Command(() => Helper.TryOpenUrl("https://hsdecktracker.net/twitch/extension/"));

		public ICommand SetupGuideCommand => new Command(() => Helper.TryOpenUrl("https://hsdecktracker.net/twitch/setup/"));

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public async Task<bool> RefreshHsreplayAccount()
		{
			var success = await HSReplayNetOAuth.UpdateAccountData();
			if(success)
				UpdateAccountName();
			return success;
		}

		public async void RefreshTwitchAccounts()
		{
			var success = await HSReplayNetOAuth.UpdateTwitchUsers();
			if(success)
				AwaitingTwitchAccountConnection = false;
			UpdateTwitchData();
		}

		internal async void UpdateTwitchData()
		{
			OnPropertyChanged(nameof(IsAuthenticated));
			OnPropertyChanged(nameof(AvailableTwitchAccounts));
			OnPropertyChanged(nameof(MultipleTwitchAccounts));
			var selected = Config.Instance.SelectedTwitchUser;
			SelectedTwitchUser = AvailableTwitchAccounts?.FirstOrDefault(x => x.Id == selected || selected == 0);
			TwitchAccountLinked = SelectedTwitchUser?.Id > 0;
			if(TwitchAccountLinked)
				TwitchStreamLive = await TwitchApi.IsStreaming(SelectedTwitchUser.Id);
		}

		internal void UpdateAccountName()
		{
			OnPropertyChanged(nameof(HSReplayUserName));
		}

		private void TwitchAccountComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateTwitchData();
		}
	}
}
