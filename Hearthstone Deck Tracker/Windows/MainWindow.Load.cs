using System.Linq;
using System.Windows;
using MahApps.Metro.Controls;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class MainWindow
	{
		public void ReloadTags()
		{
			SortFilterDecksFlyout.LoadTags(DeckList.Instance.AllTags);
			TagControlEdit.LoadTags(DeckList.Instance.AllTags.Where(tag => tag != "All" && tag != "None").ToList());
		}

		private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
		{
			var presentationsource = PresentationSource.FromVisual(this);
			if(presentationsource != null) // make sure it's connected
			{
				Helper.DpiScalingX = presentationsource.CompositionTarget.TransformToDevice.M11;
				Helper.DpiScalingY = presentationsource.CompositionTarget.TransformToDevice.M22;
			}

			Options.Load(Core.Game);
			SortFilterDecksFlyout.LoadTags(DeckList.Instance.AllTags);
			SortFilterDecksFlyout.SetSelectedTags(Config.Instance.SelectedTags);
			TagControlEdit.LoadTags(DeckList.Instance.AllTags.Where(tag => tag != "All" && tag != "None").ToList());
			ManaCurveMyDecks.Visibility = Config.Instance.ManaCurveMyDecks ? Visibility.Visible : Visibility.Collapsed;
			Core.TrayIcon.MenuItemUseNoDeck.Checked = DeckList.Instance.ActiveDeck == null;
			UpdateMyGamesPanelVisibility();
			UpdateFlyoutAnimationsEnabled();
		}

		public void UpdateFlyoutAnimationsEnabled()
		{
			foreach(var flyout in Helper.FindVisualChildren<Flyout>(this))
				flyout.AreAnimationsEnabled = Config.Instance.UseAnimations;
		}
	}
}
