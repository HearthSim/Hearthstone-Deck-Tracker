#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Stats;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public partial class MainWindow
	{
		internal void BtnNotes_Click(object sender, RoutedEventArgs e)
		{
			if(DeckList.Instance.ActiveDeck == null)
				return;
			FlyoutNotes.IsOpen = !FlyoutNotes.IsOpen;
		}

		internal async void BtnDeleteDeck_Click(object sender, RoutedEventArgs e)
		{
			var decks = DeckPickerList.SelectedDecks;
			if(!decks.Any())
				return;

			var settings = new MetroDialogSettings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"};
			var keepStatsInfo = Config.Instance.KeepStatsWhenDeletingDeck
				                    ? "The stats will be kept (can be changed in options)"
				                    : "The stats will be deleted (can be changed in options)";
			var result =
				await
				this.ShowMessageAsync("Deleting " + (decks.Count == 1 ? decks.First().Name : decks.Count + " decks"),
				                      "Are you Sure?\n" + keepStatsInfo, MessageDialogStyle.AffirmativeAndNegative, settings);
			if(result == MessageDialogResult.Negative)
				return;
			DeckManagerEvents.OnDeckDeleted.Execute(decks);
			SelectDeck(null);
			foreach(var deck in decks)
				DeleteDeck(deck, false);
			DeckStatsList.Save();
			DeckList.Save();
			DeckPickerList.UpdateDecks();
			DeckPickerList.UpdateArchivedClassVisibility();
		}

		private async void DeleteDeck(Deck deck, bool saveAndUpdate = true)
		{
			if(deck == null)
				return;

			var deckStats = DeckStatsList.Instance.DeckStats.FirstOrDefault(ds => ds.BelongsToDeck(deck));
			if(deckStats != null)
			{
				if(deckStats.Games.Any())
				{
					if(Config.Instance.KeepStatsWhenDeletingDeck)
					{
						DefaultDeckStats.Instance.GetDeckStats(deck.Class).Games.AddRange(deckStats.Games);
						DefaultDeckStats.Save();
						Logger.WriteLine(string.Format("Moved deckstats for deck {0} to default stats", deck.Name), "Edit");
					}
					else
					{
						try
						{
							foreach(var game in deckStats.Games)
								game.DeleteGameFile();
							Logger.WriteLine("Deleted games from deck: " + deck.Name, "Edit");
						}
						catch(Exception)
						{
							Logger.WriteLine("Error deleting games", "Edit");
						}
					}
				}
				DeckStatsList.Instance.DeckStats.Remove(deckStats);
				if(saveAndUpdate)
					DeckStatsList.Save();
				Logger.WriteLine("Removed deckstats from deck: " + deck.Name, "Edit");
			}

			if(HearthStatsAPI.IsLoggedIn && deck.HasHearthStatsId && await CheckHearthStatsDeckDeletion())
				HearthStatsManager.DeleteDeckAsync(deck, false, true);

			DeckList.Instance.Decks.Remove(deck);
			if(saveAndUpdate)
			{
				DeckList.Save();
				DeckPickerList.UpdateDecks();
				DeckPickerList.UpdateArchivedClassVisibility();
			}
			ListViewDeck.ItemsSource = null;
			Logger.WriteLine("Deleted deck: " + deck.Name, "Edit");
		}

		internal void BtnArchiveDeck_Click(object sender, RoutedEventArgs e)
		{
			foreach(var deck in DeckPickerList.SelectedDecks)
				ArchiveDeck(deck, true, false);

			DeckList.Save();
			DeckPickerList.UpdateDecks();
			SelectDeck(null);
			DeckPickerList.UpdateArchivedClassVisibility();
		}

		internal void BtnUnarchiveDeck_Click(object sender, RoutedEventArgs e)
		{
			foreach(var deck in DeckPickerList.SelectedDecks)
				ArchiveDeck(deck, false, false);

			DeckList.Save();
			DeckPickerList.UpdateDecks();
			DeckPickerList.SelectDeckAndAppropriateView(DeckList.Instance.ActiveDeck);
			UpdateMenuItemVisibility(DeckList.Instance.ActiveDeck);
			DeckPickerList.UpdateArchivedClassVisibility();
		}

		public void ArchiveDeck(Deck deck, bool archive, bool saveAndUpdate = true)
		{
			if(deck == null)
				return;

			var oldArchived = deck.Archived;
			if(oldArchived == archive)
				return;

			deck.Archived = archive;
			deck.Edited();

			try
			{
				if(saveAndUpdate)
				{
					DeckList.Save();
					DeckPickerList.UpdateDecks();

					if(archive)
						SelectDeck(null);
					else
					{
						DeckPickerList.SelectDeckAndAppropriateView(deck);
						UpdateMenuItemVisibility(deck);
					}

					DeckPickerList.UpdateArchivedClassVisibility();
				}

				var archivedLog = archive ? "archived" : "unarchived";
				Logger.WriteLine(String.Format("Successfully {0} deck: {1}", archivedLog, deck.Name), "ArchiveDeck");

				if(Config.Instance.HearthStatsAutoUploadNewDecks && HearthStatsAPI.IsLoggedIn)
				{
					Logger.WriteLine(String.Format("auto uploading {0} deck", archivedLog), "ArchiveDeck");
					HearthStatsManager.UpdateDeckAsync(deck, background: true);
				}
			}
			catch(Exception)
			{
				Logger.WriteLine(String.Format("Error {0} deck", archive ? "archiving" : "unarchiving", deck.Name), "ArchiveDeck");
			}
		}

		internal async void BtnCloneDeck_Click(object sender, RoutedEventArgs e)
		{
			var cloneStats =
				(await
				 this.ShowMessageAsync("Clone game history?", "(Cloned games do not count towards class or overall stats.)",
				                       MessageDialogStyle.AffirmativeAndNegative,
				                       new MetroDialogSettings
				                       {
					                       AffirmativeButtonText = "clone history",
					                       NegativeButtonText = "do not clone history"
				                       })) == MessageDialogResult.Affirmative;

			var clone = (Deck)DeckList.Instance.ActiveDeck.CloneWithNewId(false);
			var originalStats = DeckList.Instance.ActiveDeck.DeckStats;
			clone.ResetHearthstatsIds();
			clone.Versions.ForEach(v => v.ResetHearthstatsIds());
			clone.Archived = false;

			/*while(DeckList.DecksList.Any(d => d.Name == clone.Name))
			{
				var settings = new MetroDialogSettings {AffirmativeButtonText = "Set", DefaultText = clone.Name};
				var name =
					await
					this.ShowInputAsync("Name already exists", "You already have a deck with that name, please select a different one.", settings);

				if(string.IsNullOrEmpty(name))
					return;

				clone.Name = name;
			}*/

			DeckList.Instance.Decks.Add(clone);
			//DeckPickerList.AddAndSelectDeck(clone);
			DeckList.Save();

			var newStatsEntry = DeckStatsList.Instance.DeckStats.FirstOrDefault(ds => ds.BelongsToDeck(clone));
			if(newStatsEntry == null)
			{
				newStatsEntry = new DeckStats(clone);
				DeckStatsList.Instance.DeckStats.Add(newStatsEntry);
			}

			//clone game stats
			if(cloneStats)
			{
				foreach(var game in originalStats.Games)
					newStatsEntry.AddGameResult(game.CloneWithNewId());
				Logger.WriteLine("cloned gamestats", "Edit");
			}

			DeckStatsList.Save();
			DeckPickerList.SelectDeckAndAppropriateView(clone);

			if(Config.Instance.HearthStatsAutoUploadNewDecks && HearthStatsAPI.IsLoggedIn)
				HearthStatsManager.UploadDeckAsync(clone);
		}

		internal async void BtnCloneSelectedVersion_Click(object sender, RoutedEventArgs e)
		{
			var deck = DeckList.Instance.ActiveDeckVersion;
			if(deck == null)
				return;
			var cloneStats =
				(await
				 this.ShowMessageAsync("Clone game history?", "(Cloned games do not count towards class or overall stats.)",
				                       MessageDialogStyle.AffirmativeAndNegative,
				                       new MetroDialogSettings
				                       {
					                       AffirmativeButtonText = "clone history",
					                       NegativeButtonText = "do not clone history"
				                       })) == MessageDialogResult.Affirmative;
			var clone = (Deck)deck.CloneWithNewId(false);
			clone.ResetVersions();
			clone.ResetHearthstatsIds();
			clone.Archived = false;

			var originalStatsEntry = clone.DeckStats;

			/*while(DeckList.DecksList.Any(d => d.Name == clone.Name))
			{
				var settings = new MetroDialogSettings {AffirmativeButtonText = "Set", DefaultText = clone.Name};
				var name =
					await
					this.ShowInputAsync("Name already exists", "You already have a deck with that name, please select a different one.", settings);

				if(string.IsNullOrEmpty(name))
					return;

				clone.Name = name;
			}*/

			DeckList.Instance.Decks.Add(clone);
			DeckPickerList.UpdateDecks();
			DeckList.Save();


			var newStatsEntry = DeckStatsList.Instance.DeckStats.FirstOrDefault(ds => ds.BelongsToDeck(clone));
			if(newStatsEntry == null)
			{
				newStatsEntry = new DeckStats(clone);
				DeckStatsList.Instance.DeckStats.Add(newStatsEntry);
			}

			//clone game stats
			if(cloneStats)
			{
				foreach(var game in originalStatsEntry.Games)
					newStatsEntry.AddGameResult(game.CloneWithNewId());
				Logger.WriteLine("cloned gamestats (version)", "Edit");
			}

			DeckStatsList.Save();
			//DeckPickerList.UpdateList();
			DeckPickerList.SelectDeckAndAppropriateView(clone);

			if(Config.Instance.HearthStatsAutoUploadNewDecks && HearthStatsAPI.IsLoggedIn)
				HearthStatsManager.UploadDeckAsync(clone);
		}

		internal void BtnTags_Click(object sender, RoutedEventArgs e)
		{
			FlyoutMyDecksSetTags.IsOpen = true;
			TagControlEdit.SetSelectedTags(DeckPickerList.SelectedDecks);
		}

		internal void BtnEditDeck_Click(object sender, RoutedEventArgs e)
		{
			var selectedDeck = DeckList.Instance.ActiveDeck;
			if(selectedDeck == null)
				return;
			SetNewDeck(selectedDeck, true);
		}

		internal async void BtnUpdateDeck_Click(object sender, RoutedEventArgs e)
		{
			var selectedDeck = DeckList.Instance.ActiveDeck;
			if(selectedDeck == null || string.IsNullOrEmpty(selectedDeck.Url))
				return;
			var deck = await DeckImporter.Import(selectedDeck.Url);
			if(deck == null)
			{
				await this.ShowMessageAsync("Error", "Could not load deck from specified url.");
				return;
			}
			//this could be expanded to check against the last version of the deck that was not modified after downloading
			if(deck.Cards.All(c1 => selectedDeck.GetSelectedDeckVersion().Cards.Any(c2 => c1.Name == c2.Name && c1.Count == c2.Count)))
			{
				await this.ShowMessageAsync("Already up to date.", "No changes found.");
				return;
			}

			SetNewDeck(selectedDeck, true);
			TextBoxDeckName.Text = deck.Name;
			_newDeck.Cards.Clear();
			foreach(var card in deck.Cards)
				_newDeck.Cards.Add(card);
			_newDeck.Edited();

			UpdateCardCount();
			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
			ManaCurveMyDecks.UpdateValues();

			TagControlEdit.SetSelectedTags(deck.Tags);
		}

		internal void BtnMoveDeckToArena_Click(object sender, RoutedEventArgs e)
		{
			foreach(var deck in DeckPickerList.SelectedDecks)
				deck.IsArenaDeck = true;
			DeckPickerList.UpdateDecks();
			MenuItemMoveDecktoArena.Visibility = Visibility.Collapsed;
			MenuItemMoveDeckToConstructed.Visibility = Visibility.Visible;
		}

		internal void BtnMoveDeckToConstructed_Click(object sender, RoutedEventArgs e)
		{
			foreach(var deck in DeckPickerList.SelectedDecks)
				deck.IsArenaDeck = false;
			DeckPickerList.UpdateDecks();
			MenuItemMoveDecktoArena.Visibility = Visibility.Visible;
			MenuItemMoveDeckToConstructed.Visibility = Visibility.Collapsed;
		}

		internal void BtnOpenDeckUrl_Click(object sender, RoutedEventArgs e)
		{
			if(DeckList.Instance.ActiveDeck != null)
			{
				try
				{
					Process.Start(DeckList.Instance.ActiveDeck.Url);
				}
				catch(Exception ex)
				{
					Logger.WriteLine("Error opening deck website " + ex);
				}
			}
		}

		internal async void BtnName_Click(object sender, RoutedEventArgs e)
		{
			var deck = DeckList.Instance.ActiveDeck;
			if(deck == null)
				return;
			var settings = new MetroDialogSettings {AffirmativeButtonText = "set", NegativeButtonText = "cancel", DefaultText = deck.Name};
			var newName = await this.ShowInputAsync("Set deck name", "", settings);
			if(!string.IsNullOrEmpty(newName) && deck.Name != newName)
			{
				deck.Name = newName;
				deck.Edited();
				DeckList.Save();
				DeckPickerList.UpdateDecks();
				if(Config.Instance.HearthStatsAutoUploadNewDecks && HearthStatsAPI.IsLoggedIn)
					HearthStatsManager.UpdateDeckAsync(deck, true, true);
			}
		}
	}
}