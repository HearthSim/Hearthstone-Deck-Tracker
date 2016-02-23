#region

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using HDTHelper;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
			Help.TxtblockVersion.Text = "v" + Helper.GetCurrentVersion().ToVersionString();

			Core.TrayIcon.SetContextMenuProperty("autoSelectDeck", "Checked", Config.Instance.AutoDeckDetection);

			// Don't select the 'archived' class on load
			var selectedClasses = Config.Instance.SelectedDeckPickerClasses.Where(c => c.ToString() != "Archived").ToList();
			if(selectedClasses.Count == 0)
				selectedClasses.Add(HeroClassAll.All);

			DeckPickerList.SelectClasses(selectedClasses);
			DeckPickerList.SelectDeckType(Config.Instance.SelectedDeckType, true);
			DeckPickerList.UpdateAutoSelectToggleButton();

			SortFilterDecksFlyout.LoadTags(DeckList.Instance.AllTags);

			SortFilterDecksFlyout.SetSelectedTags(Config.Instance.SelectedTags);

			TagControlEdit.LoadTags(DeckList.Instance.AllTags.Where(tag => tag != "All" && tag != "None").ToList());
			SortFilterDecksFlyout.OperationSwitch.IsChecked = Config.Instance.TagOperation == TagFilerOperation.And;

			SortFilterDecksFlyout.ComboboxDeckSorting.SelectedItem = Config.Instance.SelectedDeckSorting;
			SortFilterDecksFlyout.CheckBoxSortByClass.IsChecked = Config.Instance.SortDecksByClass;
			SortFilterDecksFlyout.ComboboxDeckSortingArena.SelectedItem = Config.Instance.SelectedDeckSortingArena;
			SortFilterDecksFlyout.CheckBoxSortByClassArena.IsChecked = Config.Instance.SortDecksByClassArena;

			if(!Helper.EventKeys.Contains(Config.Instance.KeyPressOnGameStart))
				Config.Instance.KeyPressOnGameStart = "None";

			if(!Helper.EventKeys.Contains(Config.Instance.KeyPressOnGameEnd))
				Config.Instance.KeyPressOnGameEnd = "None";

			ManaCurveMyDecks.Visibility = Config.Instance.ManaCurveMyDecks ? Visibility.Visible : Visibility.Collapsed;

			Core.TrayIcon.SetContextMenuProperty("classCardsFirst", "Checked", Config.Instance.CardSortingClassFirst);
			Core.TrayIcon.SetContextMenuProperty("useNoDeck", "Checked", DeckList.Instance.ActiveDeck == null);


			DeckStatsFlyout.LoadConfig();
			GameDetailsFlyout.LoadConfig();
			Core.Windows.StatsWindow.StatsControl.LoadConfig();
			Core.Windows.StatsWindow.GameDetailsFlyout.LoadConfig();

			MenuItemCheckBoxSyncOnStart.IsChecked = Config.Instance.HearthStatsSyncOnStart;
			MenuItemCheckBoxAutoUploadDecks.IsChecked = Config.Instance.HearthStatsAutoUploadNewDecks;
			MenuItemCheckBoxAutoUploadGames.IsChecked = Config.Instance.HearthStatsAutoUploadNewGames;
			MenuItemCheckBoxAutoSyncBackground.IsChecked = Config.Instance.HearthStatsAutoSyncInBackground;
			MenuItemCheckBoxAutoDeleteDecks.IsChecked = Config.Instance.HearthStatsAutoDeleteDecks;
			MenuItemCheckBoxAutoDeleteGames.IsChecked = Config.Instance.HearthStatsAutoDeleteMatches;
		}

		public void ReloadTags()
		{
			SortFilterDecksFlyout.LoadTags(DeckList.Instance.AllTags);
			TagControlEdit.LoadTags(DeckList.Instance.AllTags.Where(tag => tag != "All" && tag != "None").ToList());
			MenuItemQuickSetTag.ItemsSource = TagControlEdit.Tags;
		}

		private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
		{
			var presentationsource = PresentationSource.FromVisual(this);
			if(presentationsource != null) // make sure it's connected
			{
				Helper.DpiScalingX = presentationsource.CompositionTarget.TransformToDevice.M11;
				Helper.DpiScalingY = presentationsource.CompositionTarget.TransformToDevice.M22;
			}
			LoadHearthStatsMenu();
			LoadAndUpdateDecks();
			UpdateFlyoutAnimationsEnabled();
		}

		public void UpdateFlyoutAnimationsEnabled()
		{
			foreach(var flyout in Helper.FindVisualChildren<Flyout>(Core.MainWindow))
				flyout.AreAnimationsEnabled = Config.Instance.UseAnimations;
		}

		internal async Task<bool> SetupProtocol()
		{
			if(!HDTProtocol.Verify())
			{
				var result =
					await
					this.ShowMessageAsync("Enable \"hdt\" protocol?",
					                      "The \"hdt\" protocol allows other processes and websites to directly communicate with HDT.",
					                      MessageDialogStyle.AffirmativeAndNegative);
				if(result == MessageDialogResult.Affirmative)
				{
					var procInfo = new ProcessStartInfo("HDTHelper.exe", "registerProtocol") {Verb = "runas", UseShellExecute = true};
					var proc = Process.Start(procInfo);
					await Task.Run(() => proc.WaitForExit());
				}
			}
			else
			{
				this.ShowMessage("Protocol already active",
				                 "The \"hdt\" protocol allows other processes and websites to directly communicate with HDT.").Forget();
			}
			if(HDTProtocol.Verify())
			{
				PipeServer.StartAll();
				return true;
			}
			return false;
		}
	}
}