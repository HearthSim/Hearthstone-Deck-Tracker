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

		private bool _available;
		public bool Available
		{
			get => _available;
			set
			{
				if(_available == value)
					return;
				_available = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(CardsVisibility));
				OnPropertyChanged(nameof(UnavailableVisibility));
			}
		}

		public Visibility CardsVisibility => Available ? Visibility.Visible : Visibility.Collapsed;

		public Visibility UnavailableVisibility => Available ? Visibility.Collapsed : Visibility.Visible;

		public void UpdateCards(List<Hearthstone.Card> cards, bool available)
		{
			Available = available;
			Cards.Update(cards, true);
			Visibility = Visibility.Visible;
		}

		public void Hide()
		{
			Visibility = Visibility.Collapsed;
		}
	}
}
