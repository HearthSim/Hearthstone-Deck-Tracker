#region

using System.Windows.Controls;
using System.Windows.Navigation;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Information
{
	public partial class HsReplayStatisticsInfo : UserControl
	{
		public HsReplayStatisticsInfo()
		{
			InitializeComponent();
		}

		public string SampleReplayUrl => Helper.BuildHsReplayNetUrl("/decks/ZrydJsC1jKZ3TpSiFWQXNg", "updatenotes");

		private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e) => Helper.TryOpenUrl(e.Uri.AbsoluteUri);
	}
}
