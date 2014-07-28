#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using Clipboard = System.Windows.Clipboard;
using Color = System.Windows.Media.Color;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MessageBox = System.Windows.MessageBox;
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

		public readonly Decks _deckList;
		private readonly bool _initialized;

		private readonly string _logConfigPath =
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
			@"\Blizzard\Hearthstone\log.config";

		private readonly string _decksPath = Config.Instance.HomeDir + "PlayerDecks.xml";
		private readonly string _configPath;

		private readonly NotifyIcon _notifyIcon;
		public readonly OpponentWindow _opponentWindow;
		public readonly OverlayWindow _overlay;
		public readonly TimerWindow _timerWindow;
		public bool _editingDeck;
		private bool _newContainsDeck;
		public Deck _newDeck;
		private bool _doUpdate;
		public bool _showingIncorrectDeckMessage;
		public bool _showIncorrectDeckMessage;
		public readonly Version _newVersion;
		public readonly PlayerWindow _playerWindow;
		private readonly bool _updatedLogConfig;
		private readonly bool _foundHsDirectory;
		public ReadOnlyCollection<string> EventKeys = new ReadOnlyCollection<string>(new[] { "None", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12" });

		public bool ShowToolTip
		{
			get { return Config.Instance.TrackerCardToolTips; }
		}

		#endregion

		#region GENERAL GUI

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
			if (!Config.Instance.MinimizeToTray) return;
			if (WindowState == WindowState.Minimized)
			{
				_notifyIcon.Visible = true;
				_notifyIcon.ShowBalloonTip(2000, "Hearthstone Deck Tracker", "Minimized to tray", System.Windows.Forms.ToolTipIcon.Info);
				Hide();
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			try
			{
				_doUpdate = false;
				Config.Instance.SelectedTags = Config.Instance.SelectedTags.Distinct().ToList();
				Config.Instance.ShowAllDecks = DeckPickerList.ShowAll;
				Config.Instance.WindowHeight = (int)Height;
				_overlay.Close();
				HsLogReader.Instance.Stop();
				_timerWindow.Shutdown();
				_playerWindow.Shutdown();
				_opponentWindow.Shutdown();
				WriteConfig();
				WriteDecks();
			}
			catch (Exception)
			{
				//doesnt matter
			}
		}

		private void NotifyIconOnMouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs mouseEventArgs)
		{
			_notifyIcon.Visible = false;
			Show();
			WindowState = WindowState.Normal;
			Activate();
		}

		private void BtnFilterTag_Click(object sender, RoutedEventArgs e)
		{
			FlyoutFilterTags.IsOpen = !FlyoutFilterTags.IsOpen;
		}

		private void TagControlFilterOnSelectedTagsChanged(TagControl sender, List<string> tags)
		{
			DeckPickerList.SetSelectedTags(tags);
			Config.Instance.SelectedTags = tags;
			WriteConfig();
		}

		private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
		{
			PresentationSource presentationsource = PresentationSource.FromVisual(this);
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
			if (_newVersion != null)
			{
				ShowNewUpdateMessage();
			}
			if (_updatedLogConfig)
			{
				ShowMessage("Restart Hearthstone", "This is either your first time starting the tracker or the log.config file has been updated. Please restart Heartstone once, for the tracker to work properly.");
			}

			//preload the manacurve in new deck
			TabControlTracker.SelectedIndex = 1;
			TabControlTracker.UpdateLayout();
			TabControlTracker.SelectedIndex = 0;

			ManaCurveMyDecks.UpdateValues();

		}

		private void MetroWindow_LocationChanged(object sender, EventArgs e)
		{
			if (WindowState == WindowState.Minimized) return;
			Config.Instance.TrackerWindowTop = (int)Top;
			Config.Instance.TrackerWindowLeft = (int)Left;
		}

		private void TabControlTracker_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;
			var tabItem = TabControlTracker.SelectedItem as TabItem;
			if (tabItem == null) return;
			SelectedTabMarker.Width = tabItem.ActualWidth;
			var offset = TabControlTracker.Items.Cast<TabItem>().TakeWhile(t => t != tabItem).Sum(t => t.ActualWidth);
			SelectedTabMarker.Margin = new Thickness(offset, 40, 0, 0);
		}
		#endregion

		#region GENERAL METHODS

		public void ShowIncorrectDeckMessage()
		{
			var decks =
				_deckList.DecksList.Where(
					d => d.Class == Game.PlayingAs && Game.PlayerDrawn.All(c => d.Cards.Contains(c))
				).ToList();
			if (decks.Contains(DeckPickerList.SelectedDeck))
				decks.Remove(DeckPickerList.SelectedDeck);

			Logger.WriteLine(decks.Count + " possible decks found.", "IncorrectDeckMessage");
			if (decks.Count > 0)
			{
				DeckSelectionDialog dsDialog = new DeckSelectionDialog(decks);

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

			_showingIncorrectDeckMessage = false;
			_showIncorrectDeckMessage = false;
		}

		private void LoadConfig()
		{
			if (Config.Instance.TrackerWindowTop >= 0)
				Top = Config.Instance.TrackerWindowTop;
			if (Config.Instance.TrackerWindowLeft >= 0)
				Left = Config.Instance.TrackerWindowLeft;

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
			CheckboxFlashHs.IsChecked = Config.Instance.BringHsToForeground;
			CheckboxHideSecrets.IsChecked = Config.Instance.HideSecrets;
			CheckboxHighlightDiscarded.IsChecked = Config.Instance.HighlightDiscarded;

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


			TagControlFilter.LoadTags(_deckList.AllTags);

			TagControlFilter.SetSelectedTags(Config.Instance.SelectedTags);
			DeckPickerList.SetSelectedTags(Config.Instance.SelectedTags);

			var tags = new List<string>(_deckList.AllTags);
			tags.Remove("All");
			TagControlNewDeck.LoadTags(tags);
			TagControlMyDecks.LoadTags(tags);
			DeckPickerList.SetTagOperation(Config.Instance.TagOperation);
			TagControlFilter.OperationSwitch.IsChecked = Config.Instance.TagOperation == Operation.And;

			ComboboxWindowBackground.SelectedItem = Config.Instance.SelectedWindowBackground;
			TextboxCustomBackground.IsEnabled = Config.Instance.SelectedWindowBackground == "Custom";
			TextboxCustomBackground.Text = string.IsNullOrEmpty(Config.Instance.WindowsBackgroundHex)
											   ? "#696969"
											   : Config.Instance.WindowsBackgroundHex;
			UpdateAdditionalWindowsBackground();

			ComboboxTextLocationPlayer.SelectedIndex = Config.Instance.TextOnTopPlayer ? 0 : 1;
			ComboboxTextLocationOpponent.SelectedIndex = Config.Instance.TextOnTopOpponent ? 0 : 1;
			_overlay.SetOpponentTextLocation(Config.Instance.TextOnTopOpponent);
			_opponentWindow.SetTextLocation(Config.Instance.TextOnTopOpponent);
			_overlay.SetPlayerTextLocation(Config.Instance.TextOnTopPlayer);
			_playerWindow.SetTextLocation(Config.Instance.TextOnTopPlayer);

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

			CheckboxLogGames.IsChecked = Config.Instance.SavePlayedGames;
			TextboxLogGamesPath.IsEnabled = Config.Instance.SavePlayedGames;
			BtnLogGamesSelectDir.IsEnabled = Config.Instance.SavePlayedGames;
			TextboxLogGamesPath.Text = Config.Instance.SavePlayedGamesPath;

			if (Config.Instance.SavePlayedGames && TextboxLogGamesPath.Text.Length == 0)
				TextboxLogGamesPath.BorderBrush = new SolidColorBrush(Colors.Red);

			CheckboxDeckSortingClassFirst.IsChecked = Config.Instance.CardSortingClassFirst;
		}

		private async void UpdateOverlayAsync()
		{
			bool hsForegroundChanged = false;
			while (_doUpdate)
			{
				if (Process.GetProcessesByName("Hearthstone").Length == 1)
				{
					_overlay.UpdatePosition();

					if (!User32.IsForegroundWindow("Hearthstone") && !hsForegroundChanged)
					{
						if (Config.Instance.WindowsTopmostIfHsForeground && Config.Instance.WindowsTopmost)
						{
							_playerWindow.Topmost = false;
							_opponentWindow.Topmost = false;
							_timerWindow.Topmost = false;
						}
						hsForegroundChanged = true;

					}
					else if (hsForegroundChanged && User32.IsForegroundWindow("Hearthstone"))
					{
						_overlay.Update(true);
						if (Config.Instance.WindowsTopmostIfHsForeground && Config.Instance.WindowsTopmost)
						{
							//if player topmost is set to true before opponent:
							//clicking on the playerwindow and back to hs causes the playerwindow to be behind hs.
							//other way around it works for both windows... what?
							_opponentWindow.Topmost = true;
							_playerWindow.Topmost = true;
							_timerWindow.Topmost = true;
						}
						hsForegroundChanged = false;
					}
				}
				else
				{
					_overlay.ShowOverlay(false);
				}
				await Task.Delay(Config.Instance.UpdateDelay);
			}
		}

		private async void ShowNewUpdateMessage()
		{
			var releaseDownloadUrl = @"https://github.com/Epix37/Hearthstone-Deck-Tracker/releases";
			var settings = new MetroDialogSettings();
			settings.AffirmativeButtonText = "Download";
			settings.NegativeButtonText = "Not now";

			var result = await this.ShowMessageAsync("New Update available!", "Download version " + string.Format("{0}.{1}.{2}", _newVersion.Major, _newVersion.Minor,
													 _newVersion.Build) + " at\n" + releaseDownloadUrl, MessageDialogStyle.AffirmativeAndNegative, settings);

			if (result == MessageDialogResult.Affirmative)
			{
				Process.Start(releaseDownloadUrl);
			}
		}

		public async void ShowMessage(string title, string message)
		{
			await this.ShowMessageAsync(title, message);
		}

		private async void ShowHsNotInstalledMessage()
		{
			var settings = new MetroDialogSettings();
			settings.AffirmativeButtonText = "Ok";
			settings.NegativeButtonText = "Select manually";
			var result = await this.ShowMessageAsync("Hearthstone install directory not found", "Hearthstone Deck Tracker will not work properly if Hearthstone is not installed on your machine (obviously).", MessageDialogStyle.AffirmativeAndNegative, settings);
			if (result == MessageDialogResult.Negative)
			{
				var dialog = new OpenFileDialog();
				dialog.Title = "Select Hearthstone.exe";
				dialog.DefaultExt = "Hearthstone.exe";
				dialog.Filter = "Hearthstone.exe|Hearthstone.exe";
				var dialogResult = dialog.ShowDialog();

				if (dialogResult == true)
				{
					Config.Instance.HearthstoneDirectory = Path.GetDirectoryName(dialog.FileName);
					WriteConfig();
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

		private void WriteConfig()
		{
			XmlManager<Config>.Save(_configPath, Config.Instance);
		}

		public void WriteDecks()
		{
			XmlManager<Decks>.Save(_decksPath, _deckList);
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
			_overlay.ListViewPlayer.ItemsSource = Game.PlayerDrawn;
			_playerWindow.ListViewPlayer.ItemsSource = Game.PlayerDrawn;
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
				var settings = new MetroDialogSettings();
				settings.AffirmativeButtonText = "Yes";
				settings.NegativeButtonText = "No";
				var result = await this.ShowMessageAsync("Found unfinished deck", "New Deck Section still contains an unfinished deck. Discard?", MessageDialogStyle.AffirmativeAndNegative, settings);
				if (result == MessageDialogResult.Negative)
				{
					TabControlTracker.SelectedIndex = 1;
					return;
				}
			}

			ClearNewDeckSection();
			_editingDeck = true;
			_newContainsDeck = true;
			_newDeck = (Deck)selectedDeck.Clone();
			ListViewNewDeck.ItemsSource = _newDeck.Cards;

			if (ComboBoxSelectClass.Items.Contains(_newDeck.Class))
				ComboBoxSelectClass.SelectedValue = _newDeck.Class;

			TextBoxDeckName.Text = _newDeck.Name;
			UpdateNewDeckHeader(true);
			UpdateDbListView();


			TagControlNewDeck.SetSelectedTags(_newDeck.Tags);

			TabControlTracker.SelectedIndex = 1;
		}

		private void BtnSetTag_Click(object sender, RoutedEventArgs e)
		{
			FlyoutNewDeckSetTags.IsOpen = !FlyoutNewDeckSetTags.IsOpen;
		}

		public async Task ShowSavedFileMessage(string fileName, string dir)
		{
			var settings = new MetroDialogSettings();
			settings.NegativeButtonText = "Open folder";
			var result =
				await
				this.ShowMessageAsync("", "Saved to\n\"" + fileName + "\"", MessageDialogStyle.AffirmativeAndNegative, settings);
			if (result == MessageDialogResult.Negative)
			{
				Process.Start(Path.GetDirectoryName(Application.ResourceAssembly.Location) + "\\" + dir);
			}
		}

		private void TagControlFilterOnOperationChanged(TagControl sender, Operation operation)
		{
			Config.Instance.TagOperation = operation;
			DeckPickerList.SetTagOperation(operation);
			DeckPickerList.UpdateList();
		}

		private void BtnDeckOptions_Click(object sender, RoutedEventArgs e)
		{
			FlyoutDeckOptions.IsOpen = true;
		}

		#endregion

		#region MY DECKS - METHODS

		public void UseDeck(Deck selected)
		{
			Game.Reset();

			if (selected != null)
				Game.SetPremadeDeck((Deck)selected.Clone());

			HsLogReader.Instance.Reset(true);

			_overlay.SortViews();

		}

		public void UpdateDeckList(Deck selected)
		{
			ListViewDeck.ItemsSource = null;
			if (selected == null)
			{

				Config.Instance.LastDeck = string.Empty;
				WriteConfig();
				return;
			}
			ListViewDeck.ItemsSource = selected.Cards;

			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
			Config.Instance.LastDeck = selected.Name;
			WriteConfig();
		}

		#endregion

		#region NEW DECK GUI

		private void ComboBoxFilterClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;
			_newDeck.Class = ComboBoxSelectClass.SelectedValue.ToString();
			_newContainsDeck = true;
			UpdateDbListView();

			ManaCurveNewDeck.UpdateValues();
		}

		private async void BtnSaveDeck_Click(object sender, RoutedEventArgs e)
		{
			_newDeck.Cards = new ObservableCollection<Card>(_newDeck.Cards.OrderBy(c => c.Cost).ThenByDescending(c => c.Type).ThenBy(c => c.Name).ToList());
			ListViewNewDeck.ItemsSource = _newDeck.Cards;

			if (_editingDeck)
			{
				var settings = new MetroDialogSettings();
				settings.AffirmativeButtonText = "Overwrite";
				settings.NegativeButtonText = "Save as new";
				var result =
					await
					this.ShowMessageAsync("Saving deck", "How do you wish to save the deck?",
										  MessageDialogStyle.AffirmativeAndNegative, settings);
				if (result == MessageDialogResult.Affirmative)
				{
					SaveDeck(true);
				}
				else if (result == MessageDialogResult.Negative)
				{
					SaveDeck(false);
				}
			}
			else
			{
				SaveDeck(false);
			}

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
			if (e.Key == Key.Enter)
			{
				if (ListViewDB.Items.Count == 1)
				{
					var card = (Card)ListViewDB.Items[0];
					AddCardToDeck((Card)card.Clone());
				}
			}
		}

		private void BtnImport_OnClick(object sender, RoutedEventArgs e)
		{
			FlyoutDeckImport.IsOpen = true;

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

		private void Grid_Drop(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

			var file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
			var info = new FileInfo(file);

			if (info.Extension != ".txt") return;

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
				if (_newDeck != null)
					ManaCurveNewDeck.SetDeck(_newDeck);

				Helper.SortCardCollection(ListViewDB.Items, Config.Instance.CardSortingClassFirst);
			}
		}

		private async void SaveDeck(bool overwrite)
		{
			var deckName = TextBoxDeckName.Text;

			if (string.IsNullOrEmpty(deckName))
			{
				var settings = new MetroDialogSettings();
				settings.AffirmativeButtonText = "Set";
				settings.DefaultText = deckName;

				var name = await this.ShowInputAsync("No name set", "Please set a name for the deck", settings);

				if (String.IsNullOrEmpty(name))
					return;

				deckName = name;
				TextBoxDeckName.Text = name;

			}

			while (_deckList.DecksList.Any(d => d.Name == deckName) && (!_editingDeck || !overwrite))
			{
				var settings = new MetroDialogSettings();
				settings.AffirmativeButtonText = "Set";
				settings.DefaultText = deckName;
				string name = await this.ShowInputAsync("Name already exists", "You already have a deck with that name, please select a different one.", settings);

				if (String.IsNullOrEmpty(name))
					return;

				deckName = name;
				TextBoxDeckName.Text = name;
			}

			if (_newDeck.Cards.Sum(c => c.Count) != 30)
			{
				var settings = new MetroDialogSettings();
				settings.AffirmativeButtonText = "Yes";
				settings.NegativeButtonText = "No";

				var result =
					await this.ShowMessageAsync("Not 30 cards", string.Format("Deck contains {0} cards. Is this what you want to save anyway?",
										  _newDeck.Cards.Sum(c => c.Count)), MessageDialogStyle.AffirmativeAndNegative,
												settings);
				if (result != MessageDialogResult.Affirmative)
				{
					return;
				}
			}

			if (_editingDeck && overwrite)
			{
				_deckList.DecksList.Remove(_newDeck);
				DeckPickerList.RemoveDeck(_newDeck);
			}
			_newDeck.Name = deckName;
			_newDeck.Class = ComboBoxSelectClass.SelectedValue.ToString();
			_newDeck.Tags = TagControlNewDeck.GetTags();

			var newDeckClone = (Deck)_newDeck.Clone();
			_deckList.DecksList.Add(newDeckClone);
			DeckPickerList.AddAndSelectDeck(newDeckClone);

			WriteDecks();
			BtnSaveDeck.Content = "Save";

			if (_editingDeck)
			{
				TagControlNewDeck.SetSelectedTags(new List<string>());
			}

			TabControlTracker.SelectedIndex = 0;
			_editingDeck = false;

			foreach (var tag in _newDeck.Tags)
			{
				TagControlFilter.AddSelectedTag(tag);
			}

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
			_newDeck = new Deck();
			ListViewNewDeck.ItemsSource = _newDeck.Cards;
			_newContainsDeck = false;
			_editingDeck = false;
			ManaCurveNewDeck.ClearDeck();

		}

		private void RemoveCardFromDeck(Card card)
		{
			if (card == null)
				return;
			if (card.Count > 1)
			{
				_newDeck.Cards.Remove(card);
				card.Count--;
				_newDeck.Cards.Add(card);
			}
			else
				_newDeck.Cards.Remove(card);

			Helper.SortCardCollection(ListViewNewDeck.Items, Config.Instance.CardSortingClassFirst);
			BtnSaveDeck.Content = "Save*";
			UpdateNewDeckHeader(true);
		}

		private void UpdateNewDeckHeader(bool show)
		{
			var headerText = "New Deck";
			var cardCount = _newDeck.Cards.Sum(c => c.Count);
			TabItemNewDeck.Header = show ? string.Format("{0} ({1})", headerText, cardCount) : headerText;
		}

		private void AddCardToDeck(Card card)
		{
			if (card == null)
				return;
			if (_newDeck.Cards.Contains(card))
			{
				var cardInDeck = _newDeck.Cards.First(c => c.Name == card.Name);
				cardInDeck.Count++;
			}
			else
			{
				_newDeck.Cards.Add(card);
			}

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
				_editingDeck = editing;

				_newDeck = (Deck)deck.Clone();
				ListViewNewDeck.ItemsSource = _newDeck.Cards;

				if (ComboBoxSelectClass.Items.Contains(_newDeck.Class))
					ComboBoxSelectClass.SelectedValue = _newDeck.Class;

				TextBoxDeckName.Text = _newDeck.Name;
				UpdateNewDeckHeader(true);
				UpdateDbListView();
			}
		}

		private async void ShowClearNewDeckMessage()
		{
			var settings = new MetroDialogSettings();
			settings.AffirmativeButtonText = "Yes";
			settings.NegativeButtonText = "No";
			var result = await this.ShowMessageAsync("Clear deck?", "", MessageDialogStyle.AffirmativeAndNegative, settings);
			if (result == MessageDialogResult.Affirmative)
			{
				ClearNewDeckSection();
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
			_playerWindow.LblDrawChance1.Visibility = Visibility.Collapsed;
			_playerWindow.LblDrawChance2.Visibility = Visibility.Collapsed;

		}

		private void CheckboxHideDrawChances_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideDrawChances = false;
			SaveConfig(true);
			_playerWindow.LblDrawChance1.Visibility = Visibility.Visible;
			_playerWindow.LblDrawChance2.Visibility = Visibility.Visible;
		}

		private void CheckboxHideOpponentDrawChances_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentDrawChances = true;
			SaveConfig(true);
			_opponentWindow.LblOpponentDrawChance2.Visibility = Visibility.Collapsed;
			_opponentWindow.LblOpponentDrawChance1.Visibility = Visibility.Collapsed;
		}

		private void CheckboxHideOpponentDrawChances_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentDrawChances = false;
			SaveConfig(true);
			_opponentWindow.LblOpponentDrawChance2.Visibility = Visibility.Visible;
			_opponentWindow.LblOpponentDrawChance1.Visibility = Visibility.Visible;

		}

		private void CheckboxHidePlayerCardCounter_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HidePlayerCardCount = true;
			SaveConfig(true);
			_playerWindow.LblCardCount.Visibility = Visibility.Collapsed;
			_playerWindow.LblDeckCount.Visibility = Visibility.Collapsed;
		}

		private void CheckboxHidePlayerCardCounter_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HidePlayerCardCount = false;
			SaveConfig(true);
			_playerWindow.LblCardCount.Visibility = Visibility.Visible;
			_playerWindow.LblDeckCount.Visibility = Visibility.Visible;
		}

		private void CheckboxHidePlayerCards_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HidePlayerCards = true;
			SaveConfig(true);
			_playerWindow.ListViewPlayer.Visibility = Visibility.Collapsed;
		}

		private void CheckboxHidePlayerCards_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HidePlayerCards = false;
			SaveConfig(true);
			_playerWindow.ListViewPlayer.Visibility = Visibility.Visible;
		}


		private void CheckboxHideOpponentCardCounter_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCardCount = true;
			SaveConfig(true);
			_opponentWindow.LblOpponentCardCount.Visibility = Visibility.Collapsed;
			_opponentWindow.LblOpponentDeckCount.Visibility = Visibility.Collapsed;
		}

		private void CheckboxHideOpponentCardCounter_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCardCount = false;
			SaveConfig(true);
			_opponentWindow.LblOpponentCardCount.Visibility = Visibility.Visible;
			_opponentWindow.LblOpponentDeckCount.Visibility = Visibility.Visible;
		}

		private void CheckboxHideOpponentCards_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCards = true;
			SaveConfig(true);
			_opponentWindow.ListViewOpponent.Visibility = Visibility.Collapsed;
		}

		private void CheckboxHideOpponentCards_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideOpponentCards = false;
			SaveConfig(true);
			_opponentWindow.ListViewOpponent.Visibility = Visibility.Visible;
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
			_playerWindow.Topmost = true;
			_opponentWindow.Topmost = true;
			CheckboxWinTopmostHsForeground.IsEnabled = true;
			SaveConfig(true);
		}

		private void CheckboxWindowsTopmost_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.WindowsTopmost = false;
			_playerWindow.Topmost = false;
			_opponentWindow.Topmost = false;
			CheckboxWinTopmostHsForeground.IsEnabled = false;
			CheckboxWinTopmostHsForeground.IsChecked = false;
			SaveConfig(true);
		}

		private void CheckboxWindowsOpenAutomatically_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			_playerWindow.Show();
			_playerWindow.Activate();
			_opponentWindow.Show();
			_opponentWindow.Activate();

			_playerWindow.SetCardCount(Game.PlayerHandCount, 30 - Game.PlayerDrawn.Sum(card => card.Count));

			_opponentWindow.SetOpponentCardCount(Game.OpponentHandCount, Game.OpponentDeckCount, Game.OpponentHasCoin);

			Config.Instance.WindowsOnStartup = true;
			SaveConfig(true);
		}

		private void CheckboxWindowsOpenAutomatically_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			_playerWindow.Hide();
			_opponentWindow.Hide();
			Config.Instance.WindowsOnStartup = false;
			SaveConfig(true);
		}

		private void CheckboxWinTopmostHsForeground_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.WindowsTopmostIfHsForeground = true;
			_playerWindow.Topmost = false;
			_opponentWindow.Topmost = false;
			SaveConfig(false);
		}

		private void CheckboxWinTopmostHsForeground_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.WindowsTopmostIfHsForeground = false;
			if (Config.Instance.WindowsTopmost)
			{
				_playerWindow.Topmost = true;
				_opponentWindow.Topmost = true;
			}
			SaveConfig(false);
		}

		private void CheckboxTimerTopmost_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.TimerWindowTopmost = true;
			_timerWindow.Topmost = true;
			CheckboxTimerTopmostHsForeground.IsEnabled = true;
			SaveConfig(true);
		}

		private void CheckboxTimerTopmost_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.TimerWindowTopmost = false;
			_timerWindow.Topmost = false;
			CheckboxTimerTopmostHsForeground.IsEnabled = false;
			CheckboxTimerTopmostHsForeground.IsChecked = false;
			SaveConfig(true);
		}

		private void CheckboxTimerWindow_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			_timerWindow.Show();
			_timerWindow.Activate();
			Config.Instance.TimerWindowOnStartup = true;
			SaveConfig(true);
		}

		private void CheckboxTimerWindow_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			_timerWindow.Hide();
			Config.Instance.TimerWindowOnStartup = false;
			SaveConfig(true);
		}

		private void CheckboxTimerTopmostHsForeground_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.TimerWindowTopmostIfHsForeground = true;
			_timerWindow.Topmost = false;
			SaveConfig(false);
		}

		private void CheckboxTimerTopmostHsForeground_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.TimerWindowTopmostIfHsForeground = false;
			if (Config.Instance.TimerWindowTopmost)
			{
				_timerWindow.Topmost = true;
			}
			SaveConfig(false);
		}

		private void SaveConfig(bool updateOverlay)
		{
			WriteConfig();
			if (updateOverlay)
				_overlay.Update(true);
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
			_overlay.UpdateScaling();

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
			_overlay.UpdateScaling();

			if (Config.Instance.UseSameScaling && SliderOverlayPlayerScaling.Value != scaling)
			{
				SliderOverlayPlayerScaling.Value = scaling;
			}
		}

		private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
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
			Brush background = brush;

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
					_playerWindow.Background = hexBackground;
					_opponentWindow.Background = hexBackground;
					_timerWindow.Background = hexBackground;
				}
			}
			else
			{
				_playerWindow.Background = background;
				_opponentWindow.Background = background;
				_timerWindow.Background = background;
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
			_overlay.SetOpponentTextLocation(Config.Instance.TextOnTopOpponent);
			_opponentWindow.SetTextLocation(Config.Instance.TextOnTopOpponent);

		}

		private void ComboboxTextLocationPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_initialized) return;

			Config.Instance.TextOnTopPlayer = ComboboxTextLocationPlayer.SelectedItem.ToString() == "Top";
			SaveConfig(false);

			_overlay.SetPlayerTextLocation(Config.Instance.TextOnTopPlayer);
			_playerWindow.SetTextLocation(Config.Instance.TextOnTopPlayer);
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
			Config.Instance.SaveInAppData = true;
			SaveConfig(false);
			await Restart();
		}

		private async void CheckboxAppData_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.SaveInAppData = false;
			SaveConfig(false);
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
			SaveConfig(true);
		}

		private void CheckboxOverlayCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.OverlayCardToolTips = false;
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
			Config.Instance.FlashHs = true;
			SaveConfig(false);
		}

		private void CheckboxFlashHs_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.FlashHs = false;
			SaveConfig(false);
		}
		
		private void CheckboxHideSecrets_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideSecrets = true;
			SaveConfig(false);
			_overlay.HideSecrets();
		}

		private void CheckboxHideSecrets_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized) return;
			Config.Instance.HideSecrets = false;
			SaveConfig(false);
			if (!Game.IsInMenu)
				_overlay.ShowSecrets(Game.PlayingAgainst);
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
			BtnUnlockOverlay.Content = await _overlay.UnlockUI() ? "Lock" : "Unlock";
		}

		private async void BtnResetOverlay_Click(object sender, RoutedEventArgs e)
		{
			var result = await this.ShowMessageAsync("Resetting overlay to default", "Positions of: Player Deck, Opponent deck, Timers and Secrets will be reset to default. Are you sure?", MessageDialogStyle.AffirmativeAndNegative);
			if (result != MessageDialogResult.Affirmative)
				return;

			if (BtnUnlockOverlay.Content == "Lock")
			{
				await _overlay.UnlockUI();
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

		#endregion

		#region Constructor

		public MainWindow()
		{
			InitializeComponent();

			Helper.MainWindow = this;
			_configPath = Config.Load();
			HsLogReader.Create();

			var version = Helper.CheckForUpdates(out _newVersion);
			if (version != null)
				TxtblockVersion.Text = string.Format("Version: {0}.{1}.{2}", version.Major, version.Minor, version.Build);

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

			if(_foundHsDirectory)
				_updatedLogConfig = UpdateLogConfigFile();

			//hearthstone, loads db etc - needs to be loaded before playerdecks, since cards are only saved as ids now
			//Game.Create();
			Game.Reset();

			Setup_Deck_List_File();
			try
			{
				_deckList = XmlManager<Decks>.Load(_decksPath);
			}
			catch (Exception e)
			{
				MessageBox.Show(
					e.Message + "\n\n" + e.InnerException +
					"\n\n If you don't know how to fix this, please delete " + _decksPath + " (this will cause you to lose your decks).",
					"Error loading PlayerDecks.xml");
				Application.Current.Shutdown();
			}

			foreach (var deck in _deckList.DecksList)
			{
				DeckPickerList.AddDeck(deck);
			}

			_notifyIcon = new System.Windows.Forms.NotifyIcon();
			_notifyIcon.Icon = new Icon(@"Images/HearthstoneDeckTracker.ico");
			_notifyIcon.MouseDoubleClick += NotifyIconOnMouseDoubleClick;
			_notifyIcon.Visible = false;

			_newDeck = new Deck();
			ListViewNewDeck.ItemsSource = _newDeck.Cards;

			//create overlay
			_overlay = new OverlayWindow() { Topmost = true };
			if (_foundHsDirectory)
				_overlay.Show();

			_playerWindow = new PlayerWindow(Config.Instance, Game.IsUsingPremade ? Game.PlayerDeck : Game.PlayerDrawn);
			_opponentWindow = new OpponentWindow(Config.Instance, Game.OpponentCards);
			_timerWindow = new TimerWindow(Config.Instance);

			if (Config.Instance.WindowsOnStartup)
			{
				_playerWindow.Show();
				_opponentWindow.Show();
			}
			if (Config.Instance.TimerWindowOnStartup)
			{
				_timerWindow.Show();
			}
			if (!_deckList.AllTags.Contains("All"))
			{
				_deckList.AllTags.Add("All");
				WriteDecks();
			}
			if (!_deckList.AllTags.Contains("Arena"))
			{
				_deckList.AllTags.Add("Arena");
				WriteDecks();
			}
			if (!_deckList.AllTags.Contains("Constructed"))
			{
				_deckList.AllTags.Add("Constructed");
				WriteDecks();
			}

			ComboboxAccent.ItemsSource = ThemeManager.Accents;
			ComboboxTheme.ItemsSource = ThemeManager.AppThemes;
			ComboboxLanguages.ItemsSource = Helper.LanguageDict.Keys;

			ComboboxKeyPressGameStart.ItemsSource = EventKeys;
			ComboboxKeyPressGameEnd.ItemsSource = EventKeys;

			LoadConfig();

			//this has to happen before reader starts
			var lastDeck = _deckList.DecksList.FirstOrDefault(d => d.Name == Config.Instance.LastDeck);
			DeckPickerList.SelectDeck(lastDeck);

			DeckOptionsFlyout.DeckOptionsButtonClicked += (DeckOptions sender) => { FlyoutDeckOptions.IsOpen = false; };

			DeckImportFlyout.DeckOptionsButtonClicked += (DeckImport sender) => { FlyoutDeckImport.IsOpen = false; };

			TurnTimer.Create(90);

			TagControlFilter.HideStuffToCreateNewTag();
			TagControlNewDeck.OperationSwitch.Visibility = Visibility.Collapsed;
			TagControlMyDecks.OperationSwitch.Visibility = Visibility.Collapsed;

			TagControlFilter.SelectedTagsChanged += TagControlFilterOnSelectedTagsChanged;
			TagControlFilter.OperationChanged += TagControlFilterOnOperationChanged;

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

		}

		private bool FindHearthstoneDir()
		{
			var found = false;
			if (string.IsNullOrEmpty(Config.Instance.HearthstoneDirectory) || !File.Exists(Config.Instance.HearthstoneDirectory + @"\Hearthstone.exe"))
			{
				using (var hsDirKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Hearthstone"))
				{
					if (hsDirKey != null)
					{
						var hsDir = (string)hsDirKey.GetValue("InstallLocation");

						//verify the installlocation actually is correct (possibly moved?)
						if (File.Exists(hsDir + @"\Hearthstone.exe"))
						{
							Config.Instance.HearthstoneDirectory = hsDir;
							WriteConfig();
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

		private void Setup_Deck_List_File()
		{
			if (Config.Instance.SaveInAppData)
			{
				if (File.Exists("PlayerDecks.xml"))
				{
					if (File.Exists(_decksPath))
					{
						//backup in case the file already exists
						File.Move(_decksPath, _decksPath + DateTime.Now.ToFileTime());
					}
					File.Move("PlayerDecks.xml", _decksPath);
					Logger.WriteLine("Moved decks to appdata");
				}
			}
			else
			{
				var appDataPath = Config.Instance.AppDataPath + @"\PlayerDecks.xml";
				if (File.Exists(appDataPath))
				{
					if (File.Exists(_decksPath))
					{
						//backup in case the file already exists
						File.Move(_decksPath, _decksPath + DateTime.Now.ToFileTime());
					}
					File.Move(appDataPath, _decksPath);
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