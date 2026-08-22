#region

using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Point = System.Drawing.Point;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	/// <summary>
	/// Interaction logic for StatsWindow.xaml
	/// </summary>
	public partial class StatsWindow
	{
		internal readonly MainWindow MainWindowParent;

		public StatsWindow(MainWindow mainWindowParent)
		{
			InitializeComponent();
			MainWindowParent = mainWindowParent;
			Height = Config.Instance.StatsWindowHeight;
			Width = Config.Instance.StatsWindowWidth;
			if(Config.Instance.StatsWindowLeft.HasValue)
				Left = Config.Instance.StatsWindowLeft.Value;
			if(Config.Instance.StatsWindowTop.HasValue)
				Top = Config.Instance.StatsWindowTop.Value;

			var titleBarCorners = new[]
			{
				new Point((int)Left + 5, (int)Top + 5),
				new Point((int)(Left + Width) - 5, (int)Top + 5),
				new Point((int)Left + 5, (int)(Top + TitlebarHeight) - 5),
				new Point((int)(Left + Width) - 5, (int)(Top + TitlebarHeight) - 5)
			};
			if(!Screen.AllScreens.Any(s => titleBarCorners.Any(c => s.WorkingArea.Contains(c))))
			{
				Top = 100;
				Left = 100;
			}
		}

		public Thickness TitleBarMargin => new Thickness(0, TitlebarHeight, 0, 0);

		private void BtnSwitchToMainWindow_OnClick(object sender, RoutedEventArgs e)
		{
			Config.Instance.StatsInWindow = false;
			Config.Save();
			ContentControl.Content = null;
			MainWindowParent.StatsFlyoutContentControl.Content = MainWindowParent.StatsOverview;
			MainWindowParent.WindowState = WindowState.Normal;
			MainWindowParent.Show();
			MainWindowParent.Activate();
			MainWindowParent.FlyoutStats.IsOpen = true;
			MainWindowParent.StatsOverview.UpdateStats();
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
