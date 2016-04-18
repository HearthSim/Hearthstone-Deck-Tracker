#region

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.Toasts.ToastControls
{
	/// <summary>
	/// Interaction logic for ReplayProgressToast.xaml
	/// </summary>
	public partial class ReplayProgressToast : INotifyPropertyChanged
	{
		private ReplayProgress _status = ReplayProgress.Converting;

		public ReplayProgressToast()
		{
			InitializeComponent();
		}

		public string StatusText => Status.ToString().ToUpper() + (Status == ReplayProgress.Complete ? "" : "...");

		public ReplayProgress Status
		{
			get { return _status; }
			set
			{
				_status = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(StatusText));
				if(_status == ReplayProgress.Complete)
					ToastManager.ForceCloseToast(this);
			}
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