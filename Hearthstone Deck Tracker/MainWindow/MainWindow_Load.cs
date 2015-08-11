#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using HDTHelper;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
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
				var requiredLogs = new[] {"Zone", "Bob", "Power", "Asset", "Rachelle"};

				string[] actualLogs = {};
				if(File.Exists(_logConfigPath))
				{
					using(var sr = new StreamReader(_logConfigPath))
					{
						var content = sr.ReadToEnd();
						actualLogs =
							content.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
							       .Where(x => x.StartsWith("["))
							       .Select(x => x.Substring(1, x.Length - 2))
							       .ToArray();
					}
				}

				var missing = requiredLogs.Where(x => !actualLogs.Contains(x)).ToList();
				if(missing.Any())
				{
					using(var sw = new StreamWriter(_logConfigPath, true))
					{
						foreach(var log in missing)
						{
							sw.WriteLine("[{0}]", log);
							sw.WriteLine("LogLevel=1");
							sw.WriteLine("FilePrinting=false");
							sw.WriteLine("ConsolePrinting=true");
							sw.WriteLine("ScreenPrinting=false");
							Logger.WriteLine("Added " + log + " to log.config.", "UpdateLogConfig");
						}
					}
					updated = true;
				}
				var additional = actualLogs.Where(x => !requiredLogs.Contains(x)).ToList();
				foreach(var log in additional)
					Logger.WriteLine("log.config contains additional log: " + log + ".", "UpdateLogConfig");
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


			
			Application.Current.Resources["GrayTextColorBrush"] = theme.Name == "BaseLight"
				                                                           ? new SolidColorBrush((Color)Application.Current.Resources["GrayTextColor1"])
				                                                           : new SolidColorBrush((Color)Application.Current.Resources["GrayTextColor2"]);

			Options.Load();


			Game.HighlightCardsInHand = Config.Instance.HighlightCardsInHand;
			Game.HighlightDiscarded = Config.Instance.HighlightDiscarded;
			CheckboxDeckDetection.IsChecked = Config.Instance.AutoDeckDetection;
			SetContextMenuProperty("autoSelectDeck", "Checked", (bool)CheckboxDeckDetection.IsChecked);

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

			if(!EventKeys.Contains(Config.Instance.KeyPressOnGameStart))
				Config.Instance.KeyPressOnGameStart = "None";

			if(!EventKeys.Contains(Config.Instance.KeyPressOnGameEnd))
				Config.Instance.KeyPressOnGameEnd = "None";

			ManaCurveMyDecks.Visibility = Config.Instance.ManaCurveMyDecks ? Visibility.Visible : Visibility.Collapsed;
			//ManaCurveMyDecks.ListViewStatType.SelectedIndex = (int)Config.Instance.ManaCurveFilter;

			CheckboxClassCardsFirst.IsChecked = Config.Instance.CardSortingClassFirst;
			SetContextMenuProperty("classCardsFirst", "Checked", (bool)CheckboxClassCardsFirst.IsChecked);
			SetContextMenuProperty("useNoDeck", "Checked", DeckList.Instance.ActiveDeck == null);


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
			if(_updatedVersion != null)
				await this.ShowUpdateNotesMessage();

			if(!_foundHsDirectory)
				await this.ShowHsNotInstalledMessage();
			else if(_updatedLogConfig)
			{
				await
					this.ShowMessage("Restart Hearthstone",
					                 "This is either your first time starting the tracker or the log.config file has been updated. Please restart Hearthstone once, for the tracker to work properly.");
			}

			if(!Config.Instance.ResolvedOpponentNames)
				ResolveOpponentNames();
			if(!Config.Instance.ResolvedDeckStatsIds)
			{
				if(ResolveDeckStatsIds())
					Restart();
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