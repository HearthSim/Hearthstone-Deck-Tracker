﻿#region

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
			// TODO: Find a better way to interact with the MainWindow
			ContentControl.Content = null;
			Core.MainWindow.StatsFlyoutContentControl.Content = Core.StatsOverview;
			Core.MainWindow.WindowState = WindowState.Normal;
			Core.MainWindow.Show();
			Core.MainWindow.Activate();
			Core.MainWindow.FlyoutStats.IsOpen = true;
			Core.StatsOverview.UpdateStats();
			Close();
		}

		private void StatsWindow_OnClosing(object sender, CancelEventArgs e)
		{
			if(Core.IsShuttingDown)
			{
				if(!double.IsNaN(Left))
					Config.Instance.StatsWindowLeft = (int)Left;
				if(!double.IsNaN(Top))
					Config.Instance.StatsWindowTop = (int)Top;
				if(!double.IsNaN(Height) && Height > 0)
					Config.Instance.StatsWindowHeight = (int)Height;
				if(!double.IsNaN(Width) && Width > 0)
					Config.Instance.StatsWindowWidth = (int)Width;
			}
			else
			{
				e.Cancel = true;
				Hide();
			}
		}
	}
}
