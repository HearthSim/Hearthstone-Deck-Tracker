using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Hearthstone_Deck_Tracker.Controls.Stats;
using Hearthstone_Deck_Tracker.Stats;

namespace Hearthstone_Deck_Tracker.Windows
{
	/// <summary>
	/// Interaction logic for TestWindow.xaml
	/// </summary>
	public partial class TestWindow
	{
		public TestWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			;
		}

		private void ButtonArenaReward_Click(object sender, RoutedEventArgs e)
		{
			var arenaRun = ArenaStats.DataGridArenaRuns.SelectedItem as ArenaRun;
			if(arenaRun != null)
			{
				var dialog = new ArenaRewardDialog();
				dialog.ShowDialog();
				if(dialog.ArenaRewards != null)
				{
					arenaRun.Deck.ArenaReward = dialog.ArenaRewards.Reward;
					DeckList.Save();
					ArenaStats.DataGridArenaRuns.Items.Refresh();
				}
			}
		}
	}
}
