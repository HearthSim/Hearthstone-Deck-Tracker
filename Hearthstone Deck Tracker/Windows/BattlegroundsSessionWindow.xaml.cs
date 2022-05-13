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
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if(_appIsClosing)
				return;
			e.Cancel = true;
			Hide();
			Config.Instance.BattlegroundsSessionRecapWindowOnStart = false;
			Config.Save();
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
			BattlegroundsSession.RenderTransformOrigin = new Point(1, 0);
			UpdateScaling();
		}

		public void OnGameStart()
		{
			UpdateBannedMinionsVisibility();
			BattlegroundsSession.Update();
			UpdateBattlegroundsSessionLayoutHeight();
		}

		public void OnGameEnd()
		{
			BattlegroundsSession.BgBannedTribesSection.Visibility = Visibility.Collapsed;

			var currentRating = Core.Game.CurrentGameStats?.BattlegroundsRatingAfter;
			BattlegroundsSession.BgRatingCurrent.Text = $"{currentRating:N0}";

			BattlegroundsSession.UpdateLatestGames();
			UpdateBattlegroundsSessionLayoutHeight();
		}

		public void UpdateScaling()
		{
			var scale = Config.Instance.OverlaySessionRecapScaling / 100;
			BattlegroundsSession.RenderTransform = new ScaleTransform(scale, scale);
			UpdateBattlegroundsSessionLayoutHeight();
		}

		public void UpdateSectionsVisibilities()
		{
			UpdateBannedMinionsVisibility();

			BattlegroundsSession.BgStartCurrentMMRSection.Visibility = Config.Instance.ShowSessionRecapStartCurrentMMR
				? Visibility.Visible
				: Visibility.Collapsed;

			BattlegroundsSession.BgLastestGamesSection.Visibility = Config.Instance.ShowSessionRecapLatestGames
				? Visibility.Visible
				: Visibility.Collapsed;

			UpdateBattlegroundsSessionLayoutHeight();
		}

		private void UpdateBannedMinionsVisibility()
		{
			var shouldShow = Config.Instance.ShowSessionRecapMinionsBanned && Core.Game.IsBattlegroundsMatch;
			BattlegroundsSession.BgBannedTribesSection.Visibility = shouldShow
				   ? Visibility.Visible
				   : Visibility.Collapsed;
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
