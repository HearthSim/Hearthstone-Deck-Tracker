#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Stats;
using MahApps.Metro.Controls.Dialogs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListViewItem = System.Windows.Controls.ListViewItem;
using RadioButton = System.Windows.Controls.RadioButton;
using TextBox = System.Windows.Controls.TextBox;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public partial class MainWindow
	{
		internal double? _movedLeft;
		private string editedDeckName;

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

				foreach(var card in Game.GetActualCards())
				{
					var cardName = Helper.RemoveDiacritics(card.LocalizedName.ToLowerInvariant(), true);
					if(!Config.Instance.UseFullTextSearch && !cardName.Contains(formattedInput)
					   && (!string.IsNullOrEmpty(card.RaceOrType) && formattedInput != card.RaceOrType.ToLowerInvariant()))
						continue;
					if(Config.Instance.UseFullTextSearch
					   && words.Any(
					                w =>
					                !cardName.Contains(w) && !(!string.IsNullOrEmpty(card.Text) && card.Text.ToLowerInvariant().Contains(w))
					                && (!string.IsNullOrEmpty(card.RaceOrType) && w != card.RaceOrType.ToLowerInvariant())
					                && (!string.IsNullOrEmpty(card.Rarity) && w != card.Rarity.ToLowerInvariant())))
						continue;

					// mana filter
					if(selectedManaCost != "ALL" && ((selectedManaCost != "9+" || card.Cost < 9) && (selectedManaCost != card.Cost.ToString())))
						continue;
					if(selectedSet != "ALL" && !string.Equals(selectedSet, card.Set, StringComparison.InvariantCultureIgnoreCase))
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

		private async void SaveDeck(bool overwrite, SerializableVersion newVersion)
		{
			var deckName = TextBoxDeckName.Text;

			if(string.IsNullOrEmpty(deckName))
			{
				var settings = new MetroDialogSettings {AffirmativeButtonText = "Set", DefaultText = deckName};

				var name = await this.ShowInputAsync("No name set", "Please set a name for the deck", settings);

				if(String.IsNullOrEmpty(name))
					return;

				deckName = name;
				TextBoxDeckName.Text = name;
			}

			/*while(DeckList.DecksList.Any(d => d.Name == deckName) && (!EditingDeck || !overwrite))
			{
				var settings = new MetroDialogSettings {AffirmativeButtonText = "Set", DefaultText = deckName};
				var name =
					await
					this.ShowInputAsync("Name already exists", "You already have a deck with that name, please select a different one.", settings);

				if(String.IsNullOrEmpty(name))
					return;

				deckName = name;
				TextBoxDeckName.Text = name;
			}*/

			if(_newDeck.Cards.Sum(c => c.Count) != 30)
			{
				var settings = new MetroDialogSettings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"};

				var result =
					await
					this.ShowMessageAsync("Not 30 cards",
					                      string.Format("Deck contains {0} cards. Is this what you want to save anyway?",
					                                    _newDeck.Cards.Sum(c => c.Count)), MessageDialogStyle.AffirmativeAndNegative, settings);
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
				//UpdateDeckHistoryPanel(_newDeck, false);
			}

			if(EditingDeck && overwrite)
			{
				DeckList.Instance.Decks.Remove(_newDeck);
				//DeckPickerList.RemoveDeck(_newDeck);
			}

			var oldDeckName = _newDeck.Name;

			_newDeck.Name = deckName;

			var newDeckClone = (Deck)_newDeck.Clone();
			newDeckClone.Archived = false;

			DeckList.Instance.Decks.Add(newDeckClone);

			newDeckClone.LastEdited = DateTime.Now;

			DeckList.Save();

			Logger.WriteLine("Saved Decks", "SaveDeck");

			if(EditingDeck)
			{
				TagControlEdit.SetSelectedTags(new List<string>());
				if(deckName != oldDeckName)
				{
					var statsEntry = DeckStatsList.Instance.DeckStats.FirstOrDefault(ds => ds.BelongsToDeck(_newDeck));
					if(statsEntry != null)
					{
						if(overwrite)
						{
							statsEntry.Name = deckName;
							Logger.WriteLine("Deck has new name, updated deckstats", "SaveDeck");
							foreach(var game in statsEntry.Games)
								game.DeckName = deckName;
						}
						else
						{
							var newStatsEntry = DeckStatsList.Instance.DeckStats.FirstOrDefault(ds => ds.BelongsToDeck(_newDeck));
							if(newStatsEntry == null)
							{
								newStatsEntry = new DeckStats(_newDeck);
								DeckStatsList.Instance.DeckStats.Add(newStatsEntry);
							}
							foreach(var game in statsEntry.Games)
								newStatsEntry.AddGameResult(game.CloneWithNewId());
							Logger.WriteLine("cloned gamestats for \"Set as new\"", "SaveDeck");
						}
						DeckStatsList.Save();
					}
				}
			}


			if(Config.Instance.HearthStatsAutoUploadNewDecks && HearthStatsAPI.IsLoggedIn)
			{
				Logger.WriteLine("auto uploading new/edited deck", "SaveDeck");
				if(EditingDeck)
				{
					if(previousVersion != newVersion)
						HearthStatsManager.UploadVersionAsync(newDeckClone, _originalDeck.HearthStatsIdForUploading, background: true);
					else
						HearthStatsManager.UpdateDeckAsync(newDeckClone, background: true);
				}
				else
					HearthStatsManager.UploadDeckAsync(newDeckClone, background: true);
			}

			//after cloning the stats, otherwise new stats will be generated
			//DeckPickerList.AddAndSelectDeck(newDeckClone);
			if(EditingDeck)
				DeckManagerEvents.OnDeckUpdated.Execute(newDeckClone);
			else
				DeckManagerEvents.OnDeckCreated.Execute(newDeckClone);


			EditingDeck = false;

			foreach(var tag in _newDeck.Tags)
				SortFilterDecksFlyout.AddSelectedTag(tag);

			DeckPickerList.SelectDeckAndAppropriateView(newDeckClone);
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
		}

		private void AddCardToDeck(Card card)
		{
			if(card == null)
				return;
			var cardInDeck = _newDeck.Cards.FirstOrDefault(c => c.Name == card.Name);
			if(cardInDeck != null)
			{
				if(!_newDeck.IsArenaDeck && (cardInDeck.Count >= 2 || cardInDeck.Rarity == "Legendary" && cardInDeck.Count >= 1))
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
		}

		private void UpdateCardCount()
		{
			var count = _newDeck == null ? 0 : _newDeck.Cards.Sum(c => c.Count);
			TextBlockCardCount.Text = count + " / 30";
			CardCountWarning.Visibility = count > 30 ? Visibility.Visible : Visibility.Collapsed;
		}

		public void SetNewDeck(Deck deck, bool editing = false)
		{
			if(deck != null)
			{
				ClearNewDeckSection();
				SelectDeck(null);
				EditingDeck = editing;
				if(editing)
				{
					editedDeckName = deck.Name;
					_originalDeck = deck;
				}
				_newDeck = (Deck)deck.Clone();

				_newDeck.Cards.Clear();
				foreach(var card in deck.GetSelectedDeckVersion().Cards)
					_newDeck.Cards.Add(card.Clone() as Card);
				_newDeck.SelectedVersion = _newDeck.Version;

				ListViewDeck.ItemsSource = _newDeck.Cards;
				Helper.SortCardCollection(ListViewDeck.ItemsSource, false);
				TextBoxDeckName.Text = _newDeck.Name;
				UpdateDeckHistoryPanel(deck, !editing);
				UpdateDbListView();
				ExpandNewDeck();
				UpdateCardCount();
				ManaCurveMyDecks.SetDeck(_newDeck);
			}
		}

		private void ExpandNewDeck()
		{
			const int widthWithHistoryPanel = 485;
			const int widthWithoutHistoryPanel = 240;
			if(GridNewDeck.Visibility != Visibility.Visible)
			{
				GridNewDeck.Visibility = Visibility.Visible;
				MenuNewDeck.Visibility = Visibility.Visible;
				if(_newDeck != null && _newDeck.HasVersions)
				{
					PanelDeckHistory.Visibility = Visibility.Visible;
					GridNewDeck.Width = widthWithHistoryPanel;
				}
				else
					GridNewDeck.Width = widthWithoutHistoryPanel;
				GridNewDeck.UpdateLayout();
				Width += GridNewDeck.ActualWidth;
				MinWidth += GridNewDeck.ActualWidth;
			}
			DeckPickerListCover.Visibility = Visibility.Visible;
			PanelVersionComboBox.Visibility = Visibility.Collapsed;
			PanelCardCount.Visibility = Visibility.Visible;

			//move window left if opening the edit panel causes it to be outside of a screen
			foreach(var screen in Screen.AllScreens)
			{
				int windowLeft = (int)Left;
				int windowRight = (int)(Left + Width);
				int screenLeft = screen.WorkingArea.X;
				int screenRight = screen.WorkingArea.Right;

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

					_movedLeft = windowLeft - Left;
					break;
				}
			}
		}

		private void CloseNewDeck()
		{
			if(DeckList.Instance.ActiveDeck != null)
				EnableMenuItems(true);
			if(GridNewDeck.Visibility != Visibility.Collapsed)
			{
				var width = GridNewDeck.ActualWidth;
				GridNewDeck.Visibility = Visibility.Collapsed;
				MenuNewDeck.Visibility = Visibility.Collapsed;
				PanelDeckHistory.Visibility = Visibility.Collapsed;
				MinWidth -= width;
				Width -= width;
			}
			ClearNewDeckSection();
			DeckPickerListCover.Visibility = Visibility.Hidden;
			PanelVersionComboBox.Visibility = DeckList.Instance.ActiveDeck != null && DeckList.Instance.ActiveDeck.HasVersions
				                                  ? Visibility.Visible : Visibility.Collapsed;
			PanelCardCount.Visibility = Visibility.Collapsed;

			if(_movedLeft.HasValue)
			{
				Left += _movedLeft.Value;
				_movedLeft = null;
			}
		}

		private void EnableMenuItems(bool enable)
		{
			//MenuItemSelectedDeckStats.IsEnabled = enable;
			MenuItemEdit.IsEnabled = enable;
			MenuItemExportIds.IsEnabled = enable;
			MenuItemExportScreenshot.IsEnabled = enable;
			MenuItemExportToHs.IsEnabled = enable;
			MenuItemExportXml.IsEnabled = enable;
		}

		private async void MenuItem_OnSubmenuOpened(object sender, RoutedEventArgs e)
		{
			//a menuitems clickevent does not fire if it has subitems
			//bit of a hacky workaround, but this does the trick (subitems are disabled when a new deck is created, enabled when one is edited)
			if(_newDeck.IsArenaDeck
			   || !MenuItemSaveVersionCurrent.IsEnabled && !MenuItemSaveVersionMinor.IsEnabled && !MenuItemSaveVersionMajor.IsEnabled)
			{
				MenuItemSave.IsSubmenuOpen = false;
				await SaveDeckWithOverwriteCheck();
			}
		}

		private void MenuItemDashboard_OnClick(object sender, RoutedEventArgs e)
		{
			Process.Start(@"http://hearthstats.net/dashboards");
		}

		#region UI

		private void BtnNewDeckDruid_Click(object sender, RoutedEventArgs e)
		{
			CreateNewDeck("Druid");
		}

		private void BtnNewDeckHunter_Click(object sender, RoutedEventArgs e)
		{
			CreateNewDeck("Hunter");
		}

		private void BtnNewDeckMage_Click(object sender, RoutedEventArgs e)
		{
			CreateNewDeck("Mage");
		}

		private void BtnNewDeckPaladin_Click(object sender, RoutedEventArgs e)
		{
			CreateNewDeck("Paladin");
		}

		private void BtnNewDeckPriest_Click(object sender, RoutedEventArgs e)
		{
			CreateNewDeck("Priest");
		}

		private void BtnNewDeckRogue_Click(object sender, RoutedEventArgs e)
		{
			CreateNewDeck("Rogue");
		}

		private void BtnNewDeckShaman_Click(object sender, RoutedEventArgs e)
		{
			CreateNewDeck("Shaman");
		}

		private void BtnNewDeckWarrior_Click(object sender, RoutedEventArgs e)
		{
			CreateNewDeck("Warrior");
		}

		private void BtnNewDeckWarlock_Click(object sender, RoutedEventArgs e)
		{
			CreateNewDeck("Warlock");
		}

		private async void CreateNewDeck(string hero)
		{
			_newDeck = new Deck {Class = hero};

			var result =
				await
				this.ShowMessageAsync("Deck type?", "Please select a deck type.", MessageDialogStyle.AffirmativeAndNegative,
				                      new MetroDialogSettings {AffirmativeButtonText = "constructed", NegativeButtonText = "arena run"});
			if(result == MessageDialogResult.Negative)
				_newDeck.IsArenaDeck = true;

			SelectDeck(null);
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
			if(DeckList.Instance.Decks.Any(d => d.Name == name) && !(EditingDeck && name == editedDeckName))
			{
				if(DeckNameExistsWarning.Visibility == Visibility.Collapsed)
					tb.Width -= 19;
				DeckNameExistsWarning.Visibility = Visibility.Visible;
			}
			else
			{
				if(DeckNameExistsWarning.Visibility == Visibility.Visible)
					tb.Width += 19;
				DeckNameExistsWarning.Visibility = Visibility.Collapsed;
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
			ListViewDeck.ItemsSource = DeckList.Instance.ActiveDeck != null ? DeckList.Instance.ActiveDeckVersion.Cards : null;
			CloseNewDeck();
			EditingDeck = false;
			editedDeckName = string.Empty;
			SelectLastUsedDeck();
		}

		private async Task SaveDeckWithOverwriteCheck()
		{
			await SaveDeckWithOverwriteCheck(_newDeck.Version);
		}

		private async Task SaveDeckWithOverwriteCheck(SerializableVersion newVersion, bool saveAsNew = false)
		{
			var deckName = TextBoxDeckName.Text;
			if(saveAsNew)
			{
				EditingDeck = false;
				_newDeck.ResetVersions();
				_newDeck.ResetHearthstatsIds();
				_newDeck.DeckId = Guid.NewGuid();
				_newDeck.Archived = false;
			}
			/*else if(!EditingDeck && DeckList.DecksList.Any(d => d.Name == deckName))
			{
				var settings = new MetroDialogSettings {AffirmativeButtonText = "Overwrite", NegativeButtonText = "Set new name"};

				var keepStatsInfo = Config.Instance.KeepStatsWhenDeletingDeck
					                    ? "The stats will be moved to the default-deck (can be changed in options)"
					                    : "The stats will be deleted (can be changed in options)";
				var result =
					await
					this.ShowMessageAsync("A deck with that name already exists", "Overwriting the deck can not be undone!\n" + keepStatsInfo,
					                      MessageDialogStyle.AffirmativeAndNegative, settings);
				if(result == MessageDialogResult.Affirmative)
				{
					Deck oldDeck;
					while((oldDeck = DeckList.DecksList.FirstOrDefault(d => d.Name == deckName)) != null)
						DeleteDeck(oldDeck);

					SaveDeck(true, newVersion);
				}
				else if(result == MessageDialogResult.Negative)
					SaveDeck(false, newVersion);
			}*/

			SaveDeck(EditingDeck, newVersion);
			DeckPickerList.UpdateArchivedClassVisibility();

			editedDeckName = string.Empty;
		}

		private void TextBoxDBFilter_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			var index = ListViewDB.SelectedIndex;
			Card card = null;
			switch(e.Key)
			{
				case Key.Enter:
					if(ListViewDB.SelectedItem != null)
						card = (Card)ListViewDB.SelectedItem;
					else if(ListViewDB.Items.Count > 0)
						card = (Card)ListViewDB.Items[0];
					break;
				case Key.D1:
					if(ListViewDB.Items.Count > 0)
						card = (Card)ListViewDB.Items[0];
					break;
				case Key.D2:
					if(ListViewDB.Items.Count > 1)
						card = (Card)ListViewDB.Items[1];
					break;
				case Key.D3:
					if(ListViewDB.Items.Count > 2)
						card = (Card)ListViewDB.Items[2];
					break;
				case Key.D4:
					if(ListViewDB.Items.Count > 3)
						card = (Card)ListViewDB.Items[3];
					break;
				case Key.D5:
					if(ListViewDB.Items.Count > 4)
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
				if(card == null || string.IsNullOrEmpty(card.Name))
					return;
				AddCardToDeck((Card)card.Clone());
			}
		}

		private void TextBoxDBFilter_TextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateDbListView();
		}

		private void BtnFilter_OnClick(object sender, RoutedEventArgs e)
		{
			UpdateDbListView();
		}

		private void AddDeckHistory()
		{
			if(_originalDeck == null)
				return;
			var currentClone = _originalDeck.Clone() as Deck;
			if(currentClone == null)
				return;
			currentClone.Versions = new List<Deck>(); //empty ref to history
			_newDeck.Versions.Add(currentClone);
		}

		#endregion
	}
}