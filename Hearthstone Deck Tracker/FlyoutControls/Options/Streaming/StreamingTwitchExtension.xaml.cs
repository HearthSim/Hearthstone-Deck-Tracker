using System;
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
		private bool _oAuthSuccess;
		private bool _oAuthError;
		private string _hsreplayUserName;
		private bool _twitchAccountLinked;
		private bool _twitchStreamLive;
		private List<TwitchAccount> _availableTwitchAccounts;
		private TwitchAccount _selectedTwitchUser;

		public StreamingTwitchExtension()
		{
			InitializeComponent();
			LiveDataManager.OnStreamingChecked += streaming => TwitchStreamLive = streaming;
		}

		public SolidColorBrush SelectedColor => Helper.BrushFromHex(Config.Instance.StreamingOverlayBackground);

		public ICommand AuthenticateCommand => new Command(async () =>
		{
			var success = await HSReplayNetOAuth.Authenticate();
			if(success)
			{
				success = await RefreshHsreplayAccount();
				RefreshTwitchAccounts();
			}
			OAuthSuccess = success;
			OAuthError = !success;
		});

		public bool OAuthSuccess
		{
			get => _oAuthSuccess; set
			{
				_oAuthSuccess = value; 
				OnPropertyChanged();
				OnPropertyChanged(nameof(SetupComplete));
			}
		}

		public bool OAuthError
		{
			get => _oAuthError; set
			{
				_oAuthError = value; 
				OnPropertyChanged();
			}
		}

		// ReSharper disable once InconsistentNaming
		public string HSReplayUserName
		{
			get => _hsreplayUserName;
			set
			{
				_hsreplayUserName = value; 
				OnPropertyChanged();
			}
		}

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

		public bool SetupComplete => OAuthSuccess && TwitchAccountLinked;

		public bool TwitchStreamLive
		{
			get => _twitchStreamLive;
			set
			{
				_twitchStreamLive = value; 
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

		public List<TwitchAccount> AvailableTwitchAccounts
		{
			get => _availableTwitchAccounts;
			set
			{
				_availableTwitchAccounts = value; 
				OnPropertyChanged();
				OnPropertyChanged(nameof(MultipleTwitchAccounts));
			}
		}

		public bool MultipleTwitchAccounts => AvailableTwitchAccounts?.Count > 1;

		public ICommand RefreshTwitchAccountsCommand => new Command(RefreshTwitchAccounts);

		public ICommand LinkTwitchAccountCommand => new Command(() =>
		{
			Helper.TryOpenUrl("https://hsreplay.net/account/social/connections/");
			AwaitingTwitchAccountConnection = true;
		});

		public bool AwaitingTwitchAccountConnection { get; private set; }

		public ICommand InstallTwitchExtensionCommand => new Command(() => Helper.TryOpenUrl("https://hsdecktracker.net/twitch/extension/"));

		public ICommand SetupGuideCommand => new Command(() => Helper.TryOpenUrl("https://hsdecktracker.net/twitch/setup/"));

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
			OAuthSuccess = HSReplayNetOAuth.IsAuthenticatedFor(Scope.ReadSocialAccounts);
			AvailableTwitchAccounts = HSReplayNetOAuth.TwitchUsers;
			var selected = Config.Instance.SelectedTwitchUser;
			SelectedTwitchUser = AvailableTwitchAccounts?.FirstOrDefault(x => x.Id == selected || selected == 0);
			TwitchAccountLinked = SelectedTwitchUser?.Id > 0;
			if(TwitchAccountLinked)
				TwitchStreamLive = await TwitchApi.IsStreaming(SelectedTwitchUser.Id);
		}

		internal void UpdateAccountName()
		{
			HSReplayUserName = HSReplayNetOAuth.AccountData?.Username;
		}

		private void TwitchAccountComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateTwitchData();
		}
	}
}
