using System.Windows;

namespace Hearthstone_Deck_Tracker.Controls
{
	/// <summary>
	/// Interaction logic for Card.xaml
	/// </summary>
	public partial class Card
	{
		public static readonly DependencyProperty HasTooltipProperty = DependencyProperty.Register("HasTooltip", typeof(bool), typeof(Card), new PropertyMetadata(Config.Instance.TrackerCardToolTips));

		public Card()
		{
			InitializeComponent();
		}

		public bool HasTooltip
		{
			get => (bool)GetValue(HasTooltipProperty);
			set => SetValue(HasTooltipProperty, value);
		}

		private void Rectangle_ToolTipOpening(object sender, System.Windows.Controls.ToolTipEventArgs e)
		{
			TooltipCardImage.SetCardIdFromCard(DataContext as Hearthstone.Card);
		}
		public string? CardId => (DataContext as Hearthstone.Card)?.Id;

		private void Rectangle_ToolTipClosing(object sender, System.Windows.Controls.ToolTipEventArgs e)
		{
			TooltipCardImage.SetCardIdFromCard(null);
		}
	}
}
