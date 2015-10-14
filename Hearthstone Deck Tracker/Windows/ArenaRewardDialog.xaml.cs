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
using MahApps.Metro.Controls;

namespace Hearthstone_Deck_Tracker.Windows
{
	/// <summary>
	/// Interaction logic for ArenaRewardDialog.xaml
	/// </summary>
	public partial class ArenaRewardDialog
	{
		public ArenaRewardDialog()
		{
			InitializeComponent();
		}
		public ArenaReward Reward { get; set; }
		private async void ButtonSave_OnClick(object sender, RoutedEventArgs e)
		{
			string warning;
			if(!ArenaRewards.Validate(out warning))
			{
				await this.ShowMessage("Error saving Arena Rewards", warning);
				return;
			}
			Reward = ArenaRewards.Reward;
			Close();
		}
	}
}
