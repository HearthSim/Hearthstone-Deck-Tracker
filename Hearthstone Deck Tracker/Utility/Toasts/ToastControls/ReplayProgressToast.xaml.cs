#region

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Utility.Extensions;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.Toasts.ToastControls
{
	/// <summary>
	/// Interaction logic for ReplayProgressToast.xaml
	/// </summary>
	public partial class ReplayProgressToast : INotifyPropertyChanged
	{
		private ReplayProgress _status = ReplayProgress.Uploading;
		private ProgressIndicatorState _progressState = ProgressIndicatorState.Working;

		public ReplayProgressToast()
		{
			InitializeComponent();
		}

		public string StatusText => Status.ToString().ToUpper() + (Status == ReplayProgress.Uploading ? "..." : "");

		public ProgressIndicatorState ProgressState
		{
			get { return _progressState; }
			set
			{
				if(_progressState == value)
					return;
				_progressState = value;
				OnPropertyChanged();
			}
		}

		public ReplayProgress Status
		{
			get { return _status; }
			set
			{
				_status = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(StatusText));
				switch(_status)
				{
					case ReplayProgress.Error:
						ProgressState = ProgressIndicatorState.Error;
						DelayedForceClose(2000).Forget();
						break;
					case ReplayProgress.Complete:
						ProgressState = ProgressIndicatorState.Success;
						ToastManager.ForceCloseToast(this);
						break;
				}
			}
		}

		private async Task DelayedForceClose(int ms)
		{
			await Task.Delay(ms);
			ToastManager.ForceCloseToast(this);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void ReplayProgressToast_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => ToastManager.ForceCloseToast(this);
	}
}
