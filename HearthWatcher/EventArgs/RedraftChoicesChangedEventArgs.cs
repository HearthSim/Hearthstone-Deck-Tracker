using HearthMirror.Objects;

namespace HearthWatcher.EventArgs;

public class RedraftChoicesChangedEventArgs : System.EventArgs
{
	public Card[] Choices { get; }

	public Deck Deck { get; }
	public Deck RedraftDeck { get; }
	public int Losses { get; }

	public int Slot { get; }

	public bool IsUnderground  { get; }

	public RedraftChoicesChangedEventArgs(Card[] choices, Deck deck, Deck redraftDeck, int slot, int losses, bool isUnderground)
	{
		Choices = choices;
		Deck = deck;
		RedraftDeck = redraftDeck;
		Slot = slot;
		Losses = losses;
		IsUnderground = isUnderground;
	}
}
