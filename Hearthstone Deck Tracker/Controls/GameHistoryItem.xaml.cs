using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for GameHistoryItem.xaml
	/// </summary>
	public partial class GameHistoryItem
	{
		public GameHistoryItem(TurnStats.Play play)
		{
			InitializeComponent();

			LblItem.Content = play.Type;
			if(!string.IsNullOrEmpty(play.CardId))
				SetCard(play.CardId);
			else
				GridCard.Visibility = Visibility.Collapsed;
		}

		public void SetCard(string cardId)
		{
			var card = Game.GetCardFromId(cardId);
			if(card != null)
			{
				GridCard.Background = card.Background;
				TxtCardCost.Text = card.Cost.ToString();
				TxtCardName.Text = card.Name;
				CardTooltip.SetValue(DataContextProperty, card);
			}
		}
	}
}