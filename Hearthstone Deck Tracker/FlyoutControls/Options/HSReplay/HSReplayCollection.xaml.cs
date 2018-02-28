using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.HSReplay
{
	public partial class HSReplayCollection : INotifyPropertyChanged
	{
		private bool _loginButtonEnabled = true;
		private bool _collectionUpToDate;

		public HSReplayCollection()
		{
			InitializeComponent();
			HSReplayNetOAuth.Authenticated += Update;
			HSReplayNetOAuth.LoggedOut += Update;
			HSReplayNetHelper.CollectionSynced += () =>
			{
				CollectionUpToDate = CollectionSyncingEnabled;
				OnPropertyChanged(nameof(CollectionSynced));
				OnPropertyChanged(nameof(LastSyncDate));
			};
			ConfigWrapper.CollectionSyncingChanged += () =>
				OnPropertyChanged(nameof(CollectionSyncingEnabled));
			HSReplayNetHelper.Authenticating += EnableLoginButton;
		}

		private void EnableLoginButton(bool authenticating)
		{
			if(authenticating)
			{
				LoginButtonEnabled = false;
				Task.Run(async () =>
				{
					await Task.Delay(5000);
					LoginButtonEnabled = true;
				}).Forget();
			}
			else
				LoginButtonEnabled = true;
		}

		private void Update()
		{
			OnPropertyChanged(nameof(IsAuthenticated));
			OnPropertyChanged(nameof(CollectionSynced));
		}

		public bool IsAuthenticated => HSReplayNetOAuth.IsAuthenticated;

		public ICommand LoginCommand => new Command(async () => await HSReplayNetHelper.TryAuthenticate());

		public bool LoginButtonEnabled
		{
			get => _loginButtonEnabled;
			set
			{
				_loginButtonEnabled = value; 
				OnPropertyChanged();
			}
		}

		public bool CollectionSynced => Account.Instance.CollectionState.Any();

		public bool CollectionUpToDate
		{
			get => _collectionUpToDate;
			set
			{
				_collectionUpToDate = value; 
				OnPropertyChanged();
			}
		}

		public string LastSyncDate => CollectionSynced
			? Account.Instance.CollectionState.Values.Max(x => x.Date).ToShortDateString()
			: string.Empty;

		public object HSReplayDecksCommand => new Command(() =>
		{
			var url = Helper.BuildHsReplayNetUrl("decks", "options_collection");
			Helper.TryOpenUrl(url);
		});

		public bool CollectionSyncingEnabled
		{
			get => ConfigWrapper.CollectionSyncingEnabled;
			set => ConfigWrapper.CollectionSyncingEnabled = value;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
