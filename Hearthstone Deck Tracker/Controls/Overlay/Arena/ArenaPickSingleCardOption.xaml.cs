using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Arena;

public partial class ArenaPickSingleCardOption : UserControl
{
	public ArenaPickSingleCardOption()
	{
		InitializeComponent();
	}

	public static readonly DependencyProperty ArenasmithScoreVisibilityProperty =
		DependencyProperty.Register(nameof(ArenasmithScoreVisibility), typeof(Visibility), typeof(ArenaPickSingleCardOption), new PropertyMetadata(Visibility.Visible));

	public Visibility ArenasmithScoreVisibility
	{
		get { return (Visibility)GetValue(ArenasmithScoreVisibilityProperty); }
		set { SetValue(ArenasmithScoreVisibilityProperty, value); }
	}

	public static readonly DependencyProperty RelatedCardsVisibilityProperty =
		DependencyProperty.Register(nameof(RelatedCardsVisibility), typeof(Visibility), typeof(ArenaPickSingleCardOption), new PropertyMetadata(Visibility.Visible));

	public Visibility RelatedCardsVisibility
	{
		get { return (Visibility)GetValue(RelatedCardsVisibilityProperty); }
		set { SetValue(RelatedCardsVisibilityProperty, value); }
	}

	public static readonly DependencyProperty SynergyVisibilityProperty =
		DependencyProperty.Register(nameof(SynergyVisibility), typeof(Visibility), typeof(ArenaPickSingleCardOption), new PropertyMetadata(Visibility.Visible));

	public Visibility SynergyVisibility
	{
		get { return (Visibility)GetValue(SynergyVisibilityProperty); }
		set { SetValue(SynergyVisibilityProperty, value); }
	}

}

