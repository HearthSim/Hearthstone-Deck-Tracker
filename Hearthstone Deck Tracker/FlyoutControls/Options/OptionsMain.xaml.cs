#region

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.Other;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls
{
	/// <summary>
	/// Interaction logic for Options.xaml
	/// </summary>
	public partial class OptionsMain
	{
		public OtherExporting OptionsOtherExporting;
		public OtherImporting OptionsOtherImporting;
		public OtherLogging OptionsOtherLogging;
		public OtherStats OptionsOtherStats;
		public OtherTracker OptionsOtherTracker;
		public OverlayDeckWindows OptionsOverlayDeckWindows;
		public OverlayGeneral OptionsOverlayGeneral;
		public OverlayInteractivity OptionsOverlayInteractivity;
		public OverlayOpponent OptionsOverlayOpponent;
		public OverlayOther OptionsOverlayOther;
		public OverlayPlayer OptionsOverlayPlayer;

		public OptionsMain()
		{
			InitializeComponent();
			Helper.OptionsMain = this;
			OptionsOverlayGeneral = new OverlayGeneral();
			OptionsOverlayDeckWindows = new OverlayDeckWindows();
			OptionsOverlayOther = new OverlayOther();
			OptionsOverlayOpponent = new OverlayOpponent();
			OptionsOverlayPlayer = new OverlayPlayer();
			OptionsOverlayInteractivity = new OverlayInteractivity();
			OptionsOtherExporting = new OtherExporting();
			OptionsOtherImporting = new OtherImporting();
			OptionsOtherLogging = new OtherLogging();
			OptionsOtherStats = new OtherStats();
			OptionsOtherExporting = new OtherExporting();
			OptionsOtherTracker = new OtherTracker();
			foreach(var treeItem in TreeViewOptions.Items.Cast<TreeViewItem>())
				treeItem.ExpandSubtree();
		}

		public void Load()
		{
			OptionsOverlayGeneral.Load();
			OptionsOverlayOther.Load();
			OptionsOverlayDeckWindows.Load();
			OptionsOverlayPlayer.Load();
			OptionsOverlayOpponent.Load();
			OptionsOverlayInteractivity.Load();
			OptionsOtherTracker.Load();
			OptionsOtherExporting.Load();
			OptionsOtherImporting.Load();
			OptionsOtherLogging.Load();
			OptionsOtherStats.Load();
			OptionsOtherLogging.Load();
		}

		private void TreeViewItemGeneralOverlay_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsOverlayGeneral;
		}

		private void TreeViewItemOverlayDeckWindows_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsOverlayDeckWindows;
		}

		private void TreeViewItemOverlayOther_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsOverlayOther;
		}

		private void TreeViewItemOverlayOpponent_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsOverlayOpponent;
		}

		private void TreeViewItemOverlayPlayer_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsOverlayPlayer;
		}

		private void TreeViewItemOtherTracker_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsOtherTracker;
		}

		private void TreeViewItemOtherStats_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsOtherStats;
		}

		private void TreeViewItemOtherExporting_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsOtherExporting;
		}

		private void TreeViewItemOtherImporting_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsOtherImporting;
		}

		private void TreeViewItemOtherLogging_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsOtherLogging;
		}

		private void TreeViewItemOverlayInteractivity_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsOverlayInteractivity;
		}
	}
}