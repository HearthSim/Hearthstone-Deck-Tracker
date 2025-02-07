using System.Windows;
using System.Windows.Navigation;
using Hearthstone_Deck_Tracker.Windows;

namespace Hearthstone_Deck_Tracker.FlyoutControls
{
	public partial class HsReplayInfo
	{
		public HsReplayInfo()
		{
			InitializeComponent();
		}

		private void ButtonContinue_OnClick(object sender, RoutedEventArgs e)
		{
			if(this.ParentMainWindow() is {} window)
				window.FlyoutHsReplayNote.IsOpen = false;
			Config.Instance.DisplayHsReplayNoteLive = false;
			Config.Save();
		}

		private void Hyperlink_OnClick(object sender, RequestNavigateEventArgs e) => Helper.TryOpenUrl(e.Uri.AbsoluteUri);
	}
}
