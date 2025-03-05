using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class DeckLens : UserControl
	{
		public DeckLens()
		{
			InitializeComponent();
		}

		public string Label
		{
			get { return (string)GetValue(LabelProperty); }
			set { SetValue(LabelProperty, value); }
		}

		public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(DeckLens), new PropertyMetadata(""));

		public async Task Update(List<Hearthstone.Card> cards, bool reset)
		{
			if(cards.Count > 0)
				Visibility = Visibility.Visible;
			await CardList.Update(cards, reset);
			if(cards.Count == 0)
				Visibility = Visibility.Collapsed;
		}
	}

}
