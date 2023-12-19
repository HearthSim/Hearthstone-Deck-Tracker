using System.Collections;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Minions;

public partial class BattlegroundsCardsGroup : UserControl
{
	public BattlegroundsCardsGroup()
	{
		InitializeComponent();
	}

	public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
		nameof(Title),
		typeof(string),
		typeof(BattlegroundsCardsGroup),
		new PropertyMetadata("", null)
	);

	public string Title
	{
		get { return (string)GetValue(TitleProperty); }
		set
		{
			SetValue(TitleProperty, value);
		}
	}

	public Visibility TitleVisibility => string.IsNullOrEmpty(Title) ? Visibility.Collapsed : Visibility.Visible;


	public static readonly DependencyProperty CardsProperty = DependencyProperty.Register(
		nameof(Cards),
		typeof(IEnumerable<Hearthstone.Card>),
		typeof(BattlegroundsCardsGroup),
		new PropertyMetadata(CardsChanged)
	);

	private static void CardsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var group = (BattlegroundsCardsGroup)d;
		group.Cards = (IEnumerable<Hearthstone.Card>)e.NewValue;
	}

	public IEnumerable<Hearthstone.Card> Cards
	{
		get { return (IEnumerable<Hearthstone.Card>)GetValue(CardsProperty); }
		set
		{
			SetValue(CardsProperty, value);
			UpdateCards(value.ToList());
		}
	}

	public void UpdateCards(List<Hearthstone.Card> cards)
	{
		CardsList.Update(cards, true);
	}
}
