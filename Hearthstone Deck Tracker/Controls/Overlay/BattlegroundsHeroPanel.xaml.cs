using System.Collections.Generic;
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

		public int[]? HeroIds { get; internal set; }
		public bool Duos { get; internal set;  }
		public int? AnomalyDbfId { get; internal set; }
		public Dictionary<string, string>? Parameters { get; internal set; }

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
			if(HeroIds != null)
				Helper.OpenBattlegroundsHeroPicker(HeroIds, Duos, AnomalyDbfId, Parameters);
		}
	}
}
