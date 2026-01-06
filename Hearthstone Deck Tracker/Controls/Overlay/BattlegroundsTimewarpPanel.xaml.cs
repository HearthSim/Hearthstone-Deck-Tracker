using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using HearthMirror.Objects;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class BattlegroundsTimewarpPanel : UserControl
	{
		public BattlegroundsTimewarpPanel()
		{
			InitializeComponent();
		}

		public List<BoardCard>? BoardCards { get; internal set; }

		private void UserControl_MouseEnter(object sender, MouseEventArgs e)
		{
			(FindResource("StoryboardHover") as Storyboard)?.Begin();
		}

		private void UserControl_MouseLeave(object sender, MouseEventArgs e)
		{
			(FindResource("StoryboardNormal") as Storyboard)?.Begin();
		}

		private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if(BoardCards != null && BoardCards.Count > 0)
				Helper.OpenBattlegroundsTimewarpPage(BoardCards);
		}
	}
}
