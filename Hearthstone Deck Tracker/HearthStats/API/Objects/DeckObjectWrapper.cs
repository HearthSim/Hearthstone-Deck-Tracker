using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.HearthStats.API.Objects
{
	public class DeckObjectWrapper
	{
		public CardObject[] cards;
		public DeckObject deck;
		public DeckVersion[] versions;
		public string current_version;

		public Deck ToDeck()
		{
			if(deck == null)
				return null;
			return deck.ToDeck(cards, versions, current_version);
		}
	}
}