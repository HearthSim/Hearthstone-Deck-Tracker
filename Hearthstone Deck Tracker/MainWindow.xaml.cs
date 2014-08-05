#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SystemColors = System.Windows.SystemColors;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		#region Properties

		public readonly Decks DeckList;
		public readonly Version NewVersion;
		public readonly OpponentWindow OpponentWindow;
		public readonly OverlayWindow Overlay;
		public readonly PlayerWindow PlayerWindow;
		public readonly TimerWindow TimerWindow;
		private readonly string _configPath;
		private readonly string _decksPath;
		private readonly bool _foundHsDirectory;
		private readonly bool _initialized;

		private readonly string _logConfigPath =
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
			@"\Blizzard\Hearthstone\log.config";

		private readonly NotifyIcon _notifyIcon;
		private readonly bool _updatedLogConfig;

		public bool EditingDeck;

		public ReadOnlyCollection<string> EventKeys =
			new ReadOnlyCollection<string>(new[] { "None", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12" });

		public bool IsShowingIncorrectDeckMessage;
		public bool NeedToIncorrectDeckMessage;
		public Deck NewDeck;
		private bool _doUpdate;
		private bool _newContainsDeck;

		public bool ShowToolTip
		{
			get { return Config.Instance.TrackerCardToolTips; }
		}

		#endregion

		#region GENERAL GUI

		private int _lastSelectedTab;

		private void MetroWindow_Activated(object sender, EventArgs e)
		{
			Topmost = true;
		}

		private void MetroWindow_Deactivated(object sender, EventArgs e)
		{
			Topmost = false;
		}

		private void MetroWindow_StateChanged(object sender, EventArgs e)
		{
			if (Config.Instance.MinimizeToTray && WindowState == WindowState.Minimized)
			{
				MinimizeToTray();
			}
		}

		private void MinimizeToTray()
		{
			_notifyIcon.Visible = true;
			//_notifyIcon.ShowBalloonTip(2000, "Hearthstone Deck Tracker", "Minimized to tray", ToolTipIcon.Info);
			Hide();
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			try
			{
				_doUpdate = false;
				Config.Instance.SelectedTags = Config.Instance.SelectedTags.Distinct().ToList();
				Config.Instance.ShowAllDecks = DeckPickerList.ShowAll;

				Config.Instance.WindowHeight = (int)Height;
				Config.Instance.TrackerWindowTop = (int)Top;
				Config.Instance.TrackerWindowLeft = (int)Left;

				//position of add. windows is NaN if they were never opened.
				if (!double.IsNaN(PlayerWindow.Left))
					Config.Instance.PlayerWindowLeft = (int)PlayerWindow.Left;
				if (!double.IsNaN(PlayerWindow.Top))
					Config.Instance.PlayerWindowTop = (int)PlayerWindow.Top;
				Config.Instance.PlayerWindowHeight = (int)PlayerWindow.Height;

				if (!double.IsNaN(OpponentWindow.Left))
					Config.Instance.OpponentWindowLeft = (int)OpponentWindow.Left;
				if (!double.IsNaN(OpponentWindow.Top))
					Config.Instance.OpponentWindowTop = (int)OpponentWindow.Top;
				Config.Instance.OpponentWindowHeight = (int)OpponentWindow.Height;

				if (!double.IsNaN(TimerWindow.Left))
					Config.Instance.TimerWindowLeft = (int)TimerWindow.Left;
				if (!double.IsNaN(TimerWindow.Top))
					Config.Instance.TimerWindowTop = (int)TimerWindow.Top;
				Config.Instance.TimerWindowHeight = (int)TimerWindow.Height;
				Config.Instance.TimerWindowWidth = (int)TimerWindow.Width;

				Overlay.Close();
				HsLogReader.Instance.Stop();
				TimerWindow.Shutdown();
				PlayerWindow.Shutdown();
				OpponentWindow.Shutdown();
				Config.Save();
				WriteDecks();
			}
			catch (Exception)
			{
				//doesnt matter
			}
		}

		private void NotifyIconOnMouseDoubleClick(object sender, MouseEventArgs mouseEventArgs)
		{
			_notifyIcon.Visible = false;
			Show();
			WindowState = WindowState.Normal;
			Activate();
		}

		private void BtnSortFilter_Click(object sender, RoutedEventArgs e)
		{
			FlyoutSortFilter.IsOpen = !FlyoutSortFilter.IsOpen;
		}

		private void SortFilterDecksFlyoutOnSelectedTagsChanged(SortFilterDecks sender, List<string> tags)
		{
			DeckPickerList.SetSelectedTags(tags);
			Config.Instance.SelectedTags = tags;
			Config.Save();
		}

		private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
		{
			var presentationsource = PresentationSource.FromVisual(this);
			if (presentationsource != null) // make sure it's connected
			{
				Helper.DpiScalingX = presentationsource.CompositionTarget.TransformToDevice.M11;
				Helper.DpiScalingY = presentationsource.CompositionTarget.TransformToDevice.M22;
			}
			if (!_foundHsDirectory)
			{
				ShowHsNotInstalledMessage();
				return;
			}
			if (NewVersion != null)
			{
				ShowNewUpdateMessage();
			}
			if (_updatedLogConfig)
			{
				ShowMessage("Restart Hearthstone",
							"This is either your first time starting the tracker or the log.config file has been updated. Please restart Heartstone once, for the tracker to work properly.");
			}

			//preload the manacurve in new deck
			TabControlTracker.SelectedIndex = 1;
			TabControlTracker.UpdateLayout();
			TabControlTracker.SelectedIndex = 0;

			ManaCurveMyDecks.UpdateValues();
		}
		
		private void TabControlTracker_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;
			if (_lastSelectedTab == TabControlTracker.SelectedIndex) return;
			_lastSelectedTab = TabControlTracker.SelectedIndex;
			UpdateTabMarker();
		}

		private async void UpdateTabMarker()
		{
			var tabItem = TabControlTracker.SelectedItem as TabItem;
			if (tabItem == null) return;
			await Task.Delay(50);
			SelectedTabMarker.Width = tabItem.ActualWidth;
			var offset = TabControlTracker.Items.Cast<TabItem>().TakeWhile(t => t != tabItem).Sum(t => t.ActualWidth);
			SelectedTabMarker.Margin = new Thickness(offset, 40, 0, 0);
		}

		#endregion

		#region GENERAL METHODS

		public void ShowIncorrectDeckMessage()
		{
			var decks =
				DeckList.DecksList.Where(
					d => d.Class == Game.PlayingAs && Game.PlayerDrawn.All(c => d.Cards.Contains(c))
					).ToList();
			if (decks.Contains(DeckPickerList.SelectedDeck))
				decks.Remove(DeckPickerList.SelectedDeck);

			Logger.WriteLine(decks.Count + " possible decks found.", "IncorrectDeckMessage");
			if (decks.Count > 0)
			{
				var dsDialog = new DeckSelectionDialog(decks);

				//todo: System.Windows.Data Error: 2 : Cannot find governing FrameworkElement or FrameworkContentElement for target element. BindingExpression:Path=ClassColor; DataItem=null; target element is 'GradientStop' (HashCode=7260326); target property is 'Color' (type 'Color')
				//when opened for seconds time. why?
				dsDialog.ShowDialog();


				var selectedDeck = dsDialog.SelectedDeck;

				if (selectedDeck != null)
				{
					Logger.WriteLine("Selected deck: " + selectedDeck.Name);
					DeckPickerList.SelectDeck(selectedDeck);
					UpdateDeckList(selectedDeck);
					UseDeck(selectedDeck);
				}
				else
				{
					Logger.WriteLine("No deck selected. disabled deck detection.");
					CheckboxDeckDetection.IsChecked = false;
					SaveConfig(false);
				}
			}

			IsShowingIncorrectDeckMessage = false;
			NeedToIncorrectDeckMessage = false;
		}

		private void LoadConfig()
		{
			if (Config.Instance.TrackerWindowTop.HasValue)
				Top = Config.Instance.TrackerWindowTop.Value;
			if (Config.Instance.TrackerWindowLeft.HasValue)
				Left = Config.Instance.TrackerWindowLeft.Value;

			if (Config.Instance.StartMinimized)
			{
				WindowState = WindowState.Minimized;
				if (Config.Instance.MinimizeToTray)
					MinimizeToTray();
			}

			var theme = string.IsNullOrEmpty(Config.Instance.ThemeName)
							? ThemeManager.DetectAppStyle().Item1
							: ThemeManager.AppThemes.First(t => t.Name == Config.Instance.ThemeName);
			var accent = string.IsNullOrEmpty(Config.Instance.AccentName)
							 ? ThemeManager.DetectAppStyle().Item2
							 : ThemeManager.Accents.First(a => a.Name == Config.Instance.AccentName);
			ThemeManager.ChangeAppStyle(Application.Current, accent, theme);
			ComboboxTheme.SelectedItem = theme;
			ComboboxAccent.SelectedItem = accent;

			CheckboxSaveAppData.IsChecked = Config.Instance.SaveInAppData;

			Height = Config.Instance.WindowHeight;
			Game.HighlightCardsInHand = Config.Instance.HighlightCardsInHand;
			Game.HighlightDiscarded = Config.Instance.HighlightDiscarded;
			CheckboxHideOverlayInBackground.IsChecked = Config.Instance.HideInBackground;
			CheckboxHideDrawChances.IsChecked = Config.Instance.HideDrawChances;
			CheckboxHideOpponentDrawChances.IsChecked = Config.Instance.HideOpponentDrawChances;
			CheckboxHideOpponentCards.IsChecked = Config.Instance.HideOpponentCards;
			CheckboxHideOpponentCardCounter.IsChecked = Config.Instance.HideOpponentCardCount;
			CheckboxHideOpponentCardAge.IsChecked = Config.Instance.HideOpponentCardAge;
			CheckboxHidePlayerCardCounter.IsChecked = Config.Instance.HidePlayerCardCount;
			CheckboxHidePlayerCards.IsChecked = Config.Instance.HidePlayerCards;
			CheckboxHideOverlayInMenu.IsChecked = Config.Instance.HideInMenu;
			CheckboxHighlightCardsInHand.IsChecked = Config.Instance.HighlightCardsInHand;
			CheckboxHideOverlay.IsChecked = Config.Instance.HideOverlay;
			CheckboxHideDecksInOverlay.IsChecked = Config.Instance.HideDecksInOverlay;
			CheckboxKeepDecksVisible.IsChecked = Config.Instance.KeepDecksVisible;
			CheckboxMinimizeTray.IsChecked = Config.Instance.MinimizeToTray;
			CheckboxWindowsTopmost.IsChecked = Config.Instance.WindowsTopmost;
			CheckboxWindowsOpenAutomatically.IsChecked = Config.Instance.WindowsOnStartup;
			CheckboxTimerTopmost.IsChecked = Config.Instance.TimerWindowTopmost;
			CheckboxTimerWindow.IsChecked = Config.Instance.TimerWindowOnStartup;
			CheckboxTimerTopmostHsForeground.IsChecked = Config.Instance.TimerWindowTopmostIfHsForeground;
			CheckboxTimerTopmostHsForeground.IsEnabled = Config.Instance.TimerWindowTopmost;
			CheckboxSameScaling.IsChecked = Config.Instance.UseSameScaling;
			CheckboxDeckDetection.IsChecked = Config.Instance.AutoDeckDetection;
			CheckboxWinTopmostHsForeground.IsChecked = Config.Instance.WindowsTopmostIfHsForeground;
			CheckboxWinTopmostHsForeground.IsEnabled = Config.Instance.WindowsTopmost;
			CheckboxAutoSelectDeck.IsEnabled = Config.Instance.AutoDeckDetection;
			CheckboxAutoSelectDeck.IsChecked = Config.Instance.AutoSelectDetectedDeck;
			CheckboxExportName.IsChecked = Config.Instance.ExportSetDeckName;
			CheckboxPrioGolden.IsChecked = Config.Instance.PrioritizeGolden;
			CheckboxBringHsToForegorund.IsChecked = Config.Instance.BringHsToForeground;
			CheckboxFlashHs.IsChecked = Config.Instance.FlashHsOnTurnStart;
			CheckboxHideSecrets.IsChecked = Config.Instance.HideSecrets;
			CheckboxHighlightDiscarded.IsChecked = Config.Instance.HighlightDiscarded;
			CheckboxRemoveCards.IsChecked = Config.Instance.RemoveCardsFromDeck;
			CheckboxHighlightLastDrawn.IsChecked = Config.Instance.HighlightLastDrawn;
			CheckboxStartMinimized.IsChecked = Config.Instance.StartMinimized;
			CheckboxShowPlayerGet.IsChecked = Config.Instance.ShowPlayerGet;
			ToggleSwitchExtraFeatures.IsChecked = Config.Instance.ExtraFeatures;
			CheckboxCheckForUpdates.IsChecked = Config.Instance.CheckForUpdates;

			SliderOverlayOpacity.Value = Config.Instance.OverlayOpacity;
			SliderOpponentOpacity.Value = Config.Instance.OpponentOpacity;
			SliderPlayerOpacity.Value = Config.Instance.PlayerOpacity;
			SliderOverlayPlayerScaling.Value = Config.Instance.OverlayPlayerScaling;
			SliderOverlayOpponentScaling.Value = Config.Instance.OverlayOpponentScaling;

			DeckPickerList.ShowAll = Config.Instance.ShowAllDecks;
			DeckPickerList.SetSelectedTags(Config.Instance.SelectedTags);

			CheckboxHideTimers.IsChecked = Config.Instance.HideTimers;
			SliderTimersHorizontalSpacing.Value = Config.Instance.TimersHorizontalSpacing;
			SliderTimersVerticalSpacing.Value = Config.Instance.TimersVerticalSpacing;


			SortFilterDecksFlyout.LoadTags(DeckList.AllTags);

			SortFilterDecksFlyout.SetSelectedTags(Config.Instance.SelectedTags);
			DeckPickerList.SetSelectedTags(Config.Instance.SelectedTags);

			var tags = new List<string>(DeckList.AllTags);
			tags.Remove("All");
			TagControlNewDeck.LoadTags(tags);
			TagControlMyDecks.LoadTags(tags);
			DeckPickerList.SetTagOperation(Config.Instance.TagOperation);
			SortFilterDecksFlyout.OperationSwitch.IsChecked = Config.Instance.TagOperation == Operation.And;

			SortFilterDecksFlyout.ComboboxDeckSorting.SelectedItem = Config.Instance.SelectedDeckSorting;

			ComboboxWindowBackground.SelectedItem = Config.Instance.SelectedWindowBackground;
			TextboxCustomBackground.IsEnabled = Config.Instance.SelectedWindowBackground == "Custom";
			TextboxCustomBackground.Text = string.IsNullOrEmpty(Config.Instance.WindowsBackgroundHex)
											   ? "#696969"
											   : Config.Instance.WindowsBackgroundHex;
			UpdateAdditionalWindowsBackground();

			ComboboxTextLocationPlayer.SelectedIndex = Config.Instance.TextOnTopPlayer ? 0 : 1;
			ComboboxTextLocationOpponent.SelectedIndex = Config.Instance.TextOnTopOpponent ? 0 : 1;
			Overlay.SetOpponentTextLocation(Config.Instance.TextOnTopOpponent);
			OpponentWindow.SetTextLocation(Config.Instance.TextOnTopOpponent);
			Overlay.SetPlayerTextLocation(Config.Instance.TextOnTopPlayer);
			PlayerWindow.SetTextLocation(Config.Instance.TextOnTopPlayer);

			if (Helper.LanguageDict.Values.Contains(Config.Instance.SelectedLanguage))
			{
				ComboboxLanguages.SelectedItem = Helper.LanguageDict.First(x => x.Value == Config.Instance.SelectedLanguage).Key;
			}

			if (!EventKeys.Contains(Config.Instance.KeyPressOnGameStart))
			{
				Config.Instance.KeyPressOnGameStart = "None";
			}
			ComboboxKeyPressGameStart.SelectedValue = Config.Instance.KeyPressOnGameStart;

			if (!EventKeys.Contains(Config.Instance.KeyPressOnGameEnd))
			{
				Config.Instance.KeyPressOnGameEnd = "None";
			}
			ComboboxKeyPressGameEnd.SelectedValue = Config.Instance.KeyPressOnGameEnd;

			CheckboxHideManaCurveMyDecks.IsChecked = Config.Instance.ManaCurveMyDecks;
			ManaCurveMyDecks.Visibility = Config.Instance.ManaCurveMyDecks ? Visibility.Visible : Visibility.Collapsed;
			CheckboxHideManaCurveNewDeck.IsChecked = Config.Instance.ManaCurveNewDeck;
			ManaCurveNewDeck.Visibility = Config.Instance.ManaCurveNewDeck ? Visibility.Visible : Visibility.Collapsed;

			CheckboxTrackerCardToolTips.IsChecked = Config.Instance.TrackerCardToolTips;
			CheckboxWindowCardToolTips.IsChecked = Config.Instance.WindowCardToolTips;
			CheckboxOverlayCardToolTips.IsChecked = Config.Instance.OverlayCardToolTips;
			CheckboxOverlayAdditionalCardToolTips.IsEnabled = Config.Instance.OverlayCardToolTips;
			CheckboxOverlayAdditionalCardToolTips.IsChecked = Config.Instance.AdditionalOverlayTooltips;

			CheckboxLogGames.IsChecked = Config.Instance.SavePlayedGames;
			TextboxLogGamesPath.IsEnabled = Config.Instance.SavePlayedGames;
			BtnLogGamesSelectDir.IsEnabled = Config.Instance.SavePlayedGames;
			TextboxLogGamesPath.Text = Config.Instance.SavePlayedGamesPath;

			if (Config.Instance.SavePlayedGames && TextboxLogGamesPath.Text.Length == 0)
				TextboxLogGamesPath.BorderBrush = new SolidColorBrush(Colors.Red);

			CheckboxDeckSortingClassFirst.IsChecked = Config.Instance.CardSortingClassFirst;
		}


		private DateTime _lastUpdateCheck;
		private bool _tempUpdateCheckDisabled;

		private async void UpdateOverlayAsync()
		{
			var hsForegroundChanged = false;
			while (_doUpdate)
			{
				if (Process.GetProcessesByName("Hearthstone").Length == 1)
				{
					Overlay.UpdatePosition();

					if (!_tempUpdateCheckDisabled && Config.Instance.CheckForUpdates)
						if (!Game.IsRunning && (DateTime.Now - _lastUpdateCheck) > new TimeSpan(0, 10, 0))
						{
							Version newVersion;
							var currentVersion = Helper.CheckForUpdates(out newVersion);
							if (currentVersion != null && newVersion != null)
							{
								ShowNewUpdateMessage(newVersion);
							}
							_lastUpdateCheck = DateTime.Now;
						}

					Game.IsRunning = true;
					if (!User32.IsForegroundWindow("Hearthstone") && !hsForegroundChanged)
					{
						if (Config.Instance.WindowsTopmostIfHsForeground && Config.Instance.WindowsTopmost)
						{
							PlayerWindow.Topmost = false;
							OpponentWindow.Topmost = false;
							TimerWindow.Topmost = false;
						}
						hsForegroundChanged = true;
					}
					else if (hsForegroundChanged && User32.IsForegroundWindow("Hearthstone"))
					{
						Overlay.Update(true);
						if (Config.Instance.WindowsTopmostIfHsForeground && Config.Instance.WindowsTopmost)
						{
							//if player topmost is set to true before opponent:
							//clicking on the playerwindow and back to hs causes the playerwindow to be behind hs.
							//other way around it works for both windows... what?
							OpponentWindow.Topmost = true;
							PlayerWindow.Topmost = true;
							TimerWindow.Topmost = true;
						}
						hsForegroundChanged = false;
					}
				}
				else
				{
					Overlay.ShowOverlay(false);
					Game.IsRunning = false;
				}
				await Task.Delay(Config.Instance.UpdateDelay);
			}
		}

		private async void ShowNewUpdateMessage(Version newVersion = null)
		{
			const string releaseDownloadUrl = @"https://github.com/Epix37/Hearthstone-Deck-Tracker/releases";
			var settings = new MetroDialogSettings { AffirmativeButtonText = "Download", NegativeButtonText = "Not now" };
			var version = newVersion ?? NewVersion;
			var result =
				await
				this.ShowMessageAsync("New Update available!",
									  "Download version " + string.Format("{0}.{1}.{2}", version.Major, version.Minor,
																		  version.Build) + " at\n" + releaseDownloadUrl,
									  MessageDialogStyle.AffirmativeAndNegative, settings);

			if (result == MessageDialogResult.Affirmative)
			{
				Process.Start(releaseDownloadUrl);
			}
			else
			{
				_tempUpdateCheckDisabled = true;
			}
		}

		public async void ShowMessage(string title, string message)
		{
			await this.ShowMessageAsync(title, message);
		}

		private async void ShowHsNotInstalledMessage()
		{
			var settings = new MetroDialogSettings { AffirmativeButtonText = "Ok", NegativeButtonText = "Select manually" };
			var result =
				await
				this.ShowMessageAsync("Hearthstone install directory not found",
									  "Hearthstone Deck Tracker will not work properly if Hearthstone is not installed on your machine (obviously).",
									  MessageDialogStyle.AffirmativeAndNegative, settings);
			if (result == MessageDialogResult.Negative)
			{
				var dialog = new OpenFileDialog
					{
						Title = "Select Hearthstone.exe",
						DefaultExt = "Hearthstone.exe",
						Filter = "Hearthstone.exe|Hearthstone.exe"
					};
				var dialogResult = dialog.ShowDialog();

				if (dialogResult == true)
				{
					Config.Instance.HearthstoneDirectory = Path.GetDirectoryName(dialog.FileName);
					Config.Save();
					await Restart();
				}
			}
		}

		private async Task Restart()
		{
			await this.ShowMessageAsync("Restarting tracker", "");
			Process.Start(Application.ResourceAssembly.Location);
			Application.Current.Shutdown();
		}

		public void WriteDecks()
		{
			XmlManager<Decks>.Save(_decksPath, DeckList);
		}

		public void SavePlayedCards()
		{
			try
			{
				if (Game.PlayerDrawn != null && Game.PlayerDrawn.Count > 0)
				{
					var serializer = new XmlSerializer(typeof(Card[]));

					if (string.IsNullOrEmpty(Config.Instance.SavePlayedGamesPath))
						return;

					Directory.CreateDirectory(Config.Instance.SavePlayedGamesPath);
					var path = Config.Instance.SavePlayedGamesPath + "\\" + DateTime.Now.ToString("ddMMyyyyHHmmss");
					Directory.CreateDirectory(path);
					Logger.WriteLine("Saving games to: " + path);
					using (var sw = new StreamWriter(path + "\\Player.xml"))
					{
						serializer.Serialize(sw, Game.PlayerDrawn.ToArray());
						Logger.WriteLine("Success saving Player.xml");
					}
					using (var sw = new StreamWriter(path + "\\Opponent.xml"))
					{
						if (Game.OpponentCards != null)
							serializer.Serialize(sw, Game.OpponentCards.ToArray());
						Logger.WriteLine("Success saving Opponent.xml");
					}
				}
			}
			catch (Exception e)
			{
				Logger.WriteLine("Error saving game\n" + e.StackTrace);
			}
		}

		#endregion

		#region MY DECKS - GUI

		private void ButtonNoDeck_Click(object sender, RoutedEventArgs e)
		{
			Logger.WriteLine("set player item source as drawn");
			Overlay.ListViewPlayer.ItemsSource = Game.PlayerDrawn;
			PlayerWindow.ListViewPlayer.ItemsSource = Game.PlayerDrawn;
			Game.IsUsingPremade = false;

			if (DeckPickerList.SelectedDeck != null)
				DeckPickerList.SelectedDeck.IsSelectedInGui = false;

			DeckPickerList.SelectedDeck = null;
			DeckPickerList.SelectedIndex = -1;
			DeckPickerList.ListboxPicker.Items.Refresh();

			UpdateDeckList(null);
			UseDeck(null);
			EnableDeckButtons(false);
			ManaCurveMyDecks.ClearDeck();
		}

		public void EnableDeckButtons(bool enable)
		{
			DeckOptionsFlyout.EnableButtons(enable);
			BtnEditDeck.IsEnabled = enable;
			BtnDeckOptions.IsEnabled = enable;
		}

		private async void BtnEditDeck_Click(object sender, RoutedEventArgs e)
		{
			var selectedDeck = DeckPickerList.SelectedDeck;
			if (selectedDeck == null) return;

			if (_newContainsDeck)
			{
				var settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };
				var result =
					await
					this.ShowMessageAsync("Found unfinished deck", "New Deck Section still contains an unfinished deck. Discard?",
										  MessageDialogStyle.AffirmativeAndNegative, settings);
				if (result == MessageDialogResult.Negative)
				{
					TabControlTracker.SelectedIndex = 1;
					return;
				}
			}

			ClearNewDeckSection();
			EditingDeck = true;
			_newContainsDeck = true;
			NewDeck = (Deck)selectedDeck.Clone();
			ListViewNewDeck.ItemsSource = NewDeck.Cards;

			if (ComboBoxSelectClass.Items.Contains(NewDeck.Class))
				ComboBoxSelectClass.SelectedValue = NewDeck.Class;

			TextBoxDeckName.Text = NewDeck.Name;
			UpdateNewDeckHeader(true);
			UpdateDbListView();


			TagControlNewDeck.SetSelectedTags(NewDeck.Tags);

			TabControlTracker.SelectedIndex = 1;
		}

		private void BtnSetTag_Click(object sender, RoutedEventArgs e)
		{
			FlyoutNewDeckSetTags.IsOpen = !FlyoutNewDeckSetTags.IsOpen;
		}

		public async Task ShowSavedFileMessage(string fileName, string dir)
		{
			var settings = new MetroDialogSettings { NegativeButtonText = "Open folder" };
			var result =
				await
				this.ShowMessageAsync("", "Saved to\n\"" + fileName + "\"", MessageDialogStyle.AffirmativeAndNegative, settings);
			if (result == MessageDialogResult.Negative)
			{
				Process.Start(Path.GetDirectoryName(Application.ResourceAssembly.Location) + "\\" + dir);
			}
		}



		private void BtnDeckOptions_Click(object sender, RoutedEventArgs e)
		{
			FlyoutDeckOptions.IsOpen = true;
		}

		#endregion

		#region MY DECKS - METHODS

		public void UseDeck(Deck selected)
		{
			if (!Game.IsInMenu)
				Game.Reset();

			if (selected != null)
				Game.SetPremadeDeck((Deck)selected.Clone());

			if (!Game.IsInMenu)
			{
				//needs to be true for automatic deck detection to work
				HsLogReader.Instance.Reset(true);
				Overlay.Update(false);
			}

			Overlay.SortViews();
		}

		public void UpdateDeckList(Deck selected)
		{
			ListViewDeck.ItemsSource = null;
			if (selected == null)
			{
				Config.Instance.LastDeck = string.Empty;
				Config.Save();
				return;
			}
			ListViewDeck.ItemsSource = selected.Cards;

			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
			Config.Instance.LastDeck = selected.Name;
			Config.Save();
		}

		#endregion

		#region NEW DECK GUI

		private void ComboBoxFilterClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;
			NewDeck.Class = ComboBoxSelectClass.SelectedValue.ToString();
			_newContainsDeck = true;
			UpdateDbListView();

			ManaCurveNewDeck.UpdateValues();
		}

		private async void BtnSaveDeck_Click(object sender, RoutedEventArgs e)
		{
			NewDeck.Cards = new ObservableCollection<Card>(NewDeck.Cards.OrderBy(c => c.Cost).ThenByDescending(c => c.Type).ThenBy(c => c.Name).ToList());
			ListViewNewDeck.ItemsSource = NewDeck.Cards;

			var deckName = TextBoxDeckName.Text;
			if (EditingDeck)
			{
				var settings = new MetroDialogSettings { AffirmativeButtonText = "Overwrite", NegativeButtonText = "Save as new" };
				var result =
					await
					this.ShowMessageAsync("Saving deck", "How do you wish to save the deck?", MessageDialogStyle.AffirmativeAndNegative, settings);
				if (result == MessageDialogResult.Affirmative)
					SaveDeck(true);
				else if (result == MessageDialogResult.Negative)
					SaveDeck(false);
			}
			else if (DeckList.DecksList.Any(d => d.Name == deckName))
			{
				var settings = new MetroDialogSettings { AffirmativeButtonText = "Overwrite", NegativeButtonText = "Set new name" };
				var result =
					await
					this.ShowMessageAsync("A deck with that name already exists", "Overwriting the deck can not be undone!", MessageDialogStyle.AffirmativeAndNegative, settings);
				if (result == MessageDialogResult.Affirmative)
				{
					Deck oldDeck;
					while ((oldDeck = DeckList.DecksList.FirstOrDefault(d => d.Name == deckName)) != null)
					{
						DeckList.DecksList.Remove(oldDeck);
						DeckPickerList.RemoveDeck(oldDeck);
					}

					SaveDeck(true);
				}
				else if (result == MessageDialogResult.Negative)
					SaveDeck(false);
			}
			else
				SaveDeck(false);

			FlyoutNewDeckSetTags.IsOpen = false;
		}

		private void ComboBoxFilterMana_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;
			UpdateDbListView();
		}

		private void ComboboxNeutral_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;
			UpdateDbListView();
		}

		private void TextBoxDBFilter_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && ListViewDB.Items.Count == 1)
			{
				var card = (Card)ListViewDB.Items[0];
				AddCardToDeck((Card)card.Clone());
			}
		}

		private void BtnImport_OnClick(object sender, RoutedEventArgs e)
		{
			FlyoutDeckImport.IsOpen = true;
			DeckImportFlyout.BtnLastGame.IsEnabled = Game.DrawnLastGame != null;
		}

		private void ListViewDB_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var originalSource = (DependencyObject)e.OriginalSource;
			while ((originalSource != null) && !(originalSource is ListViewItem))
			{
				originalSource = VisualTreeHelper.GetParent(originalSource);
			}

			if (originalSource != null)
			{
				var card = (Card)ListViewDB.SelectedItem;
				if (card == null) return;
				AddCardToDeck((Card)card.Clone());
				_newContainsDeck = true;
			}
		}

		private void ListViewNewDeck_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var originalSource = (DependencyObject)e.OriginalSource;
			while ((originalSource != null) && !(originalSource is ListViewItem))
			{
				originalSource = VisualTreeHelper.GetParent(originalSource);
			}

			if (originalSource != null)
			{
				var card = (Card)ListViewNewDeck.SelectedItem;
				RemoveCardFromDeck(card);
			}
		}

		private void ListViewNewDeck_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			var originalSource = (DependencyObject)e.OriginalSource;
			while ((originalSource != null) && !(originalSource is ListViewItem))
			{
				originalSource = VisualTreeHelper.GetParent(originalSource);
			}

			if (originalSource != null)
			{
				var card = (Card)ListViewNewDeck.SelectedItem;
				AddCardToDeck((Card)card.Clone());
			}
		}

		private void ListViewDB_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				var card = (Card)ListViewDB.SelectedItem;
				if (string.IsNullOrEmpty(card.Name)) return;
				AddCardToDeck((Card)card.Clone());
			}
		}

		private void TextBoxDBFilter_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateDbListView();
		}

		private void BtnClear_Click(object sender, RoutedEventArgs e)
		{
			ShowClearNewDeckMessage();
		}

		#endregion

		#region NEW DECK METHODS

		private void UpdateDbListView()
		{
			var selectedClass = ComboBoxSelectClass.SelectedValue.ToString();
			var selectedNeutral = ComboboxNeutral.SelectedValue.ToString();
			if (selectedClass == "Select a Class")
			{
				ListViewDB.Items.Clear();
			}
			else
			{
				ListViewDB.Items.Clear();

				foreach (var card in Game.GetActualCards())
				{
					if (!card.LocalizedName.ToLowerInvariant().Contains(TextBoxDBFilter.Text.ToLowerInvariant()))
						continue;
					// mana filter
					if (ComboBoxFilterMana.SelectedItem.ToString() == "All"
						|| ((ComboBoxFilterMana.SelectedItem.ToString() == "9+" && card.Cost >= 9)
						|| (ComboBoxFilterMana.SelectedItem.ToString() == card.Cost.ToString())))
					{
						switch (selectedNeutral)
						{
							case "Class + Neutral":
								if (card.GetPlayerClass == selectedClass || card.GetPlayerClass == "Neutral")
									ListViewDB.Items.Add(card);
								break;
							case "Class Only":
								if (card.GetPlayerClass == selectedClass)
									ListViewDB.Items.Add(card);
								break;
							case "Neutral Only":
								if (card.GetPlayerClass == "Neutral")
									ListViewDB.Items.Add(card);
								break;
						}
					}
				}

				Helper.SortCardCollection(ListViewDB.Items, Config.Instance.CardSortingClassFirst);
			}
		}

		private async void SaveDeck(bool overwrite)
		{
			var deckName = TextBoxDeckName.Text;

			if (string.IsNullOrEmpty(deckName))
			{
				var settings = new MetroDialogSettings { AffirmativeButtonText = "Set", DefaultText = deckName };

				var name = await this.ShowInputAsync("No name set", "Please set a name for the deck", settings);

				if (String.IsNullOrEmpty(name))
					return;

				deckName = name;
				TextBoxDeckName.Text = name;
			}

			while (DeckList.DecksList.Any(d => d.Name == deckName) && (!EditingDeck || !overwrite))
			{
				var settings = new MetroDialogSettings { AffirmativeButtonText = "Set", DefaultText = deckName };
				var name =
					await
					this.ShowInputAsync("Name already exists", "You already have a deck with that name, please select a different one.",
										settings);

				if (String.IsNullOrEmpty(name))
					return;

				deckName = name;
				TextBoxDeckName.Text = name;
			}

			if (NewDeck.Cards.Sum(c => c.Count) != 30)
			{
				var settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };

				var result =
					await
					this.ShowMessageAsync("Not 30 cards",
										  string.Format("Deck contains {0} cards. Is this what you want to save anyway?", NewDeck.Cards.Sum(c => c.Count)),
										  MessageDialogStyle.AffirmativeAndNegative, settings);
				if (result != MessageDialogResult.Affirmative)
					return;
			}

			if (EditingDeck && overwrite)
			{
				DeckList.DecksList.Remove(NewDeck);
				DeckPickerList.RemoveDeck(NewDeck);
			}
			NewDeck.Name = deckName;
			NewDeck.Class = ComboBoxSelectClass.SelectedValue.ToString();
			NewDeck.Tags = TagControlNewDeck.GetTags();

			var newDeckClone = (Deck)NewDeck.Clone();
			DeckList.DecksList.Add(newDeckClone);
			DeckPickerList.AddAndSelectDeck(newDeckClone);

			newDeckClone.LastEdited = DateTime.Now;

			WriteDecks();
			BtnSaveDeck.Content = "Save";

			if (EditingDeck)
				TagControlNewDeck.SetSelectedTags(new List<string>());

			TabControlTracker.SelectedIndex = 0;
			EditingDeck = false;

			foreach (var tag in NewDeck.Tags)
				SortFilterDecksFlyout.AddSelectedTag(tag);

			DeckPickerList.UpdateList();
			DeckPickerList.SelectDeck(newDeckClone);

			ClearNewDeckSection();
		}

		private void ClearNewDeckSection()
		{
			UpdateNewDeckHeader(false);
			ComboBoxSelectClass.SelectedIndex = 0;
			TextBoxDeckName.Text = string.Empty;
			TextBoxDBFilter.Text = string.Empty;
			ComboBoxFilterMana.SelectedIndex = 0;
			NewDeck = new Deck();
			ListViewNewDeck.ItemsSource = NewDeck.Cards;
			_newContainsDeck = false;
			EditingDeck = false;
			ManaCurveNewDeck.ClearDeck();
		}

		private void RemoveCardFromDeck(Card card)
		{
			if (card == null)
				return;
			if (card.Count > 1)
			{
				card.Count--;
				ManaCurveNewDeck.UpdateValues();
			}
			else
				NewDeck.Cards.Remove(card);

			ManaCurveNewDeck.SetDeck(NewDeck);
			Helper.SortCardCollection(ListViewNewDeck.Items, Config.Instance.CardSortingClassFirst);
			BtnSaveDeck.Content = "Save*";
			UpdateNewDeckHeader(true);
		}

		private void UpdateNewDeckHeader(bool show)
		{
			const string headerText = "New Deck";
			var cardCount = NewDeck.Cards.Sum(c => c.Count);
			TabItemNewDeck.Header = show ? string.Format("{0} ({1})", headerText, cardCount) : headerText;
			UpdateTabMarker();
		}

		private void AddCardToDeck(Card card)
		{
			if (card == null)
				return;
			if (NewDeck.Cards.Contains(card))
			{
				var cardInDeck = NewDeck.Cards.First(c => c.Name == card.Name);
				cardInDeck.Count++;
				ManaCurveNewDeck.UpdateValues();
			}
			else
			{
				NewDeck.Cards.Add(card);
			}

			ManaCurveNewDeck.SetDeck(NewDeck);
			Helper.SortCardCollection(ListViewNewDeck.Items, Config.Instance.CardSortingClassFirst);
			BtnSaveDeck.Content = "Save*";
			UpdateNewDeckHeader(true);
		}

		public void SetNewDeck(Deck deck, bool editing = false)
		{
			if (deck != null)
			{
				ClearNewDeckSection();
				_newContainsDeck = true;
				EditingDeck = editing;

				NewDeck = (Deck)deck.Clone();
				ListViewNewDeck.ItemsSource = NewDeck.Cards;
				Helper.SortCardCollection(ListViewNewDeck.ItemsSource, false);

				if (ComboBoxSelectClass.Items.Contains(NewDeck.Class))
					ComboBoxSelectClass.SelectedValue = NewDeck.Class;

				TextBoxDeckName.Text = NewDeck.Name;
				UpdateNewDeckHeader(true);
				UpdateDbListView();
			}
		}

		private async void ShowClearNewDeckMessage()
		{
			var settings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" };
			var result = await this.ShowMessageAsync("Clear deck?", "", MessageDialogStyle.AffirmativeAndNegative, settings);
			if (result == MessageDialogResult.Affirmative)
			{
				ClearNewDeckSection();
				UpdateTabMarker();
			}
		}

		#endregion

		#region OPTIONS

		private void CheckboxHighlightCardsInHand_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HighlightCardsInHand = true;
			Game.HighlightCardsInHand = true;
			SaveConfig(true);
		}

		private void CheckboxHighlightCardsInHand_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HighlightCardsInHand = false;
			Game.HighlightCardsInHand = false;
			SaveConfig(true);
		}

		private void CheckboxHideOverlay_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOverlay = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlay_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOverlay = false;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInMenu_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideInMenu = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInMenu_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideInMenu = false;
			SaveConfig(true);
		}

		private void CheckboxHideDrawChances_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideDrawChances = true;
			SaveConfig(true);
			PlayerWindow.LblDrawChance1.Visibility = Visibility.Collapsed;
			PlayerWindow.LblDrawChance2.Visibility = Visibility.Collapsed;
		}

		private void CheckboxHideDrawChances_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideDrawChances = false;
			SaveConfig(true);
			PlayerWindow.LblDrawChance1.Visibility = Visibility.Visible;
			PlayerWindow.LblDrawChance2.Visibility = Visibility.Visible;
		}

		private void CheckboxHideOpponentDrawChances_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentDrawChances = true;
			SaveConfig(true);
			OpponentWindow.LblOpponentDrawChance2.Visibility = Visibility.Collapsed;
			OpponentWindow.LblOpponentDrawChance1.Visibility = Visibility.Collapsed;
		}

		private void CheckboxHideOpponentDrawChances_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentDrawChances = false;
			SaveConfig(true);
			OpponentWindow.LblOpponentDrawChance2.Visibility = Visibility.Visible;
			OpponentWindow.LblOpponentDrawChance1.Visibility = Visibility.Visible;
		}

		private void CheckboxHidePlayerCardCounter_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HidePlayerCardCount = true;
			SaveConfig(true);
			PlayerWindow.LblCardCount.Visibility = Visibility.Collapsed;
			PlayerWindow.LblDeckCount.Visibility = Visibility.Collapsed;
		}

		private void CheckboxHidePlayerCardCounter_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HidePlayerCardCount = false;
			SaveConfig(true);
			PlayerWindow.LblCardCount.Visibility = Visibility.Visible;
			PlayerWindow.LblDeckCount.Visibility = Visibility.Visible;
		}

		private void CheckboxHidePlayerCards_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HidePlayerCards = true;
			SaveConfig(true);
			PlayerWindow.ListViewPlayer.Visibility = Visibility.Collapsed;
		}

		private void CheckboxHidePlayerCards_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HidePlayerCards = false;
			SaveConfig(true);
			PlayerWindow.ListViewPlayer.Visibility = Visibility.Visible;
		}

		private void CheckboxHideOpponentCardCounter_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCardCount = true;
			SaveConfig(true);
			OpponentWindow.LblOpponentCardCount.Visibility = Visibility.Collapsed;
			OpponentWindow.LblOpponentDeckCount.Visibility = Visibility.Collapsed;
		}

		private void CheckboxHideOpponentCardCounter_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCardCount = false;
			SaveConfig(true);
			OpponentWindow.LblOpponentCardCount.Visibility = Visibility.Visible;
			OpponentWindow.LblOpponentDeckCount.Visibility = Visibility.Visible;
		}

		private void CheckboxHideOpponentCards_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCards = true;
			SaveConfig(true);
			OpponentWindow.ListViewOpponent.Visibility = Visibility.Collapsed;
		}

		private void CheckboxHideOpponentCards_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCards = false;
			SaveConfig(true);
			OpponentWindow.ListViewOpponent.Visibility = Visibility.Visible;
		}

		private void CheckboxHideOpponentCardAge_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCardAge = false;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentCardAge_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCardAge = true;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentCardMarks_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCardMarks = false;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentCardMarks_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCardMarks = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInBackground_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideInBackground = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInBackground_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideInBackground = false;
			SaveConfig(true);
		}

		private void CheckboxWindowsTopmost_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.WindowsTopmost = true;
			PlayerWindow.Topmost = true;
			OpponentWindow.Topmost = true;
			CheckboxWinTopmostHsForeground.IsEnabled = true;
			SaveConfig(true);
		}

		private void CheckboxWindowsTopmost_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.WindowsTopmost = false;
			PlayerWindow.Topmost = false;
			OpponentWindow.Topmost = false;
			CheckboxWinTopmostHsForeground.IsEnabled = false;
			CheckboxWinTopmostHsForeground.IsChecked = false;
			SaveConfig(true);
		}

		private void CheckboxWindowsOpenAutomatically_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			PlayerWindow.Show();
			PlayerWindow.Activate();
			OpponentWindow.Show();
			OpponentWindow.Activate();

			PlayerWindow.SetCardCount(Game.PlayerHandCount, 30 - Game.PlayerDrawn.Where(c => !c.IsStolen).Sum(card => card.Count));

			OpponentWindow.SetOpponentCardCount(Game.OpponentHandCount, Game.OpponentDeckCount, Game.OpponentHasCoin);

			Config.Instance.WindowsOnStartup = true;
			SaveConfig(true);
		}

		private void CheckboxWindowsOpenAutomatically_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			PlayerWindow.Hide();
			OpponentWindow.Hide();
			Config.Instance.WindowsOnStartup = false;
			SaveConfig(true);
		}

		private void CheckboxWinTopmostHsForeground_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.WindowsTopmostIfHsForeground = true;
			PlayerWindow.Topmost = false;
			OpponentWindow.Topmost = false;
			SaveConfig(false);
		}

		private void CheckboxWinTopmostHsForeground_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.WindowsTopmostIfHsForeground = false;
			if (Config.Instance.WindowsTopmost)
			{
				PlayerWindow.Topmost = true;
				OpponentWindow.Topmost = true;
			}
			SaveConfig(false);
		}

		private void CheckboxTimerTopmost_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.TimerWindowTopmost = true;
			TimerWindow.Topmost = true;
			CheckboxTimerTopmostHsForeground.IsEnabled = true;
			SaveConfig(true);
		}

		private void CheckboxTimerTopmost_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.TimerWindowTopmost = false;
			TimerWindow.Topmost = false;
			CheckboxTimerTopmostHsForeground.IsEnabled = false;
			CheckboxTimerTopmostHsForeground.IsChecked = false;
			SaveConfig(true);
		}

		private void CheckboxTimerWindow_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			TimerWindow.Show();
			TimerWindow.Activate();
			Config.Instance.TimerWindowOnStartup = true;
			SaveConfig(true);
		}

		private void CheckboxTimerWindow_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			TimerWindow.Hide();
			Config.Instance.TimerWindowOnStartup = false;
			SaveConfig(true);
		}

		private void CheckboxTimerTopmostHsForeground_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.TimerWindowTopmostIfHsForeground = true;
			TimerWindow.Topmost = false;
			SaveConfig(false);
		}

		private void CheckboxTimerTopmostHsForeground_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.TimerWindowTopmostIfHsForeground = false;
			if (Config.Instance.TimerWindowTopmost)
			{
				TimerWindow.Topmost = true;
			}
			SaveConfig(false);
		}

		private void SaveConfig(bool updateOverlay)
		{
			Config.Save();
			if (updateOverlay)
				Overlay.Update(true);
		}


		private void SliderOverlayOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			Config.Instance.OverlayOpacity = SliderOverlayOpacity.Value;
			SaveConfig(true);
		}

		private void SliderOpponentOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			Config.Instance.OpponentOpacity = SliderOpponentOpacity.Value;
			SaveConfig(true);
		}

		private void SliderPlayerOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			Config.Instance.PlayerOpacity = SliderPlayerOpacity.Value;
			SaveConfig(true);
		}

		private void CheckboxKeepDecksVisible_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.KeepDecksVisible = true;
			SaveConfig(true);
		}

		private void CheckboxKeepDecksVisible_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.KeepDecksVisible = false;
			SaveConfig(true);
		}

		private void CheckboxMinimizeTray_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.MinimizeToTray = true;
			SaveConfig(false);
		}

		private void CheckboxMinimizeTray_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.MinimizeToTray = false;
			SaveConfig(false);
		}

		private void CheckboxSameScaling_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.UseSameScaling = true;
			SaveConfig(false);
		}

		private void CheckboxSameScaling_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.UseSameScaling = false;
			SaveConfig(false);
		}

		private void CheckboxDeckDetection_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.AutoDeckDetection = true;
			CheckboxAutoSelectDeck.IsEnabled = true;
			SaveConfig(false);
		}

		private void CheckboxDeckDetection_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.AutoDeckDetection = false;
			CheckboxAutoSelectDeck.IsChecked = false;
			CheckboxAutoSelectDeck.IsEnabled = false;
			SaveConfig(false);
		}

		private void CheckboxAutoSelectDeck_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.AutoSelectDetectedDeck = true;
			SaveConfig(false);
		}

		private void CheckboxAutoSelectDeck_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.AutoSelectDetectedDeck = false;
			SaveConfig(false);
		}

		private void SliderOverlayPlayerScaling_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			var scaling = SliderOverlayPlayerScaling.Value;
			Config.Instance.OverlayPlayerScaling = scaling;
			SaveConfig(false);
			Overlay.UpdateScaling();

			if (Config.Instance.UseSameScaling && SliderOverlayOpponentScaling.Value != scaling)
			{
				SliderOverlayOpponentScaling.Value = scaling;
			}
		}

		private void SliderOverlayOpponentScaling_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			var scaling = SliderOverlayOpponentScaling.Value;
			Config.Instance.OverlayOpponentScaling = scaling;
			SaveConfig(false);
			Overlay.UpdateScaling();

			if (Config.Instance.UseSameScaling && SliderOverlayPlayerScaling.Value != scaling)
			{
				SliderOverlayPlayerScaling.Value = scaling;
			}
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(e.Uri.AbsoluteUri);
		}

		private void CheckboxHideTimers_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideTimers = true;
			SaveConfig(true);
		}

		private void CheckboxHideTimers_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideTimers = false;
			SaveConfig(true);
		}

		private void SliderTimersHorizontalSpacing_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			Config.Instance.TimersHorizontalSpacing = SliderTimersHorizontalSpacing.Value;
			SaveConfig(true);
		}

		private void SliderTimersVerticalSpacing_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!_initialized) return;
			Config.Instance.TimersVerticalSpacing = SliderTimersVerticalSpacing.Value;
			SaveConfig(true);
		}

		private void ComboboxAccent_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;
			var accent = ComboboxAccent.SelectedItem as Accent;
			if (accent != null)
			{
				ThemeManager.ChangeAppStyle(Application.Current, accent, ThemeManager.DetectAppStyle().Item1);
				Config.Instance.AccentName = accent.Name;
				SaveConfig(false);
			}
		}

		private void ComboboxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;
			var theme = ComboboxTheme.SelectedItem as AppTheme;
			if (theme != null)
			{
				ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.DetectAppStyle().Item2, theme);
				Config.Instance.ThemeName = theme.Name;
				//if(ComboboxWindowBackground.SelectedItem.ToString() != "Default")
				UpdateAdditionalWindowsBackground();
				SaveConfig(false);
			}
		}

		private void ComboboxWindowBackground_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;
			TextboxCustomBackground.IsEnabled = ComboboxWindowBackground.SelectedItem.ToString() == "Custom";
			Config.Instance.SelectedWindowBackground = ComboboxWindowBackground.SelectedItem.ToString();
			UpdateAdditionalWindowsBackground();
		}

		private void UpdateAdditionalWindowsBackground(Brush brush = null)
		{
			var background = brush;

			switch (ComboboxWindowBackground.SelectedItem.ToString())
			{
				case "Theme":
					background = Background;
					break;
				case "Light":
					background = SystemColors.ControlLightBrush;
					break;
				case "Dark":
					background = SystemColors.ControlDarkDarkBrush;
					break;
			}
			if (background == null)
			{
				var hexBackground = BackgroundFromHex();
				if (hexBackground != null)
				{
					PlayerWindow.Background = hexBackground;
					OpponentWindow.Background = hexBackground;
					TimerWindow.Background = hexBackground;
				}
			}
			else
			{
				PlayerWindow.Background = background;
				OpponentWindow.Background = background;
				TimerWindow.Background = background;
			}
		}

		private SolidColorBrush BackgroundFromHex()
		{
			SolidColorBrush brush = null;
			var hex = TextboxCustomBackground.Text;
			if (hex.StartsWith("#")) hex = hex.Remove(0, 1);
			if (!string.IsNullOrEmpty(hex) && hex.Length == 6 && Helper.IsHex(hex))
			{
				var color = ColorTranslator.FromHtml("#" + hex);
				brush = new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
			}
			return brush;
		}

		private void TextboxCustomBackground_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (!_initialized || ComboboxWindowBackground.SelectedItem.ToString() != "Custom") return;
			var background = BackgroundFromHex();
			if (background != null)
			{
				UpdateAdditionalWindowsBackground(background);
				Config.Instance.WindowsBackgroundHex = TextboxCustomBackground.Text;
				SaveConfig(false);
			}
		}

		private void ComboboxTextLocationOpponent_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.TextOnTopOpponent = ComboboxTextLocationOpponent.SelectedItem.ToString() == "Top";

			SaveConfig(false);
			Overlay.SetOpponentTextLocation(Config.Instance.TextOnTopOpponent);
			OpponentWindow.SetTextLocation(Config.Instance.TextOnTopOpponent);
		}

		private void ComboboxTextLocationPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;

			Config.Instance.TextOnTopPlayer = ComboboxTextLocationPlayer.SelectedItem.ToString() == "Top";
			SaveConfig(false);

			Overlay.SetPlayerTextLocation(Config.Instance.TextOnTopPlayer);
			PlayerWindow.SetTextLocation(Config.Instance.TextOnTopPlayer);
		}

		private async void ComboboxLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;
			var language = ComboboxLanguages.SelectedValue.ToString();
			if (!Helper.LanguageDict.ContainsKey(language))
				return;

			var selectedLanguage = Helper.LanguageDict[language];

			if (!File.Exists(string.Format("Files/cardsDB.{0}.json", selectedLanguage)))
			{
				return;
			}

			Config.Instance.SelectedLanguage = selectedLanguage;


			await Restart();
		}

		private void CheckboxExportName_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.ExportSetDeckName = true;
			SaveConfig(false);
		}

		private void CheckboxExportName_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.ExportSetDeckName = false;
			SaveConfig(false);
		}

		private void CheckboxPrioGolden_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.PrioritizeGolden = true;
			SaveConfig(false);
		}

		private void CheckboxPrioGolden_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.PrioritizeGolden = false;
			SaveConfig(false);
		}

		private void ComboboxKeyPressGameStart_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.KeyPressOnGameStart = ComboboxKeyPressGameStart.SelectedValue.ToString();
			SaveConfig(false);
		}

		private void ComboboxKeyPressGameEnd_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.KeyPressOnGameEnd = ComboboxKeyPressGameEnd.SelectedValue.ToString();
			SaveConfig(false);
		}

		private void CheckboxHideDecksInOverlay_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.HideDecksInOverlay = true;
			SaveConfig(true);
		}

		private void CheckboxHideDecksInOverlay_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.HideDecksInOverlay = false;
			SaveConfig(true);
		}

		private async void CheckboxAppData_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			var path = Config.Instance.ConfigPath;
			Config.Instance.SaveInAppData = true;
			XmlManager<Config>.Save(path, Config.Instance);
			await Restart();
		}

		private async void CheckboxAppData_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			var path = Config.Instance.ConfigPath;
			Config.Instance.SaveInAppData = false;
			XmlManager<Config>.Save(path, Config.Instance);
			await Restart();
		}

		private void CheckboxManaCurveMyDecks_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.ManaCurveMyDecks = true;
			ManaCurveMyDecks.Visibility = Visibility.Visible;
			SaveConfig(false);
		}

		private void CheckboxManaCurveMyDecks_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.ManaCurveMyDecks = false;
			ManaCurveMyDecks.Visibility = Visibility.Collapsed;
			SaveConfig(false);
		}

		private void CheckboxManaCurveNewDeck_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.ManaCurveNewDeck = true;
			ManaCurveNewDeck.Visibility = Visibility.Visible;
			SaveConfig(false);
		}

		private void CheckboxManaCurveNewDeck_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.ManaCurveNewDeck = false;
			ManaCurveNewDeck.Visibility = Visibility.Collapsed;
			SaveConfig(false);
		}

		private async void CheckboxTrackerCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			//this is probably somehow possible without restarting
			if (!_initialized) return;
			Config.Instance.TrackerCardToolTips = true;
			SaveConfig(false);
			await Restart();
		}

		private async void CheckboxTrackerCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			//this is probably somehow possible without restarting
			if (!_initialized) return;
			Config.Instance.TrackerCardToolTips = false;
			SaveConfig(false);
			await Restart();
		}

		private void CheckboxWindowCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.WindowCardToolTips = true;
			SaveConfig(false);
		}

		private void CheckboxWindowCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.WindowCardToolTips = false;
			SaveConfig(false);
		}

		private void CheckboxOverlayCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.OverlayCardToolTips = true;
			CheckboxOverlayAdditionalCardToolTips.IsEnabled = true;
			SaveConfig(true);
		}

		private void CheckboxOverlayCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.OverlayCardToolTips = false;
			CheckboxOverlayAdditionalCardToolTips.IsChecked = false;
			CheckboxOverlayAdditionalCardToolTips.IsEnabled = false;
			SaveConfig(true);
		}

		private void CheckboxLogGames_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			TextboxLogGamesPath.IsEnabled = true;
			BtnLogGamesSelectDir.IsEnabled = true;
			Config.Instance.SavePlayedGames = true;
			if (TextboxLogGamesPath.Text.Length == 0)
				TextboxLogGamesPath.BorderBrush = new SolidColorBrush(Colors.Red);
			SaveConfig(false);
		}

		private void CheckboxLogGames_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			TextboxLogGamesPath.IsEnabled = false;
			BtnLogGamesSelectDir.IsEnabled = false;
			Config.Instance.SavePlayedGames = false;
			SaveConfig(false);
		}

		private void BtnLogGamesSelectDir_Click(object sender, RoutedEventArgs e)
		{
			var folderDialog = new FolderBrowserDialog();
			var result = folderDialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				TextboxLogGamesPath.Text = folderDialog.SelectedPath;
				Config.Instance.SavePlayedGamesPath = folderDialog.SelectedPath;

				TextboxLogGamesPath.BorderBrush =
					new SolidColorBrush(TextboxLogGamesPath.Text.Length == 0
											? Colors.Red
											: SystemColors.ActiveBorderColor);
				SaveConfig(false);
			}
		}

		private void CheckboxDeckSortingClassFirst_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.CardSortingClassFirst = true;
			SaveConfig(false);
			Helper.SortCardCollection(ListViewDeck.ItemsSource, true);
			Helper.SortCardCollection(ListViewNewDeck.Items, true);
		}

		private void CheckboxDeckSortingClassFirst_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.CardSortingClassFirst = false;
			SaveConfig(false);
			Helper.SortCardCollection(ListViewDeck.ItemsSource, false);
			Helper.SortCardCollection(ListViewNewDeck.Items, false);
		}

		private void CheckboxBringHsToForegorund_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.BringHsToForeground = true;
			SaveConfig(false);
		}

		private void CheckboxBringHsToForegorund_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.BringHsToForeground = false;
			SaveConfig(false);
		}

		private void CheckboxFlashHs_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.FlashHsOnTurnStart = true;
			SaveConfig(false);
		}

		private void CheckboxFlashHs_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.FlashHsOnTurnStart = false;
			SaveConfig(false);
		}

		private void CheckboxHideSecrets_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideSecrets = true;
			SaveConfig(false);
			Overlay.HideSecrets();
		}

		private void CheckboxHideSecrets_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideSecrets = false;
			SaveConfig(false);
			if (!Game.IsInMenu)
				Overlay.ShowSecrets(Game.PlayingAgainst);
		}

		private void CheckboxHighlightDiscarded_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HighlightDiscarded = true;
			Game.HighlightDiscarded = true;
			SaveConfig(true);
		}

		private void CheckboxHighlightDiscarded_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HighlightDiscarded = false;
			Game.HighlightDiscarded = false;
			SaveConfig(true);
		}

		private async void BtnUnlockOverlay_Click(object sender, RoutedEventArgs e)
		{
			if (User32.GetHearthstoneWindow() == IntPtr.Zero) return;
			BtnUnlockOverlay.Content = await Overlay.UnlockUI() ? "Lock" : "Unlock";
		}

		private async void BtnResetOverlay_Click(object sender, RoutedEventArgs e)
		{
			var result =
				await
				this.ShowMessageAsync("Resetting overlay to default",
									  "Positions of: Player Deck, Opponent deck, Timers and Secrets will be reset to default. Are you sure?",
									  MessageDialogStyle.AffirmativeAndNegative);
			if (result != MessageDialogResult.Affirmative)
				return;

			if ((string)BtnUnlockOverlay.Content == "Lock")
			{
				await Overlay.UnlockUI();
				BtnUnlockOverlay.Content = "Unlock";
			}

			var defaultConfig = new Config();

			Config.Instance.PlayerDeckTop = defaultConfig.PlayerDeckTop;
			Config.Instance.PlayerDeckLeft = defaultConfig.PlayerDeckLeft;
			Config.Instance.PlayerDeckHeight = defaultConfig.PlayerDeckHeight;

			Config.Instance.OpponentDeckTop = defaultConfig.OpponentDeckTop;
			Config.Instance.OpponentDeckLeft = defaultConfig.OpponentDeckLeft;
			Config.Instance.OpponentDeckHeight = defaultConfig.OpponentDeckHeight;

			Config.Instance.TimersHorizontalPosition = defaultConfig.TimersHorizontalPosition;
			Config.Instance.TimersHorizontalSpacing = defaultConfig.TimersHorizontalSpacing;

			Config.Instance.SecretsTop = defaultConfig.SecretsTop;
			Config.Instance.SecretsLeft = defaultConfig.SecretsLeft;

			SaveConfig(true);
		}

		private void CheckboxRemoveCards_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized || !Game.IsUsingPremade) return;
			Config.Instance.RemoveCardsFromDeck = true;
			SaveConfig(false);
			Game.Reset();
			Game.SetPremadeDeck((Deck)DeckPickerList.SelectedDeck.Clone());
			HsLogReader.Instance.Reset(true);
			Overlay.Update(true);
		}

		private void CheckboxRemoveCards_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized || !Game.IsUsingPremade) return;
			Config.Instance.RemoveCardsFromDeck = false;
			SaveConfig(false);
			Game.Reset();
			Game.SetPremadeDeck((Deck)DeckPickerList.SelectedDeck.Clone());
			HsLogReader.Instance.Reset(true);
			Overlay.Update(true);
		}

		private void CheckboxHighlightLastDrawn_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HighlightLastDrawn = true;
			SaveConfig(false);
		}

		private void CheckboxHighlightLastDrawn_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HighlightLastDrawn = false;
			SaveConfig(false);
		}

		private void CheckboxStartMinimized_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.StartMinimized = true;
			SaveConfig(false);
		}

		private void CheckboxStartMinimized_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.StartMinimized = false;
			SaveConfig(false);
		}

		private void CheckboxShowPlayerGet_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.ShowPlayerGet = true;
			Overlay.Update(true);
		}

		private void CheckboxShowPlayerGet_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.ShowPlayerGet = false;
			Overlay.Update(true);
		}

		private void CheckboxOverlayAdditionalCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.AdditionalOverlayTooltips = true;
			SaveConfig(false);
		}

		private void CheckboxOverlayAdditionalCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.AdditionalOverlayTooltips = false;
			SaveConfig(false);
		}

		private void ToggleSwitchExtraFeatures_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.ExtraFeatures = true;
			Overlay.HookMouse();
			SaveConfig(false);
		}

		private void ToggleSwitchExtraFeatures_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.ExtraFeatures = false;
			Overlay.UnHookMouse();
			SaveConfig(false);
		}

		private void CheckboxCheckForUpdates_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.CheckForUpdates = true;
			SaveConfig(false);
		}

		private void CheckboxCheckForUpdates_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.CheckForUpdates = false;
			SaveConfig(false);
		}

		#endregion

		#region Constructor

		public MainWindow()
		{
			// Set working directory to path of executable
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

			InitializeComponent();

			Helper.MainWindow = this;
			_configPath = Config.Load();
			HsLogReader.Create();

			var configVersion = string.IsNullOrEmpty(Config.Instance.CreatedByVersion)
				                    ? null
				                    : new Version(Config.Instance.CreatedByVersion);

			Version currentVersion;
			if (Config.Instance.CheckForUpdates)
			{
				currentVersion = Helper.CheckForUpdates(out NewVersion);
				_lastUpdateCheck = DateTime.Now;
			}
			else
				currentVersion = Helper.GetCurrentVersion();

			if (currentVersion != null)
			{
				TxtblockVersion.Text = string.Format("Version: {0}.{1}.{2}", currentVersion.Major, currentVersion.Minor,
													 currentVersion.Build);

				// Assign current version to the config instance so that it will be saved when the config
				// is rewritten to disk, thereby telling us what version of the application created it
				Config.Instance.CreatedByVersion = currentVersion.ToString();
			}

			ConvertLegacyConfig(currentVersion, configVersion);

			if (Config.Instance.SelectedTags.Count == 0)
				Config.Instance.SelectedTags.Add("All");

			if (Config.Instance.GenerateLog)
			{
				Directory.CreateDirectory("Logs");
				var listener = new TextWriterTraceListener(Config.Instance.LogFilePath);
				Trace.Listeners.Add(listener);
				Trace.AutoFlush = true;
			}

			_foundHsDirectory = FindHearthstoneDir();

			if (_foundHsDirectory)
				_updatedLogConfig = UpdateLogConfigFile();

			//hearthstone, loads db etc - needs to be loaded before playerdecks, since cards are only saved as ids now
			//Game.Create();
			Game.Reset();

			_decksPath = Config.Instance.HomeDir + "PlayerDecks.xml";
			SetupDeckListFile();
			try
			{
				DeckList = XmlManager<Decks>.Load(_decksPath);
			}
			catch (Exception e)
			{
				MessageBox.Show(
					e.Message + "\n\n" + e.InnerException +
					"\n\n If you don't know how to fix this, please delete " + _decksPath +
					" (this will cause you to lose your decks).",
					"Error loading PlayerDecks.xml");
				Application.Current.Shutdown();
			}

			foreach (var deck in DeckList.DecksList)
			{
				DeckPickerList.AddDeck(deck);
			}

			_notifyIcon = new NotifyIcon { Icon = new Icon(@"Images/HearthstoneDeckTracker.ico") };
			_notifyIcon.MouseDoubleClick += NotifyIconOnMouseDoubleClick;
			_notifyIcon.Visible = false;

			NewDeck = new Deck();
			ListViewNewDeck.ItemsSource = NewDeck.Cards;

			//create overlay
			Overlay = new OverlayWindow { Topmost = true };
			if (_foundHsDirectory)
				Overlay.Show();

			PlayerWindow = new PlayerWindow(Config.Instance, Game.IsUsingPremade ? Game.PlayerDeck : Game.PlayerDrawn);
			OpponentWindow = new OpponentWindow(Config.Instance, Game.OpponentCards);
			TimerWindow = new TimerWindow(Config.Instance);

			if (Config.Instance.WindowsOnStartup)
			{
				PlayerWindow.Show();
				OpponentWindow.Show();
			}
			if (Config.Instance.TimerWindowOnStartup)
			{
				TimerWindow.Show();
			}
			if (!DeckList.AllTags.Contains("All"))
			{
				DeckList.AllTags.Add("All");
				WriteDecks();
			}
			if (!DeckList.AllTags.Contains("Arena"))
			{
				DeckList.AllTags.Add("Arena");
				WriteDecks();
			}
			if (!DeckList.AllTags.Contains("Constructed"))
			{
				DeckList.AllTags.Add("Constructed");
				WriteDecks();
			}

			ComboboxAccent.ItemsSource = ThemeManager.Accents;
			ComboboxTheme.ItemsSource = ThemeManager.AppThemes;
			ComboboxLanguages.ItemsSource = Helper.LanguageDict.Keys;

			ComboboxKeyPressGameStart.ItemsSource = EventKeys;
			ComboboxKeyPressGameEnd.ItemsSource = EventKeys;

			LoadConfig();

			//this has to happen before reader starts
			var lastDeck = DeckList.DecksList.FirstOrDefault(d => d.Name == Config.Instance.LastDeck);
			DeckPickerList.SelectDeck(lastDeck);

			DeckOptionsFlyout.DeckOptionsButtonClicked += sender => { FlyoutDeckOptions.IsOpen = false; };

			DeckImportFlyout.DeckOptionsButtonClicked += sender => { FlyoutDeckImport.IsOpen = false; };

			TurnTimer.Create(90);

			SortFilterDecksFlyout.HideStuffToCreateNewTag();
			TagControlNewDeck.OperationSwitch.Visibility = Visibility.Collapsed;
			TagControlMyDecks.OperationSwitch.Visibility = Visibility.Collapsed;
			TagControlNewDeck.PnlSortDecks.Visibility = Visibility.Collapsed;
			TagControlMyDecks.PnlSortDecks.Visibility = Visibility.Collapsed;

			//SortFilterDecksFlyout.SelectedTagsChanged += SortFilterDecksFlyoutOnSelectedTagsChanged;
			//SortFilterDecksFlyout.OperationChanged += SortFilterDecksFlyoutOnOperationChanged;

			UpdateDbListView();

			_doUpdate = _foundHsDirectory;
			UpdateOverlayAsync();

			_initialized = true;

			DeckPickerList.UpdateList();
			if (lastDeck != null)
			{
				DeckPickerList.SelectDeck(lastDeck);
				UpdateDeckList(lastDeck);
				UseDeck(lastDeck);
			}

			if (_foundHsDirectory)
			{
				HsLogReader.Instance.Start();
			}

			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
			DeckPickerList.SortDecks();
		}

		// Logic for dealing with legacy config file semantics
		// Use difference of versions to determine what should be done
		private static void ConvertLegacyConfig(Version currentVersion, Version configVersion)
		{
			var config = Config.Instance;
			var converted = false;

			var v0_3_21 = new Version(0, 3, 21, 0);

			if (configVersion == null) // Config was created prior to version tracking being introduced (v0.3.20)
			{
				// We previously assumed negative pixel coordinates were invalid, but in fact they can go negative
				// with multi-screen setups. Negative positions were being used to represent 'no specific position'
				// as a default. That means that when the windows are created for the first time, we let the operating
				// system decide where to place them. As we should not be using negative positions for this purpose, since
				// they are in fact a valid range of pixel positions, we now use nullable types instead. The default
				// 'no specific position' is now expressed when the positions are null.
				{
					if (config.TrackerWindowLeft.HasValue && config.TrackerWindowLeft.Value < 0)
					{
						config.TrackerWindowLeft = Config.Defaults.TrackerWindowLeft;
						converted = true;
					}
					if (config.TrackerWindowTop.HasValue && config.TrackerWindowTop.Value < 0)
					{
						config.TrackerWindowTop = Config.Defaults.TrackerWindowTop;
						converted = true;
					}

					if (config.PlayerWindowLeft.HasValue && config.PlayerWindowLeft.Value < 0)
					{
						config.PlayerWindowLeft = Config.Defaults.PlayerWindowLeft;
						converted = true;
					}
					if (config.PlayerWindowTop.HasValue && config.PlayerWindowTop.Value < 0)
					{
						config.PlayerWindowTop = Config.Defaults.PlayerWindowTop;
						converted = true;
					}

					if (config.OpponentWindowLeft.HasValue && config.OpponentWindowLeft.Value < 0)
					{
						config.OpponentWindowLeft = Config.Defaults.OpponentWindowLeft;
						converted = true;
					}
					if (config.OpponentWindowTop.HasValue && config.OpponentWindowTop.Value < 0)
					{
						config.OpponentWindowTop = Config.Defaults.OpponentWindowTop;
						converted = true;
					}

					if (config.TimerWindowLeft.HasValue && config.TimerWindowLeft.Value < 0)
					{
						config.TimerWindowLeft = Config.Defaults.TimerWindowLeft;
						converted = true;
					}
					if (config.TimerWindowTop.HasValue && config.TimerWindowTop.Value < 0)
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
					if (config.PlayerWindowHeight == 0)
					{
						config.PlayerWindowHeight = Config.Defaults.PlayerWindowHeight;
						converted = true;
					}

					if (config.OpponentWindowHeight == 0)
					{
						config.OpponentWindowHeight = Config.Defaults.OpponentWindowHeight;
						converted = true;
					}
				}
			}
			else if (configVersion <= v0_3_21) // Config must be between v0.3.20 and v0.3.21 inclusive
			{
				// It was still possible in 0.3.21 to see (-32000, -32000) window positions
				// under certain circumstances (GitHub issue #135).
				{
					if (config.TrackerWindowLeft == -32000)
					{
						config.TrackerWindowLeft = Config.Defaults.TrackerWindowLeft;
						converted = true;
					}
					if (config.TrackerWindowTop == -32000)
					{
						config.TrackerWindowTop = Config.Defaults.TrackerWindowTop;
						converted = true;
					}

					if (config.PlayerWindowLeft == -32000)
					{
						config.PlayerWindowLeft = Config.Defaults.PlayerWindowLeft;
						converted = true;
					}
					if (config.PlayerWindowTop == -32000)
					{
						config.PlayerWindowTop = Config.Defaults.PlayerWindowTop;
						converted = true;
					}

					if (config.OpponentWindowLeft == -32000)
					{
						config.OpponentWindowLeft = Config.Defaults.OpponentWindowLeft;
						converted = true;
					}
					if (config.OpponentWindowTop == -32000)
					{
						config.OpponentWindowTop = Config.Defaults.OpponentWindowTop;
						converted = true;
					}

					if (config.TimerWindowLeft == -32000)
					{
						config.TimerWindowLeft = Config.Defaults.TimerWindowLeft;
						converted = true;
					}
					if (config.TimerWindowTop == -32000)
					{
						config.TimerWindowTop = Config.Defaults.TimerWindowTop;
						converted = true;
					}
				}
			}

			if (converted)
			{
				Config.SaveBackup();
				Config.Save();
			}
		}

		private bool FindHearthstoneDir()
		{
			var found = false;
			if (string.IsNullOrEmpty(Config.Instance.HearthstoneDirectory) ||
				!File.Exists(Config.Instance.HearthstoneDirectory + @"\Hearthstone.exe"))
			{
				using (
					var hsDirKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Hearthstone")
					)
				{
					if (hsDirKey != null)
					{
						var hsDir = (string)hsDirKey.GetValue("InstallLocation");

						//verify the installlocation actually is correct (possibly moved?)
						if (File.Exists(hsDir + @"\Hearthstone.exe"))
						{
							Config.Instance.HearthstoneDirectory = hsDir;
							Config.Save();
							found = true;
						}
					}
				}
			}
			else
			{
				found = true;
			}

			return found;
		}

		private bool UpdateLogConfigFile()
		{
			var updated = false;
			//check for log config and create if not existing
			try
			{
				//always overwrite is true by default. 
				if (!File.Exists(_logConfigPath))
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
					if (file.LastWriteTime > localFile.LastWriteTime)
					{
						updated = true;
						File.Copy("Files/log.config", _logConfigPath, true);
						Logger.WriteLine(string.Format("Copied log.config to {0} (file newer)", _configPath));
					}
					else if (Config.Instance.AlwaysOverwriteLogConfig)
					{
						File.Copy("Files/log.config", _logConfigPath, true);
						Logger.WriteLine(string.Format("Copied log.config to {0} (AlwaysOverwriteLogConfig)", _configPath));
					}
				}
			}
			catch (Exception e)
			{
				if (_updatedLogConfig)
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
			if (Config.Instance.SaveInAppData)
			{
				if (File.Exists(localPath))
				{
					if (File.Exists(appDataPath))
					{
						//backup in case the file already exists
						File.Move(appDataPath, appDataPath + DateTime.Now.ToFileTime());
					}
					File.Move(localPath, appDataPath);
					Logger.WriteLine("Moved decks to appdata");
				}
			}
			else
			{
				if (File.Exists(appDataPath))
				{
					if (File.Exists(localPath))
					{
						//backup in case the file already exists
						File.Move(localPath, localPath + DateTime.Now.ToFileTime());
					}
					File.Move(appDataPath, localPath);
					Logger.WriteLine("Moved decks to local");
				}
			}

			//load saved decks
			if (!File.Exists(_decksPath))
			{
				//avoid overwriting decks file with new releases.
				using (var sr = new StreamWriter(_decksPath, false))
				{
					sr.WriteLine("<Decks></Decks>");
				}
			}
			else if (!File.Exists(_decksPath + ".old"))
			{
				//the new playerdecks.xml wont work with versions below 0.2.19, make copy
				File.Copy(_decksPath, _decksPath + ".old");
			}
		}

		#endregion

	}
}