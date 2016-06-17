#region

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Extensions;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls
{
	/// <summary>
	/// Interaction logic for DeckPanel.xaml
	/// </summary>
	public partial class DeckPanel : UserControl
	{
		private Deck _deck;

		public DeckPanel()
		{
			InitializeComponent();
		}

		private void ButtonImport_OnClick(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.SetNewDeck(_deck);
			Core.MainWindow.FlyoutStats.IsOpen = false;
			Core.MainWindow.FlyoutDeck.IsOpen = false;
			if(Config.Instance.StatsInWindow)
				Core.MainWindow.ActivateWindow();
		}

		public void SetDeck(IEnumerable<TrackedCard> cards, bool showImportButton = true)
		{
			var deck = new Deck();
			foreach(var c in cards)
			{
				var existing = deck.Cards.FirstOrDefault(x => x.Id == c.Id);
				if(existing != null)
				{
					existing.Count++;
					continue;
				}
				var card = Database.GetCardFromId(c.Id);
				card.Count = c.Count;
				deck.Cards.Add(card);
				if(string.IsNullOrEmpty(deck.Class) && !string.IsNullOrEmpty(card.PlayerClass))
					deck.Class = card.PlayerClass;
			}
			SetDeck(deck, showImportButton);
		}

		public void SetDeck(Deck deck, bool showImportButton = true)
		{
			_deck = deck;
			ListViewDeck.Items.Clear();
			foreach(var card in deck.Cards.ToSortedCardList())
				ListViewDeck.Items.Add(card);
			Helper.SortCardCollection(ListViewDeck.Items, false);
			ButtonImport.Visibility = showImportButton ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}