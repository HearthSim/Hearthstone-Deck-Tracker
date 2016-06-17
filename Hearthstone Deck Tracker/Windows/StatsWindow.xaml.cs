#region

using System.ComponentModel;
using System.Windows;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	/// <summary>
	/// Interaction logic for StatsWindow.xaml
	/// </summary>
	public partial class StatsWindow
	{
		public StatsWindow()
		{
			InitializeComponent();
			Height = Config.Instance.StatsWindowHeight;
			Width = Config.Instance.StatsWindowWidth;
			if(Config.Instance.StatsWindowLeft.HasValue)
				Left = Config.Instance.StatsWindowLeft.Value;
			if(Config.Instance.StatsWindowTop.HasValue)
				Top = Config.Instance.StatsWindowTop.Value;
		}

		public Thickness TitleBarMargin => new Thickness(0, TitlebarHeight, 0, 0);

		private void BtnSwitchToMainWindow_OnClick(object sender, RoutedEventArgs e)
		{
			Config.Instance.StatsInWindow = false;
			Config.Save();
			ContentControl.Content = null;
			Core.MainWindow.StatsFlyoutContentControl.Content = Core.StatsOverview;
			Core.MainWindow.WindowState = WindowState.Normal;
			Core.MainWindow.Show();
			Core.MainWindow.Activate();
			Core.MainWindow.FlyoutStats.IsOpen = true;
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