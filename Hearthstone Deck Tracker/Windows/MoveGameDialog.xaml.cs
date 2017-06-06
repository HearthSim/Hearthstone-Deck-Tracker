#region

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Controls.DeckPicker;
using Hearthstone_Deck_Tracker.Controls.DeckPicker.DeckPickerItemLayouts;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for MoveGameDialog.xaml
	/// </summary>
	public partial class MoveGameDialog
	{
		public Deck SelectedDeck;
		public SerializableVersion SelectedVersion;

		public MoveGameDialog(IEnumerable<Deck> decks)
		{
			InitializeComponent();

			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			ListViewDecks.Items.Clear();
			foreach(var deck in decks.OrderByDescending(d => d.Name))
				ListViewDecks.Items.Add(new DeckPickerItem(deck, typeof(DeckPickerItemLayoutMinimal)));
		}

		private void ListViewDecks_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			foreach(var item in e.AddedItems)
			{
				var pickerItem = item as DeckPickerItem;
				if(pickerItem == null)
					continue;
				DeckList.Instance.ActiveDeck = pickerItem.DataContext as Deck;
				pickerItem.RefreshProperties();
			}
			foreach(var item in e.RemovedItems)
				(item as DeckPickerItem)?.RefreshProperties();
			var dpi = ListViewDecks.SelectedItem as DeckPickerItem;
			if(dpi == null)
				return;
			SelectedDeck = dpi.Deck;
			ComboBoxVersions.Items.Clear();
			foreach(var version in SelectedDeck.VersionsIncludingSelf)
				ComboBoxVersions.Items.Add(version);
			ComboBoxVersions.SelectedItem = SelectedDeck.SelectedVersion;
		}

		private void ComboBoxVersions_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => SelectedVersion = ComboBoxVersions.SelectedItem as SerializableVersion;

		private void ButtonMoveToSelected_OnClick(object sender, RoutedEventArgs e) => Close();
	}
}
