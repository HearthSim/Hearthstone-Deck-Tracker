using Hearthstone_Deck_Tracker.Annotations;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(BattlegroundsCardsGroup));

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set
			{
				SetValue(TitleProperty, value);
				OnPropertyChanged(nameof(TitleVisibility));
			}
		}

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
