using System.ComponentModel;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Hearthstone_Deck_Tracker.Utility.Updating;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class SplashScreenWindow
	{
		public SplashScreenWindow()
		{
			InitializeComponent();
		}

		public void ShowConditional()
		{
			if(Config.Instance.ShowSplashScreen)
			{
				Log.Info("Showing splashscreen...");
				Show();
			}
		}
	}

	public class SplashScreenWindowViewModel : ViewModel
	{
		private const string LocLoading = "SplashScreen_Text_Loading";
		private const string LocUpdating = "SplashScreen_Text_Updating";
		private const string LocInstalling = "SplashScreen_Text_Installing";

		public SplashScreenWindowViewModel()
		{
			Updater.Status.PropertyChanged += OnStatusOnPropertyChanged;
		}

		private void OnStatusOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if(args.PropertyName == nameof(Updater.Status.UpdaterState))
			{
				if(Updater.Status.UpdaterState == UpdaterState.Downloading)
				{
					LoadingString = LocUtil.Get(LocUpdating);
					VersionString = $"{Updater.Status.UpdateProgress}%";
				}
				else if(Updater.Status.UpdaterState == UpdaterState.Installing)
				{
					LoadingString = LocUtil.Get(LocInstalling);
					VersionString = $"{Updater.Status.UpdateProgress}%";
				}
			}
			else if(args.PropertyName == nameof(Updater.Status.UpdateProgress))
			{
				if(Updater.Status.UpdaterState == UpdaterState.Downloading
				   || Updater.Status.UpdaterState == UpdaterState.Installing)
				{
					VersionString = $"{Updater.Status.UpdateProgress}%";
				}
				else
				{
					VersionString = Helper.GetCurrentVersion().ToVersionString();
				}
			}
		}

		public string? VersionString
		{
			get => GetProp(Helper.GetCurrentVersion().ToVersionString());
			set => SetProp(value);
		}

		public string? LoadingString
		{
			get => GetProp(LocUtil.Get(LocLoading));
			set => SetProp(value);
		}
	}
}
