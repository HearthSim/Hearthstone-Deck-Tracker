using System.Collections.Generic;
using System.Windows;
using HearthDb;
using Hearthstone_Deck_Tracker.Utility.Assets;

namespace Hearthstone_Deck_Tracker.Controls
{
	/// <summary>
	/// Interaction logic for Card.xaml
	/// </summary>
	public partial class Card
	{
		public static readonly HashSet<Card> LoadedCards = new();
		static Card()
		{
			CardDefsManager.CardsChanged += () =>
			{
				foreach(var card in LoadedCards)
					card.UpdateBackground();
			};
		}

		public Card()
		{
			InitializeComponent();
		}

		public string? CardId => (DataContext as Hearthstone.Card)?.Id;

		private void Card_OnLoaded(object sender, RoutedEventArgs e) => LoadedCards.Add(this);
		private void Card_OnUnloaded(object sender, RoutedEventArgs e) => LoadedCards.Remove(this);

		public void UpdateBackground()
		{
			(DataContext as Hearthstone.Card)?.Update();
		}
	}
}
