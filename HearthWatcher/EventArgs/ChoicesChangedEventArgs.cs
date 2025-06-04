using HearthMirror.Objects;

namespace HearthWatcher.EventArgs
{
	public class ChoicesChangedEventArgs : System.EventArgs
	{
		public Card[] Choices { get; }
		public Deck Deck { get; }

		public int Slot { get; }

		public bool IsUnderground  { get; }

		public ChoicesChangedEventArgs(Card[] choices, Deck deck, int slot, bool isUnderground)
		{
			Choices = choices;
			Deck = deck;
			Slot = slot;
			IsUnderground = isUnderground;
		}
	}
}
