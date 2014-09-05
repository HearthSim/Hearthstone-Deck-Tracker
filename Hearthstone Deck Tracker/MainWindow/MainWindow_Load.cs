using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro;
using Microsoft.Win32;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Point = System.Drawing.Point;

namespace Hearthstone_Deck_Tracker
{
	public partial class MainWindow
	{
		private void FillElementSorters()
		{
			Options.ElementSorterPlayer.IsPlayer = true;
			foreach(var itemName in Config.Instance.PanelOrderPlayer)
			{
				switch(itemName)
				{
					case "Deck Title":
						Options.ElementSorterPlayer.AddItem(new ElementSorterItem("Deck Title", Config.Instance.ShowDeckTitle, value => Config.Instance.ShowDeckTitle = value, true));
						break;
					case "Cards":
						Options.ElementSorterPlayer.AddItem(new ElementSorterItem("Cards", !Config.Instance.HidePlayerCards, value => Config.Instance.HidePlayerCards = !value, true));
						break;
					case "Card Counter":
						Options.ElementSorterPlayer.AddItem(new ElementSorterItem("Card Counter", !Config.Instance.HidePlayerCardCount, value => Config.Instance.HidePlayerCardCount = !value, true));
						break;
					case "Draw Chances":
						Options.ElementSorterPlayer.AddItem(new ElementSorterItem("Draw Chances", !Config.Instance.HideDrawChances, value => Config.Instance.HideDrawChances = !value, true));
						break;
					case "Wins":
						Options.ElementSorterPlayer.AddItem(new ElementSorterItem("Wins", Config.Instance.ShowDeckWins, value => Config.Instance.ShowDeckWins = value, true));
						break;
				}
			}
			Overlay.UpdatePlayerLayout();
			PlayerWindow.UpdatePlayerLayout();

			Options.ElementSorterOpponent.IsPlayer = false;
			foreach(var itemName in Config.Instance.PanelOrderOpponent)
			{
				switch(itemName)
				{
					case "Cards":
						Options.ElementSorterOpponent.AddItem(new ElementSorterItem("Cards", !Config.Instance.HideOpponentCards, value => Config.Instance.HideOpponentCards = !value, false));
						break;
					case "Card Counter":
						Options.ElementSorterOpponent.AddItem(new ElementSorterItem("Card Counter", !Config.Instance.HideOpponentCardCount, value => Config.Instance.HideOpponentCardCount = !value, false));
						break;
					case "Draw Chances":
						Options.ElementSorterOpponent.AddItem(new ElementSorterItem("Draw Chances", !Config.Instance.HideOpponentDrawChances, value => Config.Instance.HideOpponentDrawChances = !value, false));
						break;
					case "Win Rate":
						Options.ElementSorterOpponent.AddItem(new ElementSorterItem("Win Rate", Config.Instance.ShowWinRateAgainst, value => Config.Instance.ShowWinRateAgainst = value, false));
						break;
				}
			}
			Overlay.UpdateOpponentLayout();
			OpponentWindow.UpdateOpponentLayout();
		}

		private void SetupDeckStatsFile()
		{
			var appDataPath = Config.Instance.AppDataPath + @"\DeckStats.xml";
			var appDataGamesDirPath = Config.Instance.AppDataPath + @"\Games";
			const string localPath = "DeckStats.xml";
			const string localGamesDirPath = "Games";
			if(Config.Instance.SaveInAppData)
			{
				if(File.Exists(localPath))
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
						Logger.WriteLine("Created backups of deckstats and games in appdata");
					}
					File.Move(localPath, appDataPath);
					Logger.WriteLine("Moved DeckStats to appdata");
					if(Directory.Exists(localGamesDirPath))
					{
						Helper.CopyFolder(localGamesDirPath, appDataGamesDirPath);
						Directory.Delete(localGamesDirPath, true);
					}
					Logger.WriteLine("Moved Games to appdata");
				}
			}
			else if(File.Exists(appDataPath))
			{
				if(File.Exists(localPath))
				{
					//backup in case the file already exists
					var time = DateTime.Now.ToFileTime();
					File.Move(localPath, localPath + time);
					if(Directory.Exists(localGamesDirPath))
					{
						Helper.CopyFolder(localGamesDirPath, localGamesDirPath + time);
						Directory.Delete(localGamesDirPath, true);
					}
					Logger.WriteLine("Created backups of deckstats and games locally");
				}
				File.Move(appDataPath, localPath);
				Logger.WriteLine("Moved DeckStats to local");
				if(Directory.Exists(appDataGamesDirPath))
				{
					Helper.CopyFolder(appDataGamesDirPath, localGamesDirPath);
					Directory.Delete(appDataGamesDirPath, true);
				}
				Logger.WriteLine("Moved Games to appdata");
			}

