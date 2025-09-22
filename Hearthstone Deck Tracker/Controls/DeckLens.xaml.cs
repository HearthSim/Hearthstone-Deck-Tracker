using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class DeckLens : UserControl
	{
		public DeckLens()
		{
			InitializeComponent();
			Loaded += (s, e) =>
			{
				UpdateIconsVisibility(Icon);
				UpdateColors(IsPremium);
			};
		}

		public string Label
		{
			get { return (string)GetValue(LabelProperty); }
			set { SetValue(LabelProperty, value); }
		}

		public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(DeckLens), new PropertyMetadata(""));

		public static readonly DependencyProperty DeckLensIconProperty = DependencyProperty.Register(
		    "Icon",
		    typeof(DeckLensIcon),
		    typeof(DeckLens),
		    new PropertyMetadata(DeckLensIcon.Lens, OnDeckLensIconChanged));

		public static readonly DependencyProperty IsPremiumProperty = DependencyProperty.Register(
		    "IsPremium",
		    typeof(bool),
		    typeof(DeckLens),
		    new PropertyMetadata(false, OnIsPremiumChanged));

		private static void OnDeckLensIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
		    if (d is DeckLens deckLens && e.NewValue is DeckLensIcon newIcon)
		    {
		        deckLens.UpdateIconsVisibility(newIcon);
		    }
		}

		private static void OnIsPremiumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
		    if (d is DeckLens deckLens && e.NewValue is bool isPremium)
		    {
		        deckLens.UpdateColors(isPremium);
		    }
		}

		private void UpdateColors(bool isPremium)
		{
			var color = isPremium ? (Brush)Application.Current.Resources["HSReplayNetPremiumGold"] : Brushes.White;
			LabelText.Foreground = color;
			LensIcon.Fill = color;
			ArenasmithIcon.LogoBrush = color;
		}

		private void UpdateIconsVisibility(DeckLensIcon icon)
		{
		    if (ArenasmithIcon == null || LensIcon == null)
		        return;

		    switch (icon)
		    {
		        case DeckLensIcon.Lens:
		            ArenasmithIcon.Visibility = Visibility.Collapsed;
		            LensIcon.Visibility = Visibility.Visible;
		            break;
		        case DeckLensIcon.Arenasmith:
		            LensIcon.Visibility = Visibility.Collapsed;
		            ArenasmithIcon.Visibility = Visibility.Visible;
		            break;
		        default:
		            ArenasmithIcon.Visibility = Visibility.Collapsed;
		            LensIcon.Visibility = Visibility.Visible;
		            break;
		    }
		}

		public DeckLensIcon Icon
		{
		    get => (DeckLensIcon)GetValue(DeckLensIconProperty);
		    set => SetValue(DeckLensIconProperty, value);
		}

		public bool IsPremium
		{
		    get => (bool)GetValue(IsPremiumProperty);
		    set => SetValue(IsPremiumProperty, value);
		}

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

public enum DeckLensIcon
{
	Lens,
	Arenasmith,
}
