using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for OverallStats.xaml
	/// </summary>
	public partial class OverallStats
	{
		public OverallStats()
		{
			InitializeComponent();
		}

		public void LoadStats()
		{
			/*DataGridOverallWinLoss.Items.Clear();
			DataGridOverallTotal.Items.Clear();
			var total = new List<GameStats>();
			foreach(var @class in Game.Classes)
			{
				var unassigned = DefaultDeckStats.Instance.GetDeckStats(@class).Games;
				var assigned = Helper.MainWindow.DeckList.DecksList.Where(x => x.Class == @class).SelectMany(d => d.DeckStats.Games);
				var allGames = unassigned.Concat(assigned).ToList();
				total.AddRange(allGames);
				DataGridOverallWinLoss.Items.Add(new DeckStatsControl.WinLoss(allGames, CheckboxPercent.IsChecked ?? true, @class));
			}

			DataGridOverallTotal.Items.Add(new DeckStatsControl.WinLoss(total, "%"));
			DataGridOverallTotal.Items.Add(new DeckStatsControl.WinLoss(total, "Win - Loss"));*/

		}

		private void CheckboxPercent_Checked(object sender, RoutedEventArgs e)
		{
			LoadStats();
		}

		private void CheckboxPercent_Unchecked(object sender, RoutedEventArgs e)
		{
			LoadStats();
		}
	}
}
