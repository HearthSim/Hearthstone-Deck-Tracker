using System.ComponentModel;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.HSReplay
{
	public partial class HSReplayReplays : INotifyPropertyChanged
	{
		public HSReplayReplays()
		{
			InitializeComponent();
			ConfigWrapper.ReplayAutoUploadChanged += () => OnPropertyChanged(nameof(HsReplayAutoUpload));
		}

		public bool HsReplayAutoUpload
		{
			get => ConfigWrapper.HsReplayAutoUpload;
			set => ConfigWrapper.HsReplayAutoUpload = value;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
