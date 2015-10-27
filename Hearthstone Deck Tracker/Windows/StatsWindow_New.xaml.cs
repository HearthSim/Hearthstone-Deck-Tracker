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
			Height = Config.Instance.StatsWindowHeight;
			Width = Config.Instance.StatsWindowWidth;
			if(Config.Instance.StatsWindowLeft.HasValue)
				Left = Config.Instance.StatsWindowLeft.Value;
			if(Config.Instance.StatsWindowTop.HasValue)
				Top = Config.Instance.StatsWindowTop.Value;
		}

		private void BtnSwitchToMainWindow_OnClick(object sender, RoutedEventArgs e)
		{
			Config.Instance.StatsInWindow = false;
			Config.Save();
			ContentControl.Content = null;
			Core.MainWindow.StatsFlyoutContentControl.Content = Core.StatsOverview;
			Core.MainWindow.WindowState = WindowState.Normal;
			Core.MainWindow.Show();
			Core.MainWindow.Activate();
			Core.MainWindow.FlyoutNewStats.IsOpen = true;
			Core.StatsOverview.UpdateStats();
			Close();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if(!double.IsNaN(Left))
				Config.Instance.StatsWindowLeft = (int)Left;
			if(!double.IsNaN(Top))
				Config.Instance.StatsWindowTop = (int)Top;
			Config.Instance.StatsWindowHeight = (int)Height;
			Config.Instance.StatsWindowWidth = (int)Width;
			Config.Save();
			e.Cancel = true;
			Hide();
		}
	}
}
