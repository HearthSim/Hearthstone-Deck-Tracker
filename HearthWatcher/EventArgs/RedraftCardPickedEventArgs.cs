using System.Collections.Generic;
using HearthMirror.Objects;

namespace HearthWatcher.EventArgs;

public class RedraftCardPickedEventArgs : System.EventArgs
{
	public Card Picked { get; }
	public Card[] Choices { get; }
	public Deck Deck { get; }
	public Deck RedraftDeck { get; }
	public int Losses { get; }

	public int Slot { get;  }

	public bool IsUnderground { get; }

	public RedraftCardPickedEventArgs(Card picked, Card[] choices, Deck deck, Deck redraftDeck, int slot, int losses, bool isUnderground)
	{
		Picked = picked;
		Choices = choices;
		Deck = deck;
		RedraftDeck = redraftDeck;
		Slot = slot;
		Losses = losses;
		IsUnderground = isUnderground;
	}

}
