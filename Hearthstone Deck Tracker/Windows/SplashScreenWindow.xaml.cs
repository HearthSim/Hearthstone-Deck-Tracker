using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class SplashScreenWindow : INotifyPropertyChanged
	{
		private string _loadingString = "Loading...";
		private string _versionString = Helper.GetCurrentVersion().ToVersionString();
		private Visibility _skipVisibility = Visibility.Collapsed;
		private SolidColorBrush _skipBackground = new SolidColorBrush(Colors.White);

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

		public Visibility SkipVisibility
		{
			get { return _skipVisibility; }
			set
			{
				if(value == _skipVisibility)
					return;
				_skipVisibility = value;
				OnPropertyChanged();
			}
		}

		public SolidColorBrush SkipBackground
		{
			get { return _skipBackground; }
			set
			{
				if(Equals(value, _skipBackground))
					return;
				_skipBackground = value;
				OnPropertyChanged();
			}
		}

		public bool SkipUpdate { get; set; }

		public void ShowConditional()
		{
			if(Config.Instance.ShowSplashScreen)
			{
				Log.Info("Showing splashscreen...");
				Show();
			}
		}

		public void Updating(int percentage)
		{
			if(SkipUpdate)
				return;
			LoadingString = "Updating...";
			VersionString = percentage + "%";
		}

		public void Installing(int percentage)
		{
			if(SkipUpdate)
				return;
			LoadingString = "Installing...";
			VersionString = percentage + "%";
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void SkipBorder_OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			Log.Info("Skip was pressed");
			SkipUpdate = true;
		}

		public async void StartSkipTimer()
		{
			await Task.Delay(10000);
			if(IsVisible)
				Log.Info("Showing skip button");
			SkipVisibility = Visibility.Visible;
		}

		private void SkipBorder_OnMouseEnter(object sender, MouseEventArgs e) => SkipBackground = new SolidColorBrush(Colors.LightBlue);

		private void SkipBorder_OnMouseLeave(object sender, MouseEventArgs e) => SkipBackground = new SolidColorBrush(Colors.White);
	}
}