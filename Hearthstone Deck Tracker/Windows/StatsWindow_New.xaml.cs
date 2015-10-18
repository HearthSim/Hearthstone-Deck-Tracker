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
using System.Windows.Shapes;
using Hearthstone_Deck_Tracker.Controls.Stats;
using Hearthstone_Deck_Tracker.Stats;

namespace Hearthstone_Deck_Tracker.Windows
{
	/// <summary>
	/// Interaction logic for StatsWindow_New.xaml
	/// </summary>
	public partial class StatsWindow_New
	{
		public StatsWindow_New()
		{
			InitializeComponent();
		}

		private bool _shutdown = true;
		private void TestWindow_OnClosed(object sender, EventArgs e)
		{
			if(_shutdown)
				Application.Current.Shutdown();
		}

		private void BtnSwitchToMainWindow_OnClick(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.WindowState = WindowState.Normal;
			Core.MainWindow.Show();
			Core.MainWindow.Activate();
			Core.MainWindow.FlyoutNewStats.IsOpen = true;
			_shutdown = false;
			Close();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			e.Cancel = true;
			Hide();
		}
	}
}
