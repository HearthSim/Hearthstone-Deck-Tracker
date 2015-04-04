#region

using System;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.HearthStats.API.Objects
{
	public class DeckObjectWrapper
	{
		public CardObject[] cards;
		public string current_version;
		public DeckObject deck;
		public DeckVersion[] versions;
		public DateTime updated_at;

		public Deck ToDeck()
		{
			if(deck == null)
				return null;
			return deck.ToDeck(cards, versions, current_version);
		}
	}
}