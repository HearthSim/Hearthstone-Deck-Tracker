#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class MainWindow
	{
		internal void BtnNotes_Click(object sender, RoutedEventArgs e)
		{
			if(DeckPickerList.SelectedDecks.FirstOrDefault() == null)
				return;
			FlyoutNotes.IsOpen = !FlyoutNotes.IsOpen;
		}

		internal async void BtnDeleteDeck_Click(object sender, RoutedEventArgs e)
		{
			var decks = DeckPickerList.SelectedDecks;
			if(!decks.Any())
				return;

			var settings = new MessageDialogs.Settings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"};
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
			SelectDeck(null, true);
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

			DeckStats deckStats;
			if(DeckStatsList.Instance.DeckStats.TryGetValue(deck.DeckId, out deckStats))
			{
				if(deckStats.Games.Any())
				{
					if(Config.Instance.KeepStatsWhenDeletingDeck)
					{
						var defaultDeck = DefaultDeckStats.Instance.GetDeckStats(deck.Class);
						defaultDeck?.Games.AddRange(deckStats.Games);
						DefaultDeckStats.Save();
						Log.Info($"Moved deckstats for deck {deck.Name} to default stats");
					}
					else
					{
						try
						{
							foreach(var game in deckStats.Games)
								game.DeleteGameFile();
							Log.Info("Deleted games from deck: " + deck.Name);
						}
						catch(Exception ex)
						{
							Log.Error("Error deleting games " + ex);
						}
					}
				}
				DeckStatsList.Instance.DeckStats.TryRemove(deckStats.DeckId, out deckStats);
				if(saveAndUpdate)
					DeckStatsList.Save();
				Log.Info("Removed deckstats from deck: " + deck.Name);
			}

			if(HearthStatsAPI.IsLoggedIn && deck.HasHearthStatsId && await CheckHearthStatsDeckDeletion())
				HearthStatsManager.DeleteDeckAsync(deck, false, true).Forget();

			DeckList.Instance.Decks.Remove(deck);
			if(saveAndUpdate)
			{
				DeckList.Save();
				DeckPickerList.UpdateDecks();
				DeckPickerList.UpdateArchivedClassVisibility();
			}
			ListViewDeck.ItemsSource = null;
			Log.Info("Deleted deck: " + deck.Name);
		}

		internal void BtnArchiveDeck_Click(object sender, RoutedEventArgs e)
		{
			foreach(var deck in DeckPickerList.SelectedDecks)
				ArchiveDeck(deck, true, false);

			DeckList.Save();
			DeckPickerList.UpdateDecks();
			SelectDeck(null, true);
			DeckPickerList.UpdateArchivedClassVisibility();
		}

		internal void BtnUnarchiveDeck_Click(object sender, RoutedEventArgs e)
		{
			foreach(var deck in DeckPickerList.SelectedDecks)
				ArchiveDeck(deck, false, false);

			DeckList.Save();
			DeckPickerList.UpdateDecks();
			DeckPickerList.SelectDeckAndAppropriateView(DeckPickerList.SelectedDecks.FirstOrDefault());
			UpdateMenuItemVisibility();
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
						SelectDeck(null, true);
					else
					{
						DeckPickerList.SelectDeckAndAppropriateView(deck);
						UpdateMenuItemVisibility();
					}

					DeckPickerList.UpdateArchivedClassVisibility();
				}

				var archivedLog = archive ? "archived" : "unarchived";
				Log.Info($"Successfully {archivedLog} deck: {deck.Name}");

				if(Config.Instance.HearthStatsAutoUploadNewDecks && HearthStatsAPI.IsLoggedIn)
				{
					Log.Info($"auto uploading {archivedLog} deck");
					HearthStatsManager.UpdateDeckAsync(deck, background: true).Forget();
				}
			}
			catch(Exception ex)
			{
				Log.Error($"Error {(archive ? "archiving" : "unarchiving")} deck {deck.Name}/n{ex}");
			}
		}

		internal async void BtnCloneDeck_Click(object sender, RoutedEventArgs e)
		{
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault();

			if(deck == null)
				return;
			var cloneStats =
				(await
				 this.ShowMessageAsync("Clone game history?", "(Cloned games do not count towards class or overall stats.)",
				                       MessageDialogStyle.AffirmativeAndNegative,
				                       new MessageDialogs.Settings
				                       {
					                       AffirmativeButtonText = "clone history",
					                       NegativeButtonText = "do not clone history"
				                       })) == MessageDialogResult.Affirmative;

			var clone = (Deck)deck.CloneWithNewId(false);

			clone.ResetHearthstatsIds();
			clone.Versions.ForEach(v => v.ResetHearthstatsIds());
			clone.Archived = false;

			var originalStats = deck.DeckStats;

			DeckList.Instance.Decks.Add(clone);
			DeckList.Save();

			DeckStats newStatsEntry;
			if(!DeckStatsList.Instance.DeckStats.TryGetValue(clone.DeckId, out newStatsEntry))
			{
				newStatsEntry = new DeckStats(clone);
				DeckStatsList.Instance.DeckStats.TryAdd(clone.DeckId, newStatsEntry);
			}

			if(cloneStats)
			{
				foreach(var game in originalStats.Games)
					newStatsEntry.AddGameResult(game.CloneWithNewId());
				Log.Info("cloned gamestats");
			}

			DeckStatsList.Save();
			DeckPickerList.SelectDeckAndAppropriateView(clone);

			if(Config.Instance.HearthStatsAutoUploadNewDecks && HearthStatsAPI.IsLoggedIn)
				HearthStatsManager.UploadDeckAsync(clone).Forget();
		}

		internal async void BtnCloneSelectedVersion_Click(object sender, RoutedEventArgs e)
		{
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault();

			if(deck == null)
				return;

			deck = deck.GetSelectedDeckVersion();

			var cloneStats =
				(await
				 this.ShowMessageAsync("Clone game history?", "(Cloned games do not count towards class or overall stats.)",
				                       MessageDialogStyle.AffirmativeAndNegative,
				                       new MessageDialogs.Settings
				                       {
					                       AffirmativeButtonText = "clone history",
					                       NegativeButtonText = "do not clone history"
				                       })) == MessageDialogResult.Affirmative;

			var clone = (Deck)deck.CloneWithNewId(false);

			clone.ResetVersions();
			clone.ResetHearthstatsIds();
			clone.Archived = false;

			var originalStatsEntry = clone.DeckStats;

			DeckList.Instance.Decks.Add(clone);
			DeckPickerList.UpdateDecks();
			DeckList.Save();
			
			DeckStats newStatsEntry;
			if(!DeckStatsList.Instance.DeckStats.TryGetValue(clone.DeckId, out newStatsEntry))
			{
				newStatsEntry = new DeckStats(clone);
				DeckStatsList.Instance.DeckStats.TryAdd(clone.DeckId, newStatsEntry);
			}

			//clone game stats
			if(cloneStats)
			{
				foreach(var game in originalStatsEntry.Games)
					newStatsEntry.AddGameResult(game.CloneWithNewId());
				Log.Info("cloned gamestats (version)");
			}

			DeckStatsList.Save();
			//DeckPickerList.UpdateList();
			DeckPickerList.SelectDeckAndAppropriateView(clone);

			if(Config.Instance.HearthStatsAutoUploadNewDecks && HearthStatsAPI.IsLoggedIn)
				HearthStatsManager.UploadDeckAsync(clone).Forget();
		}

		internal void BtnTags_Click(object sender, RoutedEventArgs e)
		{
			FlyoutMyDecksSetTags.IsOpen = true;
			TagControlEdit.SetSelectedTags(DeckPickerList.SelectedDecks);
		}

		internal void BtnEditDeck_Click(object sender, RoutedEventArgs e)
		{
			var selectedDeck = DeckPickerList.SelectedDecks.FirstOrDefault();
			if(selectedDeck == null)
				return;
			SetNewDeck(selectedDeck, true);
		}

		internal async void BtnUpdateDeck_Click(object sender, RoutedEventArgs e)
		{
			var selectedDeck = DeckPickerList.SelectedDecks.FirstOrDefault();
			if(string.IsNullOrEmpty(selectedDeck?.Url))
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

		internal async void BtnSetDeckUrl_Click(object sender, RoutedEventArgs e)
		{
			var selectedDeck = DeckPickerList.SelectedDecks.FirstOrDefault();
			if (selectedDeck == null)
				return;

			var url = await InputDeckURL();
			if (string.IsNullOrEmpty(url))
				return;

			selectedDeck.Url = url;
			BtnUpdateDeck_Click(sender, e);
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
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault();
			if(string.IsNullOrEmpty(deck?.Url))
				return;
			Helper.TryOpenUrl(deck.Url);
		}

		internal async void BtnName_Click(object sender, RoutedEventArgs e)
		{
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault();
			if(deck == null)
				return;
			var settings = new MessageDialogs.Settings {AffirmativeButtonText = "set", NegativeButtonText = "cancel", DefaultText = deck.Name};
			var newName = await this.ShowInputAsync("Set deck name", "", settings);
			if(string.IsNullOrEmpty(newName) || deck.Name == newName)
				return;
			deck.Name = newName;
			deck.Edited();
			if(deck.DeckStats.Games.Any())
			{
				foreach(var game in deck.DeckStats.Games)
					game.DeckName = newName;
				DeckStatsList.Save();
			}

			DeckList.Save();
			DeckPickerList.UpdateDecks();
			if(Config.Instance.HearthStatsAutoUploadNewDecks && HearthStatsAPI.IsLoggedIn)
				HearthStatsManager.UpdateDeckAsync(deck, true, true).Forget();
		}

		private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control && _newDeck != null)
			{
				MenuItemSave.IsSubmenuOpen = true;
				MenuItemSave.Focus();
			}
		}
	}
}