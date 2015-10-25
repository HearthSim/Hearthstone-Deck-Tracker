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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker.Controls.Stats.Arena
{
	/// <summary>
	/// Interaction logic for ArenaAdvancedCharts.xaml
	/// </summary>
	public partial class ArenaAdvancedCharts : UserControl
	{
		public ArenaAdvancedCharts()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			ArenaStats.Instance.UpdateExpensiveArenaStats();
		}
	}
}
