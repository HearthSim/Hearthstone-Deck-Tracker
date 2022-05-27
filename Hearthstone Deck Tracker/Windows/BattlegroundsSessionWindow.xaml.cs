using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class BattlegroundsSessionWindow
	{
		private bool _appIsClosing;

		public BattlegroundsSessionWindow()
		{
			InitializeComponent();
			HideBannedTribes();
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

		private void BattlegroundsSessionWindow_OnActivated(object sender, EventArgs e) => Topmost = true;

		private void BattlegroundsSessionWindow_OnDeactivated(object sender, EventArgs e)
		{
			if(!Config.Instance.WindowsTopmost)
				Topmost = false;
		}

		private void BattlegroundsSessionWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			BattlegroundsSession.Update();
			UpdateSectionsVisibilities();
			BattlegroundsSession.RenderTransformOrigin = new Point(0.5, 0);
			UpdateScaling();
		}

		public void OnGameStart()
		{
			Update();
		}

		public void Update()
		{
			ShowBannedTribes();
			BattlegroundsSession.Update();
			UpdateBattlegroundsSessionLayoutHeight();
		}

		public void OnGameEnd()
		{
			var currentRating = Core.Game.CurrentGameStats?.BattlegroundsRatingAfter;
			BattlegroundsSession.BgRatingCurrent.Text = $"{currentRating:N0}";

			BattlegroundsSession.UpdateLatestGames();
			UpdateBattlegroundsSessionLayoutHeight();
		}

		public void ShowBannedTribes()
		{
			BattlegroundsSession.BgTribe1.Visibility = Visibility.Visible;
			BattlegroundsSession.BgTribe2.Visibility = Visibility.Visible;
			BattlegroundsSession.BgTribe3.Visibility = Visibility.Visible;
			BattlegroundsSession.BgTribe4.Visibility = Visibility.Visible;
			BattlegroundsSession.BgTribeWaiting.Visibility = Visibility.Collapsed;
		}

		public void HideBannedTribes()
		{
			BattlegroundsSession.BgTribe1.Visibility = Visibility.Collapsed;
			BattlegroundsSession.BgTribe2.Visibility = Visibility.Collapsed;
			BattlegroundsSession.BgTribe3.Visibility = Visibility.Collapsed;
			BattlegroundsSession.BgTribe4.Visibility = Visibility.Collapsed;
			BattlegroundsSession.BgTribeWaiting.Visibility = Visibility.Visible;
		}

		public void UpdateScaling()
		{
			var scale = Config.Instance.OverlaySessionRecapScaling / 100;
			BattlegroundsSession.RenderTransform = new ScaleTransform(scale, scale);
			UpdateBattlegroundsSessionLayoutHeight();
		}

		public void UpdateSectionsVisibilities()
		{
			BattlegroundsSession.BgBannedTribesSection.Visibility = Config.Instance.ShowSessionRecapMinionsBanned
				   ? Visibility.Visible
				   : Visibility.Collapsed;

			BattlegroundsSession.BgStartCurrentMMRSection.Visibility = Config.Instance.ShowSessionRecapStartCurrentMMR
				? Visibility.Visible
				: Visibility.Collapsed;

			BattlegroundsSession.BgLastestGamesSection.Visibility = Config.Instance.ShowSessionRecapLatestGames
				? Visibility.Visible
				: Visibility.Collapsed;

			UpdateBattlegroundsSessionLayoutHeight();
		}

		private void UpdateBattlegroundsSessionLayoutHeight()
		{
			var scale = Config.Instance.OverlaySessionRecapScaling / 100;
			BattlegroundsSession.UpdateLayout();
			BattlegroundsSession.Height =
				BattlegroundsSession.BattlegroundsSessionPanel.ActualHeight * scale;
			BattlegroundsSession.Width =
				BattlegroundsSession.BattlegroundsSessionPanel.ActualWidth * scale;
			UpdateLayout();
		}
	}
}
