﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker
{
	public partial class MainWindow
	{
		private string editedDeckName;

		private void UpdateDbListView()
		{
			if(_newDeck == null) return;
			var selectedClass = _newDeck.Class;
			string selectedNeutral;
			try
			{
				selectedNeutral = MenuFilterType.Items.Cast<RadioButton>().First(x => x.IsChecked.HasValue && x.IsChecked.Value).Content.ToString();
			}
			catch(Exception)
			{
				selectedNeutral = "ALL";
			}
			string selectedManaCost;
			try
			{
				selectedManaCost = MenuFilterMana.Items.Cast<RadioButton>().First(x => x.IsChecked.HasValue && x.IsChecked.Value).Content.ToString();
			}
			catch(Exception)
			{
				selectedManaCost = "ALL";
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
					if(!Config.Instance.UseFullTextSearch && !cardName.Contains(formattedInput))
						continue;
					if(Config.Instance.UseFullTextSearch && words.Any(w => !cardName.Contains(w)
					                                                       && !(!string.IsNullOrEmpty(card.Text) && card.Text.ToLowerInvariant().Contains(w))
					                                                       && (!string.IsNullOrEmpty(card.RaceOrType) && w != card.RaceOrType.ToLowerInvariant())
					                                                       && (!string.IsNullOrEmpty(card.Rarity) && w != card.Rarity.ToLowerInvariant())))
						continue;

					// mana filter
					if(selectedManaCost == "ALL"
					   || ((selectedManaCost == "9+" && card.Cost >= 9)
					       || (selectedManaCost == card.Cost.ToString())))
					{
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
				}

				Helper.SortCardCollection(ListViewDB.Items, Config.Instance.CardSortingClassFirst);
			}
		}

		private async void SaveDeck(bool overwrite)
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

			while(DeckList.DecksList.Any(d => d.Name == deckName) && (!EditingDeck || !overwrite))
			{
				var settings = new MetroDialogSettings {AffirmativeButtonText = "Set", DefaultText = deckName};
				var name =
					await
						this.ShowInputAsync("Name already exists", "You already have a deck with that name, please select a different one.", settings);

				if(String.IsNullOrEmpty(name))
					return;

				deckName = name;
				TextBoxDeckName.Text = name;
			}

			if(_newDeck.Cards.Sum(c => c.Count) != 30)
			{
				var settings = new MetroDialogSettings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"};

				var result =
					await
						this.ShowMessageAsync("Not 30 cards",
							string.Format("Deck contains {0} cards. Is this what you want to save anyway?",
								_newDeck.Cards.Sum(c => c.Count)),
							MessageDialogStyle.AffirmativeAndNegative, settings);
				if(result != MessageDialogResult.Affirmative)
					return;
			}

			if(EditingDeck && overwrite)
			{
				DeckList.DecksList.Remove(_newDeck);
				DeckPickerList.RemoveDeck(_newDeck);
			}

			var oldDeckName = _newDeck.Name;

			_newDeck.Name = deckName;
			_newDeck.Tags = TagControlEdit.GetTags();

			var newDeckClone = (Deck)_newDeck.Clone();
			DeckList.DecksList.Add(newDeckClone);

			newDeckClone.LastEdited = DateTime.Now;

			WriteDecks();
			Logger.WriteLine("Saved Decks");

			if(EditingDeck)
			{
				TagControlEdit.SetSelectedTags(new List<string>());
				if(deckName != oldDeckName)
				{
					var statsEntry = DeckStatsList.Instance.DeckStats.FirstOrDefault(d => d.Name == oldDeckName);
					if(statsEntry != null)
					{
						if(overwrite)
						{
							statsEntry.Name = deckName;
							Logger.WriteLine("Deck has new name, updated deckstats");
						}
						else
						{
							var newStatsEntry = DeckStatsList.Instance.DeckStats.FirstOrDefault(d => d.Name == deckName);
							if(newStatsEntry == null)
							{
								newStatsEntry = new DeckStats(deckName);
								DeckStatsList.Instance.DeckStats.Add(newStatsEntry);
							}
							foreach(var game in statsEntry.Games)
								newStatsEntry.AddGameResult(game.CloneWithNewId());
							Logger.WriteLine("cloned gamestats for \"Set as new\"");
						}
						DeckStatsList.Save();
					}
				}
			}

			//after cloning the stats, otherwise new stats will be generated
			DeckPickerList.AddAndSelectDeck(newDeckClone);

			EditingDeck = false;

			foreach(var tag in _newDeck.Tags)
				SortFilterDecksFlyout.AddSelectedTag(tag);

			DeckPickerList.UpdateList();
			DeckPickerList.SelectDeck(newDeckClone);

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
			UpdateTitle();
		}

		private void RemoveCardFromDeck(Card card)
		{
			if(card == null)
				return;
			if(card.Count > 1)
				card.Count--;
			else
				_newDeck.Cards.Remove(card);

			UpdateTitle();
			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
			ManaCurveMyDecks.UpdateValues();
            updateCharts(_newDeck);

		}

		private void AddCardToDeck(Card card)
		{
			if(card == null)
				return;
			if(_newDeck.Cards.Contains(card))
			{
				var cardInDeck = _newDeck.Cards.First(c => c.Name == card.Name);
				cardInDeck.Count++;
			}
			else
				_newDeck.Cards.Add(card);

			UpdateTitle();
			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
			ManaCurveMyDecks.UpdateValues();
            updateCharts(_newDeck);
			try
			{
				TextBoxDBFilter.Focus();
				TextBoxDBFilter.Select(0, TextBoxDBFilter.Text.Length);
			}
			catch
			{
			}
		}

		private void UpdateTitle()
		{
			Title = _newDeck == null ? "Hearthstone Deck Tracker" : string.Format("Hearthstone Deck Tracker - Cards: {0}", _newDeck.Cards.Sum(c => c.Count));
		}

		public void SetNewDeck(Deck deck, bool editing = false)
		{
			if(deck != null)
			{
				ClearNewDeckSection();
				DeselectDeck();
				EditingDeck = editing;
				if(editing)
					editedDeckName = deck.Name;
				_newDeck = (Deck)deck.Clone();
				ListViewDeck.ItemsSource = _newDeck.Cards;
				Helper.SortCardCollection(ListViewDeck.ItemsSource, false);
				TextBoxDeckName.Text = _newDeck.Name;
				UpdateDbListView();
				ExpandNewDeck();
				UpdateTitle();
			}
		}

		private void ExpandNewDeck()
		{
			if(GridNewDeck.Visibility != Visibility.Visible)
			{
				GridNewDeck.Visibility = Visibility.Visible;
				MenuNewDeck.Visibility = Visibility.Visible;
				GridNewDeck.UpdateLayout();
				Width += GridNewDeck.ActualWidth;
				MinWidth += GridNewDeck.ActualWidth;
			}
			DeckPickerListCover.Visibility = Visibility.Visible;
		}

		private void CloseNewDeck()
		{
			if(DeckPickerList.SelectedDeck != null)
				EnableMenuItems(true);
			if(GridNewDeck.Visibility != Visibility.Collapsed)
			{
				var width = GridNewDeck.ActualWidth;
				GridNewDeck.Visibility = Visibility.Collapsed;
				MenuNewDeck.Visibility = Visibility.Collapsed;
				MinWidth -= width;
				Width -= width;
			}
			ClearNewDeckSection();
			DeckPickerListCover.Visibility = Visibility.Hidden;
		}

		private void EnableMenuItems(bool enable)
		{
			MenuItemEdit.IsEnabled = enable;
			MenuItemExportIds.IsEnabled = enable;
			MenuItemExportScreenshot.IsEnabled = enable;
			MenuItemExportToHs.IsEnabled = enable;
			MenuItemExportXml.IsEnabled = enable;
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

		private void CreateNewDeck(string hero)
		{
			DeselectDeck();
			ExpandNewDeck();
			_newDeck = new Deck {Class = hero};
			ListViewDeck.ItemsSource = _newDeck.Cards;
			ManaCurveMyDecks.SetDeck(_newDeck);
			UpdateDbListView();
		}

		private void TextBoxDeckName_TextChanged(object sender, TextChangedEventArgs e)
		{
			var tb = (TextBox)sender;
			var name = tb.Text;
			if(DeckList.DecksList.Any(d => d.Name == name) && !(EditingDeck && name == editedDeckName))
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
				var result = await this.ShowMessageAsync(EditingDeck ? "Cancel editing" : "Cancel deck creation", EditingDeck ? "All changes made to the deck will be lost." : "The new deck will be lost.", MessageDialogStyle.AffirmativeAndNegative);
				if(result != MessageDialogResult.Affirmative)
					return;
			}
			ListViewDeck.ItemsSource = DeckPickerList.SelectedDeck != null ? DeckPickerList.SelectedDeck.Cards : null;
			CloseNewDeck();
			EditingDeck = false;
			editedDeckName = string.Empty;
		}

		private async void BtnSaveDeck_Click(object sender, RoutedEventArgs e)
		{
			//NewDeck.Cards =
			//	new ObservableCollection<Card>(
			//		NewDeck.Cards.OrderBy(c => c.Cost).ThenByDescending(c => c.Type).ThenBy(c => c.Name).ToList());
			//ListViewNewDeck.ItemsSource = NewDeck.Cards;
			var deckName = TextBoxDeckName.Text;
			if(EditingDeck)
			{
				var settings = new MetroDialogSettings {AffirmativeButtonText = "Overwrite", NegativeButtonText = "Save as new"};
				var result =
					await
						this.ShowMessageAsync("Saving deck", "How do you wish to save the deck?", MessageDialogStyle.AffirmativeAndNegative,
							settings);
				if(result == MessageDialogResult.Affirmative)
					SaveDeck(true);
				else if(result == MessageDialogResult.Negative)
					SaveDeck(false);
			}
			else if(DeckList.DecksList.Any(d => d.Name == deckName))
			{
				var settings = new MetroDialogSettings {AffirmativeButtonText = "Overwrite", NegativeButtonText = "Set new name"};
				var result =
					await
						this.ShowMessageAsync("A deck with that name already exists", "Overwriting the deck can not be undone!",
							MessageDialogStyle.AffirmativeAndNegative, settings);
				if(result == MessageDialogResult.Affirmative)
				{
					Deck oldDeck;
					while((oldDeck = DeckList.DecksList.FirstOrDefault(d => d.Name == deckName)) != null)
					{
						var deckStats = DeckStatsList.Instance.DeckStats.FirstOrDefault(ds => ds.Name == oldDeck.Name);
						if(deckStats != null)
						{
							foreach(var game in deckStats.Games)
								game.DeleteGameFile();
							DeckStatsList.Instance.DeckStats.Remove(deckStats);
							DeckStatsList.Save();
							Logger.WriteLine("Deleted deckstats for deck: " + oldDeck.Name);
						}
						DeckList.DecksList.Remove(oldDeck);
						DeckPickerList.RemoveDeck(oldDeck);
					}

					SaveDeck(true);
				}
				else if(result == MessageDialogResult.Negative)
					SaveDeck(false);
			}
			else
				SaveDeck(false);

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
				if(card == null) return;
				AddCardToDeck((Card)card.Clone());
				_newDeckUnsavedChanges = true;
			}
		}

		private void ListViewDeck_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if(_newDeck == null) return;
			var originalSource = (DependencyObject)e.OriginalSource;
			while((originalSource != null) && !(originalSource is ListViewItem))
				originalSource = VisualTreeHelper.GetParent(originalSource);

			if(originalSource != null)
			{
				var card = (Card)ListViewDeck.SelectedItem;
				RemoveCardFromDeck(card);
				_newDeckUnsavedChanges = true;
			}
		}

		private void ListViewDeck_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			if(_newDeck == null) return;
			var originalSource = (DependencyObject)e.OriginalSource;
			while((originalSource != null) && !(originalSource is ListViewItem))
				originalSource = VisualTreeHelper.GetParent(originalSource);

			if(originalSource != null)
			{
				var card = (Card)ListViewDeck.SelectedItem;
				AddCardToDeck((Card)card.Clone());
				_newDeckUnsavedChanges = true;
			}
		}

		private void ListViewDB_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Enter)
			{
				var card = (Card)ListViewDB.SelectedItem;
				if(string.IsNullOrEmpty(card.Name)) return;
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

		#endregion
	}
}