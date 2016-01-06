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
		public string[] tags;
		public DateTime updated_at;
		public DeckVersion[] versions;

		public Deck ToDeck() => deck?.ToDeck(cards, tags, versions, current_version);
	}
}