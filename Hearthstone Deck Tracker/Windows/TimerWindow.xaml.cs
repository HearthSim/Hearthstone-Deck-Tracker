#region

using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Brushes = System.Windows.Media.Brushes;

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

		internal void Update(TimerState timerState)
		{
			if((timerState.PlayerSeconds <= 0 && timerState.OpponentSeconds <= 0) || Core.Game.CurrentMode != Mode.GAMEPLAY)
				return;
			var seconds = (int)Math.Abs(timerState.Seconds);
			LblTurnTime.Text = double.IsPositiveInfinity(timerState.Seconds) ? "\u221E" : $"{(seconds / 60) % 60:00}:{seconds % 60:00}";
			LblTurnTime.Fill = timerState.Seconds < 0 ? Brushes.LimeGreen : Brushes.White;
			LblPlayerTurnTime.Text = $"{timerState.PlayerSeconds / 60 % 60:00}:{timerState.PlayerSeconds % 60:00}";
			LblOpponentTurnTime.Text = $"{timerState.OpponentSeconds / 60 % 60:00}:{timerState.OpponentSeconds % 60:00}";
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
