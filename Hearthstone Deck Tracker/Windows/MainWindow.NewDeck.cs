#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls.Dialogs;
using static System.Windows.Visibility;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListViewItem = System.Windows.Controls.ListViewItem;
using RadioButton = System.Windows.Controls.RadioButton;
using TextBox = System.Windows.Controls.TextBox;
using HearthDb.Enums;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class MainWindow
	{
		internal double? MovedLeft;
		private string _editedDeckName;

		private void UpdateDbListView()
		{
			if(_newDeck == null)
				return;
			var selectedClass = _newDeck.Class;
			string selectedNeutral;
			string selectedManaCost;
			string selectedSet;
			try
			{
				selectedNeutral = MenuFilterType.Items.Cast<RadioButton>().First(x => x.IsChecked.HasValue && x.IsChecked.Value).Content.ToString();
			}
			catch(Exception)
			{
				selectedNeutral = "ALL";
			}
			try
			{
				selectedManaCost =
					MenuFilterMana.Items.Cast<RadioButton>().First(x => x.IsChecked.HasValue && x.IsChecked.Value).Content.ToString();
			}
			catch(Exception)
			{
				selectedManaCost = "ALL";
			}
			try
			{
				selectedSet = MenuFilterSet.Items.Cast<RadioButton>().First(x => x.IsChecked.HasValue && x.IsChecked.Value).Content.ToString();
			}
			catch(Exception)
			{
				selectedSet = "ALL";
			}
			if(selectedClass == "Select a Class")
				ListViewDB.Items.Clear();
			else
			{
				ListViewDB.Items.Clear();

				var formattedInput = Helper.RemoveDiacritics(TextBoxDBFilter.Text.ToLowerInvariant(), true);
				var words = formattedInput.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

				foreach(var card in Database.GetActualCards())
				{
					var cardName = Helper.RemoveDiacritics(card.LocalizedName.ToLowerInvariant(), true);
					if(!Config.Instance.UseFullTextSearch && !cardName.Contains(formattedInput)
					   && card.AlternativeNames.All(x => !Helper.RemoveDiacritics(x.ToLowerInvariant(), true).Contains(formattedInput))
					   && (!string.IsNullOrEmpty(card.RaceOrType) && formattedInput != card.RaceOrType.ToLowerInvariant()))
						continue;
					if(Config.Instance.UseFullTextSearch
					   && words.Any(
					                w =>
					                !cardName.Contains(w) && !(!string.IsNullOrEmpty(card.Text) && card.Text.ToLowerInvariant().Contains(w))
					                && card.AlternativeNames.All(x => !Helper.RemoveDiacritics(x.ToLowerInvariant(), true).Contains(formattedInput))
					                && card.AlternativeTexts.All(x => x == null || !x.ToLowerInvariant().Contains(formattedInput))
					                && (!string.IsNullOrEmpty(card.RaceOrType) && w != card.RaceOrType.ToLowerInvariant())
					                && (w != card.Rarity.ToString().ToLowerInvariant())))
						continue;

					// mana filter
					if(selectedManaCost != "ALL" && ((selectedManaCost != "9+" || card.Cost < 9) && (selectedManaCost != card.Cost.ToString())))
						continue;
					if(selectedSet != "ALL" && !string.Equals(selectedSet, card.Set, StringComparison.InvariantCultureIgnoreCase))
						continue;
					if(!_newDeck.IsArenaDeck && !_newDeck.IsBrawlDeck && !(CheckBoxIncludeWild.IsChecked ?? true) && Helper.WildOnlySets.Contains(card.Set))
						continue;
					switch(selectedNeutral)
					{
						case "ALL":
							if(card.GetPlayerClass == selectedClass || card.GetPlayerClass == "Neutral")
								ListViewDB.Items.Add(card);
							break;
						case "CLASS ONLY":
							if(card.GetPlayerClass == selectedClass)
								ListViewDB.Items.Add(card);
							break;
						case "NEUTRAL ONLY":
							if(card.GetPlayerClass == "Neutral")
								ListViewDB.Items.Add(card);
							break;
					}
				}

				Helper.SortCardCollection(ListViewDB.Items, Config.Instance.CardSortingClassFirst);
			}
		}

		public async void SaveDeck(bool overwrite, SerializableVersion newVersion, bool workInProgressDeck = false)
		{
			var deckName = TextBoxDeckName.Text;

			if(string.IsNullOrEmpty(deckName))
			{
				var settings = new MessageDialogs.Settings {AffirmativeButtonText = "Set", DefaultText = deckName};

				var name = await this.ShowInputAsync("No name set", "Please set a name for the deck", settings);

				if(string.IsNullOrEmpty(name))
					return;

				deckName = name;
				TextBoxDeckName.Text = name;
			}

			if(_newDeck.Cards.Sum(c => c.Count) != 30 && workInProgressDeck == false)
			{
				var settings = new MessageDialogs.Settings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"};

				var result =
					await
					this.ShowMessageAsync("Not 30 cards",
										  $"Deck contains {_newDeck.Cards.Sum(c => c.Count)} cards. Is this what you want to save anyway?", MessageDialogStyle.AffirmativeAndNegative, settings);
				if(result != MessageDialogResult.Affirmative)
					return;
			}

			var previousVersion = _newDeck.Version;
			if(overwrite && (_newDeck.Version != newVersion))
			{
				AddDeckHistory();
				_newDeck.Version = newVersion;
				_newDeck.SelectedVersion = newVersion;
				_newDeck.HearthStatsDeckVersionId = "";
			}

			if(EditingDeck && overwrite)
				DeckList.Instance.Decks.Remove(_newDeck);

			var oldDeckName = _newDeck.Name;

			_newDeck.Name = deckName;

			var newDeckClone = (Deck)_newDeck.Clone();
			newDeckClone.Archived = false;

			DeckList.Instance.Decks.Add(newDeckClone);

			newDeckClone.LastEdited = DateTime.Now;

			DeckList.Save();

			Log.Info("Saved Decks");

			if(EditingDeck)
			{
				TagControlEdit.SetSelectedTags(new List<string>());
				if(deckName != oldDeckName)
				{
					DeckStats statsEntry;
					if(DeckStatsList.Instance.DeckStats.TryGetValue(_newDeck.DeckId, out statsEntry))
					{
						if(overwrite)
						{
							statsEntry.Name = deckName;
							Log.Info("Deck has new name, updated deckstats");
							foreach(var game in statsEntry.Games)
								game.DeckName = deckName;
						}
						else
						{
							DeckStats newStatsEntry;
							if(DeckStatsList.Instance.DeckStats.TryGetValue(_newDeck.DeckId, out newStatsEntry))
							{
								newStatsEntry = new DeckStats(_newDeck);
								DeckStatsList.Instance.DeckStats.TryAdd(_newDeck.DeckId, newStatsEntry);
							}
							foreach(var game in statsEntry.Games)
								newStatsEntry.AddGameResult(game.CloneWithNewId());
							Log.Info("cloned gamestats for \"Set as new\"");
						}
						DeckStatsList.Save();
					}
				}
			}


			if(Config.Instance.HearthStatsAutoUploadNewDecks && HearthStatsAPI.IsLoggedIn)
			{
				Log.Info("auto uploading new/edited deck");
				if(EditingDeck)
				{
					if(previousVersion != newVersion)
						HearthStatsManager.UploadVersionAsync(newDeckClone, _originalDeck.HearthStatsIdForUploading, background: true).Forget();
					else
						HearthStatsManager.UpdateDeckAsync(newDeckClone, background: true).Forget();
				}
				else
					HearthStatsManager.UploadDeckAsync(newDeckClone, background: true).Forget();
			}

			if(EditingDeck)
				DeckManagerEvents.OnDeckUpdated.Execute(newDeckClone);
			else
				DeckManagerEvents.OnDeckCreated.Execute(newDeckClone);


			EditingDeck = false;

			foreach(var tag in _newDeck.Tags)
				SortFilterDecksFlyout.AddSelectedTag(tag);

			DeckPickerList.SelectDeckAndAppropriateView(newDeckClone);
			DeckPickerList.UpdateDecks(forceUpdate: new[] {newDeckClone});
			SelectDeck(newDeckClone, true);
			CloseNewDeck();
			ClearNewDeckSection();
		}

		private void ClearNewDeckSection()
		{
			TextBoxDeckName.Text = string.Empty;
			TextBoxDBFilter.Text = string.Empty;
			MenuFilterMana.Items.Cast<RadioButton>().First().IsChecked = true;
			MenuFilterType.Items.Cast<RadioButton>().First().IsChecked = true;
			_newDeck = null;
			EditingDeck = false;
			_newDeckUnsavedChanges = false;
			UpdateCardCount();
			UpdateExpansionIcons();
		}

		private void RemoveCardFromDeck(Card card)
		{
			if(card == null)
				return;
			if(card.Count > 1)
				card.Count--;
			else
				_newDeck.Cards.Remove(card);

			UpdateCardCount();
			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
			ManaCurveMyDecks.UpdateValues();
			try
			{
				TextBoxDBFilter.Focus();
				TextBoxDBFilter.Select(0, TextBoxDBFilter.Text.Length);
			}
			catch
			{
			}
			UpdateExpansionIcons();
		}

		private void AddCardToDeck(Card card)
		{
			if(card == null)
				return;
			var cardInDeck = _newDeck.Cards.FirstOrDefault(c => c.Name == card.Name);
			if(cardInDeck != null)
			{
				if(!_newDeck.IsArenaDeck && CheckBoxConstructedCardLimits.IsChecked == true 
					&&(cardInDeck.Count >= 2 || cardInDeck.Rarity == Rarity.LEGENDARY && cardInDeck.Count >= 1))
					return;
				cardInDeck.Count++;
			}
			else
				_newDeck.Cards.Add(card);

			UpdateCardCount();
			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
			ManaCurveMyDecks.UpdateValues();
			try
			{
				TextBoxDBFilter.Focus();
				TextBoxDBFilter.Select(0, TextBoxDBFilter.Text.Length);
			}
			catch
			{
			}
			UpdateExpansionIcons();
		}

		private void UpdateExpansionIcons() => SetIcons.Update(_newDeck);

		private void UpdateCardCount()
		{
			var count = _newDeck?.Cards.Sum(c => c.Count) ?? 0;
			TextBlockCardCount.Text = count + " / 30";
			CardCountWarning.Visibility = count > 30 ? Visible : Collapsed;
		}

		public void SetNewDeck(Deck deck, bool editing = false)
		{
			if(deck == null)
				return;
			ClearNewDeckSection();
			SelectDeck(null, false);
			EditingDeck = editing;
			if(editing)
			{
				_editedDeckName = deck.Name;
				_originalDeck = deck;
			}
			_newDeck = (Deck)deck.Clone();

			_newDeck.Cards.Clear();
			foreach(var card in deck.GetSelectedDeckVersion().Cards)
				_newDeck.Cards.Add(card.Clone() as Card);
			_newDeck.SelectedVersion = _newDeck.Version;
			UpdateExpansionIcons();

			ListViewDeck.ItemsSource = _newDeck.Cards;
			Helper.SortCardCollection(ListViewDeck.ItemsSource, false);
			TextBoxDeckName.Text = _newDeck.Name;
			BorderConstructedCardLimits.Visibility = _newDeck.IsArenaDeck ? Collapsed : Visible;
			CheckBoxIncludeWild.Visibility = _newDeck.IsBrawlDeck ? Collapsed : Visible;
			CheckBoxConstructedCardLimits.IsChecked = true;
			UpdateDeckHistoryPanel(deck, !editing);
			UpdateDbListView();
			ExpandNewDeck();
			UpdateCardCount();
			ManaCurveMyDecks.SetDeck(_newDeck);
		}

		private void ExpandNewDeck()
		{
			const int widthWithoutHistoryPanel = 240;
			if(GridNewDeck.Visibility != Visible)
			{
				GridNewDeck.Visibility = Visible;
				MenuNewDeck.Visibility = Visible;
				ButtonVersionHistory.Visibility = _newDeck?.HasVersions ?? false ? Visible : Collapsed;
				GridNewDeck.Width = widthWithoutHistoryPanel;
				GridNewDeck.UpdateLayout();
				Width += GridNewDeck.ActualWidth;
				MinWidth += GridNewDeck.ActualWidth;
			}
			DeckPickerListCover.Visibility = Visible;
			PanelVersionComboBox.Visibility = Collapsed;
			BtnStartHearthstone.Visibility = Collapsed;
			PanelCardCount.Visibility = Visible;

			//move window left if opening the edit panel causes it to be outside of a screen
			foreach(var screen in Screen.AllScreens)
			{
				var windowLeft = (int)Left;
				var windowRight = (int)(Left + Width);
				var screenLeft = screen.WorkingArea.X;
				var screenRight = screen.WorkingArea.Right;

				//if the window is completely outside of this screen, just skip this screen
				if(windowRight < screenLeft || windowLeft > screenRight)
					continue;

				//if the original window was partially on this screen but mostly on the screen to the right, just skip this screen
				if(windowLeft + (Width - GridNewDeck.ActualWidth) / 2 > screenRight && windowLeft > screenLeft)
					continue;

				//if the new window is partially on this screen and partially on the screen to the right
				if(windowRight > screenRight && windowLeft < screenRight)
				{
					//move window left by the change in width
					Left -= (int)GridNewDeck.ActualWidth;

					//if we would leave a distance to the edge of the screen greater than 50px
					//just fully align the window to the right of the screen instead
					if(screenRight - (Left + Width) > 50)
						Left = screenRight - Width;

					MovedLeft = windowLeft - Left;
					break;
				}
			}
			UpdateExpansionIcons();
		}

		private void CloseNewDeck()
		{
			if(DeckPickerList.SelectedDecks.Any())
				EnableMenuItems(true);
			if(GridNewDeck.Visibility != Collapsed)
			{
				var width = GridNewDeck.ActualWidth;
				GridNewDeck.Visibility = Collapsed;
				MenuNewDeck.Visibility = Collapsed;
				PanelDeckHistory.Visibility = Collapsed;
				MinWidth -= width;
				Width -= width;
			}
			ClearNewDeckSection();
			DeckPickerListCover.Visibility = Hidden;
			var selectedDeck = DeckPickerList.SelectedDecks.FirstOrDefault();
			PanelVersionComboBox.Visibility = selectedDeck != null && selectedDeck.HasVersions ? Visible : Collapsed;
			PanelCardCount.Visibility = Collapsed;
			BtnStartHearthstone.Visibility = Core.Game.IsRunning ? Collapsed : Visible;
			TextBlockButtonVersionHistory.Text = "SHOW VERSION HISTORY";

			if(MovedLeft.HasValue)
			{
				Left += MovedLeft.Value;
				MovedLeft = null;
			}
			UpdateIntroLabelVisibility();
		}

		private void EnableMenuItems(bool enable)
		{
			//MenuItemSelectedDeckStats.IsEnabled = enable;
			MenuItemEdit.IsEnabled = enable;
			MenuItemExportIds.IsEnabled = enable;
			MenuItemExportScreenshot.IsEnabled = enable;
			MenuItemExportScreenshotWithInfo.IsEnabled = enable;
			MenuItemExportToHs.IsEnabled = enable;
			MenuItemExportXml.IsEnabled = enable;
		}

		private void MenuItem_OnSubmenuOpened(object sender, RoutedEventArgs e)
		{
			if(_newDeck == null)
				return;
			//a menuitems clickevent does not fire if it has subitems
			//bit of a hacky workaround, but this does the trick (subitems are disabled when a new deck is created, enabled when one is edited)
			if(_newDeck.IsArenaDeck
			   || !MenuItemSaveVersionCurrent.IsEnabled && !MenuItemSaveVersionMinor.IsEnabled && !MenuItemSaveVersionMajor.IsEnabled)
			{
				try
				{
					MenuItemSave.IsSubmenuOpen = false;
					SaveDeckWithOverwriteCheck();
				}
				catch(Exception ex)
				{
					Log.Error("Error closing submenu:\r\n" + ex);
				}
			}
		}

		private void MenuItemDashboard_OnClick(object sender, RoutedEventArgs e) => Helper.TryOpenUrl(@"http://hearthstats.net/dashboards");

		#region UI

		private void BtnNewDeckDruid_Click(object sender, RoutedEventArgs e) => CreateNewDeck("Druid");
		private void BtnNewDeckHunter_Click(object sender, RoutedEventArgs e) => CreateNewDeck("Hunter");
		private void BtnNewDeckMage_Click(object sender, RoutedEventArgs e) => CreateNewDeck("Mage");
		private void BtnNewDeckPaladin_Click(object sender, RoutedEventArgs e) => CreateNewDeck("Paladin");
		private void BtnNewDeckPriest_Click(object sender, RoutedEventArgs e) => CreateNewDeck("Priest");
		private void BtnNewDeckRogue_Click(object sender, RoutedEventArgs e) => CreateNewDeck("Rogue");
		private void BtnNewDeckShaman_Click(object sender, RoutedEventArgs e) => CreateNewDeck("Shaman");
		private void BtnNewDeckWarrior_Click(object sender, RoutedEventArgs e) => CreateNewDeck("Warrior");
		private void BtnNewDeckWarlock_Click(object sender, RoutedEventArgs e) => CreateNewDeck("Warlock");

		private async void CreateNewDeck(string hero)
		{
			_newDeck = new Deck {Class = hero};
			var type = await this.ShowDeckTypeDialog();
			if(type == null)
				return;
			if(type == DeckType.Arena)
				_newDeck.IsArenaDeck = true;
			else if(type == DeckType.Brawl)
			{
				if(!DeckList.Instance.AllTags.Contains("Brawl"))
				{
					DeckList.Instance.AllTags.Add("Brawl");
					DeckList.Save();
					Core.MainWindow?.ReloadTags();
				}
				_newDeck.Tags.Add("Brawl");
			}

			BorderConstructedCardLimits.Visibility = _newDeck.IsArenaDeck ? Collapsed : Visible;
			CheckBoxIncludeWild.Visibility = _newDeck.IsBrawlDeck ? Collapsed : Visible;
			CheckBoxConstructedCardLimits.IsChecked = true;
			SelectDeck(null, false);
			ExpandNewDeck();
			ListViewDeck.ItemsSource = _newDeck.Cards;
			UpdateDeckHistoryPanel(_newDeck, true);
			ManaCurveMyDecks.SetDeck(_newDeck);
			UpdateDbListView();
		}

		private void TextBoxDeckName_TextChanged(object sender, TextChangedEventArgs e)
		{
			var tb = (TextBox)sender;
			var name = tb.Text;
			if(DeckList.Instance.Decks.Any(d => d.Name == name) && !(EditingDeck && name == _editedDeckName))
			{
				if(DeckNameExistsWarning.Visibility == Collapsed)
					tb.Width -= 19;
				DeckNameExistsWarning.Visibility = Visible;
			}
			else
			{
				if(DeckNameExistsWarning.Visibility == Visible)
					tb.Width += 19;
				DeckNameExistsWarning.Visibility = Collapsed;
			}
		}

		private async void BtnCancelEdit_Click(object sender, RoutedEventArgs e)
		{
			if(_newDeckUnsavedChanges)
			{
				var result =
					await
					this.ShowMessageAsync(EditingDeck ? "Cancel editing" : "Cancel deck creation",
					                      EditingDeck ? "All changes made to the deck will be lost." : "The new deck will be lost.",
					                      MessageDialogStyle.AffirmativeAndNegative);
				if(result != MessageDialogResult.Affirmative)
					return;
			}
			CloseNewDeck();
			EditingDeck = false;
			_editedDeckName = string.Empty;
			var prev = DeckPickerList.SelectedDecks.FirstOrDefault();
			SelectLastUsedDeck();
			DeckPickerList.SelectDeck(prev);
		}

		internal void SaveDeckWithOverwriteCheck() => SaveDeckWithOverwriteCheck(_newDeck.Version);

		internal void SaveDeckWithOverwriteCheck(SerializableVersion newVersion, bool saveAsNew = false)
		{
			if(saveAsNew)
			{
				EditingDeck = false;
				_newDeck.ResetVersions();
				_newDeck.ResetHearthstatsIds();
				_newDeck.DeckId = Guid.NewGuid();
				_newDeck.Archived = false;
			}

			SaveDeck(EditingDeck, newVersion);
			DeckPickerList.UpdateArchivedClassVisibility();

			_editedDeckName = string.Empty;
		}

		private void TextBoxDBFilter_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			var index = ListViewDB.SelectedIndex;
			Card card = null;
			var shiftPressed = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;

			switch(e.Key)
			{
				case Key.Enter:
					if(ListViewDB.SelectedItem != null)
						card = (Card)ListViewDB.SelectedItem;
					else if(ListViewDB.Items.Count > 0)
						card = (Card)ListViewDB.Items[0];
					break;
				case Key.D1:
					if(ListViewDB.Items.Count > 0 && !shiftPressed)
						card = (Card)ListViewDB.Items[0];
					break;
				case Key.D2:
					if(ListViewDB.Items.Count > 1 && !shiftPressed)
						card = (Card)ListViewDB.Items[1];
					break;
				case Key.D3:
					if(ListViewDB.Items.Count > 2 && !shiftPressed)
						card = (Card)ListViewDB.Items[2];
					break;
				case Key.D4:
					if(ListViewDB.Items.Count > 3 && !shiftPressed)
						card = (Card)ListViewDB.Items[3];
					break;
				case Key.D5:
					if(ListViewDB.Items.Count > 4 && !shiftPressed)
						card = (Card)ListViewDB.Items[4];
					break;
				case Key.Down:
					if(index < ListViewDB.Items.Count - 1)
						ListViewDB.SelectedIndex += 1;
					break;
				case Key.Up:
					if(index > 0)
						ListViewDB.SelectedIndex -= 1;
					break;
			}
			if(card != null)
			{
				AddCardToDeck((Card)card.Clone());
				e.Handled = true;
			}
		}

		private void ListViewDB_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var originalSource = (DependencyObject)e.OriginalSource;
			while((originalSource != null) && !(originalSource is ListViewItem))
				originalSource = VisualTreeHelper.GetParent(originalSource);

			if(originalSource != null)
			{
				var card = (Card)ListViewDB.SelectedItem;
				if(card == null)
					return;
				AddCardToDeck((Card)card.Clone());
				_newDeckUnsavedChanges = true;
			}
		}

		private void ListViewDeck_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if(_newDeck == null)
				return;
			var originalSource = (DependencyObject)e.OriginalSource;
			while((originalSource != null) && !(originalSource is ListViewItem))
				originalSource = VisualTreeHelper.GetParent(originalSource);

			if(originalSource != null)
			{
				var card = (Card)ListViewDeck.SelectedItem;
				if(card == null)
					return;
				RemoveCardFromDeck(card);
				_newDeckUnsavedChanges = true;
			}
		}

		private void ListViewDeck_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			if(_newDeck == null)
				return;
			var originalSource = (DependencyObject)e.OriginalSource;
			while((originalSource != null) && !(originalSource is ListViewItem))
				originalSource = VisualTreeHelper.GetParent(originalSource);

			if(originalSource != null)
			{
				var card = (Card)ListViewDeck.SelectedItem;
				if(card == null)
					return;
				AddCardToDeck((Card)card.Clone());
				_newDeckUnsavedChanges = true;
			}
		}

		private void ListViewDB_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Enter)
			{
				var card = (Card)ListViewDB.SelectedItem;
				if(string.IsNullOrEmpty(card?.Name))
					return;
				AddCardToDeck((Card)card.Clone());
			}
		}

		private void TextBoxDBFilter_TextChanged(object sender, TextChangedEventArgs e) => UpdateDbListView();

		private void BtnFilter_OnClick(object sender, RoutedEventArgs e) => UpdateDbListView();

		private void AddDeckHistory()
		{
			var currentClone = _originalDeck?.Clone() as Deck;
			if(currentClone == null)
				return;
			currentClone.Versions = new List<Deck>(); //empty ref to history
			_newDeck.Versions.Add(currentClone);
		}

		#endregion

		private void CheckBoxIncludeWild_Changed(object sender, RoutedEventArgs e) => UpdateDbListView();

		private void ButtonVersionHistory_OnClick(object sender, RoutedEventArgs e)
		{
			const int widthWithHistoryPanel = 485;
			const int widthWithoutHistoryPanel = 240;
			if(PanelDeckHistory.Visibility != Visible)
			{
				TextBlockButtonVersionHistory.Text = "HIDE VERSION HISTORY";
				PanelDeckHistory.Visibility = Visible;
				GridNewDeck.Width = widthWithHistoryPanel;
				Width += widthWithHistoryPanel - widthWithoutHistoryPanel;
				MinWidth += widthWithHistoryPanel - widthWithoutHistoryPanel;
			}
			else
			{
				TextBlockButtonVersionHistory.Text = "SHOW VERSION HISTORY";
				PanelDeckHistory.Visibility = Collapsed;
				GridNewDeck.Width = widthWithoutHistoryPanel;
				MinWidth -= widthWithHistoryPanel - widthWithoutHistoryPanel;
				Width -= widthWithHistoryPanel - widthWithoutHistoryPanel;
			}
		}
	}
}