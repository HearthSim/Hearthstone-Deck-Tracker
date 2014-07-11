using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro.Controls;

namespace Hearthstone_Deck_Tracker
{
    /// <summary>
    /// Interaction logic for DeckSelectionDialog.xaml
    /// </summary>
    public partial class DeckSelectionDialog
    {
        public DeckSelectionDialog(IEnumerable<Deck> decks)
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            DeckPickerList.Items.Clear();
            foreach (var deck in decks)
            {
                DeckPickerList.Items.Add(deck);
            }
            
        }

        public Deck SelectedDeck;
        private void DeckPickerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedDeck = DeckPickerList.SelectedItem as Deck;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(SelectedDeck == null)
                MessageBox.Show("Deck detection disabled for now. You can reenable it in the options.");
        }
    }
}
