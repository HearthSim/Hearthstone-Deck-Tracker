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
			UpdateScaling();
		}

		public void OnGameStart()
		{
			Update();
		}

		public void Update()
		{
			BattlegroundsSession.Update();
			UpdateBattlegroundsSessionLayoutHeight();
		}

		public void OnGameEnd()
		{
			if (Core.Game.Spectator)
				return;

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
			var toolbarHeight = 34;
			var scale = Config.Instance.OverlaySessionRecapScaling / 100;
			BattlegroundsSession.UpdateLayout();
			MaxHeight = toolbarHeight + BattlegroundsSession.BattlegroundsSessionPanel.ActualHeight * scale;
			MaxWidth = BattlegroundsSession.BattlegroundsSessionPanel.ActualWidth * scale;
			UpdateLayout();
		}
	}
}
