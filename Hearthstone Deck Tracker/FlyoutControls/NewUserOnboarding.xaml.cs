using System.Windows;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using System.Windows.Media.Animation;

namespace Hearthstone_Deck_Tracker.FlyoutControls
{
	public partial class NewUserOnboarding
	{
		private bool working;

		public string WelcomeLabelText
			=> ConfigManager.PreviousVersion != null ? LocUtil.Get("NewUserOnboarding_WelcomeBackTo") : LocUtil.Get("NewUserOnboarding_WelcomeTo");

		public NewUserOnboarding()
		{
			InitializeComponent();
		}

		private void ButtonGo_OnClick(object sender, RoutedEventArgs e)
		{
			if(working)
				return;
			
			working = true;
			ButtonGo.IsEnabled = false;

			Core.MainWindow.SetNewUserOnboarding(false);

			Config.Instance.OnboardingSeen = true;
			Config.Save();

			HSReplayNetClientAnalytics.RunOnboarding("https://hsreplay.net/hdt/installed/").Forget();
		}

		internal async void Show()
		{
			if(Visibility == Visibility.Visible)
				return;
			Opacity = 0;
			Visibility = Visibility.Visible;
			var sb = (Storyboard)FindResource("StoryboardFadeIn");
			sb.Begin();
			await Task.Delay(sb.Duration.TimeSpan);
		}

		internal async void Hide()
		{
			if(Visibility != Visibility.Visible)
				return;
			Opacity = 1;
			var sb = (Storyboard)FindResource("StoryboardFadeOut");
			sb.Begin();
			await Task.Delay(sb.Duration.TimeSpan);
			Visibility = Visibility.Collapsed;
		}
	}
}
