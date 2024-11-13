using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class SplashScreenWindow : INotifyPropertyChanged
	{
		private const string LocLoading = "SplashScreen_Text_Loading";
		private const string LocUpdating = "SplashScreen_Text_Updating";
		private const string LocInstalling = "SplashScreen_Text_Installing";
		private readonly string _updating = LocUtil.Get(LocUpdating);
		private readonly string _installing = LocUtil.Get(LocInstalling);
		private string _loadingString = LocUtil.Get(LocLoading);
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
			LoadingString = _updating;
			VersionString = percentage + "%";
		}

		public void Installing(int percentage)
		{
			if(SkipUpdate)
				return;
			LoadingString = _installing;
			VersionString = percentage + "%";
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
