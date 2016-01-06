#region

using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for TimerWindow.xaml
	/// </summary>
	public partial class TimerWindow
	{
		private readonly Config _config;
		private bool _appIsClosing;

		public TimerWindow(Config config)
		{
			InitializeComponent();
			_config = config;

			Height = _config.TimerWindowHeight;
			Width = _config.TimerWindowWidth;

			if(_config.TimerWindowLeft.HasValue)
				Left = config.TimerWindowLeft.Value;
			if(_config.TimerWindowTop.HasValue)
				Top = config.TimerWindowTop.Value;
			Topmost = _config.TimerWindowTopmost;

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

		public void Update(TimerEventArgs timerEventArgs)
		{
			LblTurnTime.Text = $"{(timerEventArgs.Seconds / 60) % 60:00}:{timerEventArgs.Seconds % 60:00}";
			LblPlayerTurnTime.Text = $"{(timerEventArgs.PlayerSeconds / 60) % 60:00}:{timerEventArgs.PlayerSeconds % 60:00}";
			LblOpponentTurnTime.Text = $"{(timerEventArgs.OpponentSeconds / 60) % 60:00}:{timerEventArgs.OpponentSeconds % 60:00}";
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if(_appIsClosing)
				return;
			e.Cancel = true;
			Hide();
		}

		internal void Shutdown()
		{
			_appIsClosing = true;
			Close();
		}

		private void MetroWindow_Activated(object sender, EventArgs e) => Topmost = true;

		private void MetroWindow_Deactivated(object sender, EventArgs e)
		{
			if(!_config.TimerWindowTopmost)
				Topmost = false;
		}
	}
}