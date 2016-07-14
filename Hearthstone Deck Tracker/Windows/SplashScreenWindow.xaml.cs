using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Windows
{
	/// <summary>
	/// Interaction logic for SplashScreenWindow.xaml
	/// </summary>
	public partial class SplashScreenWindow
	{
		public SplashScreenWindow()
		{
			InitializeComponent();
		}

		public string VersionString => Helper.GetCurrentVersion().ToVersionString();

		public void ShowConditional()
		{
			if(Config.Instance.ShowSplashScreen)
				Show();
		}
	}
}