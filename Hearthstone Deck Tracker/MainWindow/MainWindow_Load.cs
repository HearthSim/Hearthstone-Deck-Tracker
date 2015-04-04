﻿#region

using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro;
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

		public void SetupDeckStatsFile()
		{
			if(Config.Instance.SaveDataInAppData == null)
				return;
			var appDataPath = Config.Instance.AppDataPath + @"\DeckStats.xml";
			var appDataGamesDirPath = Config.Instance.AppDataPath + @"\Games";
			var dataDirPath = Config.Instance.DataDirPath + @"\DeckStats.xml";
			var dataGamesDirPath = Config.Instance.DataDirPath + @"\Games";
			if(Config.Instance.SaveDataInAppData.Value)
			{
				if(File.Exists(dataDirPath))
				{
					if(File.Exists(appDataPath))
					{
						//backup in case the file already exists
						var time = DateTime.Now.ToFileTime();
						File.Move(appDataPath, appDataPath + time);
						if(Directory.Exists(appDataGamesDirPath))
						{
							Helper.CopyFolder(appDataGamesDirPath, appDataGamesDirPath + time);
							Directory.Delete(appDataGamesDirPath, true);
						}
						Logger.WriteLine("Created backups of DeckStats and Games in appdata", "Load");
					}
					File.Move(dataDirPath, appDataPath);
					Logger.WriteLine("Moved DeckStats to appdata", "Load");
					if(Directory.Exists(dataGamesDirPath))
					{
						Helper.CopyFolder(dataGamesDirPath, appDataGamesDirPath);
						Directory.Delete(dataGamesDirPath, true);
					}
					Logger.WriteLine("Moved Games to appdata", "Load");
				}
			}
			else if(File.Exists(appDataPath))
			{
				if(File.Exists(dataDirPath))
				{
					//backup in case the file already exists
					var time = DateTime.Now.ToFileTime();
					File.Move(dataDirPath, dataDirPath + time);
					if(Directory.Exists(dataGamesDirPath))
					{
						Helper.CopyFolder(dataGamesDirPath, dataGamesDirPath + time);
						Directory.Delete(dataGamesDirPath, true);
					}
					Logger.WriteLine("Created backups of deckstats and games locally", "Load");
				}
				File.Move(appDataPath, dataDirPath);
				Logger.WriteLine("Moved DeckStats to local", "Load");
				if(Directory.Exists(appDataGamesDirPath))
				{
					Helper.CopyFolder(appDataGamesDirPath, dataGamesDirPath);
					Directory.Delete(appDataGamesDirPath, true);
				}
				Logger.WriteLine("Moved Games to appdata", "Load");
			}

			var filePath = Config.Instance.DataDir + "DeckStats.xml";
			//create file if it does not exist
			if(!File.Exists(filePath))
			{
				using(var sr = new StreamWriter(filePath, false))
					sr.WriteLine("<DeckStatsList></DeckStatsList>");
			}
		}

		// Logic for dealing with legacy config file semantics
		// Use difference of versions to determine what should be done
		private void ConvertLegacyConfig(Version currentVersion, Version configVersion)
		{
			var converted = false;

			var v0_3_21 = new Version(0, 3, 21, 0);

			if(configVersion == null) // Config was created prior to version tracking being introduced (v0.3.20)
			{
				Config.Instance.ResetAll();
				Config.Instance.CreatedByVersion = currentVersion.ToString();
				converted = true;
			}
			else
			{
				if(configVersion <= v0_3_21)
				{
					// Config must be between v0.3.20 and v0.3.21 inclusive
					// It was still possible in 0.3.21 to see (-32000, -32000) window positions
					// under certain circumstances (GitHub issue #135).
					if(Config.Instance.TrackerWindowLeft == -32000)
					{
						Config.Instance.Reset("TrackerWindowLeft");
						converted = true;
					}
					if(Config.Instance.TrackerWindowTop == -32000)
					{
						Config.Instance.Reset("TrackerWindowTop");
						converted = true;
					}

					if(Config.Instance.PlayerWindowLeft == -32000)
					{
						Config.Instance.Reset("PlayerWindowLeft");
						converted = true;
					}
					if(Config.Instance.PlayerWindowTop == -32000)
					{
						Config.Instance.Reset("PlayerWindowTop");
						converted = true;
					}

					if(Config.Instance.OpponentWindowLeft == -32000)
					{
						Config.Instance.Reset("OpponentWindowLeft");
						converted = true;
					}
					if(Config.Instance.OpponentWindowTop == -32000)
					{
						Config.Instance.Reset("OpponentWindowTop");
						converted = true;
					}

					if(Config.Instance.TimerWindowLeft == -32000)
					{
						Config.Instance.Reset("TimerWindowLeft");
						converted = true;
					}
					if(Config.Instance.TimerWindowTop == -32000)
					{
						Config.Instance.Reset("TimerWindowTop");
						converted = true;
					}

					//player scaling used to be increased by a very minimal about to circumvent some problem,
					//should no longer be required. not sure is the increment is actually noticeable, but resetting can't hurt
					if(Config.Instance.OverlayOpponentScaling > 100)
					{
						Config.Instance.OverlayOpponentScaling = 100;
						converted = true;
					}
					if(Config.Instance.OverlayPlayerScaling > 100)
					{
						Config.Instance.OverlayPlayerScaling = 100;
						converted = true;
					}
				}


				if(configVersion <= new Version(0, 5, 1, 0))
				{
#pragma warning disable 612
					Config.Instance.SaveConfigInAppData = Config.Instance.SaveInAppData;
					Config.Instance.SaveDataInAppData = Config.Instance.SaveInAppData;
					converted = true;
#pragma warning restore 612
				}
				if(configVersion <= new Version(0, 6, 6, 0))
				{
					if(Config.Instance.ExportClearX == 0.86)
					{
						Config.Instance.Reset("ExportClearX");
						converted = true;
					}
					if(Config.Instance.ExportClearY == 0.16)
					{
						Config.Instance.Reset("ExportClearY");
						converted = true;
					}
					if(Config.Instance.ExportClearCheckYFixed == 0.2)
					{
						Config.Instance.Reset("ExportClearCheckYFixed");
						converted = true;
					}
				}
				if(configVersion <= new Version(0, 7, 6, 0))
				{
					if(Config.Instance.ExportCard1X != 0.04)
					{
						Config.Instance.Reset("ExportCard1X");
						converted = true;
					}
					if(Config.Instance.ExportCard2X != 0.2)
					{
						Config.Instance.Reset("ExportCard2X");
						converted = true;
					}
					if(Config.Instance.ExportCardsY != 0.168)
					{
						Config.Instance.Reset("ExportCardsY");
						converted = true;
					}
				}
				if(configVersion <= new Version(0, 9, 6, 0))
				{
					if(!Config.Instance.PanelOrderPlayer.Contains("Fatigue Counter"))
					{
						Config.Instance.Reset("PanelOrderPlayer");
						converted = true;
					}
					if(!Config.Instance.PanelOrderOpponent.Contains("Fatigue Counter"))
					{
						Config.Instance.Reset("PanelOrderOpponent");
						converted = true;
					}
				}
			}

			if(converted)
			{
				Logger.WriteLine("changed config values", "ConvertLegacyConfig");
				Config.SaveBackup();
				Config.Save();
			}

			if(configVersion != null && currentVersion > configVersion)
				_updatedVersion = currentVersion;
		}

		private bool FindHearthstoneDir()
		{
			var found = false;
			if(string.IsNullOrEmpty(Config.Instance.HearthstoneDirectory)
			   || !File.Exists(Config.Instance.HearthstoneDirectory + @"\Hearthstone.exe"))
			{
				using(var hsDirKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Hearthstone"))
				{
					if(hsDirKey != null)
					{
						var hsDir = (string)hsDirKey.GetValue("InstallLocation");

						//verify the install location actually is correct (possibly moved?)
						if(File.Exists(hsDir + @"\Hearthstone.exe"))
						{
							Config.Instance.HearthstoneDirectory = hsDir;
							Config.Save();
							found = true;
						}
					}
				}
			}
			else
				found = true;

			return found;
		}

		private bool UpdateLogConfigFile()
		{
			var updated = false;
			//check for log config and create if not existing
			try
			{
				//always overwrite is true by default. 
				if(!File.Exists(_logConfigPath))
				{
					updated = true;
					File.Copy("Files/log.config", _logConfigPath, true);
					Logger.WriteLine(string.Format("Copied log.config to {0} (did not exist)", _logConfigPath), "Load");
				}
				else
				{
					//update log.config if newer
					var localFile = new FileInfo(_logConfigPath);
					var file = new FileInfo("Files/log.config");
					if(file.LastWriteTime > localFile.LastWriteTime)
					{
						updated = true;
						File.Copy("Files/log.config", _logConfigPath, true);
						Logger.WriteLine(string.Format("Copied log.config to {0} (file newer)", _logConfigPath), "Load");
					}
					else if(Config.Instance.AlwaysOverwriteLogConfig)
					{
						File.Copy("Files/log.config", _logConfigPath, true);
						Logger.WriteLine(string.Format("Copied log.config to {0} (AlwaysOverwriteLogConfig)", _logConfigPath), "Load");
					}
				}
			}
			catch(Exception e)
			{
				if(_updatedLogConfig)
				{
					MessageBox.Show(
					                e.Message + "\n\n" + e.InnerException
					                + "\n\n Please manually copy the log.config from the Files directory to \"%LocalAppData%/Blizzard/Hearthstone\".",
					                "Error writing log.config");
					Application.Current.Shutdown();
				}
			}
			return updated;
		}

		public void SetupDeckListFile()
		{
			if(Config.Instance.SaveDataInAppData == null)
				return;
			var appDataPath = Config.Instance.AppDataPath + @"\PlayerDecks.xml";
			var dataDirPath = Config.Instance.DataDirPath + @"\PlayerDecks.xml";
			if(Config.Instance.SaveDataInAppData.Value)
			{
				if(File.Exists(dataDirPath))
				{
					if(File.Exists(appDataPath))
						//backup in case the file already exists
						File.Move(appDataPath, appDataPath + DateTime.Now.ToFileTime());
					File.Move(dataDirPath, appDataPath);
					Logger.WriteLine("Moved decks to appdata", "Load");
				}
			}
			else if(File.Exists(appDataPath))
			{
				if(File.Exists(dataDirPath))
					//backup in case the file already exists
					File.Move(dataDirPath, dataDirPath + DateTime.Now.ToFileTime());
				File.Move(appDataPath, dataDirPath);
				Logger.WriteLine("Moved decks to local", "Load");
			}

			//create file if it doesn't exist
			var path = Path.Combine(Config.Instance.DataDir, "PlayerDecks.xml");
			if(!File.Exists(path))
			{
				using(var sr = new StreamWriter(path, false))
					sr.WriteLine("<Decks></Decks>");
			}
		}

		public void SetupDefaultDeckStatsFile()
		{
			if(Config.Instance.SaveDataInAppData == null)
				return;
			var appDataPath = Config.Instance.AppDataPath + @"\DefaultDeckStats.xml";
			var dataDirPath = Config.Instance.DataDirPath + @"\DefaultDeckStats.xml";
			if(Config.Instance.SaveDataInAppData.Value)
			{
				if(File.Exists(dataDirPath))
				{
					if(File.Exists(appDataPath))
					{
						//backup in case the file already exists
						var time = DateTime.Now.ToFileTime();
						File.Move(appDataPath, appDataPath + time);
						Logger.WriteLine("Created backups of DefaultDeckStats in appdata", "Load");
					}
					File.Move(dataDirPath, appDataPath);
					Logger.WriteLine("Moved DefaultDeckStats to appdata", "Load");
				}
			}
			else if(File.Exists(appDataPath))
			{
				if(File.Exists(dataDirPath))
				{
					//backup in case the file already exists
					var time = DateTime.Now.ToFileTime();
					File.Move(dataDirPath, dataDirPath + time);
					Logger.WriteLine("Created backups of DefaultDeckStats locally", "Load");
				}
				File.Move(appDataPath, dataDirPath);
				Logger.WriteLine("Moved DefaultDeckStats to local", "Load");
			}

			var filePath = Config.Instance.DataDir + "DefaultDeckStats.xml";
			//create if it does not exist
			if(!File.Exists(filePath))
			{
				using(var sr = new StreamWriter(filePath, false))
					sr.WriteLine("<DefaultDeckStats></DefaultDeckStats>");
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

			Options.Load();


			Game.HighlightCardsInHand = Config.Instance.HighlightCardsInHand;
			Game.HighlightDiscarded = Config.Instance.HighlightDiscarded;
			//Options.CheckboxHideOverlayInBackground.IsChecked = Config.Instance.HideInBackground;
			//Options.CheckboxHideOpponentCardAge.IsChecked = Config.Instance.HideOpponentCardAge;
			//Options.CheckboxHideOpponentCardMarks.IsChecked = Config.Instance.HideOpponentCardMarks;
			//Options.CheckboxHideOverlayInMenu.IsChecked = Config.Instance.HideInMenu;
			//Options.CheckboxHighlightCardsInHand.IsChecked = Config.Instance.HighlightCardsInHand;
			//Options.CheckboxHideOverlay.IsChecked = Config.Instance.HideOverlay;
			//Options.CheckboxHideDecksInOverlay.IsChecked = Config.Instance.HideDecksInOverlay;
			//Options.CheckboxKeepDecksVisible.IsChecked = Config.Instance.KeepDecksVisible;
			//Options.CheckboxMinimizeTray.IsChecked = Config.Instance.MinimizeToTray;
			//Options.CheckboxWindowsTopmost.IsChecked = Config.Instance.WindowsTopmost;
			//Options.CheckboxPlayerWindowOpenAutomatically.IsChecked = Config.Instance.PlayerWindowOnStart;
			//Options.CheckboxOpponentWindowOpenAutomatically.IsChecked = Config.Instance.OpponentWindowOnStart;
			//Options.CheckboxTimerTopmost.IsChecked = Config.Instance.TimerWindowTopmost;
			//Options.CheckboxTimerWindow.IsChecked = Config.Instance.TimerWindowOnStartup;
			//Options.CheckboxTimerTopmostHsForeground.IsChecked = Config.Instance.TimerWindowTopmostIfHsForeground;
			//Options.CheckboxTimerTopmostHsForeground.IsEnabled = Config.Instance.TimerWindowTopmost;
			//Options.CheckboxSameScaling.IsChecked = Config.Instance.UseSameScaling;
			CheckboxDeckDetection.IsChecked = Config.Instance.AutoDeckDetection;
            setContextMenuProperty("autoSelectDeck", "Checked", (bool)CheckboxDeckDetection.IsChecked);
			//Options.CheckboxWinTopmostHsForeground.IsChecked = Config.Instance.WindowsTopmostIfHsForeground;
			//Options.CheckboxWinTopmostHsForeground.IsEnabled = Config.Instance.WindowsTopmost;
			//Options.CheckboxAutoSelectDeck.IsEnabled = Config.Instance.AutoDeckDetection;
			//Options.CheckboxAutoSelectDeck.IsChecked = Config.Instance.AutoSelectDetectedDeck;
			//Options.CheckboxExportName.IsChecked = Config.Instance.ExportSetDeckName;
			//Options.CheckboxPrioGolden.IsChecked = Config.Instance.PrioritizeGolden;
			//Options.CheckboxBringHsToForegorund.IsChecked = Config.Instance.BringHsToForeground;
			//Options.CheckboxFlashHs.IsChecked = Config.Instance.FlashHsOnTurnStart;
			//Options.CheckboxHideSecrets.IsChecked = Config.Instance.HideSecrets;
			//Options.CheckboxHighlightDiscarded.IsChecked = Config.Instance.HighlightDiscarded;
			//Options.CheckboxRemoveCards.IsChecked = Config.Instance.RemoveCardsFromDeck;
			//Options.CheckboxHighlightLastDrawn.IsChecked = Config.Instance.HighlightLastDrawn;
			//Options.CheckboxStartMinimized.IsChecked = Config.Instance.StartMinimized;
			//Options.CheckboxShowPlayerGet.IsChecked = Config.Instance.ShowPlayerGet;
			//Options.ToggleSwitchExtraFeatures.IsChecked = Config.Instance.ExtraFeatures;
			//Options.CheckboxCheckForUpdates.IsChecked = Config.Instance.CheckForUpdates;
			//Options.CheckboxRecordArena.IsChecked = Config.Instance.RecordArena;
			//Options.CheckboxRecordCasual.IsChecked = Config.Instance.RecordCasual;
			//Options.CheckboxRecordFriendly.IsChecked = Config.Instance.RecordFriendly;
			//Options.CheckboxRecordOther.IsChecked = Config.Instance.RecordOther;
			//Options.CheckboxRecordPractice.IsChecked = Config.Instance.RecordPractice;
			//Options.CheckboxRecordRanked.IsChecked = Config.Instance.RecordRanked;
			//Options.CheckboxFullTextSearch.IsChecked = Config.Instance.UseFullTextSearch;
			//Options.CheckboxDiscardGame.IsChecked = Config.Instance.DiscardGameIfIncorrectDeck;
			//Options.CheckboxExportPasteClipboard.IsChecked = Config.Instance.ExportPasteClipboard;
			//Options.CheckboxGoldenFeugen.IsChecked = Config.Instance.OwnsGoldenFeugen;
			//Options.CheckboxGoldenStalagg.IsChecked = Config.Instance.OwnsGoldenStalagg;
			//Options.CheckboxCloseWithHearthstone.IsChecked = Config.Instance.CloseWithHearthstone;
			//Options.CheckboxStatsInWindow.IsChecked = Config.Instance.StatsInWindow;
			//Options.CheckboxOverlaySecretToolTipsOnly.IsChecked = Config.Instance.OverlaySecretToolTipsOnly;
			//Options.CheckboxTagOnImport.IsChecked = Config.Instance.TagDecksOnImport;
			//Options.CheckboxConfigSaveAppData.IsChecked = Config.Instance.SaveConfigInAppData;
			//Options.CheckboxDataSaveAppData.IsChecked = Config.Instance.SaveDataInAppData;
			//Options.CheckboxAdvancedWindowSearch.IsChecked = Config.Instance.AdvancedWindowSearch;
			//Options.CheckboxDeleteDeckKeepStats.IsChecked = Config.Instance.KeepStatsWhenDeletingDeck;
			//Options.CheckboxNoteDialog.IsChecked = Config.Instance.ShowNoteDialogAfterGame;
			//Options.CheckboxAutoClear.IsChecked = Config.Instance.AutoClearDeck;
			//Options.CheckboxLogTab.IsChecked = Config.Instance.ShowLogTab;
			//Options.CheckboxTimerAlert.IsChecked = Config.Instance.TimerAlert;
			//Options.CheckboxRecordSpectator.IsChecked = Config.Instance.RecordSpectator;
			//Options.CheckboxHideOverlayInSpectator.IsChecked = Config.Instance.HideOverlayInSpectator;
			//Options.TextboxExportDelay.Text = Config.Instance.ExportStartDelay.ToString();
			//Options.CheckboxDiscardZeroTurnGame.IsChecked = Config.Instance.DiscardZeroTurnGame;
			//Options.CheckboxSaveHSLogIntoReplayFile.IsChecked = Config.Instance.SaveHSLogIntoReplay;
			//Options.CheckboxNoteDialogDelayed.IsChecked = Config.Instance.NoteDialogDelayed;
			//Options.CheckboxNoteDialogDelayed.IsEnabled = Config.Instance.ShowNoteDialogAfterGame;
			//Options.CheckboxStartWithWindows.IsChecked = Config.Instance.StartWithWindows;
			//Options.CheckboxOverlayCardMarkToolTips.IsChecked = Config.Instance.OverlayCardMarkToolTips;
			//Options.ComboBoxLogLevel.SelectedValue = Config.Instance.LogLevel.ToString();
			//Options.CheckBoxForceExtraFeatures.IsChecked = Config.Instance.ForceMouseHook;
			//Options.CheckBoxForceExtraFeatures.IsEnabled = Config.Instance.ExtraFeatures;
			//Options.CheckboxAutoGrayoutSecrets.IsChecked = Config.Instance.AutoGrayoutSecrets;
			//Options.CheckboxImportNetDeck.IsChecked = Config.Instance.NetDeckClipboardCheck ?? false;
			//Options.CheckboxAutoSaveOnImport.IsChecked = Config.Instance.AutoSaveOnImport;

			//Options.SliderOverlayOpacity.Value = Config.Instance.OverlayOpacity;
			//Options.SliderOpponentOpacity.Value = Config.Instance.OpponentOpacity;
			//Options.SliderPlayerOpacity.Value = Config.Instance.PlayerOpacity;
			//Options.SliderOverlayPlayerScaling.Value = Config.Instance.OverlayPlayerScaling;
			//Options.SliderOverlayOpponentScaling.Value = Config.Instance.OverlayOpponentScaling;

			//DeckPickerList.ShowAll = Config.Instance.ShowAllDecks;
			//DeckPickerList.SetSelectedTags(Config.Instance.SelectedTags);

			// Don't select the 'archived' class on load
			var selectedClasses = Config.Instance.SelectedDeckPickerClasses.Where(c => c.ToString() != "Archived").ToList();
			if(selectedClasses.Count == 0)
				selectedClasses.Add(HeroClassAll.All);

			DeckPickerList.SelectClasses(selectedClasses);
			DeckPickerList.SelectDeckType(Config.Instance.SelectedDeckType, true);

			//Options.CheckboxHideTimers.IsChecked = Config.Instance.HideTimers;

			//var delay = Config.Instance.DeckExportDelay;
			//Options.ComboboxExportSpeed.SelectedIndex = delay < 40 ? 0 : delay < 60 ? 1 : delay < 100 ? 2 : delay < 150 ? 3 : 4;

			SortFilterDecksFlyout.LoadTags(DeckList.Instance.AllTags);

			UpdateQuickFilterItemSource();

			SortFilterDecksFlyout.SetSelectedTags(Config.Instance.SelectedTags);
			//DeckPickerList.SetSelectedTags(Config.Instance.SelectedTags);


			TagControlEdit.LoadTags(DeckList.Instance.AllTags.Where(tag => tag != "All" && tag != "None").ToList());
			//DeckPickerList.SetTagOperation(Config.Instance.TagOperation);
			SortFilterDecksFlyout.OperationSwitch.IsChecked = Config.Instance.TagOperation == TagFilerOperation.And;

			SortFilterDecksFlyout.ComboboxDeckSorting.SelectedItem = Config.Instance.SelectedDeckSorting;

			//Options.ComboboxWindowBackground.SelectedItem = Config.Instance.SelectedWindowBackground;
			//Options.TextboxCustomBackground.IsEnabled = Config.Instance.SelectedWindowBackground == "Custom";
			//Options.TextboxCustomBackground.Text = string.IsNullOrEmpty(Config.Instance.WindowsBackgroundHex)
			//	                                       ? "#696969" : Config.Instance.WindowsBackgroundHex;
			//Options.UpdateAdditionalWindowsBackground();

			//if(Helper.LanguageDict.Values.Contains(Config.Instance.SelectedLanguage))
			//	Options.ComboboxLanguages.SelectedItem = Helper.LanguageDict.First(x => x.Value == Config.Instance.SelectedLanguage).Key;

			if(!EventKeys.Contains(Config.Instance.KeyPressOnGameStart))
				Config.Instance.KeyPressOnGameStart = "None";
			//Options.ComboboxKeyPressGameStart.SelectedValue = Config.Instance.KeyPressOnGameStart;

			if(!EventKeys.Contains(Config.Instance.KeyPressOnGameEnd))
				Config.Instance.KeyPressOnGameEnd = "None";
			//Options.ComboboxKeyPressGameEnd.SelectedValue = Config.Instance.KeyPressOnGameEnd;

			//Options.CheckboxHideManaCurveMyDecks.IsChecked = Config.Instance.ManaCurveMyDecks;
			ManaCurveMyDecks.Visibility = Config.Instance.ManaCurveMyDecks ? Visibility.Visible : Visibility.Collapsed;

			//Options.CheckboxTrackerCardToolTips.IsChecked = Config.Instance.TrackerCardToolTips;
			//Options.CheckboxWindowCardToolTips.IsChecked = Config.Instance.WindowCardToolTips;
			//Options.CheckboxOverlayCardToolTips.IsChecked = Config.Instance.OverlayCardToolTips;
			//Options.CheckboxOverlayAdditionalCardToolTips.IsEnabled = Config.Instance.OverlayCardToolTips;
			//Options.CheckboxOverlayAdditionalCardToolTips.IsChecked = Config.Instance.AdditionalOverlayTooltips;

			CheckboxClassCardsFirst.IsChecked = Config.Instance.CardSortingClassFirst;
            setContextMenuProperty("classCardsFirst", "Checked", (bool)CheckboxClassCardsFirst.IsChecked);


			DeckStatsFlyout.LoadConfig();
			GameDetailsFlyout.LoadConfig();
			StatsWindow.StatsControl.LoadConfig();
			StatsWindow.GameDetailsFlyout.LoadConfig();
			
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
			if(_updatedVersion != null)
				await this.ShowUpdateNotesMessage();

			if(!_foundHsDirectory)
				await this.ShowHsNotInstalledMessage();
			else if(_updatedLogConfig)
			{
				await
					this.ShowMessage("Restart Hearthstone",
					                 "This is either your first time starting the tracker or the log.config file has been updated. Please restart Heartstone once, for the tracker to work properly.");
			}

			if(!Config.Instance.ResolvedDeckStatsIds)
			{
				if(ResolveDeckStatsIds())
					await Restart();
			}
			if(Config.Instance.HearthStatsSyncOnStart && HearthStatsAPI.IsLoggedIn)
				HearthStatsManager.SyncAsync(background: true);
		}
	}
}