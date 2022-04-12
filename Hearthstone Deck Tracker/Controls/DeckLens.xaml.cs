using System.Collections.Generic;
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

		public async void Update(List<Hearthstone.Card> cards, bool reset)
		{
			if(cards.Count > 0)
				Container.Visibility = Visibility.Visible;
			await CardList.UpdateAsync(cards, reset);
			if(cards.Count == 0)
				Container.Visibility = Visibility.Collapsed;
		}
	}

}
