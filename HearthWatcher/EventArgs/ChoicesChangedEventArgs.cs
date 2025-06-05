using System.Collections.Generic;
using HearthMirror.Objects;

namespace HearthWatcher.EventArgs
{
	public class ChoicesChangedEventArgs : System.EventArgs
	{
		public Card[] Choices { get; }

		public List<List<Card>>? Packages { get; }
		public Deck Deck { get; }

		public int Slot { get; }

		public bool IsUnderground  { get; }

		public ChoicesChangedEventArgs(Card[] choices, Deck deck, int slot, bool isUnderground, List<List<Card>>? packages)
		{
			Choices = choices;
			Packages = packages;
			Deck = deck;
			Slot = slot;
			IsUnderground = isUnderground;
		}
	}
}
