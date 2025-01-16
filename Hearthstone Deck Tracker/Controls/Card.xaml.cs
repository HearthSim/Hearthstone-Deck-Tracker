using System.Collections.Generic;
using System.Windows;

namespace Hearthstone_Deck_Tracker.Controls
{
	/// <summary>
	/// Interaction logic for Card.xaml
	/// </summary>
	public partial class Card
	{
		public Card()
		{
			InitializeComponent();
		}

		public string? CardId => (DataContext as Hearthstone.Card)?.Id;

		public static readonly HashSet<Card> LoadedCards = new();

		private void Card_OnLoaded(object sender, RoutedEventArgs e) => LoadedCards.Add(this);
		private void Card_OnUnloaded(object sender, RoutedEventArgs e) => LoadedCards.Remove(this);

		public void UpdateBackground()
		{
			(DataContext as Hearthstone.Card)?.Update();
		}
	}
}
