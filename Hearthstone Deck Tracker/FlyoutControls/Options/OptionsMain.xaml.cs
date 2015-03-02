#region

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.Decks;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.General;
using Hearthstone_Deck_Tracker.FlyoutControls.Options.Other;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls
{
	/// <summary>
	/// Interaction logic for Options.xaml
	/// </summary>
	public partial class OptionsMain
	{
		public DecksGeneral OptionsDecksGeneral;
		public DecksOpponent OptionsDecksOpponent;
		public DecksPlayer OptionsDecksPlayer;
		public GeneralDeckWindows OptionsGeneralDeckWindows;
		public GeneralOther OptionsGeneralOther;
		public GeneralOverlay OptionsGeneralOverlay;
		public OtherExporting OptionsOtherExporting;
		public OtherImporting OptionsOtherImporting;
		public OtherLogging OptionsOtherLogging;
		public OtherStats OptionsOtherStats;
		public OtherTracker OptionsOtherTracker;

		public OptionsMain()
		{
			InitializeComponent();
			Helper.OptionsMain = this;
			OptionsGeneralOverlay = new GeneralOverlay();
			OptionsGeneralDeckWindows = new GeneralDeckWindows();
			OptionsGeneralOther = new GeneralOther();
			OptionsDecksGeneral = new DecksGeneral();
			OptionsDecksOpponent = new DecksOpponent();
			OptionsDecksPlayer = new DecksPlayer();
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
			OptionsGeneralOverlay.Load();
			OptionsGeneralOther.Load();
			OptionsGeneralDeckWindows.Load();
			OptionsDecksGeneral.Load();
			OptionsDecksPlayer.Load();
			OptionsDecksOpponent.Load();
			OptionsOtherTracker.Load();
			OptionsOtherExporting.Load();
			OptionsOtherImporting.Load();
			OptionsOtherLogging.Load();
			OptionsOtherStats.Load();
			OptionsOtherLogging.Load();
		}

		private void TreeViewItemGeneralOverlay_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsGeneralOverlay;
		}

		private void TreeViewItemGeneralDeckWindows_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsGeneralDeckWindows;
		}

		private void TreeViewItemGeneralOther_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsGeneralOther;
		}

		private void TreeViewItemDecksGeneral_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsDecksGeneral;
		}

		private void TreeViewItemDecksOpponent_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsDecksOpponent;
		}

		private void TreeViewItemDecksPlayer_OnSelected(object sender, RoutedEventArgs e)
		{
			ContentControlOptions.Content = OptionsDecksPlayer;
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
	}
}