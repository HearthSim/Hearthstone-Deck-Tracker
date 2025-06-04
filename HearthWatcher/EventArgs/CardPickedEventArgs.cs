using HearthMirror.Objects;

namespace HearthWatcher.EventArgs
{
	public class CardPickedEventArgs : System.EventArgs
	{
		public Card Picked { get; }
		public Card[] Choices { get; }

		public Deck Deck { get; }

		public int Slot { get;  }

		public bool IsUnderground { get; }

		public CardPickedEventArgs(Card picked, Card[] choices, Deck deck, int slot, bool isUnderground)
		{
			Picked = picked;
			Choices = choices;
			Deck = deck;
			Slot = slot;
			IsUnderground = isUnderground;
		}
	}
}
