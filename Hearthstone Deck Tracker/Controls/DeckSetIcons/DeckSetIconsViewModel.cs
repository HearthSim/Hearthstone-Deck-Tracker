using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.DeckSetIcons
{
	public class DeckSetIconsViewModel : ViewModel
	{
		private Deck _deck;

		public Deck Deck
		{
			get => _deck;
			set
			{
				_deck = value;
				OnPropertyChanged();
			}
		}
	}
}
