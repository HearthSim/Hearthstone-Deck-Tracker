using System;
using System.Linq;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker
{
    public partial class MainWindow
    {
        private void BtnNotes_Click(object sender, RoutedEventArgs e)
        {
            if(DeckPickerList.SelectedDeck == null) return;
            FlyoutNotes.IsOpen = !FlyoutNotes.IsOpen;
        }

        private async void BtnDeleteDeck_Click(object sender, RoutedEventArgs e)
        {
            var deck = DeckPickerList.SelectedDeck;
            if(deck != null)
            {
                var settings = new MetroDialogSettings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"};
                var result =
                    await
                    this.ShowMessageAsync("Deleting " + deck.Name, "Are you Sure?",
                                          MessageDialogStyle.AffirmativeAndNegative, settings);
                if(result == MessageDialogResult.Affirmative)
                {
                    try
                    {
                        var deckStats = DeckStatsList.Instance.DeckStats.FirstOrDefault(ds => ds.Name == deck.Name);
                        if(deckStats != null)
                        {
                            foreach(var game in deckStats.Games)
                                game.DeleteGameFile();
                            DeckStatsList.Instance.DeckStats.Remove(deckStats);
                            DeckStatsList.Save();
                            Logger.WriteLine("Deleted deckstats for deck: " + deck.Name);
                        }
                        DeckList.DecksList.Remove(deck);
                        WriteDecks();
                        DeckPickerList.RemoveDeck(deck);
                        ListViewDeck.ItemsSource = null;
                        Logger.WriteLine("Deleted deck: " + deck.Name);
                    }
                    catch(Exception)
                    {
                        Logger.WriteLine("Error deleting deck");
                    }
                }
            }
        }

        private async void BtnCloneDeck_Click(object sender, RoutedEventArgs e)
        {
            var clone = (Deck)DeckPickerList.SelectedDeck.Clone();
            var originalStatsEntry = clone.DeckStats;

            while(DeckList.DecksList.Any(d => d.Name == clone.Name))
            {
                var settings = new MetroDialogSettings {AffirmativeButtonText = "Set", DefaultText = clone.Name};
                var name =
                    await
                    this.ShowInputAsync("Name already exists",
                                        "You already have a deck with that name, please select a different one.", settings);

                if(String.IsNullOrEmpty(name))
                    return;

                clone.Name = name;
            }

            DeckList.DecksList.Add(clone);
            DeckPickerList.AddAndSelectDeck(clone);
            WriteDecks();

            //clone game stats
            var newStatsEntry = DeckStatsList.Instance.DeckStats.FirstOrDefault(d => d.Name == clone.Name);
            if(newStatsEntry == null)
            {
                newStatsEntry = new DeckStats(clone.Name);
                DeckStatsList.Instance.DeckStats.Add(newStatsEntry);
            }
            foreach(var game in originalStatsEntry.Games)
                newStatsEntry.AddGameResult(game.CloneWithNewId());
            Logger.WriteLine("cloned gamestats");

            DeckStatsList.Save();
            DeckPickerList.UpdateList();
        }

        private void BtnTags_Click(object sender, RoutedEventArgs e)
        {
            FlyoutMyDecksSetTags.IsOpen = true;
            if(DeckPickerList.SelectedDeck != null)
                TagControlEdit.SetSelectedTags(DeckPickerList.SelectedDeck.Tags);
        }

        private void BtnEditDeck_Click(object sender, RoutedEventArgs e)
        {
            var selectedDeck = DeckPickerList.SelectedDeck;
            if(selectedDeck == null) return;
            SetNewDeck(selectedDeck, true);
        }
    }
}