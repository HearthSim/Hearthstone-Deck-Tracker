#region

using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using MahApps.Metro.Controls;
using Point = System.Drawing.Point;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class MainWindow
	{
		internal void LoadConfigSettings()
		{
			if(Config.Instance.TrackerWindowTop.HasValue)
				Top = Config.Instance.TrackerWindowTop.Value;
			if(Config.Instance.TrackerWindowLeft.HasValue)
				Left = Config.Instance.TrackerWindowLeft.Value;

			if(Config.Instance.WindowHeight < 0)
				Config.Instance.Reset("WindowHeight");
			Height = Config.Instance.WindowHeight;
			if(Config.Instance.WindowWidth < 0)
				Config.Instance.Reset("WindowWidth");
			Width = Config.Instance.WindowWidth;
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

			if(Config.Instance.StartMinimized)
			{
				WindowState = WindowState.Minimized;
				if(Config.Instance.MinimizeToTray)
					MinimizeToTray();
			}

			Options.Load(Core.Game);

			Core.TrayIcon.MenuItemAutoSelect.Checked = Config.Instance.AutoDeckDetection;

			// Don't select the 'archived' class on load
			var selectedClasses = Config.Instance.SelectedDeckPickerClasses.Where(c => c.ToString() != "Archived").ToList();
			if(selectedClasses.Count == 0)
				selectedClasses.Add(HeroClassAll.All);

			DeckPickerList.SelectClasses(selectedClasses);
			DeckPickerList.SelectDeckType(Config.Instance.SelectedDeckPickerDeckType, true);

			SortFilterDecksFlyout.LoadTags(DeckList.Instance.AllTags);

			SortFilterDecksFlyout.SetSelectedTags(Config.Instance.SelectedTags);

			TagControlEdit.LoadTags(DeckList.Instance.AllTags.Where(tag => tag != "All" && tag != "None").ToList());
			SortFilterDecksFlyout.OperationSwitch.IsChecked = Config.Instance.TagOperation == TagFilerOperation.And;

			SortFilterDecksFlyout.ComboboxDeckSorting.SelectedItem = Config.Instance.SelectedDeckSorting;
			SortFilterDecksFlyout.CheckBoxSortByClass.IsChecked = Config.Instance.SortDecksByClass;
			SortFilterDecksFlyout.CheckBoxSortFavorites.IsChecked = Config.Instance.SortDecksFavoritesFirst;
			SortFilterDecksFlyout.ComboboxDeckSortingArena.SelectedItem = Config.Instance.SelectedDeckSortingArena;
			SortFilterDecksFlyout.CheckBoxSortByClassArena.IsChecked = Config.Instance.SortDecksByClassArena;

			ManaCurveMyDecks.Visibility = Config.Instance.ManaCurveMyDecks ? Visibility.Visible : Visibility.Collapsed;

			Core.TrayIcon.MenuItemClassCardsFirst.Checked = Config.Instance.CardSortingClassFirst;
			Core.TrayIcon.MenuItemUseNoDeck.Checked = DeckList.Instance.ActiveDeck == null;

			UpdateMyGamesPanelVisibility();
		}

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
			LoadAndUpdateDecks();
			UpdateFlyoutAnimationsEnabled();
		}

		public void UpdateFlyoutAnimationsEnabled()
		{
			foreach(var flyout in Helper.FindVisualChildren<Flyout>(Core.MainWindow))
				flyout.AreAnimationsEnabled = Config.Instance.UseAnimations;
		}
	}
}