			var filePath = Config.Instance.HomeDir + "DeckStats.xml";
			//load saved decks
			if(!File.Exists(filePath))
			{
				//avoid overwriting decks file with new releases.
				using(var sr = new StreamWriter(filePath, false))
					sr.WriteLine("<DeckStatsList></DeckStatsList>");
			}
		}

		//TODO
		// Logic for dealing with legacy config file semantics
		// Use difference of versions to determine what should be done
		private void ConvertLegacyConfig(Version currentVersion, Version configVersion)
		{
			var config = Config.Instance;
			var converted = false;

			var v0_3_21 = new Version(0, 3, 21, 0);

			if(configVersion == null) // Config was created prior to version tracking being introduced (v0.3.20)
			{
				// We previously assumed negative pixel coordinates were invalid, but in fact they can go negative
				// with multi-screen setups. Negative positions were being used to represent 'no specific position'
				// as a default. That means that when the windows are created for the first time, we let the operating
				// system decide where to place them. As we should not be using negative positions for this purpose, since
				// they are in fact a valid range of pixel positions, we now use nullable types instead. The default
				// 'no specific position' is now expressed when the positions are null.
				{
					if(config.TrackerWindowLeft.HasValue && config.TrackerWindowLeft.Value < 0)
					{
						config.TrackerWindowLeft = Config.Defaults.TrackerWindowLeft;
						converted = true;
					}
					if(config.TrackerWindowTop.HasValue && config.TrackerWindowTop.Value < 0)
					{
						config.TrackerWindowTop = Config.Defaults.TrackerWindowTop;
						converted = true;
					}

					if(config.PlayerWindowLeft.HasValue && config.PlayerWindowLeft.Value < 0)
					{
						config.PlayerWindowLeft = Config.Defaults.PlayerWindowLeft;
						converted = true;
					}
					if(config.PlayerWindowTop.HasValue && config.PlayerWindowTop.Value < 0)
					{
						config.PlayerWindowTop = Config.Defaults.PlayerWindowTop;
						converted = true;
					}

					if(config.OpponentWindowLeft.HasValue && config.OpponentWindowLeft.Value < 0)
					{
						config.OpponentWindowLeft = Config.Defaults.OpponentWindowLeft;
						converted = true;
					}
					if(config.OpponentWindowTop.HasValue && config.OpponentWindowTop.Value < 0)
					{
						config.OpponentWindowTop = Config.Defaults.OpponentWindowTop;
						converted = true;
					}

					if(config.TimerWindowLeft.HasValue && config.TimerWindowLeft.Value < 0)
					{
						config.TimerWindowLeft = Config.Defaults.TimerWindowLeft;
						converted = true;
					}
					if(config.TimerWindowTop.HasValue && config.TimerWindowTop.Value < 0)
					{
						config.TimerWindowTop = Config.Defaults.TimerWindowTop;
						converted = true;
					}
				}

				// Player and opponent window heights were previously set to zero as a default, and then
				// a bit of logic was used when creating the windows: if height == 0, then set height to 400.
				// This was a little pointless and also inconsistent with the way the default timer window
				// dimensions were implemented. Unfortunately we cannot make this consistent without
				// breaking legacy config files, where the height will still be stored as zero. So
				// we handle the changed semantics here.
				{
					if(config.PlayerWindowHeight == 0)
					{
						config.PlayerWindowHeight = Config.Defaults.PlayerWindowHeight;
						converted = true;
					}

					if(config.OpponentWindowHeight == 0)
					{
						config.OpponentWindowHeight = Config.Defaults.OpponentWindowHeight;
						converted = true;
					}
				}
			}
			else if(configVersion <= v0_3_21) // Config must be between v0.3.20 and v0.3.21 inclusive
				// It was still possible in 0.3.21 to see (-32000, -32000) window positions
				// under certain circumstances (GitHub issue #135).
			{
				if(config.TrackerWindowLeft == -32000)
				{
					config.TrackerWindowLeft = Config.Defaults.TrackerWindowLeft;
					converted = true;
				}
				if(config.TrackerWindowTop == -32000)
				{
					config.TrackerWindowTop = Config.Defaults.TrackerWindowTop;
					converted = true;
				}

				if(config.PlayerWindowLeft == -32000)
				{
					config.PlayerWindowLeft = Config.Defaults.PlayerWindowLeft;
					converted = true;
				}
				if(config.PlayerWindowTop == -32000)
				{
					config.PlayerWindowTop = Config.Defaults.PlayerWindowTop;
					converted = true;
				}

				if(config.OpponentWindowLeft == -32000)
				{
					config.OpponentWindowLeft = Config.Defaults.OpponentWindowLeft;
					converted = true;
				}
				if(config.OpponentWindowTop == -32000)
				{
					config.OpponentWindowTop = Config.Defaults.OpponentWindowTop;
					converted = true;
				}

				if(config.TimerWindowLeft == -32000)
				{
					config.TimerWindowLeft = Config.Defaults.TimerWindowLeft;
					converted = true;
				}
				if(config.TimerWindowTop == -32000)
				{
					config.TimerWindowTop = Config.Defaults.TimerWindowTop;
					converted = true;
				}

				//player scaling used to beincreased by a very minimal about to curcumvent some problem,
				//should no longer be required. not sure is the increment is actually noticable, but resetting can't hurt
				if(config.OverlayOpponentScaling > 100)
				{
					config.OverlayOpponentScaling = 100;
					converted = true;
				}
				if(config.OverlayPlayerScaling > 100)
				{
					config.OverlayPlayerScaling = 100;
					converted = true;
				}
			}

			if(converted)
			{
				Config.SaveBackup();
				Config.Save();
			}

			if(configVersion != null && currentVersion > configVersion)
				_updatedVersion = currentVersion;
		}

		private bool FindHearthstoneDir()
		{
			var found = false;
			if(string.IsNullOrEmpty(Config.Instance.HearthstoneDirectory) ||
			   !File.Exists(Config.Instance.HearthstoneDirectory + @"\Hearthstone.exe"))
			{
				using(
					var hsDirKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Hearthstone")
					)
				{
					if(hsDirKey != null)
					{
						var hsDir = (string)hsDirKey.GetValue("InstallLocation");

						//verify the installlocation actually is correct (possibly moved?)
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
					Logger.WriteLine(string.Format("Copied log.config to {0} (did not exist)", _configPath));
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
						Logger.WriteLine(string.Format("Copied log.config to {0} (file newer)", _configPath));
					}
					else if(Config.Instance.AlwaysOverwriteLogConfig)
					{
						File.Copy("Files/log.config", _logConfigPath, true);
						Logger.WriteLine(string.Format("Copied log.config to {0} (AlwaysOverwriteLogConfig)", _configPath));
					}
				}
			}
			catch(Exception e)
			{
				if(_updatedLogConfig)
				{
					MessageBox.Show(
						e.Message + "\n\n" + e.InnerException +
						"\n\n Please manually copy the log.config from the Files directory to \"%LocalAppData%/Blizzard/Hearthstone\".",
						"Error writing log.config");
					Application.Current.Shutdown();
				}
			}
			return updated;
		}

		private void SetupDeckListFile()
		{
			var appDataPath = Config.Instance.AppDataPath + @"\PlayerDecks.xml";
			const string localPath = "PlayerDecks.xml";
			if(Config.Instance.SaveInAppData)
			{
				if(File.Exists(localPath))
				{
					if(File.Exists(appDataPath))
						//backup in case the file already exists
						File.Move(appDataPath, appDataPath + DateTime.Now.ToFileTime());
					File.Move(localPath, appDataPath);
					Logger.WriteLine("Moved decks to appdata");
				}
			}
			else if(File.Exists(appDataPath))
			{
				if(File.Exists(localPath))
					//backup in case the file already exists
					File.Move(localPath, localPath + DateTime.Now.ToFileTime());
				File.Move(appDataPath, localPath);
				Logger.WriteLine("Moved decks to local");
			}

			//load saved decks
			if(!File.Exists(_decksPath))
			{
				//avoid overwriting decks file with new releases.
				using(var sr = new StreamWriter(_decksPath, false))
					sr.WriteLine("<Decks></Decks>");
			}
			else if(!File.Exists(_decksPath + ".old"))
				//the new playerdecks.xml wont work with versions below 0.2.19, make copy
				File.Copy(_decksPath, _decksPath + ".old");
		}

		private void LoadConfig()
		{
			if(Config.Instance.TrackerWindowTop.HasValue)
				Top = Config.Instance.TrackerWindowTop.Value;
			if(Config.Instance.TrackerWindowLeft.HasValue)
				Left = Config.Instance.TrackerWindowLeft.Value;

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
				            ? ThemeManager.DetectAppStyle().Item1
				            : ThemeManager.AppThemes.First(t => t.Name == Config.Instance.ThemeName);
			var accent = string.IsNullOrEmpty(Config.Instance.AccentName)
				             ? ThemeManager.DetectAppStyle().Item2
				             : ThemeManager.Accents.First(a => a.Name == Config.Instance.AccentName);
			ThemeManager.ChangeAppStyle(Application.Current, accent, theme);
			Options.ComboboxTheme.SelectedItem = theme;
			Options.ComboboxAccent.SelectedItem = accent;

			//Options.CheckboxSaveAppData.IsChecked = Config.Instance.SaveInAppData;
			//TODO


			Height = Config.Instance.WindowHeight;
			Width = Config.Instance.WindowWidth;
			Game.HighlightCardsInHand = Config.Instance.HighlightCardsInHand;
			Game.HighlightDiscarded = Config.Instance.HighlightDiscarded;
			Options.CheckboxHideOverlayInBackground.IsChecked = Config.Instance.HideInBackground;
			Options.CheckboxHideOpponentCardAge.IsChecked = Config.Instance.HideOpponentCardAge;
			Options.CheckboxHideOverlayInMenu.IsChecked = Config.Instance.HideInMenu;
			Options.CheckboxHighlightCardsInHand.IsChecked = Config.Instance.HighlightCardsInHand;
			Options.CheckboxHideOverlay.IsChecked = Config.Instance.HideOverlay;
			Options.CheckboxHideDecksInOverlay.IsChecked = Config.Instance.HideDecksInOverlay;
			Options.CheckboxKeepDecksVisible.IsChecked = Config.Instance.KeepDecksVisible;
			Options.CheckboxMinimizeTray.IsChecked = Config.Instance.MinimizeToTray;
			Options.CheckboxWindowsTopmost.IsChecked = Config.Instance.WindowsTopmost;
			Options.CheckboxPlayerWindowOpenAutomatically.IsChecked = Config.Instance.PlayerWindowOnStart;
			Options.CheckboxOpponentWindowOpenAutomatically.IsChecked = Config.Instance.OpponentWindowOnStart;
			Options.CheckboxTimerTopmost.IsChecked = Config.Instance.TimerWindowTopmost;
			Options.CheckboxTimerWindow.IsChecked = Config.Instance.TimerWindowOnStartup;
			Options.CheckboxTimerTopmostHsForeground.IsChecked = Config.Instance.TimerWindowTopmostIfHsForeground;
			Options.CheckboxTimerTopmostHsForeground.IsEnabled = Config.Instance.TimerWindowTopmost;
			Options.CheckboxSameScaling.IsChecked = Config.Instance.UseSameScaling;
			CheckboxDeckDetection.IsChecked = Config.Instance.AutoDeckDetection;
			Options.CheckboxWinTopmostHsForeground.IsChecked = Config.Instance.WindowsTopmostIfHsForeground;
			Options.CheckboxWinTopmostHsForeground.IsEnabled = Config.Instance.WindowsTopmost;
			Options.CheckboxAutoSelectDeck.IsEnabled = Config.Instance.AutoDeckDetection;
			Options.CheckboxAutoSelectDeck.IsChecked = Config.Instance.AutoSelectDetectedDeck;
			Options.CheckboxExportName.IsChecked = Config.Instance.ExportSetDeckName;
			Options.CheckboxPrioGolden.IsChecked = Config.Instance.PrioritizeGolden;
			Options.CheckboxBringHsToForegorund.IsChecked = Config.Instance.BringHsToForeground;
			Options.CheckboxFlashHs.IsChecked = Config.Instance.FlashHsOnTurnStart;
			Options.CheckboxHideSecrets.IsChecked = Config.Instance.HideSecrets;
			Options.CheckboxHighlightDiscarded.IsChecked = Config.Instance.HighlightDiscarded;
			Options.CheckboxRemoveCards.IsChecked = Config.Instance.RemoveCardsFromDeck;
			Options.CheckboxHighlightLastDrawn.IsChecked = Config.Instance.HighlightLastDrawn;
			Options.CheckboxStartMinimized.IsChecked = Config.Instance.StartMinimized;
			Options.CheckboxShowPlayerGet.IsChecked = Config.Instance.ShowPlayerGet;
			Options.ToggleSwitchExtraFeatures.IsChecked = Config.Instance.ExtraFeatures;
			Options.CheckboxCheckForUpdates.IsChecked = Config.Instance.CheckForUpdates;
			Options.CheckboxRecordArena.IsChecked = Config.Instance.RecordArena;
			Options.CheckboxRecordCasual.IsChecked = Config.Instance.RecordCasual;
			Options.CheckboxRecordFriendly.IsChecked = Config.Instance.RecordFriendly;
			Options.CheckboxRecordOther.IsChecked = Config.Instance.RecordOther;
			Options.CheckboxRecordPractice.IsChecked = Config.Instance.RecordPractice;
			Options.CheckboxRecordRanked.IsChecked = Config.Instance.RecordRanked;
			Options.CheckboxFullTextSearch.IsChecked = Config.Instance.UseFullTextSearch;
			Options.CheckboxDiscardGame.IsChecked = Config.Instance.DiscardGameIfIncorrectDeck;
			Options.CheckboxExportPasteClipboard.IsChecked = Config.Instance.ExportPasteClipboard;
			Options.CheckboxGoldenFeugen.IsChecked = Config.Instance.OwnsGoldenFeugen;
			Options.CheckboxGoldenStalagg.IsChecked = Config.Instance.OwnsGoldenStalagg;
			Options.CheckboxCloseWithHearthstone.IsChecked = Config.Instance.CloseWithHearthstone;
			Options.CheckboxStatsInWindow.IsChecked = Config.Instance.StatsInWindow;
			Options.CheckboxOverlaySecretToolTipsOnly.IsChecked = Config.Instance.OverlaySecretToolTipsOnly;
			Options.CheckboxTagOnImport.IsChecked = Config.Instance.TagDecksOnImport;

			Options.SliderOverlayOpacity.Value = Config.Instance.OverlayOpacity;
			Options.SliderOpponentOpacity.Value = Config.Instance.OpponentOpacity;
			Options.SliderPlayerOpacity.Value = Config.Instance.PlayerOpacity;
			Options.SliderOverlayPlayerScaling.Value = Config.Instance.OverlayPlayerScaling;
			Options.SliderOverlayOpponentScaling.Value = Config.Instance.OverlayOpponentScaling;

			DeckPickerList.ShowAll = Config.Instance.ShowAllDecks;
			DeckPickerList.SetSelectedTags(Config.Instance.SelectedTags);

			Options.CheckboxHideTimers.IsChecked = Config.Instance.HideTimers;

			var delay = Config.Instance.DeckExportDelay;
			Options.ComboboxExportSpeed.SelectedIndex = delay < 40 ? 0 : delay < 60 ? 1 : delay < 100 ? 2 : delay < 150 ? 3 : 4;

			SortFilterDecksFlyout.LoadTags(DeckList.AllTags);

			SortFilterDecksFlyout.SetSelectedTags(Config.Instance.SelectedTags);
			DeckPickerList.SetSelectedTags(Config.Instance.SelectedTags);

			var tags = new List<string>(DeckList.AllTags);
			tags.Remove("All");
			TagControlEdit.LoadTags(tags);
			DeckPickerList.SetTagOperation(Config.Instance.TagOperation);
			SortFilterDecksFlyout.OperationSwitch.IsChecked = Config.Instance.TagOperation == Operation.And;

			SortFilterDecksFlyout.ComboboxDeckSorting.SelectedItem = Config.Instance.SelectedDeckSorting;

			Options.ComboboxWindowBackground.SelectedItem = Config.Instance.SelectedWindowBackground;
			Options.TextboxCustomBackground.IsEnabled = Config.Instance.SelectedWindowBackground == "Custom";
			Options.TextboxCustomBackground.Text = string.IsNullOrEmpty(Config.Instance.WindowsBackgroundHex)
				                                       ? "#696969"
				                                       : Config.Instance.WindowsBackgroundHex;
			Options.UpdateAdditionalWindowsBackground();

			if(Helper.LanguageDict.Values.Contains(Config.Instance.SelectedLanguage))
				Options.ComboboxLanguages.SelectedItem = Helper.LanguageDict.First(x => x.Value == Config.Instance.SelectedLanguage).Key;

			if(!EventKeys.Contains(Config.Instance.KeyPressOnGameStart))
				Config.Instance.KeyPressOnGameStart = "None";
			Options.ComboboxKeyPressGameStart.SelectedValue = Config.Instance.KeyPressOnGameStart;

			if(!EventKeys.Contains(Config.Instance.KeyPressOnGameEnd))
				Config.Instance.KeyPressOnGameEnd = "None";
			Options.ComboboxKeyPressGameEnd.SelectedValue = Config.Instance.KeyPressOnGameEnd;

			Options.CheckboxHideManaCurveMyDecks.IsChecked = Config.Instance.ManaCurveMyDecks;
			ManaCurveMyDecks.Visibility = Config.Instance.ManaCurveMyDecks ? Visibility.Visible : Visibility.Collapsed;

			Options.CheckboxTrackerCardToolTips.IsChecked = Config.Instance.TrackerCardToolTips;
			Options.CheckboxWindowCardToolTips.IsChecked = Config.Instance.WindowCardToolTips;
			Options.CheckboxOverlayCardToolTips.IsChecked = Config.Instance.OverlayCardToolTips;
			Options.CheckboxOverlayAdditionalCardToolTips.IsEnabled = Config.Instance.OverlayCardToolTips;
			Options.CheckboxOverlayAdditionalCardToolTips.IsChecked = Config.Instance.AdditionalOverlayTooltips;

			Options.CheckboxDeckSortingClassFirst.IsChecked = Config.Instance.CardSortingClassFirst;

			DeckStatsFlyout.LoadConfig();
			GameDetailsFlyout.LoadConfig();
			StatsWindow.StatsControl.LoadConfig();
			StatsWindow.GameDetailsFlyout.LoadConfig();
		}


		private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
		{
			var presentationsource = PresentationSource.FromVisual(this);
			if(presentationsource != null) // make sure it's connected
			{
				Helper.DpiScalingX = presentationsource.CompositionTarget.TransformToDevice.M11;
				Helper.DpiScalingY = presentationsource.CompositionTarget.TransformToDevice.M22;
			}
			if(!_foundHsDirectory)
			{
				this.ShowHsNotInstalledMessage();
				return;
			}
			if(NewVersion != null)
				ShowNewUpdateMessage();
			if(_updatedVersion != null)
				this.ShowUpdateNotesMessage();

			if(_updatedLogConfig)
			{
				this.ShowMessage("Restart Hearthstone",
				                 "This is either your first time starting the tracker or the log.config file has been updated. Please restart Heartstone once, for the tracker to work properly.");
			}

			ManaCurveMyDecks.UpdateValues();
		}
	}
}