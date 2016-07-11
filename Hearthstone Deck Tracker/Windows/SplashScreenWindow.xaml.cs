using System.ComponentModel;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Windows
{
	/// <summary>
	/// Interaction logic for SplashScreenWindow.xaml
	/// </summary>
	public partial class SplashScreenWindow : INotifyPropertyChanged
	{
		private string _loadingString = "Loading...";
		private string _versionString = Helper.GetCurrentVersion().ToVersionString();

		public SplashScreenWindow()
		{
			InitializeComponent();
		}

		public string VersionString
		{
			get { return _versionString; }
			set
			{
				_versionString = value;
				OnPropertyChanged();
			}
		}

		public string LoadingString
		{
			get { return _loadingString; }
			set
			{
				_loadingString = value;
				OnPropertyChanged();
			}
		}

		public void ShowConditional()
		{
			if(Config.Instance.ShowSplashScreen)
				Show();
		}

		public void Updating(int percentage)
		{
			LoadingString = "Updating...";
			VersionString = percentage + "%";
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}