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

		private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e) => Helper.TryOpenUrl(e.Uri.AbsoluteUri);
	}
}