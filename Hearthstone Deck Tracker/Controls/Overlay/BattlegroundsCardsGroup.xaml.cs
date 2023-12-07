using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class BattlegroundsCardsGroup : UserControl
	{
		public BattlegroundsCardsGroup()
		{
			InitializeComponent();
		}

		public string Title { get; set; } = "";

		public Visibility TitleVisibility => string.IsNullOrEmpty(Title) ? Visibility.Collapsed : Visibility.Visible;

		public void UpdateCards(List<Hearthstone.Card> cards)
		{
			Cards.Update(cards, true);
			Visibility = Visibility.Visible;
		}

		public void Hide()
		{
			Visibility = Visibility.Collapsed;
		}
	}
}
