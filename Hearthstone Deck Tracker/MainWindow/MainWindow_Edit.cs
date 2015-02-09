#region

using System;
using System.Linq;
using System.Windows;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public partial class MainWindow
	{
		private void BtnNotes_Click(object sender, RoutedEventArgs e)
		{
			if(DeckList.Instance.ActiveDeck == null)
				return;
			FlyoutNotes.IsOpen = !FlyoutNotes.IsOpen;
		}

		private async void BtnDeleteDeck_Click(object sender, RoutedEventArgs e)
		{
			var deck = DeckList.Instance.ActiveDeck;
			if(deck == null)
				return;

			var settings = new MetroDialogSettings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"};
			var keepStatsInfo = Config.Instance.KeepStatsWhenDeletingDeck
				                    ? "The stats will be kept (can be changed in options)"
				                    : "The stats will be deleted (can be changed in options)";
			var result =
				await
				this.ShowMessageAsync("Deleting " + deck.Name, "Are you Sure?\n" + keepStatsInfo, MessageDialogStyle.AffirmativeAndNegative,
				                      settings);
			if(result == MessageDialogResult.Negative)
				return;

			DeselectDeck();
			DeleteDeck(deck);
		}

		private void DeleteDeck(Deck deck)
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
				DeckStatsList.Save();
				Logger.WriteLine("Removed deckstats from deck: " + deck.Name, "Edit");
			}

			HearthStatsManager.DeleteDeckAsync(deck, false, true);

			DeckList.Instance.Decks.Remove(deck);
			DeckList.Save();
			;
			//DeckPickerList.RemoveDeck(deck);
			DeckPickerList.UpdateDecks();
			ListViewDeck.ItemsSource = null;
			Logger.WriteLine("Deleted deck: " + deck.Name, "Edit");
		}

		private async void BtnCloneDeck_Click(object sender, RoutedEventArgs e)
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
			//DeckPickerList.AddAndSelectDeck(clone);
			DeckList.Save();
			;

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
				Logger.WriteLine("cloned gamestats", "Edit");
			}

			DeckStatsList.Save();
			DeckPickerList.UpdateDecks();

			if(Config.Instance.HearthStatsAutoUploadNewDecks)
				HearthStatsManager.UploadDeckAsync(clone);
		}

		private async void BtnCloneSelectedVersion_Click(object sender, RoutedEventArgs e)
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
			;

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
			DeckPickerList.UpdateDecks();

			if(Config.Instance.HearthStatsAutoUploadNewDecks)
				HearthStatsManager.UploadDeckAsync(clone);
		}

		private void BtnTags_Click(object sender, RoutedEventArgs e)
		{
			FlyoutMyDecksSetTags.IsOpen = true;
			if(DeckList.Instance.ActiveDeck != null)
				TagControlEdit.SetSelectedTags(DeckList.Instance.ActiveDeck.Tags);
		}

		private void BtnEditDeck_Click(object sender, RoutedEventArgs e)
		{
			var selectedDeck = DeckList.Instance.ActiveDeck;
			if(selectedDeck == null)
				return;
			_originalDeck = selectedDeck;
			SetNewDeck(selectedDeck, true);
		}

		private async void BtnUpdateDeck_Click(object sender, RoutedEventArgs e)
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

			UpdateTitle();
			Helper.SortCardCollection(ListViewDeck.Items, Config.Instance.CardSortingClassFirst);
			ManaCurveMyDecks.UpdateValues();

			TagControlEdit.SetSelectedTags(deck.Tags);
		}
	}
}