using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Session;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class BattlegroundsSessionWindow
	{
		public BattlegroundsSessionViewModel BattlegroundsSessionViewModelVM => Core.Game.BattlegroundsSessionViewModel;

		public BattlegroundsSessionWindow()
		{
			InitializeComponent();
		}

		private void BattlegroundsSessionWindow_OnActivated(object sender, EventArgs e) => Topmost = true;

		private void BattlegroundsSessionWindow_OnDeactivated(object sender, EventArgs e)
		{
			if(!Config.Instance.WindowsTopmost)
				Topmost = false;
		}

		private void BattlegroundsSessionWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			BattlegroundsSessionViewModelVM.Update();
			UpdateScaling();
		}

		public void OnGameEnd()
		{
			if (Core.Game.Spectator)
				return;

			UpdateBattlegroundsSessionLayoutHeight();
		}

		public void UpdateScaling()
		{
			var scale = Config.Instance.OverlaySessionRecapScaling / 100;
			BattlegroundsSession.RenderTransform = new ScaleTransform(scale, scale);
			UpdateBattlegroundsSessionLayoutHeight();
		}

		public void UpdateBattlegroundsSessionLayoutHeight()
		{
			var toolbarHeight = 34;
			var scale = Config.Instance.OverlaySessionRecapScaling / 100;
			BattlegroundsSession.UpdateLayout();
			MaxHeight = toolbarHeight + BattlegroundsSession.BattlegroundsSessionPanel.ActualHeight * scale;
			MaxWidth = BattlegroundsSession.BattlegroundsSessionPanel.ActualWidth * scale;
			UpdateLayout();
		}

		private void BattlegroundsSessionWindow_OnClosing(object sender, CancelEventArgs e)
		{
			if(Core.IsShuttingDown)
				return;
			e.Cancel = true;
			Hide();
		}
	}
}
