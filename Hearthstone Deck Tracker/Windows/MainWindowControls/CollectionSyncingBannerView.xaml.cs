using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Windows.MainWindowControls
{
	public partial class CollectionSyncingBannerView : INotifyPropertyChanged
	{
		public static readonly DependencyProperty ContainerProperty = DependencyProperty.Register("Container", typeof(Window),
			typeof(CollectionSyncingBannerView), new FrameworkPropertyMetadata(OnContainerChanged));

		private static void OnContainerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if(!(d is CollectionSyncingBannerView banner))
				return;
			if(e.NewValue != null)
				((Window)e.NewValue).Activated += (s, a) => banner.Update();
		}

		public CollectionSyncingBannerView()
		{
			InitializeComponent();
			HSReplayNetHelper.CollectionUploaded += Update;
			HSReplayNetOAuth.LoggedOut += Update;
			HSReplayNetOAuth.Authenticated += Update;
		}

		public bool CollectionSynced => Account.Instance.CollectionState.Any();

		public bool IsAuthenticated => HSReplayNetOAuth.IsFullyAuthenticated;

		public string SyncAge => CollectionSynced
			? LocUtil.GetAge(Account.Instance.CollectionState.Values.Max(x => x.Date))
			: string.Empty;

		public Window Container
		{
			get => (Window) GetValue(ContainerProperty);
			set => SetValue(ContainerProperty, value);
		}

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
