using System;
using System.ComponentModel;
using System.Windows;

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

			if (_config.TimerWindowLeft.HasValue)
			{
				Left = config.TimerWindowLeft.Value;
			}
			if (_config.TimerWindowTop.HasValue)
			{
				Top = config.TimerWindowTop.Value;
			}
			Topmost = _config.TimerWindowTopmost;
		}

		public void Update(TimerEventArgs timerEventArgs)
		{
			LblTurnTime.Text = string.Format("{0:00}:{1:00}", (timerEventArgs.Seconds/60)%60, timerEventArgs.Seconds%60);
			LblPlayerTurnTime.Text = string.Format("{0:00}:{1:00}", (timerEventArgs.PlayerSeconds/60)%60,
			                                       timerEventArgs.PlayerSeconds%60);
			LblOpponentTurnTime.Text = string.Format("{0:00}:{1:00}", (timerEventArgs.OpponentSeconds/60)%60,
			                                         timerEventArgs.OpponentSeconds%60);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (_appIsClosing) return;
			e.Cancel = true;
			Hide();
		}


		internal void Shutdown()
		{
			_appIsClosing = true;
			Close();
		}

		private void MetroWindow_LocationChanged(object sender, EventArgs e)
		{
			_config.TimerWindowLeft = (int) Left;
			_config.TimerWindowTop = (int) Top;
		}

		private void MetroWindow_Activated(object sender, EventArgs e)
		{
			Topmost = true;
		}

		private void MetroWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (WindowState == WindowState.Minimized) return;
			_config.TimerWindowHeight = (int) Height;
			_config.TimerWindowWidth = (int) Width;
		}

		private void MetroWindow_Deactivated(object sender, EventArgs e)
		{
			if (!_config.TimerWindowTopmost)
				Topmost = false;
		}
	}
}
