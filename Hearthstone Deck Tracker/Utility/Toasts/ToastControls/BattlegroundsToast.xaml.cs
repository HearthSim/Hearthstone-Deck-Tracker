using System.Collections.Generic;
using System.Windows.Media.Animation;

namespace Hearthstone_Deck_Tracker.Utility.Toasts.ToastControls
{
	public partial class BattlegroundsToast : System.Windows.Controls.UserControl
	{
		private int[] _heroDbfIds;
		private int? _anomalyDbfId;
		private Dictionary<string, string>? _parameters;

		public BattlegroundsToast(int[] heroDbfIds, int? anomalyDbfId, Dictionary<string, string>? parameters)
		{
			InitializeComponent();
			_heroDbfIds = heroDbfIds;
			_anomalyDbfId = anomalyDbfId;
			_parameters = parameters;
		}

		private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			(FindResource("StoryboardHover") as Storyboard)?.Begin();
		}

		private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			(FindResource("StoryboardNormal") as Storyboard)?.Begin();
		}

		private void UserControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			Helper.OpenBattlegroundsHeroPicker(_heroDbfIds, _anomalyDbfId, _parameters);
		}
	}
}
