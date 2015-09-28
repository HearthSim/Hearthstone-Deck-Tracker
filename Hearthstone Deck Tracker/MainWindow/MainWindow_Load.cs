#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using HDTHelper;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Point = System.Drawing.Point;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public partial class MainWindow
	{
		public void CopyReplayFiles()
		{
			if(Config.Instance.SaveDataInAppData == null)
				return;
			var appDataReplayDirPath = Config.Instance.AppDataPath + @"\Replays";
			var dataReplayDirPath = Config.Instance.DataDirPath + @"\Replays";
			if(Config.Instance.SaveDataInAppData.Value)
			{
				if(Directory.Exists(dataReplayDirPath))
				{
					//backup in case the file already exists
					var time = DateTime.Now.ToFileTime();
					if(Directory.Exists(appDataReplayDirPath))
					{
						Helper.CopyFolder(appDataReplayDirPath, appDataReplayDirPath + time);
						Directory.Delete(appDataReplayDirPath, true);
						Logger.WriteLine("Created backups of replays in appdata", "Load");
					}


					Helper.CopyFolder(dataReplayDirPath, appDataReplayDirPath);
					Directory.Delete(dataReplayDirPath, true);

					Logger.WriteLine("Moved replays to appdata", "Load");
				}
			}
			else if(Directory.Exists(appDataReplayDirPath)) //Save in DataDir and AppData Replay dir still exists
			{
				//backup in case the file already exists
				var time = DateTime.Now.ToFileTime();
				if(Directory.Exists(dataReplayDirPath))
				{
					Helper.CopyFolder(dataReplayDirPath, dataReplayDirPath + time);
					Directory.Delete(dataReplayDirPath, true);
				}
				Logger.WriteLine("Created backups of replays locally", "Load");


				Helper.CopyFolder(appDataReplayDirPath, dataReplayDirPath);
				Directory.Delete(appDataReplayDirPath, true);
				Logger.WriteLine("Moved replays to appdata", "Load");
			}
		}

		private void LoadConfig()
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

			var theme = string.IsNullOrEmpty(Config.Instance.ThemeName)
				            ? ThemeManager.DetectAppStyle().Item1 : ThemeManager.AppThemes.First(t => t.Name == Config.Instance.ThemeName);
			var accent = string.IsNullOrEmpty(Config.Instance.AccentName)
				             ? ThemeManager.DetectAppStyle().Item2 : ThemeManager.Accents.First(a => a.Name == Config.Instance.AccentName);
			ThemeManager.ChangeAppStyle(Application.Current, accent, theme);


			
			Application.Current.Resources["GrayTextColorBrush"] = theme.Name == "BaseLight"
				                                                           ? new SolidColorBrush((Color)Application.Current.Resources["GrayTextColor1"])
				                                                           : new SolidColorBrush((Color)Application.Current.Resources["GrayTextColor2"]);

			Options.Load(Core.Game);


			CheckboxDeckDetection.IsChecked = Config.Instance.AutoDeckDetection;
			Core.TrayIcon.SetContextMenuProperty("autoSelectDeck", "Checked", (bool)CheckboxDeckDetection.IsChecked);

			// Don't select the 'archived' class on load
			var selectedClasses = Config.Instance.SelectedDeckPickerClasses.Where(c => c.ToString() != "Archived").ToList();
			if(selectedClasses.Count == 0)
				selectedClasses.Add(HeroClassAll.All);

			DeckPickerList.SelectClasses(selectedClasses);
			DeckPickerList.SelectDeckType(Config.Instance.SelectedDeckType, true);

			SortFilterDecksFlyout.LoadTags(DeckList.Instance.AllTags);

			UpdateQuickFilterItemSource();

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
			//ManaCurveMyDecks.ListViewStatType.SelectedIndex = (int)Config.Instance.ManaCurveFilter;

			CheckboxClassCardsFirst.IsChecked = Config.Instance.CardSortingClassFirst;
			Core.TrayIcon.SetContextMenuProperty("classCardsFirst", "Checked", (bool)CheckboxClassCardsFirst.IsChecked);
			Core.TrayIcon.SetContextMenuProperty("useNoDeck", "Checked", DeckList.Instance.ActiveDeck == null);


			DeckStatsFlyout.LoadConfig(Core.Game);
			GameDetailsFlyout.LoadConfig(Core.Game);
			Core.Windows.StatsWindow.StatsControl.LoadConfig(Core.Game);
			Core.Windows.StatsWindow.GameDetailsFlyout.LoadConfig(Core.Game);

			MenuItemCheckBoxSyncOnStart.IsChecked = Config.Instance.HearthStatsSyncOnStart;
			MenuItemCheckBoxAutoUploadDecks.IsChecked = Config.Instance.HearthStatsAutoUploadNewDecks;
			MenuItemCheckBoxAutoUploadGames.IsChecked = Config.Instance.HearthStatsAutoUploadNewGames;
			MenuItemCheckBoxAutoSyncBackground.IsChecked = Config.Instance.HearthStatsAutoSyncInBackground;
			MenuItemCheckBoxAutoDeleteDecks.IsChecked = Config.Instance.HearthStatsAutoDeleteDecks;
			MenuItemCheckBoxAutoDeleteGames.IsChecked = Config.Instance.HearthStatsAutoDeleteMatches;
		}

		public void UpdateQuickFilterItemSource()
		{
			MenuItemQuickSelectFilter.ItemsSource =
				DeckList.Instance.AllTags.Where(
				                                t =>
				                                DeckList.Instance.Decks.Any(
				                                                            d =>
				                                                            d.Tags.Contains(t) || t == "All" || t == "None" && d.Tags.Count == 0))
				        .Select(x => x.ToUpperInvariant());
		}

		public void ReloadTags()
		{
			SortFilterDecksFlyout.LoadTags(DeckList.Instance.AllTags);
			TagControlEdit.LoadTags(DeckList.Instance.AllTags.Where(tag => tag != "All" && tag != "None").ToList());
			MenuItemQuickSetTag.ItemsSource = TagControlEdit.Tags;
		}

		private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
		{
			var presentationsource = PresentationSource.FromVisual(this);
			if(presentationsource != null) // make sure it's connected
			{
				Helper.DpiScalingX = presentationsource.CompositionTarget.TransformToDevice.M11;
				Helper.DpiScalingY = presentationsource.CompositionTarget.TransformToDevice.M22;
			}
			ManaCurveMyDecks.UpdateValues();
			if(ConfigManager.UpdatedVersion != null)
			{
				FlyoutUpdateNotes.IsOpen = true;
				UpdateNotesControl.LoadUpdateNotes();
				//await this.ShowUpdateNotesMessage();
			}

			if(!Helper.FoundHearthstoneDir)
				await this.ShowHsNotInstalledMessage();
			else if(Helper.UpdateLogConfig && Core.Game.IsRunning)
			{
				await
					this.ShowMessage("Restart Hearthstone",
					                 "This is either your first time starting HDT or the log.config file has been updated. Please restart Hearthstone, for HDT to work properly.");
			}


			if(Config.Instance.HearthStatsSyncOnStart && HearthStatsAPI.IsLoggedIn)
				HearthStatsManager.SyncAsync(background: true);

			//SetupProtocol(); turn on later
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
					var procInfo = new ProcessStartInfo("HDTHelper.exe", "registerProtocol");
					procInfo.Verb = "runas";
					procInfo.UseShellExecute = true;
					var proc = Process.Start(procInfo);
					await Task.Run(() => proc.WaitForExit());
				}
			}
			else
			{
				this.ShowMessage("Protocol already active",
				                 "The \"hdt\" protocol allows other processes and websites to directly communicate with HDT.");
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