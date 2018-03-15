using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Windows.MainWindowControls
{
	public partial class CollectionSyncingBannerView : INotifyPropertyChanged
	{
		public CollectionSyncingBannerView()
		{
			InitializeComponent();
			HSReplayNetHelper.CollectionUploaded += Update;
			HSReplayNetHelper.CollectionAlreadyUpToDate += Update;
			HSReplayNetOAuth.LoggedOut += Update;
			HSReplayNetOAuth.Authenticated += Update;
			ScheduledTaskRunner.Instance.Schedule(() => OnPropertyChanged(nameof(SyncAge)), TimeSpan.FromMinutes(1));
		}

		public bool CollectionSynced => Account.Instance.CollectionState.Any();

		public bool IsAuthenticated => HSReplayNetOAuth.IsFullyAuthenticated;

		public string SyncAge => CollectionSynced
			? LocUtil.GetAge(Account.Instance.CollectionState.Values.Max(x => x.Date))
			: string.Empty;

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void Update()
		{
			OnPropertyChanged(nameof(SyncAge));
			OnPropertyChanged(nameof(CollectionSynced));
			OnPropertyChanged(nameof(IsAuthenticated));
		}
	}
}
