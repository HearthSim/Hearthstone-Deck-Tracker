using System.Collections.Generic;
using HearthMirror.Objects;

namespace HearthWatcher.EventArgs
{
	public class CardPickedEventArgs : System.EventArgs
	{
		public Card Picked { get; }
		public Card[] Choices { get; }
		public List<Card>? PickedPackage { get; }

		public Deck Deck { get; }

		public int Slot { get;  }

		public bool IsUnderground { get; }

		public CardPickedEventArgs(Card picked, Card[] choices, Deck deck, int slot, bool isUnderground, List<Card>? pickedPackage)
		{
			Picked = picked;
			Choices = choices;
			PickedPackage = pickedPackage;
			Deck = deck;
			Slot = slot;
			IsUnderground = isUnderground;
		}
	}
}
