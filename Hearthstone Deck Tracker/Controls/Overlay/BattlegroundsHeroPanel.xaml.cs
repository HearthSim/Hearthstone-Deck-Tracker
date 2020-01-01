using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class BattlegroundsHeroPanel : UserControl
	{
		public BattlegroundsHeroPanel()
		{
			InitializeComponent();
		}

		public int[] HeroIds { get; internal set; }

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
			Core.Overlay.HideBattlegroundsHeroPanel();
			Helper.OpenBattlegroundsHeroPicker(HeroIds);
		}
	}
}
