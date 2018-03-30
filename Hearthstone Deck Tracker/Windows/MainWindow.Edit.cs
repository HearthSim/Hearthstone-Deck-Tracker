#region

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.Properties;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls.Dialogs;
using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Utility;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class MainWindow
	{
		internal void ShowDeckNotesDialog(Deck deck)
		{
			if(deck == null)
				return;
			DeckNotesEditor.SetDeck(deck);
			var flyoutHeader = deck.Name.Length >= 20 ? string.Join("", deck.Name.Take(17)) + "..." : deck.Name;
			FlyoutNotes.Header = flyoutHeader;
			FlyoutNotes.IsOpen = true;
		}

		internal void ShowDeleteDeckMessage(Deck deck) => ShowDeleteDecksMessage(deck == null ? null : new[] { deck });

		internal async void ShowDeleteDecksMessage(IEnumerable<Deck> decks)
		{
			if(decks == null)
				return;
			var decksList = decks.ToList();
			if(!decksList.Any())
				return;

			var settings = new MessageDialogs.Settings {AffirmativeButtonText = LocUtil.Get(nameof(Strings.Enum_YesNo_Yes)), NegativeButtonText = LocUtil.Get(nameof(Strings.Enum_YesNo_No))};
			var keepStatsInfo = Config.Instance.KeepStatsWhenDeletingDeck
				                    ? "The stats will be kept (can be changed in options)"
				                    : "The stats will be deleted (can be changed in options)";
			var result =
				await
				this.ShowMessageAsync("Deleting " + (decksList.Count == 1 ? decksList.First().Name : decksList.Count + " decks"),
				                      "Are you Sure?\n" + keepStatsInfo, MessageDialogStyle.AffirmativeAndNegative, settings);
			if(result == MessageDialogResult.Negative)
				return;
			foreach(var deck in decksList)
				DeleteDeck(deck, false);
			DeckStatsList.Save();
			DeckList.Save();
			DeckPickerList.UpdateDecks();
			DeckPickerList.UpdateArchivedClassVisibility();
			DeckManagerEvents.OnDeckDeleted.Execute(decksList);
		}

		private void DeleteDeck(Deck deck, bool saveAndUpdate = true)
		{
			if(deck == null)
				return;

			if(Equals(DeckList.Instance.ActiveDeck, deck))
				SelectDeck(null, true);

			if(DeckStatsList.Instance.DeckStats.TryGetValue(deck.DeckId, out var deckStats))
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
				}
				DeckStatsList.Instance.DeckStats.TryRemove(deckStats.DeckId, out deckStats);
				if(saveAndUpdate)
					DeckStatsList.Save();
				Log.Info("Removed deckstats from deck: " + deck.Name);
			}

			DeckList.Instance.Decks.Remove(deck);
			if(saveAndUpdate)
			{
				DeckList.Save();
				DeckPickerList.UpdateDecks();
				DeckPickerList.UpdateArchivedClassVisibility();
			}
			Log.Info("Deleted deck: " + deck.Name);
		}

		internal void ArchiveDecks(IEnumerable<Deck> decks)
		{
			foreach(var deck in decks)
				ArchiveDeck(deck, true, false);

			DeckList.Save();
			DeckPickerList.UpdateDecks();
			SelectDeck(null, true);
			DeckPickerList.UpdateArchivedClassVisibility();
		}

		internal void UnArchiveDecks(IEnumerable<Deck> decks)
		{
			foreach(var deck in DeckPickerList.SelectedDecks)
				ArchiveDeck(deck, false, false);

			DeckList.Save();
			DeckPickerList.UpdateDecks();
			DeckPickerList.SelectDeckAndAppropriateView(DeckPickerList.SelectedDecks.FirstOrDefault());
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
						DeckPickerList.SelectDeckAndAppropriateView(deck);

					DeckPickerList.UpdateArchivedClassVisibility();
				}

				var archivedLog = archive ? "archived" : "unarchived";
				Log.Info($"Successfully {archivedLog} deck: {deck.Name}");
			}
			catch(Exception ex)
			{
				Log.Error($"Error {(archive ? "archiving" : "unarchiving")} deck {deck.Name}/n{ex}");
			}
		}

		internal async void ShowCloneDeckDialog(Deck deck)
		{
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
			clone.Archived = false;

			var originalStats = deck.DeckStats;

			DeckList.Instance.Decks.Add(clone);
			DeckList.Save();

			if(!DeckStatsList.Instance.DeckStats.TryGetValue(clone.DeckId, out var newStatsEntry))
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
		}

		internal async void ShowCloneDeckVersionDialog(Deck deck)
		{
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
			clone.Archived = false;

			var originalStatsEntry = clone.DeckStats;

			DeckList.Instance.Decks.Add(clone);
			DeckPickerList.UpdateDecks();
			DeckList.Save();
			
			if(!DeckStatsList.Instance.DeckStats.TryGetValue(clone.DeckId, out var newStatsEntry))
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
			DeckPickerList.SelectDeckAndAppropriateView(clone);
		}

		internal void ShowTagEditDialog(IEnumerable<Deck> decks)
		{
			if(decks == null || !decks.Any())
				return;
			FlyoutMyDecksSetTags.IsOpen = true;
			TagControlEdit.SetSelectedTags(decks);
		}

		internal void BtnEditDeck_Click(object sender, RoutedEventArgs e)
		{
			var selectedDeck = DeckPickerList.SelectedDecks.FirstOrDefault();
			if(selectedDeck == null)
				return;
			ShowDeckEditorFlyout(selectedDeck, false);
		}

		internal async void UpdateDeckFromWeb(Deck existingDeck)
		{
			if(existingDeck == null || string.IsNullOrEmpty(existingDeck.Url))
				return;
			var deck = await DeckImporter.Import(existingDeck.Url);
			if(deck == null)
			{
				await this.ShowMessageAsync("Error", "Could not load deck from specified url.");
				return;
			}
			//this could be expanded to check against the last version of the deck that was not modified after downloading
			if(deck.Cards.All(c1 => existingDeck.GetSelectedDeckVersion().Cards.Any(c2 => c1.Name == c2.Name && c1.Count == c2.Count)) && deck.Name == existingDeck.Name)
			{
				await this.ShowMessageAsync("Already up to date.", "No changes found.");
				return;
			}

			var imported = (Deck)existingDeck.Clone();
			imported.Name = deck.Name;
			imported.Cards.Clear();
			foreach(var card in deck.Cards)
				imported.Cards.Add(card);
			ShowDeckEditorFlyout(imported, false);
			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
			ManaCurveMyDecks.UpdateValues();

			TagControlEdit.SetSelectedTags(deck.Tags);
		}

		internal async void SetDeckUrl(Deck deck)
		{
			if (deck == null)
				return;

			var url = await InputDeckUrl();
			if (string.IsNullOrEmpty(url))
				return;

			deck.Url = url;
			UpdateDeckFromWeb(deck);
		}

		internal void MoveDecksToArena(IEnumerable<Deck> decks)
		{
			if(decks == null || !decks.Any())
				return;
			foreach(var deck in decks)
				deck.IsArenaDeck = true;
			DeckPickerList.UpdateDecks();
		}

		internal void MoveDecksToConstructed(IEnumerable<Deck> decks)
		{
			if(decks == null || !decks.Any())
				return;
			foreach(var deck in DeckPickerList.SelectedDecks)
				deck.IsArenaDeck = false;
			DeckPickerList.UpdateDecks();
		}

		internal void OpenDeckUrl(Deck deck)
		{
			if(string.IsNullOrEmpty(deck?.Url))
				return;
			Helper.TryOpenUrl(deck.Url);
		}

		internal async void ShowEditDeckNameDialog(Deck deck)
		{
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
		}

		private async void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
			{
				if(FlyoutDeckEditor.IsOpen)
				{
					var deck = await ClipboardImporter.Import();
					if(deck != null)
					{
						var currentDeck = DeckEditorFlyout.CurrentDeck;
						if(currentDeck != null && deck.Class == currentDeck.Class)
						{
							if(string.IsNullOrEmpty(currentDeck.Name))
								DeckEditorFlyout.SetDeckName(deck.Name);
							DeckEditorFlyout.SetCards(deck.Cards);
						}
						e.Handled = true;
					}
				}
				else
				{
					if(Keyboard.FocusedElement is TextBox)
						return;
					ImportFromClipboard();
				}
			}
		}
	}
}
