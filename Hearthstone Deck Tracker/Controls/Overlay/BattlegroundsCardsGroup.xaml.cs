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

		private Visibility _unavailableRacesVisibility = Visibility.Collapsed;
		public Visibility UnavailableRacesVisibility
		{
			get => _unavailableRacesVisibility;
			set
			{
				_unavailableRacesVisibility = value;
				OnPropertyChanged();
			}
		}

		private string _unavailableRacesText;
		public string UnavailableRacesText
		{
			get => _unavailableRacesText;
			set
			{
				_unavailableRacesText = value;
				OnPropertyChanged();
			}
		}

		public void UpdateCards(List<Hearthstone.Card> cards)
		{
			Cards.Update(cards, true);
			Visibility = Visibility.Visible;
		}

		public void SetUnaivalableRaceContainer(string unavailableRaces)
		{
			UnavailableRacesVisibility = Visibility.Visible;
			UnavailableRacesText = unavailableRaces;
		}

		public void Hide()
		{
			Visibility = Visibility.Collapsed;
		}
	}
}
